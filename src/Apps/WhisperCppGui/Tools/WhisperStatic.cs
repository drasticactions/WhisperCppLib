// <copyright file="WhisperStatic.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using WhisperCppGui.Models;

namespace WhisperCppGui.Tools;

/// <summary>
/// Provides static methods for handling WhisperCppGui related operations.
/// </summary>
public static class WhisperStatic
{
    /// <summary>
    /// Gets the default path for WhisperCppGui.
    /// </summary>
    public static string DefaultPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WhisperCppGui");

    /// <summary>
    /// Converts the GgmlType and QuantizationType to a download URL.
    /// </summary>
    /// <param name="type">The GgmlType.</param>
    /// <param name="quantizationType">The QuantizationType.</param>
    /// <returns>The download URL.</returns>
    public static string ToDownloadUrl(this GgmlType type, QuantizationType quantizationType)
    {
        var subdirectory = GetQuantizationSubdirectory(quantizationType);
        var modelName = type.ToFilename();
        return $"https://huggingface.co/sandrohanea/whisper.net/resolve/v1/{subdirectory}/{modelName}";
    }

    /// <summary>
    /// Gets the model path for the specified GgmlType and QuantizationType.
    /// </summary>
    /// <param name="type">The GgmlType.</param>
    /// <param name="quantizationType">The QuantizationType.</param>
    /// <returns>The model path.</returns>
    public static string GetModelPath(GgmlType type, QuantizationType quantizationType)
        => Path.Combine(DefaultPath, GetQuantizationSubdirectory(quantizationType), type.ToFilename());

    /// <summary>
    /// Converts the GgmlType to a filename.
    /// </summary>
    /// <param name="type">The GgmlType.</param>
    /// <returns>The filename.</returns>
    public static string ToFilename(this GgmlType type) => type switch
    {
        GgmlType.Tiny => "ggml-tiny.bin",
        GgmlType.TinyEn => "ggml-tiny.en.bin",
        GgmlType.Base => "ggml-base.bin",
        GgmlType.BaseEn => "ggml-base.en.bin",
        GgmlType.Small => "ggml-small.bin",
        GgmlType.SmallEn => "ggml-small.en.bin",
        GgmlType.Medium => "ggml-medium.bin",
        GgmlType.MediumEn => "ggml-medium.en.bin",
        GgmlType.LargeV1 => "ggml-large-v1.bin",
        GgmlType.LargeV2 => "ggml-large-v2.bin",
        GgmlType.LargeV3 => "ggml-large-v3.bin",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };

    /// <summary>
    /// Gets the quantization subdirectory for the specified QuantizationType.
    /// </summary>
    /// <param name="quantization">The QuantizationType.</param>
    /// <returns>The quantization subdirectory.</returns>
    private static string GetQuantizationSubdirectory(QuantizationType quantization)
    {
        return quantization switch
        {
            QuantizationType.NoQuantization => "classic",
            QuantizationType.Q4_0 => "q4_0",
            QuantizationType.Q4_1 => "q4_1",
            QuantizationType.Q5_0 => "q5_0",
            QuantizationType.Q5_1 => "q5_1",
            QuantizationType.Q8_0 => "q8_0",
            _ => throw new ArgumentOutOfRangeException(nameof(quantization), quantization, null),
        };
    }
}
