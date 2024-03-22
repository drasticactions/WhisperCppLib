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
    /// Run the command.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public virtual Task RunAsync()
    {
        return Task.CompletedTask;
    }
}
