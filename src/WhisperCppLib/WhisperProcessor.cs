// <copyright file="WhisperProcessor.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WhisperCppLib;

/// <summary>
/// Represents a Whisper processor.
/// </summary>
public class WhisperProcessor : IAsyncDisposable, IDisposable
{
    private static readonly ConcurrentDictionary<long, WhisperProcessor> ProcessorInstances = new();
    private static long currentProcessorId;
    private const byte TrueByte = 1;
    private const byte FalseByte = 0;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="WhisperProcessor"/> class.
    /// </summary>
    /// <param name="loader">The loader for the Whisper processor model.</param>
    /// <param name="options">The options for the Whisper processor.</param>
    public WhisperProcessor(IWhisperProcessorModelLoader loader, WhisperProcessorOptions options)
    {
        this.options = options;
        this.myId = Interlocked.Increment(ref currentProcessorId);

        ProcessorInstances[this.myId] = this;

        this.currentWhisperContext = loader.LoadNativeContext();
        this.whisperParams = this.GetWhisperParams();
        this.processingSemaphore = new(1);
    }

    /// <summary>
    /// Occurs when a new segment is detected.
    /// </summary>
    public event OnSegmentEventHandler? OnSegmentEventHandler;

    /// <summary>
    /// Occurs when the progress of the processor changes.
    /// </summary>
    public event OnProgressHandler? OnProgressHandler;

    /// <summary>
    /// Occurs when the encoder begins processing.
    /// </summary>
    public event OnEncoderBeginEventHandler? OnEncoderBeginEventHandlers;

    /// <summary>
    /// Changes the language used by the Whisper processor.
    /// </summary>
    /// <param name="newLanguage">The new language to use.</param>
    public void ChangeLanguage(string? newLanguage)
    {
        var oldLanguage = this.language;

        var newParams = this.whisperParams;
        if (string.IsNullOrEmpty(newLanguage))
        {
            newParams.Language = IntPtr.Zero;
        }
        else
        {
            this.language = Marshal.StringToHGlobalAnsi(newLanguage);
            newParams.Language = this.language.Value;
        }

        if (oldLanguage.HasValue)
        {
            Marshal.FreeHGlobal(oldLanguage.Value);
        }

        this.whisperParams = newParams;
    }

    /// <summary>
    /// Detects the language of the audio samples.
    /// </summary>
    /// <param name="samples">The audio samples.</param>
    /// <param name="speedUp">Indicates whether to use the speed up mode.</param>
    /// <returns>The detected language.</returns>
    public unsafe string? DetectLanguage(float[] samples, bool speedUp = false)
    {
        var (language, _) = this.DetectLanguageWithProbability(samples.AsSpan(), speedUp);
        return language;
    }

    /// <summary>
    /// Detects the language of the audio samples and returns the probability.
    /// </summary>
    /// <param name="samples">The audio samples.</param>
    /// <param name="speedUp">Indicates whether to use the speed up mode.</param>
    /// <returns>The detected language and its probability.</returns>
    public (string? language, float probability) DetectLanguageWithProbability(float[] samples, bool speedUp = false)
    {
        return this.DetectLanguageWithProbability(samples.AsSpan(), speedUp);
    }

    /// <summary>
    /// Detects the language of the audio samples and returns the probability.
    /// </summary>
    /// <param name="samples">The audio samples.</param>
    /// <param name="speedUp">Indicates whether to use the speed up mode.</param>
    /// <returns>The detected language and its probability.</returns>
    public unsafe (string? language, float probability) DetectLanguageWithProbability(ReadOnlySpan<float> samples, bool speedUp = false)
    {
        var probs = new float[WhisperCppInterop.whisper_lang_max_id()];

        fixed (float* pData = probs)
        {
            var state = WhisperCppInterop.whisper_init_state(this.currentWhisperContext);
            try
            {
                fixed (float* pSamples = samples)
                {
                    if (speedUp)
                    {
                        // whisper_pcm_to_mel_phase_vocoder is not yet exported from whisper.cpp
                        WhisperCppInterop.whisper_pcm_to_mel_phase_vocoder_with_state(this.currentWhisperContext, state, (IntPtr)pSamples, samples.Length, this.whisperParams.Threads);
                    }
                    else
                    {
                        WhisperCppInterop.whisper_pcm_to_mel_with_state(this.currentWhisperContext, state, (IntPtr)pSamples, samples.Length, this.whisperParams.Threads);
                    }
                }

                var langId = WhisperCppInterop.whisper_lang_auto_detect_with_state(this.currentWhisperContext, state, 0, this.whisperParams.Threads, (IntPtr)pData);
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

    /// <summary>
    /// Processes the audio stream.
    /// </summary>
    /// <param name="waveStream">The audio stream to process.</param>
    public void Process(Stream waveStream)
    {
        var waveParser = new WaveParser(waveStream);

        var samples = waveParser.GetAvgSamples();

        this.Process(samples);
    }

    /// <summary>
    /// Processes the audio samples.
    /// </summary>
    /// <param name="samples">The audio samples to process.</param>
    public void Process(float[] samples)
    {
        this.Process(samples.AsSpan());
    }

    /// <summary>
    /// Processes the audio samples.
    /// </summary>
    /// <param name="samples">The audio samples to process.</param>
    public unsafe void Process(ReadOnlySpan<float> samples)
    {
        if (this.isDisposed)
        {
            throw new ObjectDisposedException("This processor has already been disposed.");
        }

        fixed (float* pData = samples)
        {
            var state = WhisperCppInterop.whisper_init_state(this.currentWhisperContext);
            try
            {
                this.processingSemaphore.Wait();
                this.segmentIndex = 0;

                WhisperCppInterop.whisper_full_with_state(this.currentWhisperContext, state, this.whisperParams, (IntPtr)pData, samples.Length);
            }
            finally
            {
                WhisperCppInterop.whisper_free_state(state);
                this.processingSemaphore.Release();
            }
        }
    }

    /// <summary>
    /// Processes the audio stream asynchronously and yields segment data.
    /// </summary>
    /// <param name="waveStream">The audio stream to process.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous enumerable of segment data.</returns>
    public async IAsyncEnumerable<SegmentData> ProcessAsync(Stream waveStream, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var waveParser = new WaveParser(waveStream);
        var samples = await waveParser.GetAvgSamplesAsync(cancellationToken);
        await foreach (var segmentData in this.ProcessAsync(samples, cancellationToken))
        {
            yield return segmentData;
        }
    }

    /// <summary>
    /// Processes the audio stream asynchronously and yields segment data.
    /// </summary>
    /// <param name="samples">The audio samples to process.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous enumerable of segment data.</returns>
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

            this.currentCancellationToken = cancellationToken;
            var whisperTask = this.ProcessInternalAsync(samples, cancellationToken)
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
        return this.ProcessAsync(samples.AsMemory(), cancellationToken);
    }

    /// <summary>
    /// Disposes the processor.
    /// </summary>
    /// <exception cref="Exception">Thrown if can't be disposed.</exception>
    public void Dispose()
    {
        if (this.processingSemaphore.CurrentCount == 0)
        {
            throw new Exception("Cannot dispose while processing, please use DisposeAsync instead.");
        }

        ProcessorInstances.TryRemove(this.myId, out _);
        if (this.language.HasValue)
        {
            Marshal.FreeHGlobal(this.language.Value);
            this.language = null;
        }

        if (this.initialPromptText.HasValue)
        {
            Marshal.FreeHGlobal(this.initialPromptText.Value);
            this.initialPromptText = null;
        }

        foreach (var gcHandle in this.gcHandles)
        {
            gcHandle.Free();
        }

        this.gcHandles.Clear();
        this.isDisposed = true;
    }

    /// <summary>
    /// Processes the audio stream asynchronously and yields segment data.
    /// </summary>
    /// <param name="waveStream">The audio stream to process.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous enumerable of segment data.</returns>
    public async ValueTask DisposeAsync()
    {
        // If a processing is still running, wait for it to finish
        await this.processingSemaphore.WaitAsync();
        this.processingSemaphore.Release();
        this.Dispose();
    }

    private unsafe Task ProcessInternalAsync(ReadOnlyMemory<float> samples, CancellationToken cancellationToken)
    {
        if (this.isDisposed)
        {
            throw new ObjectDisposedException("This processor has already been disposed.");
        }

        return Task.Factory.StartNew(
            () =>
        {
            fixed (float* pData = samples.Span)
            {
                this.processingSemaphore.Wait();
                this.segmentIndex = 0;

                var state = WhisperCppInterop.whisper_init_state(this.currentWhisperContext);

                try
                {
                    WhisperCppInterop.whisper_full_with_state(this.currentWhisperContext, state, this.whisperParams, (IntPtr)pData, samples.Length);
                }
                finally
                {
                    WhisperCppInterop.whisper_free_state(state);
                    this.processingSemaphore.Release();
                }
            }
        }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private WhisperFullParams GetWhisperParams()
    {
        var strategy = this.options.SamplingStrategy.GetNativeStrategy();
        var whisperParamsRef = WhisperCppInterop.whisper_full_default_params_by_ref(strategy);
        var whisperParams = Marshal.PtrToStructure<WhisperFullParams>(whisperParamsRef);
        WhisperCppInterop.whisper_free_params(whisperParamsRef);
        whisperParams.Strategy = strategy;

        if (this.options.Threads.HasValue)
        {
            whisperParams.Threads = this.options.Threads.Value;
        }

        if (this.options.MaxLastTextTokens.HasValue)
        {
            whisperParams.MaxLastTextTokens = this.options.MaxLastTextTokens.Value;
        }

        if (this.options.Offset.HasValue)
        {
            whisperParams.OffsetMs = (int)this.options.Offset.Value.TotalMilliseconds;
        }

        if (this.options.Duration.HasValue)
        {
            whisperParams.DurationMs = (int)this.options.Duration.Value.TotalMilliseconds;
        }

        if (this.options.Translate.HasValue)
        {
            whisperParams.Translate = this.options.Translate.Value ? TrueByte : FalseByte;
        }

        if (this.options.NoContext.HasValue)
        {
            whisperParams.NoContext = this.options.NoContext.Value ? TrueByte : FalseByte;
        }

        if (this.options.SingleSegment.HasValue)
        {
            whisperParams.SingleSegment = this.options.SingleSegment.Value ? TrueByte : FalseByte;
        }

        if (this.options.PrintSpecialTokens.HasValue)
        {
            whisperParams.PrintSpecialTokens = this.options.PrintSpecialTokens.Value ? TrueByte : FalseByte;
        }

        if (this.options.PrintProgress.HasValue)
        {
            whisperParams.PrintProgress = this.options.PrintProgress.Value ? TrueByte : FalseByte;
        }

        if (this.options.PrintResults.HasValue)
        {
            whisperParams.PrintResults = this.options.PrintResults.Value ? TrueByte : FalseByte;
        }

        if (this.options.UseTokenTimestamps.HasValue)
        {
            whisperParams.UseTokenTimestamps = this.options.UseTokenTimestamps.Value ? TrueByte : FalseByte;
        }

        if (this.options.TokenTimestampsThreshold.HasValue)
        {
            whisperParams.TokenTimestampsThreshold = this.options.TokenTimestampsThreshold.Value;
        }

        if (this.options.TokenTimestampsSumThreshold.HasValue)
        {
            whisperParams.TokenTimestampsSumThreshold = this.options.TokenTimestampsSumThreshold.Value;
        }

        if (this.options.MaxSegmentLength.HasValue)
        {
            whisperParams.MaxSegmentLength = this.options.MaxSegmentLength.Value;
        }

        if (this.options.SplitOnWord.HasValue)
        {
            whisperParams.SplitOnWord = this.options.SplitOnWord.Value ? TrueByte : FalseByte;
        }

        if (this.options.MaxTokensPerSegment.HasValue)
        {
            whisperParams.MaxTokensPerSegment = this.options.MaxTokensPerSegment.Value;
        }

        if (this.options.SpeedUp2x.HasValue)
        {
            whisperParams.SpeedUp2x = this.options.SpeedUp2x.Value ? TrueByte : FalseByte;
        }

        if (this.options.AudioContextSize.HasValue)
        {
            whisperParams.AudioContextSize = this.options.AudioContextSize.Value;
        }

        if (this.options.TinyDiarizeSpeakerTurnDirection.HasValue)
        {
            whisperParams.TinyDiarizeSpeakerTurnDirection = this.options.TinyDiarizeSpeakerTurnDirection.Value ? TrueByte : FalseByte;
        }

        if (!string.IsNullOrEmpty(this.options.Prompt))
        {
            var tokenMaxLength = this.options.Prompt!.Length + 1;

            this.initialPromptText = Marshal.StringToHGlobalAnsi(this.options.Prompt);

            whisperParams.InitialPrompt = this.initialPromptText.Value;
        }

        if (this.options.Language != null)
        {
            this.language = Marshal.StringToHGlobalAnsi(this.options.Language);
            whisperParams.Language = this.language.Value;
        }

        if (this.options.SuppressBlank.HasValue)
        {
            whisperParams.SuppressBlank = this.options.SuppressBlank.Value ? TrueByte : FalseByte;
        }

        if (this.options.Temperature.HasValue)
        {
            whisperParams.Temperature = this.options.Temperature.Value;
        }

        if (this.options.MaxInitialTs.HasValue)
        {
            whisperParams.MaxInitialTs = this.options.MaxInitialTs.Value;
        }

        if (this.options.LengthPenalty.HasValue)
        {
            whisperParams.LengthPenalty = this.options.LengthPenalty.Value;
        }

        if (this.options.TemperatureInc.HasValue)
        {
            whisperParams.TemperatureInc = this.options.TemperatureInc.Value;
        }

        if (this.options.EntropyThreshold.HasValue)
        {
            whisperParams.EntropyThreshold = this.options.EntropyThreshold.Value;
        }

        if (this.options.LogProbThreshold.HasValue)
        {
            whisperParams.LogProbThreshold = this.options.LogProbThreshold.Value;
        }

        if (this.options.NoSpeechThreshold.HasValue)
        {
            whisperParams.NoSpeechThreshold = this.options.NoSpeechThreshold.Value;
        }

        if (this.options.SamplingStrategy is GreedySamplingStrategy greedySamplingStrategy)
        {
            if (greedySamplingStrategy.BestOf.HasValue)
            {
                whisperParams.WhisperParamGreedy.BestOf = greedySamplingStrategy.BestOf.Value;
            }
        }

        if (this.options.SamplingStrategy is BeamSearchSamplingStrategy beamSamplingStrategy)
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

        var myIntPtrId = new IntPtr(this.myId);
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

    private static string StringFromNativeUtf8(IntPtr nativeUtf8)
    {
        return Marshal.PtrToStringUTF8(nativeUtf8) ?? string.Empty;
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
        if (!ProcessorInstances.TryGetValue(userData.ToInt64(), out var processor))
        {
            throw new Exception("Couldn't find processor instance for user data");
        }

        processor.OnNewSegment(state);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static byte OnEncoderBeginStatic(IntPtr ctx, IntPtr state, IntPtr userData)
    {
        if (!ProcessorInstances.TryGetValue(userData.ToInt64(), out var processor))
        {
            throw new Exception("Couldn't find processor instance for user data");
        }

        return processor.OnEncoderBegin() ? TrueByte : FalseByte;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void OnProgressStatic(IntPtr ctx, IntPtr state, int progress, IntPtr userData)
    {
        if (!ProcessorInstances.TryGetValue(userData.ToInt64(), out var processor))
        {
            throw new Exception("Couldn't find processor instance for user data");
        }

        processor.OnProgress(progress);
    }

    private void OnProgress(int progress)
    {
        if (this.currentCancellationToken.HasValue && this.currentCancellationToken.Value.IsCancellationRequested)
        {
            return;
        }

        this.OnProgressHandler?.Invoke(progress);
    }

    private bool OnEncoderBegin()
    {
        if (this.currentCancellationToken.HasValue && this.currentCancellationToken.Value.IsCancellationRequested)
        {
            return false;
        }

        var encoderBeginArgs = new EncoderBeginData();
        var shouldContinue = this.OnEncoderBeginEventHandlers?.Invoke(encoderBeginArgs) ?? true;
        return shouldContinue;
    }

    private void OnNewSegment(IntPtr state)
    {
        if (this.currentCancellationToken.HasValue && this.currentCancellationToken.Value.IsCancellationRequested)
        {
            return;
        }

        var segments = WhisperCppInterop.whisper_full_n_segments_from_state(state);

        while (this.segmentIndex < segments)
        {
            var t1 = TimeSpan.FromMilliseconds(WhisperCppInterop.whisper_full_get_segment_t1_from_state(state, this.segmentIndex) * 10);
            var t0 = TimeSpan.FromMilliseconds(WhisperCppInterop.whisper_full_get_segment_t0_from_state(state, this.segmentIndex) * 10);
            var textAnsi = StringFromNativeUtf8(WhisperCppInterop.whisper_full_get_segment_text_from_state(state, this.segmentIndex));
            var speakerTurn = WhisperCppInterop.whisper_full_get_segment_speaker_turn_next_from_state(state, this.segmentIndex);
            float minimumProbability = 0;
            float maximumProbability = 0;
            double sumProbability = 0;
            var numberOfTokens = WhisperCppInterop.whisper_full_n_tokens_from_state(state, this.segmentIndex);
            var languageId = WhisperCppInterop.whisper_full_lang_id_from_state(state);
            var language = Marshal.PtrToStringAnsi(WhisperCppInterop.whisper_lang_str(languageId));

            if (this.options.ComputeProbabilities)
            {
                for (var tokenIndex = 0; tokenIndex < numberOfTokens; tokenIndex++)
                {
                    var tokenProbability = WhisperCppInterop.whisper_full_get_token_p_from_state(state, this.segmentIndex, tokenIndex);
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
                if (this.currentCancellationToken.HasValue && this.currentCancellationToken.Value.IsCancellationRequested)
                {
                    return;
                }
            }

            this.segmentIndex++;
        }
    }
}
