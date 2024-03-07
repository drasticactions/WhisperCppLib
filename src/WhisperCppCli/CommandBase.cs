// <copyright file="CommandBase.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using DotMake.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WhisperCppLib;

/// <summary>
/// Base Command.
/// </summary>
public abstract class CommandBase(ILoggerFactory loggerFactory)
{
    /// <summary>
    /// Gets or sets the Whisper model.
    /// </summary>
    [CliOption(Description = "Whisper model")]
    required public string Model { get; set; }

    /// <summary>
    /// Gets or sets the Input File.
    /// </summary>
    [CliOption(Description = "Input File")]
    required public string InputFile { get; set; }

    /// <summary>
    /// Run the command.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public virtual async Task RunAsync()
    {
    }
}
