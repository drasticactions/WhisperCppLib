using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WhisperCppLib;

public static class WhisperCppInterop
{
    private const string libraryName = "whisper";

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr whisper_init_from_file_with_params(string path_model, WhisperContextParams parameters);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr whisper_init_from_buffer_with_params(IntPtr buffer, UIntPtr buffer_size, WhisperContextParams parameters);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr whisper_init_with_params(IntPtr loader, WhisperContextParams parameters);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr whisper_init_from_file_with_params_no_state(string path_model, WhisperContextParams parameters);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr whisper_init_from_buffer_with_params_no_state(IntPtr buffer, UIntPtr buffer_size, WhisperContextParams parameters);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr whisper_init_with_params_no_state(IntPtr loader, WhisperContextParams parameters);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void whisper_free(IntPtr context);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void whisper_free_params(IntPtr paramsPtr);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern WhisperFullParams whisper_full_default_params(WhisperSamplingStrategy strategy);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr whisper_full_default_params_by_ref(WhisperSamplingStrategy strategy);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_full(IntPtr context, WhisperFullParams parameters, IntPtr samples, int nSamples);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_full_with_state(IntPtr context, IntPtr state, WhisperFullParams parameters, IntPtr samples, int nSamples);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_full_parallel(IntPtr context, WhisperFullParams parameters, IntPtr samples, int nSamples, int nThreads);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_full_n_segments_from_state(IntPtr state);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern long whisper_full_get_segment_t0_from_state(IntPtr state, int index);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern long whisper_full_get_segment_t1_from_state(IntPtr state, int index);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr whisper_full_get_segment_text_from_state(IntPtr state, int index);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_full_n_tokens_from_state(IntPtr state, int index);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern float whisper_full_get_token_p_from_state(IntPtr state, int segmentIndex, int tokenIndex);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool whisper_full_get_segment_speaker_turn_next(IntPtr ctx, int iSegment);
    
    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool whisper_full_get_segment_speaker_turn_next_from_state(IntPtr state, int iSegment);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_tokenize(IntPtr context, IntPtr text, IntPtr tokens, int nMaxTokens);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_lang_max_id();

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_lang_auto_detect_with_state(IntPtr context, IntPtr state, int offset_ms, int n_threads, IntPtr lang_probs);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_pcm_to_mel_with_state(IntPtr context, IntPtr state, IntPtr samples, int nSamples, int nThreads);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_pcm_to_mel_phase_vocoder_with_state(IntPtr context, IntPtr state, IntPtr samples, int nSamples, int nThreads);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr whisper_lang_str(int lang_id);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_full_lang_id(IntPtr context);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr whisper_init_state(IntPtr context);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr whisper_ctx_init_openvino_encoder(IntPtr context, string path, string device, string cacheDir);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void whisper_free_state(IntPtr state);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_full_lang_id_from_state(IntPtr state);
}

// Define the structure for whisper_context_params
[StructLayout(LayoutKind.Sequential)]
public struct WhisperContextParams
{
    [MarshalAs(UnmanagedType.U1)]
    public bool use_gpu;
    public int gpu_device;
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

    //force single segment output (useful for streaming)
    public byte SingleSegment;

    // print special tokens (e.g. <SOT>, <EOT>, <BEG>, etc.)
    public byte PrintSpecialTokens;

    //print progress information
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
    public UIntPtr i_start_rule;
    public float grammar_penalty;
}


public enum WhisperGreType
{
    WhisperGreTypeEnd = 0,
    WhisperGreTypeAlt = 1,
    WhisperGreTypeRuleRef = 2,
    WhisperGreTypeChar = 3,
    WhisperGreTypeCharNot = 4,
    WhisperGreTypeCharRngUpper = 5,
    WhisperGreTypeCharAlt = 6
}

[StructLayout(LayoutKind.Sequential)]
public struct WhisperGrammarElement
{
    public WhisperGreType type;
    public uint value;
}

[StructLayout(LayoutKind.Sequential)]
public struct WhisperContext
{
}
