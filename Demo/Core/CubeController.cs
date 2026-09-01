using PlatformEngine.Core;
using PlatformInput.Enums;
using PlatformRender.Graphics;
using System.Numerics;

namespace Demo.Core;

public class CubeController(Mesh cube)
{
    private readonly Mesh Cube = cube;
    private Vector3 Position = Vector3.Zero;
    private Vector3 Rotation = Vector3.Zero;
    private Vector3 Scale = Vector3.One;
    private readonly float RotationSpeed = 1.5f;
    private readonly float MoveSpeed = 3.0f;
    private readonly float ScaleSpeed = 0.5f;

    private bool IsDragging = false;
    private Vector2 LastMousePos;

    public void Update(float deltaTime)
    {
        HandleKeyboard(deltaTime);
        HandleMouse();
        HandleGamepad(); // Для будущего
    }

    private void HandleKeyboard(float deltaTime)
    {
        var input = Engine.Input;

        // ---- Вращение ----
        if (input.IsKeyDown(Key.Right)) Rotation.Y -= RotationSpeed * deltaTime * 90f;
        if (input.IsKeyDown(Key.Left)) Rotation.Y += RotationSpeed * deltaTime * 90f;
        if (input.IsKeyDown(Key.Up)) Rotation.X -= RotationSpeed * deltaTime * 90f;
        if (input.IsKeyDown(Key.Down)) Rotation.X += RotationSpeed * deltaTime * 90f;
        if (input.IsKeyDown(Key.Q)) Rotation.Z -= RotationSpeed * deltaTime * 90f;
        if (input.IsKeyDown(Key.E)) Rotation.Z += RotationSpeed * deltaTime * 90f;

        // ---- Перемещение ----
        var move = Vector3.Zero;
        if (input.IsKeyDown(Key.W)) move.Z -= MoveSpeed * deltaTime;
        if (input.IsKeyDown(Key.S)) move.Z += MoveSpeed * deltaTime;
        if (input.IsKeyDown(Key.A)) move.X -= MoveSpeed * deltaTime;
        if (input.IsKeyDown(Key.D)) move.X += MoveSpeed * deltaTime;
        if (input.IsKeyDown(Key.Space)) move.Y += MoveSpeed * deltaTime;
        if (input.IsKeyDown(Key.LeftShift)) move.Y -= MoveSpeed * deltaTime;
        Position += move;

        // ---- Масштабирование ----
        if (input.IsKeyDown(Key.KPAdd)) Scale += Vector3.One * ScaleSpeed * deltaTime;
        if (input.IsKeyDown(Key.KPSubtract)) Scale -= Vector3.One * ScaleSpeed * deltaTime;
        Scale = Vector3.Clamp(Scale, new Vector3(0.1f), new Vector3(5f));

        // ---- Сброс ----
        if (input.IsKeyDown(Key.R)) Rotation = Vector3.Zero;
        if (input.IsKeyDown(Key.Home)) Position = Vector3.Zero;
        if (input.IsKeyDown(Key.KP0)) Scale = Vector3.One;
    }

    private void HandleMouse()
    {
        var input = Engine.Input;
        input.GetCursorPos(out double x, out double y);
        var mousePos = new Vector2((float)x, (float)y);

        var leftButton = input.IsMouseButtonDown(MouseButton.Left);
        var rightButton = input.IsMouseButtonDown(MouseButton.Right);

        // ---- Левая кнопка: перетаскивание ----
        if (leftButton && !IsDragging)
        {
            IsDragging = true;
            LastMousePos = mousePos;
        }

        if (IsDragging && leftButton)
        {
            var delta = mousePos - LastMousePos;
            Position.X += delta.X * 0.01f;
            Position.Y -= delta.Y * 0.01f;
            LastMousePos = mousePos;
        }

        if (!leftButton && IsDragging)
        {
            IsDragging = false;
        }

        // ---- Правая кнопка: вращение камеры ----
        if (rightButton)
        {
            // Вращение камеры (будет реализовано в MainScene)
        }
    }

    private static void HandleGamepad()  // <- ИСПРАВЛЕНО: добавлен static
    {
        // Заглушка для будущей поддержки геймпадов
    }

    public Matrix4x4 GetModelMatrix()
    {
        var translation = Matrix4x4.CreateTranslation(Position);
        var rotation = Matrix4x4.CreateFromYawPitchRoll(
            DegreesToRadians(Rotation.Y),
            DegreesToRadians(Rotation.X),
            DegreesToRadians(Rotation.Z)
        );
        var scale = Matrix4x4.CreateScale(Scale);

        return scale * rotation * translation;
    }

    private static float DegreesToRadians(float degrees)
        => degrees * (float)Math.PI / 180f;
}