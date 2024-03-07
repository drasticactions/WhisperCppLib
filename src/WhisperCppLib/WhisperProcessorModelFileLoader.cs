// <copyright file="WhisperProcessorModelFileLoader.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

namespace WhisperCppLib;

/// <summary>
/// Represents a file loader for the Whisper processor model.
/// </summary>
public sealed class WhisperProcessorModelFileLoader : IWhisperProcessorModelLoader
{
    private readonly string pathModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="WhisperProcessorModelFileLoader"/> class.
    /// </summary>
    /// <param name="pathModel">The path to the model file.</param>
    public WhisperProcessorModelFileLoader(string pathModel)
    {
        this.pathModel = pathModel;
    }

    /// <summary>
    /// Releases all resources used by the <see cref="WhisperProcessorModelFileLoader"/>.
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    /// Loads the native context from the model file.
    /// </summary>
    /// <returns>The pointer to the native context.</returns>
    public IntPtr LoadNativeContext()
    {
        return WhisperCppInterop.Whisper_init_from_file_with_params_no_state(this.pathModel, default(WhisperContextParams));
    }
}
