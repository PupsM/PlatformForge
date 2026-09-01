namespace PlatformEngine.Core;

/// <summary>
/// Аргументы события кадра
/// </summary>
public class FrameEventArgs(float deltaTime, float totalTime) : EventArgs
{
    public float DeltaTime { get; } = deltaTime;
    public float TotalTime { get; } = totalTime;
}

/// <summary>
/// Аргументы события изменения размера окна
/// </summary>
public class ResizeEventArgs(int width, int height) : EventArgs
{
    public int Width { get; } = width;
    public int Height { get; } = height;
}