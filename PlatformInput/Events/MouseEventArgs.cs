using PlatformInput.Enums;

namespace PlatformInput.Events;

/// <summary>
/// Аргументы события мыши
/// </summary>
public class MouseEventArgs(MouseButton button, double x, double y, int mods) : EventArgs
{
    public MouseButton Button { get; } = button;
    public double X { get; } = x;
    public double Y { get; } = y;
    public int Mods { get; } = mods;
}