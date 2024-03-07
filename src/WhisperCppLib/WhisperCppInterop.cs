// <copyright file="WhisperCppInterop.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WhisperCppLib;

/// <summary>
/// Interop class for WhisperCppLib.
/// </summary>
public static class WhisperCppInterop
{
    /// <summary>
    /// The name of the library.
    /// </summary>
    private const string LibraryName = "whisper";

    /// <summary>
    /// Initializes the Whisper context from a file with parameters.
    /// </summary>
    /// <param name="path_model">The path to the model file.</param>
    /// <param name="parameters">The context parameters.</param>
    /// <returns>The initialized Whisper context.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr Whisper_init_from_file_with_params(string path_model, WhisperContextParams parameters);

    /// <summary>
    /// Initializes the Whisper context from a buffer with parameters.
    /// </summary>
    /// <param name="buffer">The buffer containing the model data.</param>
    /// <param name="buffer_size">The size of the buffer.</param>
    /// <param name="parameters">The context parameters.</param>
    /// <returns>The initialized Whisper context.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr Whisper_init_from_buffer_with_params(IntPtr buffer, UIntPtr buffer_size, WhisperContextParams parameters);

    /// <summary>
    /// Initializes the Whisper context with parameters.
    /// </summary>
    /// <param name="loader">The loader.</param>
    /// <param name="parameters">The context parameters.</param>
    /// <returns>The initialized Whisper context.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr Whisper_init_with_params(IntPtr loader, WhisperContextParams parameters);

    /// <summary>
    /// Initializes the Whisper context from a file with parameters without state.
    /// </summary>
    /// <param name="path_model">The path to the model file.</param>
    /// <param name="parameters">The context parameters.</param>
    /// <returns>The initialized Whisper context.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr Whisper_init_from_file_with_params_no_state(string path_model, WhisperContextParams parameters);

    /// <summary>
    /// Initializes the Whisper context from a buffer with parameters without state.
    /// </summary>
    /// <param name="buffer">The buffer containing the model data.</param>
    /// <param name="buffer_size">The size of the buffer.</param>
    /// <param name="parameters">The context parameters.</param>
    /// <returns>The initialized Whisper context.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr Whisper_init_from_buffer_with_params_no_state(IntPtr buffer, UIntPtr buffer_size, WhisperContextParams parameters);

    /// <summary>
    /// Initializes the Whisper context with parameters without state.
    /// </summary>
    /// <param name="loader">The loader.</param>
    /// <param name="parameters">The context parameters.</param>
    /// <returns>The initialized Whisper context.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr Whisper_init_with_params_no_state(IntPtr loader, WhisperContextParams parameters);

    /// <summary>
    /// Frees the Whisper context.
    /// </summary>
    /// <param name="context">The Whisper context to free.</param>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void Whisper_free(IntPtr context);

    /// <summary>
    /// Frees the Whisper context parameters.
    /// </summary>
    /// <param name="paramsPtr">The pointer to the context parameters.</param>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void Whisper_free_params(IntPtr paramsPtr);

    /// <summary>
    /// Gets the default Whisper full parameters for the specified sampling strategy.
    /// </summary>
    /// <param name="strategy">The sampling strategy.</param>
    /// <returns>The default Whisper full parameters.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern WhisperFullParams Whisper_full_default_params(WhisperSamplingStrategy strategy);

    /// <summary>
    /// Gets the default Whisper full parameters for the specified sampling strategy by reference.
    /// </summary>
    /// <param name="strategy">The sampling strategy.</param>
    /// <returns>The pointer to the default Whisper full parameters.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr Whisper_full_default_params_by_ref(WhisperSamplingStrategy strategy);

    /// <summary>
    /// Performs full Whisper transcription.
    /// </summary>
    /// <param name="context">The Whisper context.</param>
    /// <param name="parameters">The full parameters.</param>
    /// <param name="samples">The audio samples.</param>
    /// <param name="nSamples">The number of audio samples.</param>
    /// <returns>The number of segments.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int Whisper_full(IntPtr context, WhisperFullParams parameters, IntPtr samples, int nSamples);

    /// <summary>
    /// Performs full Whisper transcription with state.
    /// </summary>
    /// <param name="context">The Whisper context.</param>
    /// <param name="state">The Whisper state.</param>
    /// <param name="parameters">The full parameters.</param>
    /// <param name="samples">The audio samples.</param>
    /// <param name="nSamples">The number of audio samples.</param>
    /// <returns>The number of segments.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int Whisper_full_with_state(IntPtr context, IntPtr state, WhisperFullParams parameters, IntPtr samples, int nSamples);

    /// <summary>
    /// Performs parallel full Whisper transcription.
    /// </summary>
    /// <param name="context">The Whisper context.</param>
    /// <param name="parameters">The full parameters.</param>
    /// <param name="samples">The audio samples.</param>
    /// <param name="nSamples">The number of audio samples.</param>
    /// <param name="nThreads">The number of threads.</param>
    /// <returns>The number of segments.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int Whisper_full_parallel(IntPtr context, WhisperFullParams parameters, IntPtr samples, int nSamples, int nThreads);

    /// <summary>
    /// Gets the number of segments from the Whisper state.
    /// </summary>
    /// <param name="state">The Whisper state.</param>
    /// <returns>The number of segments.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int Whisper_full_n_segments_from_state(IntPtr state);

    /// <summary>
    /// Gets the start time of the segment from the Whisper state.
    /// </summary>
    /// <param name="state">The Whisper state.</param>
    /// <param name="index">The index of the segment.</param>
    /// <returns>The start time of the segment.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern long Whisper_full_get_segment_t0_from_state(IntPtr state, int index);

    /// <summary>
    /// Gets the end time of the segment from the Whisper state.
    /// </summary>
    /// <param name="state">The Whisper state.</param>
    /// <param name="index">The index of the segment.</param>
    /// <returns>The end time of the segment.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern long Whisper_full_get_segment_t1_from_state(IntPtr state, int index);

    /// <summary>
    /// Gets the text of the segment from the Whisper state.
    /// </summary>
    /// <param name="state">The Whisper state.</param>
    /// <param name="index">The index of the segment.</param>
    /// <returns>The text of the segment.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr Whisper_full_get_segment_text_from_state(IntPtr state, int index);

    /// <summary>
    /// Gets the number of tokens in the segment from the Whisper state.
    /// </summary>
    /// <param name="state">The Whisper state.</param>
    /// <param name="index">The index of the segment.</param>
    /// <returns>The number of tokens in the segment.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int Whisper_full_n_tokens_from_state(IntPtr state, int index);

    /// <summary>
    /// Gets the probability of the token in the segment from the Whisper state.
    /// </summary>
    /// <param name="state">The Whisper state.</param>
    /// <param name="segmentIndex">The index of the segment.</param>
    /// <param name="tokenIndex">The index of the token.</param>
    /// <returns>The probability of the token.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern float Whisper_full_get_token_p_from_state(IntPtr state, int segmentIndex, int tokenIndex);

    /// <summary>
    /// Gets the speaker turn of the segment from the Whisper context.
    /// </summary>
    /// <param name="ctx">The Whisper context.</param>
    /// <param name="iSegment">The index of the segment.</param>
    /// <returns>True if it is the speaker turn, otherwise false.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool Whisper_full_get_segment_speaker_turn_next(IntPtr ctx, int iSegment);

    /// <summary>
    /// Gets the speaker turn of the segment from the Whisper state.
    /// </summary>
    /// <param name="state">The Whisper state.</param>
    /// <param name="iSegment">The index of the segment.</param>
    /// <returns>True if it is the speaker turn, otherwise false.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool Whisper_full_get_segment_speaker_turn_next_from_state(IntPtr state, int iSegment);

    /// <summary>
    /// Tokenizes the text using the Whisper context.
    /// </summary>
    /// <param name="context">The Whisper context.</param>
    /// <param name="text">The text to tokenize.</param>
    /// <param name="tokens">The output tokens.</param>
    /// <param name="nMaxTokens">The maximum number of tokens.</param>
    /// <returns>The number of tokens.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int Whisper_tokenize(IntPtr context, IntPtr text, IntPtr tokens, int nMaxTokens);

    /// <summary>
    /// Gets the maximum language ID.
    /// </summary>
    /// <returns>The maximum language ID.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int Whisper_lang_max_id();

    /// <summary>
    /// Auto detects the language using the Whisper context and state.
    /// </summary>
    /// <param name="context">The Whisper context.</param>
    /// <param name="state">The Whisper state.</param>
    /// <param name="offset_ms">The offset in milliseconds.</param>
    /// <param name="n_threads">The number of threads.</param>
    /// <param name="lang_probs">The language probabilities.</param>
    /// <returns>The detected language ID.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int Whisper_lang_auto_detect_with_state(IntPtr context, IntPtr state, int offset_ms, int n_threads, IntPtr lang_probs);

    /// <summary>
    /// Converts PCM audio to mel spectrogram using the Whisper context and state.
    /// </summary>
    /// <param name="context">The Whisper context.</param>
    /// <param name="state">The Whisper state.</param>
    /// <param name="samples">The audio samples.</param>
    /// <param name="nSamples">The number of audio samples.</param>
    /// <param name="nThreads">The number of threads.</param>
    /// <returns>The status code.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int Whisper_pcm_to_mel_with_state(IntPtr context, IntPtr state, IntPtr samples, int nSamples, int nThreads);

    /// <summary>
    /// Converts PCM audio to mel spectrogram using the Whisper context and state with phase vocoder.
    /// </summary>
    /// <param name="context">The Whisper context.</param>
    /// <param name="state">The Whisper state.</param>
    /// <param name="samples">The audio samples.</param>
    /// <param name="nSamples">The number of audio samples.</param>
    /// <param name="nThreads">The number of threads.</param>
    /// <returns>The status code.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int Whisper_pcm_to_mel_phase_vocoder_with_state(IntPtr context, IntPtr state, IntPtr samples, int nSamples, int nThreads);

    /// <summary>
    /// Gets the language string for the specified language ID.
    /// </summary>
    /// <param name="lang_id">The language ID.</param>
    /// <returns>The language string.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr Whisper_lang_str(int lang_id);

    /// <summary>
    /// Gets the language ID from the Whisper context.
    /// </summary>
    /// <param name="context">The Whisper context.</param>
    /// <returns>The language ID.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int Whisper_full_lang_id(IntPtr context);

    /// <summary>
    /// Initializes the Whisper state from the Whisper context.
    /// </summary>
    /// <param name="context">The Whisper context.</param>
    /// <returns>The initialized Whisper state.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr Whisper_init_state(IntPtr context);

    /// <summary>
    /// Initializes the OpenVINO encoder in the Whisper context.
    /// </summary>
    /// <param name="context">The Whisper context.</param>
    /// <param name="path">The path to the encoder.</param>
    /// <param name="device">The device to use.</param>
    /// <param name="cacheDir">The cache directory.</param>
    /// <returns>The initialized Whisper context.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr Whisper_ctx_init_openvino_encoder(IntPtr context, string path, string device, string cacheDir);

    /// <summary>
    /// Frees the Whisper state.
    /// </summary>
    /// <param name="state">The Whisper state to free.</param>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void Whisper_free_state(IntPtr state);

    /// <summary>
    /// Gets the language ID from the Whisper state.
    /// </summary>
    /// <param name="state">The Whisper state.</param>
    /// <returns>The language ID.</returns>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int Whisper_full_lang_id_from_state(IntPtr state);
}

// Define the structure for whisper_context_params
[StructLayout(LayoutKind.Sequential)]
public struct WhisperContextParams
{
    [MarshalAs(UnmanagedType.U1)]
    public bool UseGpu;
    public int GpuDevice;
}

public enum WhisperSamplingStrategy
{
    StrategyGreedy,      // GreedyDecoder
    StrategyBeamSearch, // BeamSearchDecoder
}

[StructLayout(LayoutKind.Sequential)]
public struct WhisperParamGreedy
{
    // ref: https://github.com/openai/whisper/blob/f82bc59f5ea234d4b97fb2860842ed38519f7e65/whisper/transcribe.py#L264
    public int BestOf;
}

[StructLayout(LayoutKind.Sequential)]
public struct WhisperParamBeamSearch
{
    public int BeamSize;

    // Note: not implemented, ref: https://arxiv.org/pdf/2204.05424.pdf
    public float Patience;
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void WhisperNewSegmentCallback(IntPtr ctx, IntPtr state, int n_new, IntPtr user_data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate byte WhisperEncoderBeginCallback(IntPtr ctx, IntPtr state, IntPtr user_data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void WhisperProgressCallback(IntPtr ctx, IntPtr state, int progress, IntPtr user_data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool WhisperLogitsFilterCallback(IntPtr ctx, IntPtr state, IntPtr tokens, int tokens_count, IntPtr logits, IntPtr user_data);

[StructLayout(LayoutKind.Sequential)]
public struct WhisperFullParams
{
    public WhisperSamplingStrategy Strategy;

    public int Threads;

    // max tokens to use from past text as prompt for the decoder
    public int MaxLastTextTokens;

    // start offset in ms
    public int OffsetMs;

    // audio duration to process in ms
    public int DurationMs;

    public byte Translate;

    // Do not use past transcription (if any) as prompt for the decoder
    public byte NoContext;

    public byte NoTimestamps;

    // force single segment output (useful for streaming)
    public byte SingleSegment;

    // print special tokens (e.g. <SOT>, <EOT>, <BEG>, etc.)
    public byte PrintSpecialTokens;

    // print progress information
    public byte PrintProgress;

    // print results from within whisper.cpp (avoid it, use callback instead)
    public byte PrintResults;

    // print timestamps for each text segment when printing real-time
    public byte PrintTimestamps;

    // [EXPERIMENTAL] token-level timestamps
    // enable token-level timestamps
    public byte UseTokenTimestamps;

    // timestamp token probability threshold (~0.01)
    public float TokenTimestampsThreshold;

    // timestamp token sum probability threshold (~0.01)
    public float TokenTimestampsSumThreshold;

    // max segment length in characters
    public int MaxSegmentLength;

    public byte SplitOnWord;

    // max tokens per segment (0 = no limit)
    public int MaxTokensPerSegment;

    // [EXPERIMENTAL] speed-up techniques
    // note: these can significantly reduce the quality of the output
    // speed-up the audio by 2x using Phase Vocoder
    public byte SpeedUp2x;

    // enable debug_mode provides extra info (eg. Dump log_mel)
    public byte DebugMode;

    // overwrite the audio context size (0 = use default)
    public int AudioContextSize;

    // [EXPERIMENTAL] [TDRZ] tinydiarize
    // enable tinydiarize speaker turn detection
    public byte TinyDiarizeSpeakerTurnDirection;

    public IntPtr InitialPrompt;

    // tokens to provide to the whisper decoder as initial prompt
    // these are prepended to any existing text context from a previous call
    public IntPtr PromptTokens;

    public int PromptNTokens;

    // for auto-detection, set to nullptr, "" or "auto"
    public IntPtr Language;

    // Will end the pipeline after detecting the language. Not used by whisper.net
    public byte DetectLanguage;

    // common decoding parameters:
    public byte SuppressBlank;

    // suppress non-speech tokens (e.g. `,`, `.`, etc.)
    public byte SupressNonSpeechTokens;

    // common decoding parameters:
    // ref: https://github.com/openai/whisper/blob/f82bc59f5ea234d4b97fb2860842ed38519f7e65/whisper/decoding.py#L89
    public float Temperature;

    // ref: https://github.com/openai/whisper/blob/f82bc59f5ea234d4b97fb2860842ed38519f7e65/whisper/decoding.py#L97
    public float MaxInitialTs;

    // ref: https://github.com/openai/whisper/blob/f82bc59f5ea234d4b97fb2860842ed38519f7e65/whisper/transcribe.py#L267
    public float LengthPenalty;

    // fallback parameters
    // ref: https://github.com/openai/whisper/blob/f82bc59f5ea234d4b97fb2860842ed38519f7e65/whisper/transcribe.py#L274-L278
    public float TemperatureInc;

    public float EntropyThreshold;

    public float LogProbThreshold;

    // Note: not implemented yet.
    public float NoSpeechThreshold;

    public WhisperParamGreedy WhisperParamGreedy;

    public WhisperParamBeamSearch WhisperParamBeamSearch;

    public IntPtr OnNewSegment;

    public IntPtr OnNewSegmentUserData;

    public IntPtr OnProgressCallback;

    public IntPtr OnProgressCallbackUserData;

    public IntPtr OnEncoderBegin;

    public IntPtr OnEncoderBeginUserData;

    public IntPtr LogitsFilterCallback;

    public IntPtr LogitsFilterCallbackData;

    public WhisperGrammarElement GrammarRules;
    public UIntPtr NGrammerRules;
    public UIntPtr IStartRule;
    public float GrammarPenalty;
}

public enum WhisperGreType
{
    WhisperGreTypeEnd = 0,
    WhisperGreTypeAlt = 1,
    WhisperGreTypeRuleRef = 2,
    WhisperGreTypeChar = 3,
    WhisperGreTypeCharNot = 4,
    WhisperGreTypeCharRngUpper = 5,
    WhisperGreTypeCharAlt = 6,
}

[StructLayout(LayoutKind.Sequential)]
public struct WhisperGrammarElement
{
    public WhisperGreType Type;
    public uint Value;
}

[StructLayout(LayoutKind.Sequential)]
public struct WhisperContext
{
}
