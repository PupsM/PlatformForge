namespace PlatformImage.Core;

/// <summary>
/// Формат пикселей изображения
/// </summary>
public enum PixelFormat
{
    /// <summary>
    /// RGB - 24 бита (3 байта на пиксель)
    /// </summary>
    RGB = 3,

    /// <summary>
    /// RGBA - 32 бита (4 байта на пиксель)
    /// </summary>
    RGBA = 4
}

/// <summary>
/// Расширения для PixelFormat
/// </summary>
public static class PixelFormatExtensions
{
    /// <summary>
    /// Получить количество байт на пиксель
    /// </summary>
    public static int GetBytesPerPixel(this PixelFormat format)
    {
        return format switch
        {
            PixelFormat.RGB => 3,
            PixelFormat.RGBA => 4,
            _ => throw new ArgumentException($"Unknown pixel format: {format}")
        };
    }
}