using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WhisperCppLib;

public interface IWhisperProcessorModelLoader : IDisposable
{
    public IntPtr LoadNativeContext();
}

public class WhisperProcessorModelBufferLoader : IWhisperProcessorModelLoader
{
    private readonly byte[] buffer;
    private readonly GCHandle pinnedBuffer;

    public WhisperProcessorModelBufferLoader(byte[] buffer)
    {
        this.buffer = buffer;
        pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
    }

    public void Dispose()
    {
        pinnedBuffer.Free();
    }

    public IntPtr LoadNativeContext()
    {
        var bufferLength = new UIntPtr((uint)buffer.Length);
        return WhisperCppInterop.whisper_init_from_buffer_with_params_no_state(pinnedBuffer.AddrOfPinnedObject(), bufferLength, new WhisperContextParams());
    }
}

public sealed class WhisperProcessorModelFileLoader : IWhisperProcessorModelLoader
{
    private readonly string pathModel;

    public WhisperProcessorModelFileLoader(string pathModel)
    {
        this.pathModel = pathModel;
    }

    public void Dispose()
    {

    }

    public IntPtr LoadNativeContext()
    {
        return WhisperCppInterop.whisper_init_from_file_with_params_no_state(pathModel, new WhisperContextParams());
    }
}
