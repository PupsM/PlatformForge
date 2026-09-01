using PlatformAudio.Enums;

namespace PlatformAudio.Interfaces;

/// <summary>
/// Интерфейс звукового буфера
/// </summary>
public interface ISoundBuffer : IDisposable
{
    /// <summary>Нативный хендл</summary>
    IntPtr Handle { get; }

    /// <summary>Формат аудио</summary>
    AudioFormat Format { get; }

    /// <summary>Частота дискретизации (Гц)</summary>
    int SampleRate { get; }

    /// <summary>Длительность в миллисекундах</summary>
    int DurationMs { get; }

    /// <summary>Размер в байтах</summary>
    int Size { get; }

    /// <summary>Установить данные (byte)</summary>
    void SetData(byte[] data, AudioFormat format, int sampleRate);

    /// <summary>Установить данные (float)</summary>
    void SetData(float[] data, AudioFormat format, int sampleRate);
}