namespace PlatformAudio.Enums;

/// <summary>
/// Модели расстояния для 3D звука
/// </summary>
public enum DistanceModel
{
    /// <summary>Без затухания</summary>
    None,

    /// <summary>Обратно-пропорциональная (1/distance)</summary>
    InverseDistance,

    /// <summary>Обратно-пропорциональная с ограничением</summary>
    InverseDistanceClamped,

    /// <summary>Линейная</summary>
    LinearDistance,

    /// <summary>Линейная с ограничением</summary>
    LinearDistanceClamped,

    /// <summary>Экспоненциальная</summary>
    ExponentDistance,

    /// <summary>Экспоненциальная с ограничением</summary>
    ExponentDistanceClamped
}