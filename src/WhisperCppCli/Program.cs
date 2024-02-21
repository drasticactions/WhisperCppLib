using System;
using System.IO;
using WhisperCppCli.Models;
using WhisperCppLib;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var modelLoader = new WhisperProcessorModelFileLoader("ggml-base.en.bin");
        var processorOptions = new WhisperProcessorOptions();
        using var whisper = new WhisperProcessor(modelLoader, processorOptions);
        using var input = File.OpenRead("samples_jfk.wav"); 
        var result = whisper.ProcessAsync(input);
        await foreach(var e in result)
        {
            Console.Write($"{e.Text}");
        }
    }
}

