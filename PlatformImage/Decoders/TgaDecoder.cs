using PlatformImage.Core;
using PlatformImage.IO;
using PlatformImage.Utils;

namespace PlatformImage.Decoders;

/// <summary>
/// Декодер TGA изображений (поддерживает 24-bit и 32-bit, включая RLE)
/// Реализован согласно TrueVision TGA Specification
/// </summary>
public class TgaDecoder : IImageDecoder
{
    public bool CanDecode(Stream stream)
    {
        if (!stream.CanSeek)
            throw new ArgumentException("Stream must be seekable", nameof(stream));

        var position = stream.Position;
        try
        {
            stream.Position = 0;

            if (stream.Length < 18)
                return false;

            var header = ReadHeader(stream);

            bool isValid = (header.ImageType == 2 || header.ImageType == 10) &&
                          (header.BitsPerPixel == 24 || header.BitsPerPixel == 32) &&
                          header.Width > 0 &&
                          header.Height > 0;

            stream.Position = position;
            return isValid;
        }
        catch
        {
            stream.Position = position;
            return false;
        }
    }

    public ImageData Decode(Stream stream)
    {
        if (!stream.CanSeek)
            throw new ArgumentException("Stream must be seekable", nameof(stream));

        stream.Position = 0;

        var header = ReadHeader(stream);

        if (header.ImageType != 2 && header.ImageType != 10)
            throw new UnsupportedFormatException($"TGA image type {header.ImageType} is not supported. Only uncompressed (2) and RLE (10) are supported.");

        if (header.BitsPerPixel != 24 && header.BitsPerPixel != 32)
            throw new UnsupportedFormatException($"TGA with {header.BitsPerPixel} bits per pixel is not supported. Only 24 and 32-bit are supported.");

        PixelFormat pixelFormat = header.BitsPerPixel == 32 ? PixelFormat.RGBA : PixelFormat.RGB;
        int bytesPerPixel = pixelFormat.GetBytesPerPixel();
        int pixelCount = header.Width * header.Height;
        int totalBytes = pixelCount * bytesPerPixel;

        if (header.IDLength > 0)
            stream.ReadBytes(header.IDLength);

        if (header.ColorMapType == 1)
        {
            int paletteSize = header.ColorMapLength * (header.ColorMapDepth / 8);
            stream.ReadBytes(paletteSize);
        }

        byte[] data = new byte[totalBytes];

        if (header.ImageType == 2)
        {
            ReadUncompressed(stream, data, totalBytes);
        }
        else if (header.ImageType == 10)
        {
            ReadRle(stream, data, pixelCount, bytesPerPixel);
        }

        // TGA хранит цвета в BGR, конвертируем в RGB
        ConvertBgrToRgb(data, bytesPerPixel);

        // Применяем вертикальный переворот, если нужно
        if (ImageLoader.FlipVerticallyOnLoad)
        {
            FlipVertical(data, header.Width, header.Height, bytesPerPixel);
        }

        return new ImageData(header.Width, header.Height, pixelFormat, data);
    }

    public ImageInfo GetInfo(Stream stream)
    {
        if (!stream.CanSeek)
            throw new ArgumentException("Stream must be seekable", nameof(stream));

        var position = stream.Position;
        try
        {
            stream.Position = 0;
            var header = ReadHeader(stream);

            if (header.BitsPerPixel != 24 && header.BitsPerPixel != 32)
                throw new UnsupportedFormatException($"TGA with {header.BitsPerPixel} bpp not supported");

            PixelFormat format = header.BitsPerPixel == 32 ? PixelFormat.RGBA : PixelFormat.RGB;
            return new ImageInfo(header.Width, header.Height, format);
        }
        finally
        {
            stream.Position = position;
        }
    }

    #region Структуры и вспомогательные методы

    private struct TgaHeader
    {
        public byte IDLength;
        public byte ColorMapType;
        public byte ImageType;
        public ushort ColorMapStart;
        public ushort ColorMapLength;
        public byte ColorMapDepth;
        public ushort XOrigin;
        public ushort YOrigin;
        public ushort Width;
        public ushort Height;
        public byte BitsPerPixel;
        public byte ImageDescriptor;
    }

    private static TgaHeader ReadHeader(Stream stream)
    {
        return new TgaHeader
        {
            IDLength = stream.ReadBytes(1)[0],
            ColorMapType = stream.ReadBytes(1)[0],
            ImageType = stream.ReadBytes(1)[0],
            ColorMapStart = stream.ReadUInt16LE(),
            ColorMapLength = stream.ReadUInt16LE(),
            ColorMapDepth = stream.ReadBytes(1)[0],
            XOrigin = stream.ReadUInt16LE(),
            YOrigin = stream.ReadUInt16LE(),
            Width = stream.ReadUInt16LE(),
            Height = stream.ReadUInt16LE(),
            BitsPerPixel = stream.ReadBytes(1)[0],
            ImageDescriptor = stream.ReadBytes(1)[0]
        };
    }

    private static void ReadUncompressed(Stream stream, byte[] data, int totalBytes)
    {
        int bytesRead = 0;
        while (bytesRead < totalBytes)
        {
            int read = stream.Read(data, bytesRead, totalBytes - bytesRead);
            if (read == 0)
                throw new EndOfStreamException("Unexpected end of TGA stream");
            bytesRead += read;
        }
    }

    private static void ReadRle(Stream stream, byte[] data, int pixelCount, int bytesPerPixel)
    {
        int dataIndex = 0;
        int pixelIndex = 0;

        while (pixelIndex < pixelCount)
        {
            // Проверяем, что в потоке есть данные
            if (stream.Position >= stream.Length)
                throw new EndOfStreamException($"Unexpected end of TGA stream at pixel {pixelIndex}/{pixelCount}");

            byte packet = stream.ReadBytes(1)[0];
            bool isRle = (packet & 0x80) != 0;
            int count = (packet & 0x7F) + 1;

            // Ограничиваем count, чтобы не выйти за пределы
            if (pixelIndex + count > pixelCount)
                count = pixelCount - pixelIndex;

            if (isRle)
            {
                // Читаем один пиксель
                byte[] pixel = stream.ReadBytes(bytesPerPixel);

                // Повторяем его count раз
                for (int i = 0; i < count; i++)
                {
                    Array.Copy(pixel, 0, data, dataIndex, bytesPerPixel);
                    dataIndex += bytesPerPixel;
                    pixelIndex++;
                }
            }
            else
            {
                // Читаем count пикселей без сжатия
                int bytesToRead = count * bytesPerPixel;
                int bytesRead = 0;
                while (bytesRead < bytesToRead)
                {
                    int read = stream.Read(data, dataIndex + bytesRead, bytesToRead - bytesRead);
                    if (read == 0)
                        throw new EndOfStreamException($"Unexpected end of TGA stream during RAW packet");
                    bytesRead += read;
                }
                dataIndex += bytesToRead;
                pixelIndex += count;
            }
        }
    }

    private static void ConvertBgrToRgb(byte[] data, int bytesPerPixel)
    {
        for (int i = 0; i < data.Length; i += bytesPerPixel)
        {
            // Меняем местами B и R (BGR -> RGB) с использованием кортежа
            (data[i + 2], data[i]) = (data[i], data[i + 2]);
        }
    }

    private static void FlipVertical(byte[] data, int width, int height, int bytesPerPixel)
    {
        int stride = width * bytesPerPixel;
        byte[] rowBuffer = new byte[stride];

        for (int y = 0; y < height / 2; y++)
        {
            int topRow = y * stride;
            int bottomRow = (height - 1 - y) * stride;

            Array.Copy(data, topRow, rowBuffer, 0, stride);
            Array.Copy(data, bottomRow, data, topRow, stride);
            Array.Copy(rowBuffer, 0, data, bottomRow, stride);
        }
    }

    #endregion
}