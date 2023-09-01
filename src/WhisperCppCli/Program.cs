using CommandLine;
using CommandLine.Text;
using System;
using System.IO;
using WhisperCppCli.Models;
using WhisperCppLib;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var parser = new Parser(settings => { settings.CaseSensitive = false; });
        var optionsResult = parser.ParseArguments<Options>(args);

        optionsResult.WithNotParsed(errors =>
        {
            var text = HelpText.AutoBuild(optionsResult);
            Console.WriteLine(text);
            Environment.Exit(1);
        });

        await optionsResult.WithParsedAsync(RunWithOptions);
    }

    static async Task RunWithOptions(Options options)
    {
        if (!File.Exists(options.Model))
        {
            Console.WriteLine($"Model file '{options.Model}' does not exist.");
            Environment.Exit(1);
        }

        if (!File.Exists(options.File))
        {
            Console.WriteLine($"File '{options.File}' does not exist.");
            Environment.Exit(1);
        }

        var whisperOptions = options.ConvertToWhisperProcessorOptions();
        using var whisperProcessor = new WhisperProcessor(new WhisperProcessorModelFileLoader(options.Model), whisperOptions);
        using var stream = File.OpenRead(options.File);
        whisperProcessor.OnProgressHandler += WhisperProcessor_OnProgressHandler;

        var result = whisperProcessor.ProcessAsync(stream);

        var subtitle = new SrtSubtitle() { Lines = new List<ISubtitleLine>() };

        await foreach(var e in result)
        {
            Console.WriteLine($"Text: {e.Text}");
            subtitle.Lines.Add(new SrtSubtitleLine(){ Start = e.Start, End = e.End, Text = e.Text.Trim(), LineNumber = subtitle.Lines.Count() + 1 });
        }

        if (options.OutputSubtitle)
        {
            var srtFile = Path.ChangeExtension(options.File, ".srt");
            await File.WriteAllTextAsync(srtFile, subtitle.ToString());
        }
    }

    private static void WhisperProcessor_OnProgressHandler(int progress)
    {
        System.Diagnostics.Debug.WriteLine($"Progress: {progress}");
    }
}

class Options
{
    [Option("output-subtitle", HelpText = "Output Subtitle (srt)")]
    public bool OutputSubtitle { get; set; }

    [Option('m', "model", Required = true, HelpText = "Path to the model file")]
    public string? Model { get; set; }

    [Option('f', "file", Required = true, HelpText = "Path to the file to parse")]
    public string? File { get; set; }

    [Option("sampling-strategy", Default = "GreedySamplingStrategy", HelpText = "Specify the sampling strategy.")]
    public string SamplingStrategy { get; set; } = "GreedySamplingStrategy";

    [Option("threads", HelpText = "Specify the number of threads.")]
    public int? Threads { get; set; }

    [Option("max-last-text-tokens", HelpText = "Specify the maximum last text tokens.")]
    public int? MaxLastTextTokens { get; set; }

    [Option("offset", HelpText = "Specify the offset.")]
    public string Offset { get; set; }

    [Option("duration", HelpText = "Specify the duration.")]
    public string Duration { get; set; }

    [Option("translate", HelpText = "Specify whether to translate.")]
    public bool? Translate { get; set; }

    [Option("no-context", HelpText = "Specify whether to disable context.")]
    public bool? NoContext { get; set; }

    [Option("single-segment", HelpText = "Specify whether to use single segment.")]
    public bool? SingleSegment { get; set; }

    [Option("print-special-tokens", HelpText = "Specify whether to print special tokens.")]
    public bool? PrintSpecialTokens { get; set; }

    [Option("print-progress", Default = false, HelpText = "Specify whether to print progress.")]
    public bool? PrintProgress { get; set; }

    [Option("print-results", HelpText = "Specify whether to print results.")]
    public bool? PrintResults { get; set; }

    [Option("print-timestamps", HelpText = "Specify whether to print timestamps.")]
    public bool? PrintTimestamps { get; set; }

    [Option("use-token-timestamps", HelpText = "Specify whether to use token timestamps.")]
    public bool? UseTokenTimestamps { get; set; }

    [Option("token-timestamps-threshold", HelpText = "Specify the token timestamps threshold.")]
    public float? TokenTimestampsThreshold { get; set; }

    [Option("token-timestamps-sum-threshold", HelpText = "Specify the token timestamps sum threshold.")]
    public float? TokenTimestampsSumThreshold { get; set; }

    [Option("max-segment-length", HelpText = "Specify the maximum segment length.")]
    public int? MaxSegmentLength { get; set; }

    [Option("split-on-word", HelpText = "Specify whether to split on word.")]
    public bool? SplitOnWord { get; set; }

    [Option("max-tokens-per-segment", HelpText = "Specify the maximum tokens per segment.")]
    public int? MaxTokensPerSegment { get; set; }

    [Option("speed-up-2x", HelpText = "Specify whether to speed up 2x.")]
    public bool? SpeedUp2x { get; set; }

    [Option("audio-context-size", HelpText = "Specify the audio context size.")]
    public int? AudioContextSize { get; set; }

    [Option("prompt", HelpText = "Specify the prompt.")]
    public string? Prompt { get; set; }

    [Option("language", HelpText = "Specify the language.")]
    public string? Language { get; set; }

    [Option("suppress-blank", HelpText = "Specify whether to suppress blank.")]
    public bool? SuppressBlank { get; set; }

    [Option("temperature", HelpText = "Specify the temperature.")]
    public float? Temperature { get; set; }

    [Option("max-initial-ts", HelpText = "Specify the maximum initial ts.")]
    public float? MaxInitialTs { get; set; }

    [Option("length-penalty", HelpText = "Specify the length penalty.")]
    public float? LengthPenalty { get; set; }

    [Option("temperature-inc", HelpText = "Specify the temperature increment.")]
    public float? TemperatureInc { get; set; }

    [Option("entropy-threshold", HelpText = "Specify the entropy threshold.")]
    public float? EntropyThreshold { get; set; }

    [Option("log-prob-threshold", HelpText = "Specify the log probability threshold.")]
    public float? LogProbThreshold { get; set; }

    [Option("no-speech-threshold", HelpText = "Specify the no speech threshold.")]
    public float? NoSpeechThreshold { get; set; }

    [Option("compute-probabilities", HelpText = "Specify whether to compute probabilities.")]
    public bool ComputeProbabilities { get; set; }
}

internal static class OptionsConverter
{
    public static WhisperProcessorOptions ConvertToWhisperProcessorOptions(this Options commandLineOptions)
    {
        return new WhisperProcessorOptions
        {
            SamplingStrategy = GetSamplingStrategy(commandLineOptions.SamplingStrategy),
            Threads = commandLineOptions.Threads,
            MaxLastTextTokens = commandLineOptions.MaxLastTextTokens,
            Offset = ParseTimeSpan(commandLineOptions.Offset),
            Duration = ParseTimeSpan(commandLineOptions.Duration),
            Translate = commandLineOptions.Translate,
            NoContext = commandLineOptions.NoContext,
            SingleSegment = commandLineOptions.SingleSegment,
            PrintSpecialTokens = commandLineOptions.PrintSpecialTokens,
            PrintProgress = commandLineOptions.PrintProgress,
            PrintResults = commandLineOptions.PrintResults,
            PrintTimestamps = commandLineOptions.PrintTimestamps,
            UseTokenTimestamps = commandLineOptions.UseTokenTimestamps,
            TokenTimestampsThreshold = commandLineOptions.TokenTimestampsThreshold,
            TokenTimestampsSumThreshold = commandLineOptions.TokenTimestampsSumThreshold,
            MaxSegmentLength = commandLineOptions.MaxSegmentLength,
            SplitOnWord = commandLineOptions.SplitOnWord,
            MaxTokensPerSegment = commandLineOptions.MaxTokensPerSegment,
            SpeedUp2x = commandLineOptions.SpeedUp2x,
            AudioContextSize = commandLineOptions.AudioContextSize,
            Prompt = commandLineOptions.Prompt,
            Language = commandLineOptions.Language,
            SuppressBlank = commandLineOptions.SuppressBlank,
            Temperature = commandLineOptions.Temperature,
            MaxInitialTs = commandLineOptions.MaxInitialTs,
            LengthPenalty = commandLineOptions.LengthPenalty,
            TemperatureInc = commandLineOptions.TemperatureInc,
            EntropyThreshold = commandLineOptions.EntropyThreshold,
            LogProbThreshold = commandLineOptions.LogProbThreshold,
            NoSpeechThreshold = commandLineOptions.NoSpeechThreshold,
            ComputeProbabilities = commandLineOptions.ComputeProbabilities
        };
    }

    private static IWhisperSamplingStrategy GetSamplingStrategy(string strategyName)
    {
        switch(strategyName.ToLower())
        {
            case "greedysamplingstrategy":
                return new GreedySamplingStrategy();
            case "beamsearchsamplingstrategy":
                return new BeamSearchSamplingStrategy();
        }

        return new GreedySamplingStrategy();
    }

    private static TimeSpan? ParseTimeSpan(string timeSpanStr)
    {
        if (TimeSpan.TryParse(timeSpanStr, out TimeSpan timeSpan))
        {
            return timeSpan;
        }
        return null;
    }
}