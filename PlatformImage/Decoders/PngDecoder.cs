using PlatformImage.Core;
using PlatformImage.IO;
using PlatformImage.Utils;
using System.IO.Compression;
using System.Text;

namespace PlatformImage.Decoders;

/// <summary>
/// Декодер PNG изображений (поддерживает 24-bit RGB и 32-bit RGBA)
/// Реализован согласно спецификации PNG (RFC 2083)
/// </summary>
public class PngDecoder : IImageDecoder
{
    private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];

    public bool CanDecode(Stream stream)
    {
        if (!stream.CanSeek)
            throw new ArgumentException("Stream must be seekable", nameof(stream));

        var position = stream.Position;
        try
        {
            stream.Position = 0;

            if (stream.Length < 8)
                return false;

            var signature = stream.ReadBytes(8);
            stream.Position = position;

            if (signature.Length != 8)
                return false;

            for (int i = 0; i < 8; i++)
            {
                if (signature[i] != PngSignature[i])
                    return false;
            }

            return true;
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

        // Проверяем сигнатуру
        var signature = stream.ReadBytes(8);
        for (int i = 0; i < 8; i++)
        {
            if (signature[i] != PngSignature[i])
                throw new UnsupportedFormatException("Not a PNG file (invalid signature)");
        }

        // Читаем чанки
        int width = 0, height = 0;
        byte bitDepth = 0;
        byte colorType = 0;

        // Собираем все IDAT данные
        using var idatStream = new MemoryStream();

        while (true)
        {
            uint length = stream.ReadUInt32BE();
            byte[] typeBytes = stream.ReadBytes(4);
            string chunkType = Encoding.ASCII.GetString(typeBytes);
            byte[] chunkData = stream.ReadBytes((int)length);
            stream.ReadBytes(4); // CRC

            if (chunkType == "IHDR")
            {
                ParseIHDR(chunkData, out width, out height, out bitDepth, out colorType);
            }
            else if (chunkType == "IDAT")
            {
                idatStream.Write(chunkData, 0, chunkData.Length);
            }
            else if (chunkType == "IEND")
            {
                break;
            }
            else if (chunkType == "gAMA" || chunkType == "sRGB" || chunkType == "iCCP" || chunkType == "pHYs")
            {
                // Игнорируем с предупреждением (не критично для загрузки)
                Diagnostics.Debug($"PNG: игнорируем чанк {chunkType}");
            }
            // Остальные чанки игнорируем
        }

        // Проверки валидности
        if (width == 0 || height == 0)
            throw new ImageLoaderException("Invalid PNG: missing IHDR chunk");

        if (idatStream.Length == 0)
            throw new ImageLoaderException("Invalid PNG: no IDAT chunks found");

        // Проверяем, что данные не пустые
        byte[] compressedData = idatStream.ToArray();
        if (compressedData.Length == 0)
            throw new ImageLoaderException("Invalid PNG: IDAT data is empty");

        if (colorType != 2 && colorType != 6)
            throw new UnsupportedFormatException($"PNG color type {colorType} is not supported. Only RGB (2) and RGBA (6) are supported.");

        if (bitDepth != 8)
            throw new UnsupportedFormatException($"PNG bit depth {bitDepth} is not supported. Only 8-bit is supported.");

        PixelFormat pixelFormat = colorType == 6 ? PixelFormat.RGBA : PixelFormat.RGB;
        int bytesPerPixel = pixelFormat.GetBytesPerPixel();

        // Распаковываем данные с правильным zlib заголовком
        byte[] decompressedData = DecompressZlib(compressedData);

        // Применяем фильтры
        byte[] result = ApplyFilters(decompressedData, width, height, bytesPerPixel);

        if (ImageLoader.FlipVerticallyOnLoad)
        {
            FlipVertical(result, width, height, bytesPerPixel);
        }

        return new ImageData(width, height, pixelFormat, result);
    }

    public ImageInfo GetInfo(Stream stream)
    {
        if (!stream.CanSeek)
            throw new ArgumentException("Stream must be seekable", nameof(stream));

        var position = stream.Position;
        try
        {
            stream.Position = 0;

            var signature = stream.ReadBytes(8);
            for (int i = 0; i < 8; i++)
            {
                if (signature[i] != PngSignature[i])
                    throw new UnsupportedFormatException("Not a PNG file");
            }

            while (true)
            {
                uint length = stream.ReadUInt32BE();
                string chunkType = Encoding.ASCII.GetString(stream.ReadBytes(4));

                if (chunkType == "IHDR")
                {
                    byte[] chunkData = stream.ReadBytes((int)length);
                    stream.ReadBytes(4);

                    int width = (chunkData[0] << 24) | (chunkData[1] << 16) | (chunkData[2] << 8) | chunkData[3];
                    int height = (chunkData[4] << 24) | (chunkData[5] << 16) | (chunkData[6] << 8) | chunkData[7];
                    byte colorType = chunkData[9];

                    if (colorType != 2 && colorType != 6)
                        throw new UnsupportedFormatException($"PNG color type {colorType} not supported");

                    PixelFormat format = colorType == 6 ? PixelFormat.RGBA : PixelFormat.RGB;
                    return new ImageInfo(width, height, format);
                }
                else
                {
                    stream.ReadBytes((int)length);
                    stream.ReadBytes(4);
                }
            }
        }
        finally
        {
            stream.Position = position;
        }
    }

    #region Вспомогательные методы

    private static void ParseIHDR(byte[] data, out int width, out int height, out byte bitDepth, out byte colorType)
    {
        width = (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3];
        height = (data[4] << 24) | (data[5] << 16) | (data[6] << 8) | data[7];
        bitDepth = data[8];
        colorType = data[9];
        byte interlaceMethod = data[12];

        if (interlaceMethod != 0)
            throw new UnsupportedFormatException("Adam7 interlacing is not supported");
    }

    /// <summary>
    /// Распаковывает zlib-сжатые данные (с правильным заголовком)
    /// </summary>
    private static byte[] DecompressZlib(byte[] compressedData)
    {
        using var inputStream = new MemoryStream(compressedData);

        // Проверяем zlib заголовок
        if (inputStream.Length < 2)
            throw new ImageLoaderException("Invalid zlib data: too short");

        byte cmf = (byte)inputStream.ReadByte();
        byte flg = (byte)inputStream.ReadByte();

        // Проверяем, что это zlib данные
        if ((cmf & 0x0F) != 8)
            throw new ImageLoaderException("Invalid zlib data: unsupported compression method");

        // Пропускаем заголовок и распаковываем
        using var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress, leaveOpen: true);
        using var outputStream = new MemoryStream();

        deflateStream.CopyTo(outputStream);

        return outputStream.ToArray();
    }

    private static byte[] ApplyFilters(byte[] data, int width, int height, int bytesPerPixel)
    {
        int stride = width * bytesPerPixel;
        byte[] result = new byte[width * height * bytesPerPixel];

        int srcIndex = 0;
        int dstIndex = 0;

        for (int y = 0; y < height; y++)
        {
            if (srcIndex >= data.Length)
                throw new ImageLoaderException($"Unexpected end of data at row {y}");

            byte filterType = data[srcIndex++];
            byte[] currentRow = new byte[stride];

            switch (filterType)
            {
                case 0:
                    Array.Copy(data, srcIndex, currentRow, 0, stride);
                    break;

                case 1:
                    for (int x = 0; x < stride; x++)
                    {
                        byte left = x >= bytesPerPixel ? currentRow[x - bytesPerPixel] : (byte)0;
                        currentRow[x] = (byte)((data[srcIndex + x] + left) & 0xFF);
                    }
                    break;

                case 2:
                    for (int x = 0; x < stride; x++)
                    {
                        byte above = y > 0 ? result[(y - 1) * stride + x] : (byte)0;
                        currentRow[x] = (byte)((data[srcIndex + x] + above) & 0xFF);
                    }
                    break;

                case 3:
                    for (int x = 0; x < stride; x++)
                    {
                        byte left = x >= bytesPerPixel ? currentRow[x - bytesPerPixel] : (byte)0;
                        byte above = y > 0 ? result[(y - 1) * stride + x] : (byte)0;
                        currentRow[x] = (byte)((data[srcIndex + x] + (byte)((left + above) / 2)) & 0xFF);
                    }
                    break;

                case 4:
                    for (int x = 0; x < stride; x++)
                    {
                        byte left = x >= bytesPerPixel ? currentRow[x - bytesPerPixel] : (byte)0;
                        byte above = y > 0 ? result[(y - 1) * stride + x] : (byte)0;
                        byte upperLeft = (x >= bytesPerPixel && y > 0) ? result[(y - 1) * stride + (x - bytesPerPixel)] : (byte)0;

                        byte paeth = PaethPredictor(left, above, upperLeft);
                        currentRow[x] = (byte)((data[srcIndex + x] + paeth) & 0xFF);
                    }
                    break;

                default:
                    throw new ImageLoaderException($"Unknown PNG filter type: {filterType}");
            }

            Array.Copy(currentRow, 0, result, dstIndex, stride);
            srcIndex += stride;
            dstIndex += stride;
        }

        return result;
    }

    // Paeth Predictor as defined in PNG specification (RFC 2083)
    // https://www.w3.org/TR/PNG-Filters.html
    private static byte PaethPredictor(byte a, byte b, byte c)
    {
        int p = a + b - c;
        int pa = Math.Abs(p - a);
        int pb = Math.Abs(p - b);
        int pc = Math.Abs(p - c);

        if (pa <= pb && pa <= pc)
            return a;
        else if (pb <= pc)
            return b;
        else
            return c;
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