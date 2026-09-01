using System.Numerics;

namespace PlatformRender.Camera;

/// <summary>
/// Ортографическая камера с дополнительными функциями
/// </summary>
public sealed class OrthographicCamera(
    float left = -10f, float right = 10f,
    float bottom = -10f, float top = 10f,
    float near = -100f, float far = 100f) : Camera
{
    private float CameraLeft = left;
    private float CameraRight = right;
    private float CameraBottom = bottom;
    private float CameraTop = top;
    private float CameraNear = near;
    private float CameraFar = far;
    private Matrix4x4 CameraProjectionMatrix = Matrix4x4.Identity;
    private bool ProjectionDirty = true;

    // ---- Свойства ----

    public float Left { get => CameraLeft; set { CameraLeft = value; ProjectionDirty = true; } }
    public float Right { get => CameraRight; set { CameraRight = value; ProjectionDirty = true; } }
    public float Bottom { get => CameraBottom; set { CameraBottom = value; ProjectionDirty = true; } }
    public float Top { get => CameraTop; set { CameraTop = value; ProjectionDirty = true; } }
    public float Near { get => CameraNear; set { CameraNear = value; ProjectionDirty = true; } }
    public float Far { get => CameraFar; set { CameraFar = value; ProjectionDirty = true; } }

    public Matrix4x4 ProjectionMatrix
    {
        get
        {
            if (ProjectionDirty)
                UpdateProjectionMatrix();
            return CameraProjectionMatrix;
        }
    }

    // ---- Уникальные методы ----

    /// <summary>
    /// Автоматически подстраивает размеры под экран
    /// </summary>
    public void FitScreen(float screenWidth, float screenHeight, float unitsPerPixel = 1f)
    {
        float aspect = screenWidth / screenHeight;
        float size = Math.Max(screenWidth, screenHeight) * unitsPerPixel * 0.5f;

        if (aspect > 1)
        {
            Left = -size * aspect;
            Right = size * aspect;
            Bottom = -size;
            Top = size;
        }
        else
        {
            Left = -size;
            Right = size;
            Bottom = -size / aspect;
            Top = size / aspect;
        }

        ProjectionDirty = true;
    }

    /// <summary>
    /// Изменяет масштаб (zoom)
    /// </summary>
    public void Zoom(float factor)
    {
        float centerX = (Left + Right) / 2f;
        float centerY = (Bottom + Top) / 2f;
        float halfWidth = (Right - Left) / 2f * factor;
        float halfHeight = (Top - Bottom) / 2f * factor;

        Left = centerX - halfWidth;
        Right = centerX + halfWidth;
        Bottom = centerY - halfHeight;
        Top = centerY + halfHeight;

        ProjectionDirty = true;
    }

    /// <summary>
    /// Сбросить настройки проекции по умолчанию
    /// </summary>
    public void Reset()
    {
        Left = -10f;
        Right = 10f;
        Bottom = -10f;
        Top = 10f;
        Near = -100f;
        Far = 100f;
        ProjectionDirty = true;
    }

    /// <summary>
    /// Установить размеры окна просмотра
    /// </summary>
    public void SetSize(float width, float height)
    {
        float halfW = width / 2f;
        float halfH = height / 2f;
        Left = -halfW;
        Right = halfW;
        Bottom = -halfH;
        Top = halfH;
        ProjectionDirty = true;
    }

    // ---- Приватные методы ----

    private void UpdateProjectionMatrix()
    {
        CameraProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(
            CameraLeft, CameraRight,
            CameraBottom, CameraTop,
            CameraNear, CameraFar
        );
        ProjectionDirty = false;
    }
}