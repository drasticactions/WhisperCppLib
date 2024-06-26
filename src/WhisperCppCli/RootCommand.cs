﻿// <copyright file="RootCommand.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Diagnostics;
using DotMake.CommandLine;
using Drastic.Services;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using WhisperCppGui.Models;
using WhisperCppGui.Services;
using WhisperCppLib;

/// <summary>
/// Root CLI command.
/// </summary>
[CliCommand(Description = "whisper", ShortFormAutoGenerate = false)]
public class RootCommand
{
    /// <summary>
    /// Model Command.
    /// </summary>
    [CliCommand(Description = "Manage Models")]
    public class ModelCommand(ILoggerFactory loggerFactory) : CommandBase(loggerFactory)
    {
        /// <summary>
        /// Model Command.
        /// </summary>
        [CliCommand(Description = "List All Models")]
        public class ListAllCommand(WhisperModelService modelService)
        {
            /// <summary>
            /// Run the command.
            /// </summary>
            public void Run()
            {
                var table = new Table();
                table.AddColumn("Type");
                table.AddColumn("Quantization");
                foreach (var model in modelService.AllModels)
                {
                    table.AddRow(model.Name, model.QuantizationType.ToString());
                }

                AnsiConsole.Write(table);
            }
        }

        /// <summary>
        /// Model Command.
        /// </summary>
        [CliCommand(Description = "List Available Models")]
        public class ListAvailableCommand(WhisperModelService modelService)
        {
            /// <summary>
            /// Run the command.
            /// </summary>
            public void Run()
            {
                var models = modelService.AvailableModels;

                if (models.Count == 0)
                {
                    AnsiConsole.Console.Write("No models available.");
                }

                var table = new Table();
                table.AddColumn("Type");
                table.AddColumn("Quantization");
                foreach (var model in models)
                {
                    table.AddRow(model.Name, model.QuantizationType.ToString());
                }

                AnsiConsole.Write(table);
            }
        }

        /// <summary>
        /// Model Command.
        /// </summary>
        [CliCommand(Description = "Download Models")]
        public class DownloadCommand(WhisperModelService modelService, IAppDispatcher dispatcher, ILoggerFactory loggerFactory) : CommandBase(loggerFactory)
        {
            /// <summary>
            /// Gets or sets the number of threads.
            /// </summary>
            [CliOption(Description = "GGML Type")]
            required public GgmlType GgmlType { get; set; }

            /// <summary>
            /// Gets or sets the number of threads.
            /// </summary>
            [CliOption(Description = "Quantization Type")]
            public QuantizationType QuantizationType { get; set; }

            /// <summary>
            /// Run the command.
            /// </summary>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
            public override async Task RunAsync()
            {
                var model = modelService.AllModels.FirstOrDefault(x => x.GgmlType == this.GgmlType && x.QuantizationType == this.QuantizationType);
                if (model == null)
                {
                    AnsiConsole.MarkupLine("[red]Model not available.[/]");
                    return;
                }

                var download = new WhisperDownload(model, modelService, dispatcher);
                if (File.Exists(download.Model.FileLocation))
                {
                    AnsiConsole.MarkupLine("[red]Model already downloaded.[/]");
                    return;
                }

                await AnsiConsole.Progress().StartAsync(async ctx =>
                {
                    var downloadTask = ctx.AddTask($"[green]Downloading {model.Name}[/]");
                    download.DownloadService.DownloadProgressChanged += (s, e) =>
                    {
                        downloadTask.Value(e.ProgressPercentage);
                    };

                    await download.DownloadCommand.ExecuteAsync();
                });
            }
        }
    }

    /// <summary>
    /// Infer Command.
    /// </summary>
    [CliCommand(Description = "Infer from a file")]
#pragma warning disable CS9107 // パラメーターは外側の型の状態にキャプチャされ、その値も基底コンストラクターに渡されます。この値は、基底クラスでもキャプチャされる可能性があります。
    public class InferCommand(WhisperModelService modelService, IAppDispatcher dispatcher, ILoggerFactory loggerFactory) : CommandBase(loggerFactory)
#pragma warning restore CS9107 // パラメーターは外側の型の状態にキャプチャされ、その値も基底コンストラクターに渡されます。この値は、基底クラスでもキャプチャされる可能性があります。
    {
        /// <summary>
        /// Gets or sets the number of threads.
        /// </summary>
        [CliOption(Description = "Threads to use for processing.")]
        public int? Threads { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of last text tokens to be considered by the WhisperProcessor.
        /// </summary>
        [CliOption(Description = "Max number of last text tokens.")]
        public int? MaxLastTextTokens { get; set; }

        /// <summary>
        /// Gets or sets the offset of the audio to be processed by the WhisperProcessor.
        /// </summary>
        [CliOption(Description = "Offset to start processing audio.")]
        public TimeSpan? Offset { get; set; }

        /// <summary>
        /// Gets or sets the duration of the audio to be processed by the WhisperProcessor.
        /// </summary>
        [CliOption(Description = "Duration of audio to process.")]
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the output should be translated.
        /// </summary>
        [CliOption(Description = "Translate the output.")]
        public bool? Translate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the context should be ignored by the WhisperProcessor.
        /// </summary>
        [CliOption(Description = "Context should be ignored.")]
        public bool? NoContext { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the audio should be processed as a single segment.
        /// </summary>
        [CliOption(Description = "Output should be processed as a single segment.")]
        public bool? SingleSegment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the progress should be printed during processing.
        /// </summary>
        [CliOption(Description = "Print special tokens in the output.")]
        public bool? PrintSpecialTokens { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the progress should be printed during processing.
        /// </summary>
        [CliOption(Description = "Print the progress.")]
        public bool? PrintProgress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the results should be printed after processing.
        /// </summary>
        [CliOption(Description = "Print the complete results.")]
        public bool PrintResults { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the timestamps should be printed in the output.
        /// </summary>
        [CliOption(Description = "Include the timestamps in the output.")]
        public bool? PrintTimestamps { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the token timestamps should be used.
        /// </summary>
        [CliOption(Description = "Use the token timestamps.")]
        public bool? UseTokenTimestamps { get; set; }

        /// <summary>
        /// Gets or sets the threshold for token timestamps.
        /// </summary>
        [CliOption(Description = "The threshold for token timestamps.")]
        public float? TokenTimestampsThreshold { get; set; }

        /// <summary>
        /// Gets or sets the threshold for the sum of token timestamps.
        /// </summary>
        [CliOption(Description = "The sum for token timestamps.")]
        public float? TokenTimestampsSumThreshold { get; set; }

        /// <summary>
        /// Gets or sets the maximum length of a segment.
        /// </summary>
        [CliOption(Description = "The max segment length.")]
        public int? MaxSegmentLength { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the audio should be split on word boundaries.
        /// </summary>
        [CliOption(Description = "Indicate whether the audio should be split on word boundaries.")]
        public bool? SplitOnWord { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens per segment.
        /// </summary>
        [CliOption(Description = "The max number of tokens per segment.")]
        public int? MaxTokensPerSegment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the tiny diarization speaker turn direction should be used.
        /// </summary>
        [CliOption(Description = "Enable diarization for speaker turn direction. Requires TinyDiarize model.")]
        public bool? TinyDiarizeSpeakerTurnDirection { get; set; }

        /// <summary>
        /// Gets or sets the size of the audio context.
        /// </summary>
        [CliOption(Description = "Sets the audio context size.")]
        public int? AudioContextSize { get; set; }

        /// <summary>
        /// Gets or sets the prompt to be used by the WhisperProcessor.
        /// </summary>
        [CliOption(Description = "Prompt to be given to whisper.", Required = false)]
        public string? Prompt { get; set; }

        /// <summary>
        /// Gets or sets the language to be used by the WhisperProcessor.
        /// </summary>
        [CliOption(Description = "Language for audio to be processed.", Required = false)]
        public string? Language { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether blank outputs should be suppressed.
        /// </summary>
        [CliOption(Description = "Suppress blank output.")]
        public bool? SuppressBlank { get; set; }

        /// <summary>
        /// Gets or sets the temperature value to be used by the WhisperProcessor.
        /// </summary>
        [CliOption(Description = "Temperature for the processor.")]
        public float? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the maximum initial timestamp value to be used by the WhisperProcessor.
        /// </summary>
        [CliOption(Description = "Maximum initial timestamp.")]
        public float? MaxInitialTs { get; set; }

        /// <summary>
        /// Gets or sets the length penalty value to be used by the WhisperProcessor.
        /// </summary>
        [CliOption(Description = "Length Penalty.")]
        public float? LengthPenalty { get; set; }

        /// <summary>
        /// Gets or sets the temperature increment value to be used by the WhisperProcessor.
        /// </summary>
        [CliOption(Description = "Temperature increment value.")]
        public float? TemperatureInc { get; set; }

        /// <summary>
        /// Gets or sets the entropy threshold value to be used by the WhisperProcessor.
        /// </summary>
        [CliOption(Description = "Entropy threshold.")]
        public float? EntropyThreshold { get; set; }

        /// <summary>
        /// Gets or sets the log probability threshold value to be used by the WhisperProcessor.
        /// </summary>
        [CliOption(Description = "Log probability threshold.")]
        public float? LogProbThreshold { get; set; }

        /// <summary>
        /// Gets or sets the no speech threshold value to be used by the WhisperProcessor.
        /// </summary>
        [CliOption(Description = "No speech threshold.")]
        public float? NoSpeechThreshold { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the probabilities should be computed by the WhisperProcessor.
        /// </summary>
        [CliOption(Description = "Compute probabilities.")]
        public bool ComputeProbabilities { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to output as a SRT file.
        /// </summary>
        [CliOption(Description = "Output SRT.")]
        public bool OutputSrt { get; set; } = true;

        /// <summary>
        /// Gets or sets the Whisper model.
        /// </summary>
        [CliOption(Description = "Whisper model", Required = false)]
        public string? Model { get; set; }

        /// <summary>
        /// Gets or sets the GGML Type..
        /// </summary>
        [CliOption(Description = "GGML Type")]
        public GgmlType GgmlType { get; set; }

        /// <summary>
        /// Gets or sets the Quantization Type.
        /// </summary>
        [CliOption(Description = "Quantization Type")]
        public QuantizationType QuantizationType { get; set; }

        /// <summary>
        /// Gets or sets the Input File.
        /// </summary>
        [CliOption(Description = "Input File")]
        required public string InputFile { get; set; }

        /// <summary>
        /// Run the command.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task RunAsync()
        {
            var logger = loggerFactory.CreateLogger<InferCommand>();
            WhisperModel? model = null;
            if (string.IsNullOrEmpty(this.Model))
            {
                model = new WhisperModel(this.GgmlType, this.QuantizationType);
                if (!File.Exists(model.FileLocation))
                {
                    AnsiConsole.Markup("[red]Model not available.[/]");
                    return;
                }

                this.Model = model.FileLocation;
            }

            if (string.IsNullOrEmpty(this.Model) || !File.Exists(this.Model))
            {
                AnsiConsole.Markup("[red]Model not available.[/]");
                return;
            }

            var modelName = model?.Name ?? Path.GetFileNameWithoutExtension(this.Model);

            var options = new WhisperProcessorOptions()
            {
                AudioContextSize = this.AudioContextSize,
                ComputeProbabilities = this.ComputeProbabilities,
                Duration = this.Duration,
                EntropyThreshold = this.EntropyThreshold,
                Language = this.Language,
                LengthPenalty = this.LengthPenalty,
                LogProbThreshold = this.LogProbThreshold,
                MaxInitialTs = this.MaxInitialTs,
                MaxLastTextTokens = this.MaxLastTextTokens,
                MaxSegmentLength = this.MaxSegmentLength,
                MaxTokensPerSegment = this.MaxTokensPerSegment,
                NoContext = this.NoContext,
                NoSpeechThreshold = this.NoSpeechThreshold,
                Offset = this.Offset,
                PrintProgress = this.PrintProgress,
                PrintResults = this.PrintResults,
                PrintSpecialTokens = this.PrintSpecialTokens,
                PrintTimestamps = this.PrintTimestamps,
                Prompt = this.Prompt,
                SingleSegment = this.SingleSegment,
                SplitOnWord = this.SplitOnWord,
                SuppressBlank = this.SuppressBlank,
                Temperature = this.Temperature,
                TemperatureInc = this.TemperatureInc,
                Threads = this.Threads,
                TinyDiarizeSpeakerTurnDirection = this.TinyDiarizeSpeakerTurnDirection,
                TokenTimestampsSumThreshold = this.TokenTimestampsSumThreshold,
                TokenTimestampsThreshold = this.TokenTimestampsThreshold,
                Translate = this.Translate,
                UseTokenTimestamps = this.UseTokenTimestamps,
            };
            using var ffmpeg = new FFMpegTranscodeService(logger);
            var output = await ffmpeg.ProcessFile(this.InputFile);
            await using var whisperProcessor = new WhisperProcessor(new WhisperProcessorModelFileLoader(this.Model), options);
            await using var stream = File.OpenRead(output);
            var stopwatch = new Stopwatch();
            var result = whisperProcessor.ProcessAsync(stream);
            stopwatch.Start();
            var srt = new SrtSubtitle();
            await foreach (var item in result)
            {
                var text = $"{item.Text.Trim()}";
                if (item.SpeakerTurn)
                {
                    text = $"{text} [Speaker Turn]";
                }

                srt.AddLine(new SrtSubtitleLine()
                {
                    Start = item.Start,
                    End = item.End,
                    Text = text,
                });
            }

            stopwatch.Stop();

            if (this.OutputSrt)
            {
                var srtFile = Path.ChangeExtension($"{Path.GetFileNameWithoutExtension(this.InputFile)}-{modelName}.srt", ".srt");
                await File.WriteAllTextAsync(srtFile, srt.ToString());
                AnsiConsole.MarkupLine($"[green]SRT File written to {srtFile}[/]");
            }

            AnsiConsole.MarkupLine($"[green]Processing took {stopwatch.ElapsedMilliseconds}ms[/]");
        }
    }
}