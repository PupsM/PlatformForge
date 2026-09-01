
namespace PlatformImage.Decoders;

/// <summary>
/// Фабрика для создания декодеров изображений
/// </summary>
public static class ImageDecoderFactory
{
    private static readonly List<IImageDecoder> Decoders =
    [
        new BmpDecoder(),
        new TgaDecoder(),
        new PngDecoder(),
    ];

    /// <summary>
    /// Получить подходящий декодер для потока
    /// </summary>
    public static IImageDecoder? GetDecoder(Stream stream)
    {
        if (!stream.CanSeek)
            throw new ArgumentException("Stream must be seekable", nameof(stream));

        var position = stream.Position;
        try
        {
            foreach (var decoder in Decoders)
            {
                if (decoder.CanDecode(stream))
                    return decoder;
            }
            return null;
        }
        finally
        {
            stream.Position = position;
        }
    }
}