// <copyright file="IWhisperSamplingStrategy.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhisperCppLib;

/// <summary>
/// Represents a sampling strategy for Whisper.
/// </summary>
public interface IWhisperSamplingStrategy
{
    /// <summary>
    /// Gets the native sampling strategy for Whisper.
    /// </summary>
    /// <returns>The native sampling strategy.</returns>
    WhisperSamplingStrategy GetNativeStrategy();
}
