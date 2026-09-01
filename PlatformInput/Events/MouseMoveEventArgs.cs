namespace PlatformInput.Events;

/// <summary>
/// Аргументы события движения мыши
/// </summary>
public class MouseMoveEventArgs(double x, double y) : EventArgs
{
    public double X { get; } = x;
    public double Y { get; } = y;
}