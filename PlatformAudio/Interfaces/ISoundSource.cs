using PlatformAudio.Enums;

namespace PlatformAudio.Interfaces;

/// <summary>
/// Интерфейс источника звука
/// </summary>
public interface ISoundSource : IDisposable
{
    /// <summary>Нативный хендл</summary>
    IntPtr Handle { get; }

    /// <summary>Состояние источника</summary>
    SourceState State { get; }

    /// <summary>Громкость (0.0 - 1.0)</summary>
    float Gain { get; set; }

    /// <summary>Высота тона (0.5 - 2.0)</summary>
    float Pitch { get; set; }

    /// <summary>Зацикливание</summary>
    bool Looping { get; set; }

    /// <summary>Позиция в 3D</summary>
    (float X, float Y, float Z) Position { get; set; }

    /// <summary>Скорость в 3D</summary>
    (float X, float Y, float Z) Velocity { get; set; }

    /// <summary>Параметры расстояния</summary>
    float ReferenceDistance { get; set; }
    float MaxDistance { get; set; }
    float RolloffFactor { get; set; }

    /// <summary>Конус направленности</summary>
    float ConeInnerAngle { get; set; }
    float ConeOuterAngle { get; set; }
    float ConeOuterGain { get; set; }

    /// <summary>Управление воспроизведением</summary>
    void Play();
    void Pause();
    void Stop();
    void Rewind();

    /// <summary>Привязать буфер</summary>
    void BindBuffer(ISoundBuffer buffer);

    /// <summary>Отвязать буфер</summary>
    void UnbindBuffer();

    /// <summary>Очередь буферов (для потоков)</summary>
    void QueueBuffer(ISoundBuffer buffer);
    void UnqueueBuffer(ISoundBuffer buffer);
    void ClearQueue();

    /// <summary>Событие окончания воспроизведения</summary>
    event Action<ISoundSource>? PlaybackEnded;
}