// <copyright file="WaveParser.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;

namespace WhisperCppLib;

/// <summary>
/// Represents a parser for Wave files.
/// </summary>
public class WaveParser
{
    private static readonly byte[] ExpectedSubFormatForPcm = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71 };
    private readonly Stream waveStream;
    private ushort channels;
    private uint sampleRate;
    private ushort bitsPerSample;
    private uint dataChunkSize;
    private long dataChunkPosition;
    private bool isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="WaveParser"/> class.
    /// </summary>
    /// <param name="waveStream">The stream containing the Wave file.</param>
    public WaveParser(Stream waveStream)
    {
        this.waveStream = waveStream;
    }

    /// <summary>
    /// Gets the number of channels in the current wave file.
    /// </summary>
    public ushort Channels => this.channels;

    /// <summary>
    /// Gets the Sample Rate in the current wave file.
    /// </summary>
    public uint SampleRate => this.sampleRate;

    /// <summary>
    /// Gets the Bits Per Sample in the current wave file.
    /// </summary>
    public ushort BitsPerSample => this.bitsPerSample;

    /// <summary>
    /// Gets the size of the data chunk in the current wave file.
    /// </summary>
    public uint DataChunkSize => this.dataChunkSize;

    /// <summary>
    /// Gets the position of the data chunk in the current wave file.
    /// </summary>
    public long DataChunkPosition => this.dataChunkPosition;

    /// <summary>
    /// Gets a value indicating whether the wave parser is initialized.
    /// </summary>
    public bool IsInitialized => this.isInitialized;

    /// <summary>
    /// Gets the number of samples for each channel in the current wave file.
    /// </summary>
    public long SamplesCount => this.dataChunkSize / (this.bitsPerSample / 8) / this.channels;

    /// <summary>
    /// Gets the size of a single frame in the current wave file.
    /// </summary>
    public int FrameSize => this.bitsPerSample / 8 * this.channels;

    /// <summary>
    /// Gets the value to divide the sample by to get the actual float value.
    /// </summary>
    public float ValueToDivide => this.bitsPerSample switch
    {
        8 => 128.0f,
        16 => 32768.0f,
        24 => 8388608.0f,
        _ => 2147483648.0f,
    };

    /// <summary>
    /// Returns the average samples from all channels asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An array of average samples.</returns>
    public async Task<float[]> GetAvgSamplesAsync(CancellationToken cancellationToken = default)
    {
        await this.InitializeAsync(cancellationToken);

        var samples = new float[this.SamplesCount];

        var sampleIndex = 0;
        await foreach (var sampleFrame in this.InternalReadSamples(useAsync: true, cancellationToken))
        {
            var sampleSum = 0L;
            for (var i = 0; i < sampleFrame.Length; i++)
            {
                sampleSum += sampleFrame[i];
            }

            samples[sampleIndex++] = (sampleSum / this.ValueToDivide) / this.channels;
        }

        return samples;
    }

    /// <summary>
    /// Returns the average samples from all channels synchronously.
    /// </summary>
    /// <returns>An array of average samples.</returns>
    public float[] GetAvgSamples()
    {
        this.Initialize();

        var asyncEnumerator = this.InternalReadSamples(useAsync: false, CancellationToken.None).GetAsyncEnumerator();
        var samples = new float[this.SamplesCount];
        var sampleIndex = 0;

        while (asyncEnumerator.MoveNextAsync().GetAwaiter().GetResult())
        {
            var sampleFrame = asyncEnumerator.Current;
            var sampleSum = 0L;
            for (var i = 0; i < sampleFrame.Length; i++)
            {
                sampleSum += sampleFrame[i];
            }

            samples[sampleIndex++] = (sampleSum / this.ValueToDivide) / this.channels;
        }

        return samples;
    }

    /// <summary>
    /// Returns the samples from a specific channel synchronously.
    /// </summary>
    /// <param name="channelIndex">The index of the channel.</param>
    /// <returns>An array of samples from the specified channel.</returns>
    public float[] GetChannelSamples(int channelIndex = 0)
    {
        this.Initialize();
        if (channelIndex >= this.channels)
        {
            throw new ArgumentOutOfRangeException(nameof(channelIndex));
        }

        var samples = new float[this.SamplesCount];
        var sampleIndex = 0;

        var asyncEnumerator = this.InternalReadSamples(useAsync: false, CancellationToken.None).GetAsyncEnumerator();

        while (asyncEnumerator.MoveNextAsync().GetAwaiter().GetResult())
        {
            var sampleFrame = asyncEnumerator.Current;
            samples[sampleIndex++] = sampleFrame[channelIndex] / this.ValueToDivide;
        }

        return samples;
    }

    /// <summary>
    /// Returns the samples from a specific channel asynchronously.
    /// </summary>
    /// <param name="channelIndex">The index of the channel.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An array of samples from the specified channel.</returns>
    public async Task<float[]> GetChannelSamplesAsync(int channelIndex = 0, CancellationToken cancellationToken = default)
    {
        await this.InitializeAsync(cancellationToken);
        if (channelIndex >= this.channels)
        {
            throw new ArgumentOutOfRangeException(nameof(channelIndex));
        }

        var samples = new float[this.SamplesCount];
        var sampleIndex = 0;

        await foreach (var sampleFrame in this.InternalReadSamples(useAsync: true, cancellationToken))
        {
            samples[sampleIndex++] = sampleFrame[channelIndex] / this.ValueToDivide;
        }

        return samples;
    }

    /// <summary>
    /// Initializes the wave parser by reading the header and the format chunk synchronously.
    /// </summary>
    public void Initialize()
    {
        this.InternalInitialize(useAsync: false, CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Initializes the wave parser by reading the header and the format chunk asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        return this.InternalInitialize(useAsync: true, cancellationToken);
    }

    private async IAsyncEnumerable<long[]> InternalReadSamples(bool useAsync, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var buffer = new byte[2048 * this.channels];
        var memoryBuffer = buffer.AsMemory();

        var sampleIndex = 0;
        var bytesRead = int.MaxValue;

        while (bytesRead > 0 && sampleIndex < this.SamplesCount)
        {
            var maxBytesToRead = (int)Math.Min(buffer.Length, (this.SamplesCount - sampleIndex) * this.FrameSize);
            if (useAsync)
            {
#if NET6_0_OR_GREATER
                var memoryToUse = maxBytesToRead == buffer.Length ? memoryBuffer : memoryBuffer[..maxBytesToRead];
                bytesRead = await this.waveStream.ReadAsync(memoryToUse, cancellationToken);
#else
                bytesRead = await waveStream.ReadAsync(buffer, 0, maxBytesToRead, cancellationToken);
#endif
            }
            else
            {
                bytesRead = this.waveStream.Read(buffer, 0, maxBytesToRead);
            }

            for (var i = 0; i < bytesRead;)
            {
                var currentSamples = new long[this.channels];

                for (var currentChannel = 0; currentChannel < this.channels; currentChannel++)
                {
                    var (currentChannelValue, bytesConsumed) = this.bitsPerSample switch
                    {
                        8 => (buffer[i] - 128, 1),
                        16 => (BitConverter.ToInt16(buffer, i), 2),
                        24 => (BitConverter.ToInt32(buffer, i) >> 8, 3),
                        _ => (BitConverter.ToInt32(buffer, i), 4),
                    };
                    currentSamples[currentChannel] = currentChannelValue;
                    i += bytesConsumed;
                }

                yield return currentSamples;
                sampleIndex++;
            }
        }

        if (sampleIndex < this.SamplesCount)
        {
            throw new CorruptedWaveException("Invalid wave file, the size is too small and couldn't read all the samples.");
        }
    }

    private async Task InternalInitialize(bool useAsync, CancellationToken cancellationToken)
    {
        if (this.isInitialized)
        {
            return;
        }

        async Task<int> ReadBytesAsync(byte[] buffer, int offset, int count)
        {
            if (useAsync)
            {
#if NET6_0_OR_GREATER
                return await this.waveStream.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
#else
                return await waveStream.ReadAsync(buffer, offset, count, cancellationToken);
#endif
            }

            return this.waveStream.Read(buffer, offset, count);
        }

        var buffer = new byte[12];
        var actualRead = await ReadBytesAsync(buffer, 0, 12);

        if (actualRead != 12)
        {
            throw new CorruptedWaveException("Invalid wave file, the size is too small.");
        }

        // Read RIFF Header
        if (buffer[0] != 'R' || buffer[1] != 'I' || buffer[2] != 'F' || buffer[3] != 'F')
        {
            throw new CorruptedWaveException("Invalid wave file RIFF header.");
        }

        // Skip FileSize 4 => 8

        // Read Wave and Fmt tags
        if (buffer[8] != 'W' || buffer[9] != 'A' || buffer[10] != 'V' || buffer[11] != 'E')
        {
            throw new CorruptedWaveException("Invalid wave file header.");
        }

        // Search for format chunk
        int fmtChunkSize;
        while (true)
        {
            var nextChunkHeader = new byte[8];

            actualRead = await ReadBytesAsync(nextChunkHeader, 0, 8);

            if (actualRead != 8)
            {
                throw new CorruptedWaveException("Invalid wave file, cannot read next chunk.");
            }

            var chunkSize = BitConverter.ToInt32(nextChunkHeader, 4);
            if (chunkSize < 0)
            {
                throw new CorruptedWaveException("Invalid wave chunk size.");
            }

            if (nextChunkHeader[0] == 'f' && nextChunkHeader[1] == 'm' && nextChunkHeader[2] == 't' && nextChunkHeader[3] == ' ')
            {
                fmtChunkSize = chunkSize;
                break;
            }

            if (this.waveStream.CanSeek)
            {
                this.waveStream.Seek(chunkSize, SeekOrigin.Current);
            }
            else
            {
                var restOfChunk = new byte[chunkSize];
                await ReadBytesAsync(restOfChunk, 0, chunkSize);
            }
        }

        if (fmtChunkSize < 16)
        {
            throw new CorruptedWaveException("Invalid wave format size.");
        }

        var fmtBuffer = new byte[fmtChunkSize];
        actualRead = await ReadBytesAsync(fmtBuffer, 0, fmtChunkSize);
        if (actualRead != fmtChunkSize)
        {
            throw new CorruptedWaveException("Invalid wave file, cannot read format chunk.");
        }

        // Read Format
        var format = BitConverter.ToUInt16(fmtBuffer, 0);
        if (format != 1 && format != 65534) // Allow both standard PCM and WAVE_FORMAT_EXTENSIBLE
        {
            throw new NotSupportedWaveException("Unsupported wave format.");
        }

        // Read Channels
        this.channels = BitConverter.ToUInt16(fmtBuffer, 2);

        // Read Sample Rate
        this.sampleRate = BitConverter.ToUInt32(fmtBuffer, 4);

        // Read Bits Per Sample
        this.bitsPerSample = BitConverter.ToUInt16(fmtBuffer, 14);

        // Search for data chunk
        while (true)
        {
            var nextChunkHeader = new byte[8];

            actualRead = await ReadBytesAsync(nextChunkHeader, 0, 8);

            if (actualRead != 8)
            {
                throw new CorruptedWaveException("Invalid wave file, cannot read next chunk.");
            }

            var chunkSize = BitConverter.ToInt32(nextChunkHeader, 4);
            if (chunkSize < 0)
            {
                throw new CorruptedWaveException("Invalid wave chunk size.");
            }

            if (nextChunkHeader[0] == 'd' && nextChunkHeader[1] == 'a' && nextChunkHeader[2] == 't' && nextChunkHeader[3] == 'a')
            {
                this.dataChunkSize = (uint)chunkSize;
                this.dataChunkPosition = this.waveStream.Position;
                break;
            }

            if (this.waveStream.CanSeek)
            {
                this.waveStream.Seek(chunkSize, SeekOrigin.Current);
            }
            else
            {
                var restOfChunk = new byte[chunkSize];
                await ReadBytesAsync(restOfChunk, 0, chunkSize);
            }
        }

        this.isInitialized = true;
    }
}