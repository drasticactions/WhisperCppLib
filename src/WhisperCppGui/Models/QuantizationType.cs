// <copyright file="QuantizationType.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

namespace WhisperCppGui.Models;

/// <summary>
/// Represents the type of quantization.
/// </summary>
public enum QuantizationType
{
    /// <summary>
    /// No quantization.
    /// </summary>
    NoQuantization,

    /// <summary>
    /// Quantization with Q4.0.
    /// </summary>
    Q4_0,

    /// <summary>
    /// Quantization with Q4.1.
    /// </summary>
    Q4_1,

    /// <summary>
    /// Quantization with Q5.0.
    /// </summary>
    Q5_0,

    /// <summary>
    /// Quantization with Q5.1.
    /// </summary>
    Q5_1,

    /// <summary>
    /// Quantization with Q8.0.
    /// </summary>
    Q8_0,
}
