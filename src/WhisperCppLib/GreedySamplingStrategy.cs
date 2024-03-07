// <copyright file="GreedySamplingStrategy.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

namespace WhisperCppLib;

/// <summary>
/// Represents a greedy sampling strategy.
/// </summary>
public class GreedySamplingStrategy : IWhisperSamplingStrategy
{
    /// <summary>
    /// Gets or sets the number of best samples to select.
    /// </summary>
    public int? BestOf { get; set; }

    /// <summary>
    /// Gets the native sampling strategy.
    /// </summary>
    /// <returns>The native sampling strategy.</returns>
    public WhisperSamplingStrategy GetNativeStrategy()
    {
        return WhisperSamplingStrategy.StrategyGreedy;
    }
}
