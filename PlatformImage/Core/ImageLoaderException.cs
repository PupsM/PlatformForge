namespace PlatformImage.Core;

/// <summary>
/// Исключение, выбрасываемое при ошибках загрузки изображений
/// </summary>
public class ImageLoaderException : Exception
{
    public ImageLoaderException() { }

    public ImageLoaderException(string message) : base(message) { }

    public ImageLoaderException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Исключение, выбрасываемое при неподдерживаемом формате
/// </summary>
public class UnsupportedFormatException(string message) : ImageLoaderException(message) { }