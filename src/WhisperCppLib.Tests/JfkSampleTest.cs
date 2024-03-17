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
        var modelLocation = (string?)context.Properties["whisperModelLocation"] ?? string.Empty;
        if (string.IsNullOrEmpty(modelLocation))
        {
            throw new InvalidOperationException("Whisper model location not found.");
        }
    }

    [TestMethod]
    public void TestMethod1()
    {
    }

    [ClassCleanup(ClassCleanupBehavior.EndOfAssembly)]
    public static void ClassCleanup()
    {
    }
}