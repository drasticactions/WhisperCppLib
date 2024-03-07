﻿// <copyright file="CorruptedWaveException.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

namespace WhisperCppLib;

/// <summary>
/// Represents an exception that is thrown when a wave file is corrupted.
/// </summary>
public class CorruptedWaveException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CorruptedWaveException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public CorruptedWaveException(string? message)
        : base(message)
    {
    }
}
