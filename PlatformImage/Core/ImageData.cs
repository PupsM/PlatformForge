using System.Buffers;

namespace PlatformImage.Core;

/// <summary>
/// Данные изображения
/// </summary>
public sealed class ImageData : IDisposable
{
    private byte[]? DataImage;
    private bool Disposed;
    private readonly bool OwnsData;

    public int Width { get; }
    public int Height { get; }
    public PixelFormat Format { get; }
    public int BytesPerPixel => Format.GetBytesPerPixel();
    public int Stride => Width * BytesPerPixel;
    public int TotalBytes => Width * Height * BytesPerPixel;
    public bool HasAlpha => Format == PixelFormat.RGBA;

    public ReadOnlySpan<byte> Data => DataImage ?? [];

    public ImageData(int width, int height, PixelFormat format, byte[] data, bool ownsData = true)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        ArgumentNullException.ThrowIfNull(data);

        int expectedSize = width * height * format.GetBytesPerPixel();
        if (data.Length != expectedSize)
            throw new ArgumentException($"Data size mismatch. Expected {expectedSize}, got {data.Length}");

        Width = width;
        Height = height;
        Format = format;
        DataImage = data;
        OwnsData = ownsData;
    }

    /// <summary>
    /// Создает копию данных
    /// </summary>
    public ImageData Copy()
    {
        var copy = new byte[TotalBytes];
        if (DataImage is not null)
            Array.Copy(DataImage, copy, TotalBytes);
        return new ImageData(Width, Height, Format, copy, true);
    }

    public void Dispose()
    {
        if (Disposed) return;

        if (OwnsData && DataImage is not null)
        {
            // Если массив был создан через new[], GC сам освободит
            // Если используешь ArrayPool, можно вернуть в пул
            DataImage = null;
        }

        Disposed = true;
        GC.SuppressFinalize(this);
    }
}