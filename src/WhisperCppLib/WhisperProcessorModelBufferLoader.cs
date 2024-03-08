// <copyright file="WhisperProcessorModelBufferLoader.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;

namespace WhisperCppLib;

/// <summary>
/// Represents a loader for Whisper processor model from a byte buffer.
/// </summary>
public class WhisperProcessorModelBufferLoader : IWhisperProcessorModelLoader
{
    private readonly byte[] buffer;
    private readonly GCHandle pinnedBuffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="WhisperProcessorModelBufferLoader"/> class.
    /// </summary>
    /// <param name="buffer">The byte buffer containing the Whisper processor model.</param>
    public WhisperProcessorModelBufferLoader(byte[] buffer)
    {
        this.buffer = buffer;
        this.pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
    }

    /// <summary>
    /// Releases all resources used by the <see cref="WhisperProcessorModelBufferLoader"/> object.
    /// </summary>
    public void Dispose()
    {
        this.pinnedBuffer.Free();
    }

    /// <summary>
    /// Loads the native context from the byte buffer.
    /// </summary>
    /// <returns>The pointer to the native context.</returns>
    public IntPtr LoadNativeContext()
    {
        var bufferLength = new UIntPtr((uint)this.buffer.Length);
        return WhisperCppInterop.whisper_init_from_buffer_with_params_no_state(this.pinnedBuffer.AddrOfPinnedObject(), bufferLength, default(WhisperContextParams));
    }
}
