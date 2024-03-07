// <copyright file="BeamSearchSamplingStrategy.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

namespace WhisperCppLib;

/// <summary>
/// Represents a sampling strategy using beam search.
/// </summary>
public class BeamSearchSamplingStrategy : IWhisperSamplingStrategy
{
    /// <summary>
    /// Gets the native sampling strategy.
    /// </summary>
    /// <returns>The native sampling strategy.</returns>
    public WhisperSamplingStrategy GetNativeStrategy()
    {
        return WhisperSamplingStrategy.StrategyBeamSearch;
    }

    /// <summary>
    /// Gets or sets the beam size.
    /// </summary>
    public int? BeamSize { get; set; }

    /// <summary>
    /// Gets or sets the patience.
    /// </summary>
    public float? Patience { get; set; }
}
