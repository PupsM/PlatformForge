using PlatformInput.Enums;
using PlatformInput.Events;

namespace PlatformInput.Interfaces;

/// <summary>
/// Интерфейс системы ввода
/// </summary>
public interface IInput : IDisposable
{
    // ---- Клавиатура ----
    bool IsKeyDown(Key key);
    bool IsKeyPressed(Key key);
    bool IsKeyReleased(Key key);
    string? GetKeyName(Key key);

    // ---- Мышь ----
    bool IsMouseButtonDown(MouseButton button);
    bool IsMouseButtonPressed(MouseButton button);
    bool IsMouseButtonReleased(MouseButton button);
    void GetCursorPos(out double x, out double y);
    void SetCursorPos(double x, double y);
    void SetCursorMode(CursorMode mode);

    // ---- Обновление ----
    void Update();

    // ---- События ----
    event EventHandler<KeyEventArgs>? KeyDown;
    event EventHandler<KeyEventArgs>? KeyUp;
    event EventHandler<char>? CharInput;
    event EventHandler<MouseEventArgs>? MouseDown;
    event EventHandler<MouseEventArgs>? MouseUp;
    event EventHandler<MouseMoveEventArgs>? MouseMove;
    event EventHandler<MouseScrollEventArgs>? MouseScroll;
}