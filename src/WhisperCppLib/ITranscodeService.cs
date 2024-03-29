﻿// <copyright file="ITranscodeService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

namespace WhisperCppLib;

/// <summary>
/// Transcode Service.
/// </summary>
public interface ITranscodeService : IDisposable
{
    /// <summary>
    /// Gets the base path.
    /// </summary>
    string BasePath { get; }

    /// <summary>
    /// Process file.
    /// </summary>
    /// <param name="filePath">File Path.</param>
    /// <returns>Path to transcoded file.</returns>
    Task<string> ProcessFile(string filePath);
}
