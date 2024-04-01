// <copyright file="JfkSampleTest.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

[assembly: ClassCleanupExecution(ClassCleanupBehavior.EndOfClass)]

namespace WhisperCppLib.Tests;

[TestClass]
public class JfkSampleTest
{
    public static string? modelPath;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        modelPath = (string?)context.Properties["whisperModelLocation"] ?? string.Empty;
        if (string.IsNullOrEmpty(modelPath))
        {
            throw new InvalidOperationException("Whisper model location not found");
        }
    }

    [TestMethod]
    public async Task DefaultOptionsTest()
    {
        await using var whisper =
            new WhisperProcessor(new WhisperProcessorModelFileLoader(modelPath), new WhisperProcessorOptions());
        var stream = File.OpenRead("samples/jfk.wav");
        var result = whisper.ProcessAsync(stream);
        Assert.IsNotNull(result);
        await foreach (var item in result)
        {
            Assert.IsNotNull(item);
            Assert.IsNotNull(item.Text);
            Assert.IsFalse(string.IsNullOrEmpty(item.Text));
        }
    }

    [ClassCleanup(ClassCleanupBehavior.EndOfAssembly)]
    public static void ClassCleanup()
    {
    }
}