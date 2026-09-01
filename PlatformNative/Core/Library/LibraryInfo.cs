
namespace PlatformNative.Core.Library;

/// <summary>
/// Информация о загруженной библиотеке
/// </summary>
public sealed class LibraryInfo
{
    public required string[] Names { get; init; }
    public required Func<string, IntPtr> Resolver { get; init; }
    public IntPtr Handle { get; set; }
    public bool Loaded { get; set; }
}