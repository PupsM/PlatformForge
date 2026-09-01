using System.Text;
using PlatformImage.Core;
using PlatformImage.IO;
using PlatformImage.Utils;

namespace PlatformImage.Decoders;

/// <summary>
/// Декодер BMP изображений (поддерживает только 24-bit и 32-bit)
/// </summary>
public class BmpDecoder : IImageDecoder
{
    private const string Signature = "BM";

    public bool CanDecode(Stream stream)
    {
        if (!stream.CanSeek)
            throw new ArgumentException("Stream must be seekable", nameof(stream));

        var position = stream.Position;
        try
        {
            var signature = stream.ReadBytes(2);
            stream.Position = position;

            if (signature.Length < 2)
                return false;

            return Encoding.ASCII.GetString(signature) == Signature;
        }
        catch
        {
            return false;
        }
    }

    public ImageData Decode(Stream stream)
    {
        if (!stream.CanSeek)
            throw new ArgumentException("Stream must be seekable", nameof(stream));

        stream.Position = 0;

        // Читаем заголовок BMP
        var signature = stream.ReadBytes(2);
        if (Encoding.ASCII.GetString(signature) != Signature)
            throw new UnsupportedFormatException("Not a BMP file (invalid signature)");

        // Пропускаем размер файла, зарезервированные поля и смещение данных
        stream.ReadBytes(4); // FileSize
        stream.ReadBytes(4); // Reserved1 + Reserved2
        int dataOffset = BitConverter.ToInt32(stream.ReadBytes(4), 0);

        // Читаем DIB-заголовок (размер заголовка)
        int headerSize = BitConverter.ToInt32(stream.ReadBytes(4), 0);

        // Поддерживаем только стандартные DIB-заголовки
        if (headerSize != 40 && headerSize != 108 && headerSize != 124)
            throw new UnsupportedFormatException($"Unsupported DIB header size: {headerSize}");

        // Читаем информацию об изображении
        int width = BitConverter.ToInt32(stream.ReadBytes(4), 0);
        int height = BitConverter.ToInt32(stream.ReadBytes(4), 0);
        stream.ReadBytes(2); // Пропускаем planes
        int bitsPerPixel = BitConverter.ToInt16(stream.ReadBytes(2), 0);

        // Проверяем, поддерживаем ли мы этот формат
        if (bitsPerPixel != 24 && bitsPerPixel != 32)
            throw new UnsupportedFormatException(
                $"BMP with {bitsPerPixel} bits per pixel is not supported. Only 24 and 32-bit are supported.");

        // Определяем формат пикселей
        PixelFormat pixelFormat = bitsPerPixel == 32 ? PixelFormat.RGBA : PixelFormat.RGB;
        int bytesPerPixel = pixelFormat.GetBytesPerPixel();

        // Если height отрицательный, изображение не перевернуто
        if (height < 0)
            height = -height;

        // Пропускаем остальные поля DIB-заголовка
        int skipBytes = headerSize - 4 - 4 - 4 - 4 - 2 - 2; // отнимаем уже прочитанные поля
        if (skipBytes > 0)
            stream.ReadBytes(skipBytes);

        // Перемещаемся к данным
        stream.Position = dataOffset;

        // Читаем пиксельные данные
        int stride = ((width * bitsPerPixel + 31) / 32) * 4; // Выравнивание строк до 4 байт
        byte[] data = new byte[width * height * bytesPerPixel];

        // BMP хранит строки снизу-вверх
        for (int y = 0; y < height; y++)
        {
            int destRow = ImageLoader.FlipVerticallyOnLoad ? height - 1 - y : y;

            byte[] rowData = stream.ReadBytes(stride);

            int srcOffset = 0;
            int destOffset = destRow * width * bytesPerPixel;

            for (int x = 0; x < width; x++)
            {
                // BMP хранит в порядке BGR, конвертируем в RGB
                data[destOffset + 0] = rowData[srcOffset + 2]; // R
                data[destOffset + 1] = rowData[srcOffset + 1]; // G
                data[destOffset + 2] = rowData[srcOffset + 0]; // B

                if (bytesPerPixel == 4 && bitsPerPixel == 32)
                {
                    data[destOffset + 3] = rowData[srcOffset + 3]; // A
                }
                else if (bytesPerPixel == 4 && bitsPerPixel == 24)
                {
                    data[destOffset + 3] = 255; // Нет альфы - ставим 255
                }

                srcOffset += bitsPerPixel / 8;
                destOffset += bytesPerPixel;
            }
        }

        return new ImageData(width, height, pixelFormat, data);
    }

    public ImageInfo GetInfo(Stream stream)
    {
        if (!stream.CanSeek)
            throw new ArgumentException("Stream must be seekable", nameof(stream));

        var position = stream.Position;
        try
        {
            stream.Position = 0;

            // Проверяем сигнатуру
            var signature = stream.ReadBytes(2);
            if (Encoding.ASCII.GetString(signature) != Signature)
                throw new UnsupportedFormatException("Not a BMP file");

            // Пропускаем заголовок и читаем DIB-заголовок
            stream.ReadBytes(8); // FileSize, Reserved1, Reserved2
            stream.ReadBytes(4); // DataOffset

            int headerSize = BitConverter.ToInt32(stream.ReadBytes(4), 0);
            if (headerSize != 40 && headerSize != 108 && headerSize != 124)
                throw new UnsupportedFormatException($"Unsupported DIB header size: {headerSize}");

            int width = BitConverter.ToInt32(stream.ReadBytes(4), 0);
            int height = BitConverter.ToInt32(stream.ReadBytes(4), 0);
            stream.ReadBytes(2); // Planes - пропускаем
            int bitsPerPixel = BitConverter.ToInt16(stream.ReadBytes(2), 0);

            if (bitsPerPixel != 24 && bitsPerPixel != 32)
                throw new UnsupportedFormatException($"BMP with {bitsPerPixel} bpp not supported");

            PixelFormat format = bitsPerPixel == 32 ? PixelFormat.RGBA : PixelFormat.RGB;
            return new ImageInfo(Math.Abs(width), Math.Abs(height), format);
        }
        finally
        {
            stream.Position = position;
        }
    }
}