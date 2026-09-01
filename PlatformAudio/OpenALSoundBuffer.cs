using PlatformAudio.Enums;
using PlatformAudio.Interfaces;
using PlatformNative.Native;

namespace PlatformAudio;

/// <summary>
/// Реализация звукового буфера через OpenAL
/// </summary>
public sealed class OpenALSoundBuffer : ISoundBuffer
{
    private uint ALHandle;
    private AudioFormat ALFormat;
    private int ALSampleRate;
    private int ALDurationMs;
    private int ALSize;
    private bool Disposed;

    public IntPtr Handle => (IntPtr)ALHandle;
    public AudioFormat Format => ALFormat;
    public int SampleRate => ALSampleRate;
    public int DurationMs => ALDurationMs;
    public int Size => ALSize;

    public OpenALSoundBuffer()
    {
        OpenAL.AlGenBuffers(1, out ALHandle);
        if (ALHandle == 0)
            throw new InvalidOperationException("Не удалось создать буфер OpenAL");
    }

    public void SetData(byte[] data, AudioFormat format, int sampleRate)
    {
        // ✅ Используем ObjectDisposedException.ThrowIf
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (data is null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty", nameof(data));

        if (sampleRate <= 0)
            throw new ArgumentException("Sample rate must be positive", nameof(sampleRate));

        ALFormat = format;
        ALSampleRate = sampleRate;
        ALSize = data.Length;

        int alFormat = format switch
        {
            AudioFormat.Mono8 => OpenAL.AL_FORMAT_MONO8,
            AudioFormat.Mono16 => OpenAL.AL_FORMAT_MONO16,
            AudioFormat.Stereo8 => OpenAL.AL_FORMAT_STEREO8,
            AudioFormat.Stereo16 => OpenAL.AL_FORMAT_STEREO16,
            // Для float используем 16-bit, так как OpenAL не поддерживает float напрямую
            AudioFormat.MonoFloat => OpenAL.AL_FORMAT_MONO16,
            AudioFormat.StereoFloat => OpenAL.AL_FORMAT_STEREO16,
            _ => OpenAL.AL_FORMAT_MONO16
        };

        var handle = System.Runtime.InteropServices.GCHandle.Alloc(data, System.Runtime.InteropServices.GCHandleType.Pinned);
        try
        {
            OpenAL.AlBufferData(ALHandle, alFormat, handle.AddrOfPinnedObject(), data.Length, sampleRate);
        }
        finally
        {
            handle.Free();
        }

        // Вычисляем длительность
        int bytesPerSample = format switch
        {
            AudioFormat.Mono8 => 1,
            AudioFormat.Mono16 => 2,
            AudioFormat.Stereo8 => 2,
            AudioFormat.Stereo16 => 4,
            AudioFormat.MonoFloat => 4,
            AudioFormat.StereoFloat => 8,
            _ => 2
        };
        int samples = data.Length / bytesPerSample;
        ALDurationMs = (int)((float)samples / sampleRate * 1000);
    }

    public void SetData(float[] data, AudioFormat format, int sampleRate)
    {
        // ✅ Используем ObjectDisposedException.ThrowIf
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (data is null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty", nameof(data));

        if (sampleRate <= 0)
            throw new ArgumentException("Sample rate must be positive", nameof(sampleRate));

        // Конвертируем float в short (16-bit) с правильным масштабированием
        short[] shortData = new short[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            shortData[i] = (short)(Math.Clamp(data[i], -1f, 1f) * 32767f);
        }

        // Конвертируем short в byte
        byte[] byteData = new byte[shortData.Length * 2];
        Buffer.BlockCopy(shortData, 0, byteData, 0, byteData.Length);

        // Передаем как 16-bit
        SetData(byteData, format, sampleRate);
    }

    public void Dispose()
    {
        if (Disposed) return;

        if (ALHandle != 0)
        {
            uint handle = ALHandle;
            OpenAL.AlDeleteBuffers(1, ref handle);
            ALHandle = 0;
        }

        Disposed = true;
        GC.SuppressFinalize(this);
    }
}