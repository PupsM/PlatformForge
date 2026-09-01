using PlatformAudio.Enums;

namespace PlatformAudio.Interfaces;

/// <summary>
/// Интерфейс аудиосистемы
/// </summary>
public interface IAudio : IDisposable
{
    /// <summary>Имя бэкенда</summary>
    string Name { get; }

    /// <summary>Инициализирован ли бэкенд</summary>
    bool IsInitialized { get; }

    /// <summary>Инициализация аудиосистемы</summary>
    void Initialize();

    /// <summary>Обновление (для потоков)</summary>
    void Update();

    /// <summary>Создать источник звука</summary>
    ISoundSource CreateSource();

    /// <summary>Уничтожить источник</summary>
    void DestroySource(ISoundSource source);

    /// <summary>Создать буфер</summary>
    ISoundBuffer CreateBuffer();

    /// <summary>Уничтожить буфер</summary>
    void DestroyBuffer(ISoundBuffer buffer);

    // ---- Слушатель ----
    void SetListenerPosition(float x, float y, float z);
    void SetListenerOrientation(float atX, float atY, float atZ, float upX, float upY, float upZ);
    void SetListenerVelocity(float x, float y, float z);

    // ---- Глобальные настройки ----
    void SetDistanceModel(DistanceModel model);
    void SetDopplerFactor(float factor);
    void SetSpeedOfSound(float speed);
}