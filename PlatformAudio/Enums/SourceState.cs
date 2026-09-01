namespace PlatformAudio.Enums;

/// <summary>
/// Состояния источника звука
/// </summary>
public enum SourceState
{
    /// <summary>Начальное состояние (не воспроизводится)</summary>
    Initial,

    /// <summary>Воспроизводится</summary>
    Playing,

    /// <summary>На паузе</summary>
    Paused,

    /// <summary>Остановлен</summary>
    Stopped
}