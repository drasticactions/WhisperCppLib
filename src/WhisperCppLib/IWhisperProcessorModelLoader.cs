// <copyright file="IWhisperProcessorModelLoader.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

namespace WhisperCppLib;

/// <summary>
/// Represents an interface for loading the native context of a Whisper processor model.
/// </summary>
public interface IWhisperProcessorModelLoader : IDisposable
{
    /// <summary>
    /// Loads the native context of the Whisper processor model.
    /// </summary>
    /// <returns>The pointer to the native context.</returns>
    public IntPtr LoadNativeContext();
}
