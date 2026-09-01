using PlatformImage.Core;

namespace PlatformImage.Decoders;

/// <summary>
/// Интерфейс декодера изображений
/// </summary>
public interface IImageDecoder
{
    /// <summary>
    /// Проверяет, может ли декодер обработать данный поток
    /// </summary>
    bool CanDecode(Stream stream);

    /// <summary>
    /// Декодирует изображение из потока
    /// </summary>
    ImageData Decode(Stream stream);

    /// <summary>
    /// Получает информацию об изображении без полного декодирования
    /// </summary>
    ImageInfo GetInfo(Stream stream);
}

/// <summary>
/// Информация об изображении
/// </summary>
public readonly struct ImageInfo(int width, int height, PixelFormat format)
{
    public int Width { get; } = width;
    public int Height { get; } = height;
    public PixelFormat Format { get; } = format;
}