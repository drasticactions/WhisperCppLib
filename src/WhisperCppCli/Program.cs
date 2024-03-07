// <copyright file="Program.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using DotMake.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
    service.AddSingleton(loggerFactory);
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