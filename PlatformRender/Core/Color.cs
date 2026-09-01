namespace PlatformRender.Core;

/// <summary>
/// Представление цвета
/// </summary>
public struct Color(float r, float g, float b, float a = 1.0f)
{
    public float R = r, G = g, B = b, A = a;

    public static Color White => new(1f, 1f, 1f, 1f);
    public static Color Black => new(0f, 0f, 0f, 1f);
    public static Color Red => new(1f, 0f, 0f, 1f);
    public static Color Green => new(0f, 1f, 0f, 1f);
    public static Color Blue => new(0f, 0f, 1f, 1f);
    public static Color CornflowerBlue => new(0.392f, 0.584f, 0.929f, 1f);
    public static Color Transparent => new(0f, 0f, 0f, 0f);
}