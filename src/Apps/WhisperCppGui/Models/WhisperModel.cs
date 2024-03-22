// <copyright file="WhisperModel.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhisperCppGui.Tools;

namespace WhisperCppGui.Models;

/// <summary>
/// Represents a Whisper model.
/// </summary>
public class WhisperModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WhisperModel"/> class.
    /// </summary>
    public WhisperModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WhisperModel"/> class with the specified type and quantization type.
    /// </summary>
    /// <param name="type">The GGML type of the model.</param>
    /// <param name="quantizationType">The quantization type of the model.</param>
    public WhisperModel(GgmlType type, QuantizationType quantizationType)
    {
        this.GgmlType = type;
        this.QuantizationType = quantizationType;
        this.Name = $"{type.ToString()} - {quantizationType}";
        this.Type = WhisperModelType.Standard;
        this.FileLocation = WhisperStatic.GetModelPath(type, quantizationType);
        this.DownloadUrl = type.ToDownloadUrl(quantizationType);

        // TODO: Add descriptions
        var modelDescription = type switch
        {
            GgmlType.Tiny => "Tiny model trained on 1.5M samples",
            GgmlType.TinyEn => "Tiny model trained on 1.5M samples (English)",
            GgmlType.Base => "Base model trained on 1.5M samples",
            GgmlType.BaseEn => "Base model trained on 1.5M samples (English)",
            GgmlType.Small => "Small model trained on 1.5M samples",
            GgmlType.SmallEn => "Small model trained on 1.5M samples (English)",
            GgmlType.Medium => "Medium model trained on 1.5M samples",
            GgmlType.MediumEn => "Medium model trained on 1.5M samples (English)",
            GgmlType.LargeV1 => "Large model trained on 1.5M samples (v1)",
            GgmlType.LargeV2 => "Large model trained on 1.5M samples (v2)",
            GgmlType.LargeV3 => "Large model trained on 1.5M samples (v3)",
            _ => throw new NotImplementedException(),
        };

        this.Description = $"{modelDescription} - {quantizationType}";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WhisperModel"/> class with the specified file path.
    /// </summary>
    /// <param name="path">The file path of the model.</param>
    public WhisperModel(string path)
    {
        if (!System.IO.Path.Exists(path))
        {
            throw new ArgumentException(nameof(path));
        }

        this.FileLocation = path;
        this.Name = System.IO.Path.GetFileNameWithoutExtension(path);
        this.Type = WhisperModelType.User;
    }

    /// <summary>
    /// Gets or sets the type of the Whisper model.
    /// </summary>
    public WhisperModelType Type { get; set; }

    /// <summary>
    /// Gets or sets the GGML type of the model.
    /// </summary>
    public GgmlType GgmlType { get; set; }

    /// <summary>
    /// Gets or sets the quantization type of the model.
    /// </summary>
    public QuantizationType QuantizationType { get; set; }

    /// <summary>
    /// Gets or sets the name of the model.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file location of the model.
    /// </summary>
    public string FileLocation { get; set; } = string.Empty;

    /// <summary>
    /// Gets the download URL of the model.
    /// </summary>
    public string DownloadUrl { get; } = string.Empty;

    /// <summary>
    /// Gets the description of the model.
    /// </summary>
    public string Description { get; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the model file exists.
    /// </summary>
    public bool Exists => !string.IsNullOrEmpty(this.FileLocation) && System.IO.Path.Exists(this.FileLocation);
}
