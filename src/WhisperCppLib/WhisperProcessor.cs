using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WhisperCppLib;

public class WhisperProcessor : IAsyncDisposable, IDisposable
{
    private static readonly ConcurrentDictionary<long, WhisperProcessor> processorInstances = new();
    private static long currentProcessorId;
    private const byte trueByte = 1;
    private const byte falseByte = 0;

    private readonly IntPtr currentWhisperContext;
    private readonly WhisperProcessorOptions options;
    private readonly List<GCHandle> gcHandles = new();
    private readonly SemaphoreSlim processingSemaphore;
    private WhisperFullParams whisperParams;
    private IntPtr? language;
    private IntPtr? initialPromptText;
    private bool isDisposed;
    private int segmentIndex;
    private CancellationToken? currentCancellationToken;

    // Id is used to identify the current instance when calling the callbacks from C++
    private readonly long myId;

    public event OnSegmentEventHandler? OnSegmentEventHandler;

    public event OnProgressHandler? OnProgressHandler;

    public event OnEncoderBeginEventHandler? OnEncoderBeginEventHandlers;

    public WhisperProcessor(IWhisperProcessorModelLoader loader, WhisperProcessorOptions options)
    {
        this.options = options;
        myId = Interlocked.Increment(ref currentProcessorId);

        processorInstances[myId] = this;

        currentWhisperContext = loader.LoadNativeContext();
        whisperParams = GetWhisperParams();
        processingSemaphore = new(1);
    }

    public void ChangeLanguage(string? newLanguage)
    {
        var oldLanguage = language;

        var newParams = whisperParams;
        if (string.IsNullOrEmpty(newLanguage))
        {
            newParams.Language = IntPtr.Zero;
        }
        else
        {
            language = Marshal.StringToHGlobalAnsi(newLanguage);
            newParams.Language = language.Value;
        }

        if (oldLanguage.HasValue)
        {
            Marshal.FreeHGlobal(oldLanguage.Value);
        }
        whisperParams = newParams;
    }

    public unsafe string? DetectLanguage(float[] samples, bool speedUp = false)
    {
        var (language, _) = DetectLanguageWithProbability(samples.AsSpan(), speedUp);
        return language;
    }

    public (string? language, float probability) DetectLanguageWithProbability(float[] samples, bool speedUp = false)
    {
        return DetectLanguageWithProbability(samples.AsSpan(), speedUp);
    }

    public unsafe (string? language, float probability) DetectLanguageWithProbability(ReadOnlySpan<float> samples, bool speedUp = false)
    {
        var probs = new float[WhisperCppInterop.whisper_lang_max_id()];

        fixed (float* pData = probs)
        {
            var state = WhisperCppInterop.whisper_init_state(currentWhisperContext);
            try
            {
                fixed (float* pSamples = samples)
                {
                    if (speedUp)
                    {
                        // whisper_pcm_to_mel_phase_vocoder is not yet exported from whisper.cpp
                        WhisperCppInterop.whisper_pcm_to_mel_phase_vocoder_with_state(currentWhisperContext, state, (IntPtr)pSamples, samples.Length, whisperParams.Threads);
                    }
                    else
                    {
                        WhisperCppInterop.whisper_pcm_to_mel_with_state(currentWhisperContext, state, (IntPtr)pSamples, samples.Length, whisperParams.Threads);
                    }
                }
                var langId = WhisperCppInterop.whisper_lang_auto_detect_with_state(currentWhisperContext, state, 0, whisperParams.Threads, (IntPtr)pData);
                if (langId == -1)
                {
                    return (null, 0f);
                }
                var languagePtr = WhisperCppInterop.whisper_lang_str(langId);
                var language = Marshal.PtrToStringAnsi(languagePtr);
                return (language, probs[langId]);
            }
            finally
            {
                WhisperCppInterop.whisper_free_state(state);
            }
        }
    }

    public void Process(Stream waveStream)
    {
        var waveParser = new WaveParser(waveStream);

        var samples = waveParser.GetAvgSamples();

        Process(samples);
    }

    public void Process(float[] samples)
    {
        Process(samples.AsSpan());
    }

    public unsafe void Process(ReadOnlySpan<float> samples)
    {
        if (isDisposed)
        {
            throw new ObjectDisposedException("This processor has already been disposed.");
        }

        fixed (float* pData = samples)
        {

            var state = WhisperCppInterop.whisper_init_state(currentWhisperContext);
            try
            {
                processingSemaphore.Wait();
                segmentIndex = 0;

                WhisperCppInterop.whisper_full_with_state(currentWhisperContext, state, whisperParams, (IntPtr)pData, samples.Length);
            }
            finally
            {
                WhisperCppInterop.whisper_free_state(state);
                processingSemaphore.Release();
            }
        }
    }

    public async IAsyncEnumerable<SegmentData> ProcessAsync(Stream waveStream, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var waveParser = new WaveParser(waveStream);
        var samples = await waveParser.GetAvgSamplesAsync(cancellationToken);
        await foreach (var segmentData in ProcessAsync(samples, cancellationToken))
        {
            yield return segmentData;
        }
    }

    public async IAsyncEnumerable<SegmentData> ProcessAsync(ReadOnlyMemory<float> samples, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var resetEvent = new AsyncAutoResetEvent();
        var buffer = new ConcurrentQueue<SegmentData>();

        void OnSegmentHandler(SegmentData segmentData)
        {
            buffer!.Enqueue(segmentData);
            resetEvent!.Set();
        }

        try
        {
            this.OnSegmentEventHandler += OnSegmentHandler;

            currentCancellationToken = cancellationToken;
            var whisperTask = ProcessInternalAsync(samples, cancellationToken)
                .ContinueWith(_ => resetEvent.Set(), cancellationToken, TaskContinuationOptions.None, TaskScheduler.Default);

            while (!whisperTask.IsCompleted || !buffer.IsEmpty)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (buffer.IsEmpty)
                {
                    await Task.WhenAny(whisperTask, resetEvent.WaitAsync())
                        .ConfigureAwait(false);
                }

                while (!buffer.IsEmpty && buffer.TryDequeue(out var segmentData))
                {
                    yield return segmentData;
                }
            }

            await whisperTask.ConfigureAwait(false);

            while (buffer.TryDequeue(out var segmentData))
            {
                yield return segmentData;
            }
        }
        finally
        {
            this.OnSegmentEventHandler -= OnSegmentHandler;
        }
    }

    public IAsyncEnumerable<SegmentData> ProcessAsync(float[] samples, CancellationToken cancellationToken = default)
    {
        return ProcessAsync(samples.AsMemory(), cancellationToken);
    }

    public void Dispose()
    {
        if (processingSemaphore.CurrentCount == 0)
        {
            throw new Exception("Cannot dispose while processing, please use DisposeAsync instead.");
        }

        processorInstances.TryRemove(myId, out _);
        if (language.HasValue)
        {
            Marshal.FreeHGlobal(language.Value);
            language = null;
        }

        if (initialPromptText.HasValue)
        {
            Marshal.FreeHGlobal(initialPromptText.Value);
            initialPromptText = null;
        }

        foreach (var gcHandle in gcHandles)
        {
            gcHandle.Free();
        }
        gcHandles.Clear();
        isDisposed = true;
    }

    private unsafe Task ProcessInternalAsync(ReadOnlyMemory<float> samples, CancellationToken cancellationToken)
    {
        if (isDisposed)
        {
            throw new ObjectDisposedException("This processor has already been disposed.");
        }
        return Task.Factory.StartNew(() =>
        {
            fixed (float* pData = samples.Span)
            {
                processingSemaphore.Wait();
                segmentIndex = 0;

                var state = WhisperCppInterop.whisper_init_state(currentWhisperContext);

                try
                {
                    WhisperCppInterop.whisper_full_with_state(currentWhisperContext, state, whisperParams, (IntPtr)pData, samples.Length);
                }
                finally
                {
                    WhisperCppInterop.whisper_free_state(state);
                    processingSemaphore.Release();
                }
            }
        }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private WhisperFullParams GetWhisperParams()
    {
        var strategy = options.SamplingStrategy.GetNativeStrategy();
        var whisperParamsRef = WhisperCppInterop.whisper_full_default_params_by_ref(strategy);
        var whisperParams = Marshal.PtrToStructure<WhisperFullParams>(whisperParamsRef);
        WhisperCppInterop.whisper_free_params(whisperParamsRef);
        whisperParams.Strategy = strategy;

        if (options.Threads.HasValue)
        {
            whisperParams.Threads = options.Threads.Value;
        }

        if (options.MaxLastTextTokens.HasValue)
        {
            whisperParams.MaxLastTextTokens = options.MaxLastTextTokens.Value;
        }

        if (options.Offset.HasValue)
        {
            whisperParams.OffsetMs = (int)options.Offset.Value.TotalMilliseconds;
        }

        if (options.Duration.HasValue)
        {
            whisperParams.DurationMs = (int)options.Duration.Value.TotalMilliseconds;
        }

        if (options.Translate.HasValue)
        {
            whisperParams.Translate = options.Translate.Value ? trueByte : falseByte;
        }

        if (options.NoContext.HasValue)
        {
            whisperParams.NoContext = options.NoContext.Value ? trueByte : falseByte;
        }

        if (options.SingleSegment.HasValue)
        {
            whisperParams.SingleSegment = options.SingleSegment.Value ? trueByte : falseByte;
        }

        if (options.PrintSpecialTokens.HasValue)
        {
            whisperParams.PrintSpecialTokens = options.PrintSpecialTokens.Value ? trueByte : falseByte;
        }

        if (options.PrintProgress.HasValue)
        {
            whisperParams.PrintProgress = options.PrintProgress.Value ? trueByte : falseByte;
        }

        if (options.PrintResults.HasValue)
        {
            whisperParams.PrintResults = options.PrintResults.Value ? trueByte : falseByte;
        }

        if (options.UseTokenTimestamps.HasValue)
        {
            whisperParams.UseTokenTimestamps = options.UseTokenTimestamps.Value ? trueByte : falseByte;
        }

        if (options.TokenTimestampsThreshold.HasValue)
        {
            whisperParams.TokenTimestampsThreshold = options.TokenTimestampsThreshold.Value;
        }

        if (options.TokenTimestampsSumThreshold.HasValue)
        {
            whisperParams.TokenTimestampsSumThreshold = options.TokenTimestampsSumThreshold.Value;
        }

        if (options.MaxSegmentLength.HasValue)
        {
            whisperParams.MaxSegmentLength = options.MaxSegmentLength.Value;
        }

        if (options.SplitOnWord.HasValue)
        {
            whisperParams.SplitOnWord = options.SplitOnWord.Value ? trueByte : falseByte;
        }

        if (options.MaxTokensPerSegment.HasValue)
        {
            whisperParams.MaxTokensPerSegment = options.MaxTokensPerSegment.Value;
        }

        if (options.SpeedUp2x.HasValue)
        {
            whisperParams.SpeedUp2x = options.SpeedUp2x.Value ? trueByte : falseByte;
        }

        if (options.AudioContextSize.HasValue)
        {
            whisperParams.AudioContextSize = options.AudioContextSize.Value;
        }
        
        if (options.TinyDiarizeSpeakerTurnDirection.HasValue)
        {
            whisperParams.TinyDiarizeSpeakerTurnDirection = options.TinyDiarizeSpeakerTurnDirection.Value ? trueByte : falseByte;
        }

        if (!string.IsNullOrEmpty(options.Prompt))
        {
            var tokenMaxLength = options.Prompt!.Length + 1;

            initialPromptText = Marshal.StringToHGlobalAnsi(options.Prompt);

            whisperParams.InitialPrompt = initialPromptText.Value;
        }

        if (options.Language != null)
        {
            language = Marshal.StringToHGlobalAnsi(options.Language);
            whisperParams.Language = language.Value;
        }

        if (options.SuppressBlank.HasValue)
        {
            whisperParams.SuppressBlank = options.SuppressBlank.Value ? trueByte : falseByte;
        }

        if (options.Temperature.HasValue)
        {
            whisperParams.Temperature = options.Temperature.Value;
        }

        if (options.MaxInitialTs.HasValue)
        {
            whisperParams.MaxInitialTs = options.MaxInitialTs.Value;
        }

        if (options.LengthPenalty.HasValue)
        {
            whisperParams.LengthPenalty = options.LengthPenalty.Value;
        }

        if (options.TemperatureInc.HasValue)
        {
            whisperParams.TemperatureInc = options.TemperatureInc.Value;
        }

        if (options.EntropyThreshold.HasValue)
        {
            whisperParams.EntropyThreshold = options.EntropyThreshold.Value;
        }

        if (options.LogProbThreshold.HasValue)
        {
            whisperParams.LogProbThreshold = options.LogProbThreshold.Value;
        }

        if (options.NoSpeechThreshold.HasValue)
        {
            whisperParams.NoSpeechThreshold = options.NoSpeechThreshold.Value;
        }

        if (options.SamplingStrategy is GreedySamplingStrategy greedySamplingStrategy)
        {
            if (greedySamplingStrategy.BestOf.HasValue)
            {
                whisperParams.WhisperParamGreedy.BestOf = greedySamplingStrategy.BestOf.Value;
            }
        }
        if (options.SamplingStrategy is BeamSearchSamplingStrategy beamSamplingStrategy)
        {
            if (beamSamplingStrategy.BeamSize.HasValue)
            {
                whisperParams.WhisperParamBeamSearch.BeamSize = beamSamplingStrategy.BeamSize.Value;
            }

            if (beamSamplingStrategy.Patience.HasValue)
            {
                whisperParams.WhisperParamBeamSearch.Patience = beamSamplingStrategy.Patience.Value;
            }
        }

        var myIntPtrId = new IntPtr(myId);
        whisperParams.OnNewSegmentUserData = myIntPtrId;
        whisperParams.OnEncoderBeginUserData = myIntPtrId;

        unsafe
        {
            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int, IntPtr, void> onNewSegmentDelegate = &OnNewSegmentStatic;
            whisperParams.OnNewSegment = (IntPtr)onNewSegmentDelegate;

            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, byte> onEncoderBeginDelegate = &OnEncoderBeginStatic;
            whisperParams.OnEncoderBegin = (IntPtr)onEncoderBeginDelegate;

            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int, IntPtr, void> onProgressDelegate = &OnProgressStatic;
            whisperParams.OnProgressCallback = (IntPtr)onProgressDelegate;
            whisperParams.OnProgressCallbackUserData = myIntPtrId;
        }

        return whisperParams;
    }

    private static string? GetAutodetectedLanguage(IntPtr state)
    {
        var detectedLanguageId = WhisperCppInterop.whisper_full_lang_id(state);
        if (detectedLanguageId == -1)
        {
            return null;
        }

        var languagePtr = WhisperCppInterop.whisper_lang_str(detectedLanguageId);
        var language = Marshal.PtrToStringAnsi(languagePtr);
        return language;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void OnNewSegmentStatic(IntPtr ctx, IntPtr state, int nNew, IntPtr userData)
    {
        if (!processorInstances.TryGetValue(userData.ToInt64(), out var processor))
        {
            throw new Exception("Couldn't find processor instance for user data");
        }
        processor.OnNewSegment(state);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static byte OnEncoderBeginStatic(IntPtr ctx, IntPtr state, IntPtr userData)
    {
        if (!processorInstances.TryGetValue(userData.ToInt64(), out var processor))
        {
            throw new Exception("Couldn't find processor instance for user data");
        }
        return processor.OnEncoderBegin() ? trueByte : falseByte;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void OnProgressStatic(IntPtr ctx, IntPtr state, int progress, IntPtr userData)
    {
        if (!processorInstances.TryGetValue(userData.ToInt64(), out var processor))
        {
            throw new Exception("Couldn't find processor instance for user data");
        }
        processor.OnProgress(progress);
    }

    private void OnProgress(int progress)
    {
        if (currentCancellationToken.HasValue && currentCancellationToken.Value.IsCancellationRequested)
        {
            return;
        }

        this.OnProgressHandler?.Invoke(progress);
    }

    private bool OnEncoderBegin()
    {
        if (currentCancellationToken.HasValue && currentCancellationToken.Value.IsCancellationRequested)
        {
            return false;
        }

        var encoderBeginArgs = new EncoderBeginData();
        var shouldContinue = this.OnEncoderBeginEventHandlers?.Invoke(encoderBeginArgs) ?? true;
        return shouldContinue;
    }

    private void OnNewSegment(IntPtr state)
    {
        if (currentCancellationToken.HasValue && currentCancellationToken.Value.IsCancellationRequested)
        {
            return;
        }

        var segments = WhisperCppInterop.whisper_full_n_segments_from_state(state);

        while (segmentIndex < segments)
        {
            var t1 = TimeSpan.FromMilliseconds(WhisperCppInterop.whisper_full_get_segment_t1_from_state(state, segmentIndex) * 10);
            var t0 = TimeSpan.FromMilliseconds(WhisperCppInterop.whisper_full_get_segment_t0_from_state(state, segmentIndex) * 10);
            var textAnsi = StringFromNativeUtf8(WhisperCppInterop.whisper_full_get_segment_text_from_state(state, segmentIndex));
            var speakerTurn = WhisperCppInterop.whisper_full_get_segment_speaker_turn_next_from_state(state, segmentIndex);
            float minimumProbability = 0;
            float maximumProbability = 0;
            double sumProbability = 0;
            var numberOfTokens = WhisperCppInterop.whisper_full_n_tokens_from_state(state, segmentIndex);
            var languageId = WhisperCppInterop.whisper_full_lang_id_from_state(state);
            var language = Marshal.PtrToStringAnsi(WhisperCppInterop.whisper_lang_str(languageId));

            if (options.ComputeProbabilities)
            {
                for (var tokenIndex = 0; tokenIndex < numberOfTokens; tokenIndex++)
                {
                    var tokenProbability = WhisperCppInterop.whisper_full_get_token_p_from_state(state, segmentIndex, tokenIndex);
                    sumProbability += tokenProbability;
                    if (tokenIndex == 0)
                    {
                        minimumProbability = tokenProbability;
                        maximumProbability = tokenProbability;
                        continue;
                    }
                    if (tokenProbability < minimumProbability)
                    {
                        minimumProbability = tokenProbability;
                    }

                    if (tokenProbability > maximumProbability)
                    {
                        maximumProbability = tokenProbability;
                    }
                }
            }

            if (!string.IsNullOrEmpty(textAnsi))
            {
                var eventHandlerArgs = new SegmentData(textAnsi, t0, t1, minimumProbability, maximumProbability, (float)(sumProbability / numberOfTokens), language!, speakerTurn);

                this.OnSegmentEventHandler?.Invoke(eventHandlerArgs);
                if (currentCancellationToken.HasValue && currentCancellationToken.Value.IsCancellationRequested)
                {
                    return;
                }
            }

            segmentIndex++;
        }
    }

    private static string StringFromNativeUtf8(IntPtr nativeUtf8)
    {

        return Marshal.PtrToStringUTF8(nativeUtf8) ?? string.Empty;
    }

    public async ValueTask DisposeAsync()
    {
        // If a processing is still running, wait for it to finish
        await processingSemaphore.WaitAsync();
        processingSemaphore.Release();
        Dispose();
    }
}
