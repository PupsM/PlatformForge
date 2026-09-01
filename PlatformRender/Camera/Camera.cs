using System.Numerics;

namespace PlatformRender.Camera;

/// <summary>
/// Базовый класс камеры
/// </summary>
public class Camera
{
    protected Vector3 CameraPosition = Vector3.Zero;
    protected Vector3 CameraTarget = new(0, 0, -1);
    protected Vector3 CameraUp = Vector3.UnitY;
    protected Matrix4x4 CameraViewMatrix = Matrix4x4.Identity;
    protected bool ViewDirty = true;

    public Vector3 Position
    {
        get => CameraPosition;
        set { CameraPosition = value; ViewDirty = true; }
    }

    public Vector3 Target
    {
        get => CameraTarget;
        set { CameraTarget = value; ViewDirty = true; }
    }

    public Vector3 Up
    {
        get => CameraUp;
        set { CameraUp = value; ViewDirty = true; }
    }

    public Matrix4x4 ViewMatrix
    {
        get
        {
            if (ViewDirty)
                UpdateViewMatrix();
            return CameraViewMatrix;
        }
    }

    protected virtual void UpdateViewMatrix()
    {
        CameraViewMatrix = Matrix4x4.CreateLookAt(CameraPosition, CameraTarget, CameraUp);
        ViewDirty = false;
    }
}