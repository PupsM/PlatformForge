using System.Numerics;

namespace PlatformRender.Camera;

/// <summary>
/// Перспективная камера с дополнительными функциями
/// </summary>
public sealed class PerspectiveCamera(float fov = 60f, float aspect = 1.333f, float near = 0.1f, float far = 100f) : Camera
{
    private float Fov = fov;
    private float CameraAspect = aspect;
    private float CameraNear = near;
    private float CameraFar = far;
    private Matrix4x4 CameraProjectionMatrix = Matrix4x4.Identity;
    private bool ProjectionDirty = true;

    // ---- Для орбиты ----
    private float CameraOrbitDistance = 5f;
    private float CameraOrbitAngleX = 0f;
    private float CameraOrbitAngleY = 30f;
    private bool CameraOrbitMode = false;

    // ---- Свойства ----

    public float FOV { get => Fov; set { Fov = value; ProjectionDirty = true; } }
    public float Aspect { get => CameraAspect; set { CameraAspect = value; ProjectionDirty = true; } }
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

    public bool OrbitMode => CameraOrbitMode;
    public float OrbitDistance => CameraOrbitDistance;
    public float OrbitAngleX => CameraOrbitAngleX;
    public float OrbitAngleY => CameraOrbitAngleY;

    // ---- Уникальные методы ----

    /// <summary>
    /// Включить режим орбиты
    /// </summary>
    public void EnableOrbit(bool enable = true)
    {
        CameraOrbitMode = enable;
        if (enable)
        {
            // Вычисляем расстояние до цели
            CameraOrbitDistance = Vector3.Distance(Position, Target);
            // Вычисляем углы
            var dir = Vector3.Normalize(Target - Position);
            CameraOrbitAngleX = MathF.Atan2(dir.X, dir.Z) * 180f / MathF.PI;
            CameraOrbitAngleY = MathF.Asin(dir.Y) * 180f / MathF.PI;
        }
    }

    /// <summary>
    /// Орбитальное движение вокруг цели
    /// </summary>
    public void Orbit(float deltaAngleX, float deltaAngleY)
    {
        if (!CameraOrbitMode) EnableOrbit(true);

        CameraOrbitAngleX += deltaAngleX;
        CameraOrbitAngleY = Math.Clamp(CameraOrbitAngleY + deltaAngleY, -89f, 89f);

        UpdateOrbitPosition();
    }

    /// <summary>
    /// Приблизить/отдалить
    /// </summary>
    public void Zoom(float delta)
    {
        if (!CameraOrbitMode) EnableOrbit(true);
        CameraOrbitDistance = Math.Clamp(CameraOrbitDistance + delta, 0.1f, 100f);
        UpdateOrbitPosition();
    }

    /// <summary>
    /// Панорамирование (перемещение цели)
    /// </summary>
    public void Pan(float deltaX, float deltaY)
    {
        if (!CameraOrbitMode) EnableOrbit(true);

        // Вычисляем векторы направления
        var forward = Vector3.Normalize(Target - Position);
        var right = Vector3.Normalize(Vector3.Cross(forward, Up));
        var up = Vector3.Cross(right, forward);

        // Масштабируем перемещение в зависимости от расстояния
        float scale = CameraOrbitDistance * 0.01f;
        Target += right * deltaX * scale + up * deltaY * scale;

        UpdateOrbitPosition();
    }

    /// <summary>
    /// Сбросить орбиту
    /// </summary>
    public void ResetOrbit()
    {
        CameraOrbitDistance = 5f;
        CameraOrbitAngleX = 0f;
        CameraOrbitAngleY = 30f;
        Target = Vector3.Zero;
        UpdateOrbitPosition();
    }

    /// <summary>
    /// Установить цель и позицию вручную (выход из орбиты)
    /// </summary>
    public void LookAt(Vector3 target, Vector3? position = null)
    {
        CameraOrbitMode = false;
        Target = target;
        if (position.HasValue)
            Position = position.Value;
        ViewDirty = true;
    }

    /// <summary>
    /// Обновить aspect ratio (для изменения размера окна)
    /// </summary>
    public void UpdateAspect(int width, int height)
    {
        Aspect = (float)width / height;
    }

    // ---- Приватные методы ----

    private void UpdateOrbitPosition()
    {
        float radX = CameraOrbitAngleX * MathF.PI / 180f;
        float radY = CameraOrbitAngleY * MathF.PI / 180f;

        var pos = new Vector3(
            CameraOrbitDistance * MathF.Cos(radY) * MathF.Sin(radX),
            CameraOrbitDistance * MathF.Sin(radY),
            CameraOrbitDistance * MathF.Cos(radY) * MathF.Cos(radX)
        );

        Position = Target + pos;
        ViewDirty = true;
    }

    private void UpdateProjectionMatrix()
    {
        CameraProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
            Fov * (float)(Math.PI / 180f),
            CameraAspect,
            CameraNear,
            CameraFar
        );
        ProjectionDirty = false;
    }

    // ---- Переопределения ----

    protected override void UpdateViewMatrix()
    {
        if (CameraOrbitMode)
        {
            UpdateOrbitPosition();
        }
        base.UpdateViewMatrix();
    }
}