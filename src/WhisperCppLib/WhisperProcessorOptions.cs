// <copyright file="WhisperProcessorOptions.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

namespace WhisperCppLib;

/// <summary>
/// Represents the options for the WhisperProcessor.
/// </summary>
public class WhisperProcessorOptions
{
    /// <summary>
    /// Gets or sets the sampling strategy used by the WhisperProcessor.
    /// </summary>
    public IWhisperSamplingStrategy SamplingStrategy { get; set; } = new GreedySamplingStrategy();

    /// <summary>
    /// Gets or sets the number of threads to be used by the WhisperProcessor.
    /// </summary>
    public int? Threads { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of last text tokens to be considered by the WhisperProcessor.
    /// </summary>
    public int? MaxLastTextTokens { get; set; }

    /// <summary>
    /// Gets or sets the offset of the audio to be processed by the WhisperProcessor.
    /// </summary>
    public TimeSpan? Offset { get; set; }

    /// <summary>
    /// Gets or sets the duration of the audio to be processed by the WhisperProcessor.
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the output should be translated.
    /// </summary>
    public bool? Translate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the context should be ignored by the WhisperProcessor.
    /// </summary>
    public bool? NoContext { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the audio should be processed as a single segment.
    /// </summary>
    public bool? SingleSegment { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the special tokens should be printed in the output.
    /// </summary>
    public bool? PrintSpecialTokens { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the progress should be printed during processing.
    /// </summary>
    public bool? PrintProgress { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the results should be printed after processing.
    /// </summary>
    public bool? PrintResults { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the timestamps should be printed in the output.
    /// </summary>
    public bool? PrintTimestamps { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the token timestamps should be used.
    /// </summary>
    public bool? UseTokenTimestamps { get; set; }

    /// <summary>
    /// Gets or sets the threshold for token timestamps.
    /// </summary>
    public float? TokenTimestampsThreshold { get; set; }

    /// <summary>
    /// Gets or sets the threshold for the sum of token timestamps.
    /// </summary>
    public float? TokenTimestampsSumThreshold { get; set; }

    /// <summary>
    /// Gets or sets the maximum length of a segment.
    /// </summary>
    public int? MaxSegmentLength { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the audio should be split on word boundaries.
    /// </summary>
    public bool? SplitOnWord { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of tokens per segment.
    /// </summary>
    public int? MaxTokensPerSegment { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the processing should be speeded up by 2x.
    /// </summary>
    public bool? SpeedUp2x { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the tiny diarization speaker turn direction should be used.
    /// </summary>
    public bool? TinyDiarizeSpeakerTurnDirection { get; set; }

    /// <summary>
    /// Gets or sets the size of the audio context.
    /// </summary>
    public int? AudioContextSize { get; set; }

    /// <summary>
    /// Gets or sets the prompt to be used by the WhisperProcessor.
    /// </summary>
    public string? Prompt { get; set; }

    /// <summary>
    /// Gets or sets the language to be used by the WhisperProcessor.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether blank outputs should be suppressed.
    /// </summary>
    public bool? SuppressBlank { get; set; }

    /// <summary>
    /// Gets or sets the temperature value to be used by the WhisperProcessor.
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// Gets or sets the maximum initial timestamp value to be used by the WhisperProcessor.
    /// </summary>
    public float? MaxInitialTs { get; set; }

    /// <summary>
    /// Gets or sets the length penalty value to be used by the WhisperProcessor.
    /// </summary>
    public float? LengthPenalty { get; set; }

    /// <summary>
    /// Gets or sets the temperature increment value to be used by the WhisperProcessor.
    /// </summary>
    public float? TemperatureInc { get; set; }

    /// <summary>
    /// Gets or sets the entropy threshold value to be used by the WhisperProcessor.
    /// </summary>
    public float? EntropyThreshold { get; set; }

    /// <summary>
    /// Gets or sets the log probability threshold value to be used by the WhisperProcessor.
    /// </summary>
    public float? LogProbThreshold { get; set; }

    /// <summary>
    /// Gets or sets the no speech threshold value to be used by the WhisperProcessor.
    /// </summary>
    public float? NoSpeechThreshold { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the probabilities should be computed by the WhisperProcessor.
    /// </summary>
    public bool ComputeProbabilities { get; set; }
}
