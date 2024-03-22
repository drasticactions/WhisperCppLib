// <copyright file="Program.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using DotMake.CommandLine;
using Drastic.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WhisperCppCli;
using WhisperCppGui.Services;

#if DEBUG
var loggerFactory = LoggerFactory.Create(
    builder => builder
        .AddConsole()
        .AddDebug()
        .SetMinimumLevel(LogLevel.Debug));
#else
var loggerFactory = LoggerFactory.Create(
    builder => builder
        .AddDebug()
        .SetMinimumLevel(LogLevel.Debug));
#endif

var logger = loggerFactory.CreateLogger<RootCommand>();

Cli.Ext.ConfigureServices(service =>
{
    service.AddSingleton<IAppDispatcher, ConsoleAppDispatcher>();
    service.AddSingleton<IErrorHandlerService, ConsoleErrorHandlerService>();
    service.AddSingleton(loggerFactory);
    service.AddSingleton<WhisperModelService>();
});

try
{
    logger.LogDebug("Console Arguments: whisper" + string.Join(" ", args));
    await Cli.RunAsync<RootCommand>(args);
}
catch (Exception e)
{
    logger.LogError(e, "An error occurred.");
}