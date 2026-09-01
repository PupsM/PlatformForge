namespace PlatformImage.Utils;

/// <summary>
/// Расширения для работы с бинарными данными
/// </summary>
public static class BinaryReaderExtensions
{
    /// <summary>
    /// Читает указанное количество байт из потока
    /// </summary>
    public static byte[] ReadBytes(this Stream stream, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if (count == 0)
            return [];

        byte[] buffer = new byte[count];
        int totalRead = 0;

        while (totalRead < count)
        {
            int bytesRead = stream.Read(buffer, totalRead, count - totalRead);
            if (bytesRead == 0)
                throw new EndOfStreamException($"Unexpected end of stream. Expected {count} bytes, read {totalRead}");
            totalRead += bytesRead;
        }

        return buffer;
    }

    /// <summary>
    /// Читает 16-битное целое в little-endian
    /// </summary>
    public static short ReadInt16LE(this Stream stream)
    {
        var data = stream.ReadBytes(2);
        return BitConverter.ToInt16(data, 0);
    }

    /// <summary>
    /// Читает 32-битное целое в little-endian
    /// </summary>
    public static int ReadInt32LE(this Stream stream)
    {
        var data = stream.ReadBytes(4);
        return BitConverter.ToInt32(data, 0);
    }

    /// <summary>
    /// Читает 16-битное беззнаковое целое в little-endian
    /// </summary>
    public static ushort ReadUInt16LE(this Stream stream)
    {
        var data = stream.ReadBytes(2);
        return BitConverter.ToUInt16(data, 0);
    }

    /// <summary>
    /// Читает 32-битное беззнаковое целое в little-endian
    /// </summary>
    public static uint ReadUInt32LE(this Stream stream)
    {
        var data = stream.ReadBytes(4);
        return BitConverter.ToUInt32(data, 0);
    }

    /// <summary>
    /// Читает 32-битное беззнаковое целое в big-endian (для PNG)
    /// </summary>
    public static uint ReadUInt32BE(this Stream stream)
    {
        var data = stream.ReadBytes(4);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(data);
        return BitConverter.ToUInt32(data, 0);
    }
}