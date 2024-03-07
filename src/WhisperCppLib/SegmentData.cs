// <copyright file="SegmentData.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

namespace WhisperCppLib;

/// <summary>
/// Represents the data of a segment.
/// </summary>
public class SegmentData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SegmentData"/> class.
    /// </summary>
    /// <param name="text">The text of the segment.</param>
    /// <param name="start">The time when the segment started.</param>
    /// <param name="end">The time when the segment ended.</param>
    /// <param name="minProbability">The minimum probability found in any of the tokens.</param>
    /// <param name="maxProbability">The maximum probability found in any of the tokens.</param>
    /// <param name="probability">The average probability of the segment.</param>
    /// <param name="language">The language of the current segment.</param>
    /// <param name="speakerTurn">A value indicating whether the speaker changed in this segment.</param>
    public SegmentData(string text, TimeSpan start, TimeSpan end, float minProbability, float maxProbability, float probability, string language, bool speakerTurn)
    {
        this.Text = text;
        this.Start = start;
        this.End = end;
        this.MinProbability = minProbability;
        this.MaxProbability = maxProbability;
        this.Probability = probability;
        this.Language = language;
        this.SpeakerTurn = speakerTurn;
    }

    /// <summary>
    /// Gets the text of the segment.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the time when the segment started.
    /// </summary>
    public TimeSpan Start { get; }

    /// <summary>
    /// Gets the time when the segment ended.
    /// </summary>
    public TimeSpan End { get; }

    /// <summary>
    /// Gets the minimum probability found in any of the tokens.
    /// </summary>
    /// <remarks>
    /// The possible values are from 0 to 1.
    /// </remarks>
    public float MinProbability { get; }

    /// <summary>
    /// Gets the maximum probability found in any of the tokens.
    /// </summary>
    /// <remarks>
    /// The possible values are from 0 to 1.
    /// </remarks>
    public float MaxProbability { get; }

    /// <summary>
    /// Gets the average probability of the segment.
    /// </summary>
    /// <remarks>
    /// The possible values are from 0 to 1.
    /// </remarks>
    public float Probability { get; }

    /// <summary>
    /// Gets the language of the current segment.
    /// </summary>
    public string Language { get; }

    /// <summary>
    /// Gets a value indicating whether the speaker changed in this segment.
    /// </summary>
    public bool SpeakerTurn { get; }
}

public delegate void OnProgressHandler(int progress);

public delegate void OnSegmentEventHandler(SegmentData e);

public delegate bool OnEncoderBeginEventHandler(EncoderBeginData e);
