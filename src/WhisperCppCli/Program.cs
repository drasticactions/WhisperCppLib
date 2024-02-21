using System;
using System.IO;
using DotMake.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WhisperCppCli.Models;
using WhisperCppLib;

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

/// <summary>
/// Root CLI command.
/// </summary>
[CliCommand(Description = "whisper", ShortFormAutoGenerate = false)]
public class RootCommand
{
    /// <summary>
    /// Infer Command.
    /// </summary>
    [CliCommand(Description = "Infer from a file")]
#pragma warning disable CS9107 // パラメーターは外側の型の状態にキャプチャされ、その値も基底コンストラクターに渡されます。この値は、基底クラスでもキャプチャされる可能性があります。
    public class InferCommand(ILoggerFactory loggerFactory) : CommandBase(loggerFactory)
#pragma warning restore CS9107 // パラメーターは外側の型の状態にキャプチャされ、その値も基底コンストラクターに渡されます。この値は、基底クラスでもキャプチャされる可能性があります。 
    {
        /// <summary>
        /// Run the command.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task RunAsync()
        {
            var options = new WhisperProcessorOptions();
            using var whisperProcessor = new WhisperProcessor(new WhisperProcessorModelFileLoader(this.Model), options);
            using var stream = File.OpenRead(this.InputFile);
            var result = whisperProcessor.ProcessAsync(stream);
            await foreach(var item in result)
            {
                Console.Write(item.Text);
            }
        }
    }
}

/// <summary>
/// Base Command.
/// </summary>
public abstract class CommandBase(ILoggerFactory loggerFactory)
{
    /// <summary>
    /// Gets or sets the Whisper model.
    /// </summary>
    [CliOption(Description = "Whisper model")]
    public required string Model { get; set; }
    
    /// <summary>
    /// Gets or sets the Input File.
    /// </summary>
    [CliOption(Description = "Input File")]
    public required string InputFile { get; set; }

    /// <summary>
    /// Run the command.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public virtual async Task RunAsync()
    {
    }
}
