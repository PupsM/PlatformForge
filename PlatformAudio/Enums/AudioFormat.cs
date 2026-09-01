namespace PlatformAudio.Enums;

/// <summary>
/// Форматы аудио (совместимость с OpenAL)
/// </summary>
public enum AudioFormat
{
    /// <summary>Моно, 8 бит</summary>
    Mono8,

    /// <summary>Моно, 16 бит</summary>
    Mono16,

    /// <summary>Стерео, 8 бит</summary>
    Stereo8,

    /// <summary>Стерео, 16 бит</summary>
    Stereo16,

    /// <summary>Моно, float (32 бит)</summary>
    MonoFloat,

    /// <summary>Стерео, float (32 бит)</summary>
    StereoFloat
}