namespace PlatformInput.Events;

/// <summary>
/// Аргументы события скролла мыши
/// </summary>
public class MouseScrollEventArgs(double xOffset, double yOffset) : EventArgs
{
    public double XOffset { get; } = xOffset;
    public double YOffset { get; } = yOffset;
}