using PlatformImage.Core;
using PlatformImage.Decoders;

namespace PlatformImage.IO;

/// <summary>
/// Основной класс для загрузки изображений
/// </summary>
public static class ImageLoader
{
    private static bool ImageFlipVerticallyOnLoad = true; // ← ИЗМЕНЕНО!

    /// <summary>
    /// Переворачивать изображение по вертикали при загрузке
    /// </summary>
    public static bool FlipVerticallyOnLoad
    {
        get => ImageFlipVerticallyOnLoad;
        set => ImageFlipVerticallyOnLoad = value;
    }

    /// <summary>
    /// Загрузить изображение из файла
    /// </summary>
    public static ImageData Load(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        using var stream = File.OpenRead(filePath);
        return Load(stream);
    }

    /// <summary>
    /// Загрузить изображение из потока
    /// </summary>
    public static ImageData Load(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
            throw new ArgumentException("Stream must be readable", nameof(stream));

        if (!stream.CanSeek)
            throw new ArgumentException("Stream must be seekable", nameof(stream));

        var decoder = ImageDecoderFactory.GetDecoder(stream) ?? throw new UnsupportedFormatException("Unsupported image format. Only BMP, TGA, and PNG are supported.");
        stream.Position = 0;
        return decoder.Decode(stream);
    }

    /// <summary>
    /// Получить информацию об изображении без полной загрузки
    /// </summary>
    public static ImageInfo GetInfo(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        using var stream = File.OpenRead(filePath);
        return GetInfo(stream);
    }

    /// <summary>
    /// Получить информацию об изображении из потока
    /// </summary>
    public static ImageInfo GetInfo(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
            throw new ArgumentException("Stream must be readable", nameof(stream));

        if (!stream.CanSeek)
            throw new ArgumentException("Stream must be seekable", nameof(stream));

        var decoder = ImageDecoderFactory.GetDecoder(stream) ?? throw new UnsupportedFormatException("Unsupported image format");
        stream.Position = 0;
        return decoder.GetInfo(stream);
    }
}