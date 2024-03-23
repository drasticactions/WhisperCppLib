// <copyright file="GgmlType.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

namespace WhisperCppGui.Models;

/// <summary>
/// Represents the type of GGML.
/// </summary>
public enum GgmlType
{
    /// <summary>
    /// Unknown.
    /// </summary>
    Unknown,

    /// <summary>
    /// Represents the Tiny GGML type.
    /// </summary>
    Tiny,

    /// <summary>
    /// Represents the Tiny GGML type in English.
    /// </summary>
    TinyEn,

    /// <summary>
    /// Represents the Base GGML type.
    /// </summary>
    Base,

    /// <summary>
    /// Represents the Base GGML type in English.
    /// </summary>
    BaseEn,

    /// <summary>
    /// Represents the Small GGML type.
    /// </summary>
    Small,

    /// <summary>
    /// Represents the Small GGML type in English.
    /// </summary>
    SmallEn,

    /// <summary>
    /// Represents the Medium GGML type.
    /// </summary>
    Medium,

    /// <summary>
    /// Represents the Medium GGML type in English.
    /// </summary>
    MediumEn,

    /// <summary>
    /// Represents the LargeV1 GGML type.
    /// </summary>
    LargeV1,

    /// <summary>
    /// Represents the LargeV2 GGML type.
    /// </summary>
    LargeV2,

    /// <summary>
    /// Represents the LargeV3 GGML type.
    /// </summary>
    LargeV3,
}
