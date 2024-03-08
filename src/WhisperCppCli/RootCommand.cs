// <copyright file="RootCommand.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using DotMake.CommandLine;
using Microsoft.Extensions.Logging;
using WhisperCppLib;

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
            var logger = loggerFactory.CreateLogger<InferCommand>();
            var options = new WhisperProcessorOptions();
            options.TinyDiarizeSpeakerTurnDirection = true;
            using var ffmpeg = new FFMpegTranscodeService(logger);
            var output = await ffmpeg.ProcessFile(this.InputFile);
            await using var whisperProcessor = new WhisperProcessor(new WhisperProcessorModelFileLoader(this.Model), options);
            await using var stream = File.OpenRead(output);
            var result = whisperProcessor.ProcessAsync(stream);
            await foreach (var item in result)
            {
                var text = $"{item.Text.Trim()}";
                if (item.SpeakerTurn)
                {
                    text = $"{text} [Speaker Turn]";
                }

                logger.LogInformation(text);
            }
        }
    }
}