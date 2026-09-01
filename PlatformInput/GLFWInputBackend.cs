using PlatformInput.Enums;
using PlatformInput.Events;
using PlatformInput.Interfaces;
using PlatformNative.Core;
using PlatformNative.Native;
using System.Runtime.InteropServices;

namespace PlatformInput;

/// <summary>
/// Бэкенд ввода через GLFW
/// </summary>
public sealed class GLFWInputBackend : IInput
{
    private readonly IntPtr Window;
    private readonly Dictionary<Key, bool> CurrentKeys = [];
    private readonly Dictionary<Key, bool> PreviousKeys = [];
    private readonly Dictionary<MouseButton, bool> CurrentMouse = [];
    private readonly Dictionary<MouseButton, bool> PreviousMouse = [];
    private readonly Lock KeysLock = new();
    private readonly Lock MouseLock = new();
    private double CursorX;
    private double CursorY;
    private bool Disposed;
    private bool CallbacksCleared;

    // Колбэки должны жить, чтобы GC их не собрал
    private GLFW.GlfwKeyFun? KeyCallback;
    private GLFW.GlfwCharFun? CharCallback;
    private GLFW.GlfwMouseButtonFun? MouseButtonCallback;
    private GLFW.GlfwCursorPosFun? CursorPosCallback;
    private GLFW.GlfwScrollFun? ScrollCallback;

    #region ---- События ----

    public event EventHandler<KeyEventArgs>? KeyDown;
    public event EventHandler<KeyEventArgs>? KeyUp;
    public event EventHandler<char>? CharInput;
    public event EventHandler<MouseEventArgs>? MouseDown;
    public event EventHandler<MouseEventArgs>? MouseUp;
    public event EventHandler<MouseMoveEventArgs>? MouseMove;
    public event EventHandler<MouseScrollEventArgs>? MouseScroll;

    #endregion

    #region ---- Конструктор ----

    public GLFWInputBackend(IntPtr window)
    {
        if (window == IntPtr.Zero)
            throw new ArgumentException("Window handle cannot be zero", nameof(window));

        Window = window;
        SetupCallbacks();
    }

    #endregion

    #region ---- Настройка колбэков ----

    private void SetupCallbacks()
    {
        if (!GLFW.IsInitialized)
        {
            Diagnostics.Warning("GLFW не инициализирован, колбэки не будут установлены");
            return;
        }

        // ---- Клавиатура ----
        KeyCallback = OnKey;
        if (GLFW.TryGetFunction<GLFW.glfwSetKeyCallbackDelegate>("glfwSetKeyCallback", out var setKey) && setKey is not null)
        {
            setKey(Window, KeyCallback);
        }

        // ---- Ввод символов ----
        CharCallback = OnChar;
        if (GLFW.TryGetFunction<GLFW.glfwSetCharCallbackDelegate>("glfwSetCharCallback", out var setChar) && setChar is not null)
        {
            setChar(Window, CharCallback);
        }

        // ---- Мышь (кнопки) ----
        MouseButtonCallback = OnMouseButton;
        if (GLFW.TryGetFunction<GLFW.glfwSetMouseButtonCallbackDelegate>("glfwSetMouseButtonCallback", out var setMouse) && setMouse is not null)
        {
            setMouse(Window, MouseButtonCallback);
        }

        // ---- Мышь (движение) ----
        CursorPosCallback = OnCursorPos;
        if (GLFW.TryGetFunction<GLFW.glfwSetCursorPosCallbackDelegate>("glfwSetCursorPosCallback", out var setCursor) && setCursor is not null)
        {
            setCursor(Window, CursorPosCallback);
        }

        // ---- Мышь (скролл) ----
        ScrollCallback = OnScroll;
        if (GLFW.TryGetFunction<GLFW.glfwSetScrollCallbackDelegate>("glfwSetScrollCallback", out var setScroll) && setScroll is not null)
        {
            setScroll(Window, ScrollCallback);
        }
    }

    #endregion

    #region ---- Обработчики колбэков ----

    private void OnKey(IntPtr window, int key, int scancode, int action, int mods)
    {
        var keyEnum = (Key)key;

        lock (KeysLock)
        {
            switch (action)
            {
                case GLFW.GLFW_PRESS:
                    CurrentKeys[keyEnum] = true;
                    KeyDown?.Invoke(this, new KeyEventArgs(keyEnum, scancode, mods, false));
                    break;

                case GLFW.GLFW_RELEASE:
                    CurrentKeys[keyEnum] = false;
                    KeyUp?.Invoke(this, new KeyEventArgs(keyEnum, scancode, mods, false));
                    break;

                case GLFW.GLFW_REPEAT:
                    KeyDown?.Invoke(this, new KeyEventArgs(keyEnum, scancode, mods, true));
                    break;
            }
        }
    }

    private void OnChar(IntPtr window, uint codepoint)
    {
        CharInput?.Invoke(this, (char)codepoint);
    }

    private void OnMouseButton(IntPtr window, int button, int action, int mods)
    {
        var btn = (MouseButton)button;

        lock (MouseLock)
        {
            if (action == GLFW.GLFW_PRESS)
            {
                CurrentMouse[btn] = true;
                MouseDown?.Invoke(this, new MouseEventArgs(btn, CursorX, CursorY, mods));
            }
            else if (action == GLFW.GLFW_RELEASE)
            {
                CurrentMouse[btn] = false;
                MouseUp?.Invoke(this, new MouseEventArgs(btn, CursorX, CursorY, mods));
            }
        }
    }

    private void OnCursorPos(IntPtr window, double xpos, double ypos)
    {
        CursorX = xpos;
        CursorY = ypos;
        MouseMove?.Invoke(this, new MouseMoveEventArgs(xpos, ypos));
    }

    private void OnScroll(IntPtr window, double xoffset, double yoffset)
    {
        MouseScroll?.Invoke(this, new MouseScrollEventArgs(xoffset, yoffset));
    }

    #endregion

    #region ---- Клавиатура ----

    public bool IsKeyDown(Key key)
    {
        lock (KeysLock)
        {
            return CurrentKeys.TryGetValue(key, out var value) && value;
        }
    }

    public bool IsKeyPressed(Key key)
    {
        lock (KeysLock)
        {
            return IsKeyDown(key) && !PreviousKeys.GetValueOrDefault(key);
        }
    }

    public bool IsKeyReleased(Key key)
    {
        lock (KeysLock)
        {
            return !IsKeyDown(key) && PreviousKeys.GetValueOrDefault(key);
        }
    }

    public string? GetKeyName(Key key)
    {
        if (GLFW.TryGetFunction<GLFW.glfwGetKeyNameDelegate>("glfwGetKeyName", out var getKeyName) && getKeyName is not null)
        {
            IntPtr ptr = getKeyName((int)key, 0);
            return Marshal.PtrToStringAnsi(ptr);
        }
        return null;
    }

    #endregion

    #region ---- Мышь ----

    public bool IsMouseButtonDown(MouseButton button)
    {
        lock (MouseLock)
        {
            return CurrentMouse.TryGetValue(button, out var value) && value;
        }
    }

    public bool IsMouseButtonPressed(MouseButton button)
    {
        lock (MouseLock)
        {
            return IsMouseButtonDown(button) && !PreviousMouse.GetValueOrDefault(button);
        }
    }

    public bool IsMouseButtonReleased(MouseButton button)
    {
        lock (MouseLock)
        {
            return !IsMouseButtonDown(button) && PreviousMouse.GetValueOrDefault(button);
        }
    }

    public void GetCursorPos(out double x, out double y)
    {
        x = CursorX;
        y = CursorY;
    }

    public void SetCursorPos(double x, double y)
    {
        if (GLFW.TryGetFunction<GLFW.glfwSetCursorPosDelegate>("glfwSetCursorPos", out var setPos) && setPos is not null)
        {
            setPos(Window, x, y);
            CursorX = x;
            CursorY = y;
        }
    }

    /// <summary>
    /// Установить режим курсора
    /// </summary>
    public void SetCursorMode(CursorMode mode)
    {
        // ✅ Маппинг CursorMode -> GLFW константы
        int glfwMode = MapCursorMode(mode);

        if (GLFW.TryGetFunction<GLFW.glfwSetInputModeDelegate>("glfwSetInputMode", out var setInputMode) && setInputMode is not null)
        {
            setInputMode(Window, GLFW.GLFW_CURSOR, glfwMode);
        }
    }

    /// <summary>
    /// Маппинг CursorMode в GLFW константы
    /// </summary>
    private static int MapCursorMode(CursorMode mode)
    {
        return mode switch
        {
            CursorMode.Normal => GLFW.GLFW_CURSOR_NORMAL,
            CursorMode.Hidden => GLFW.GLFW_CURSOR_HIDDEN,
            CursorMode.Disabled => GLFW.GLFW_CURSOR_DISABLED,
            _ => GLFW.GLFW_CURSOR_NORMAL
        };
    }

    #endregion

    #region ---- Обновление ----

    public void Update()
    {
        // Сохраняем предыдущее состояние клавиш
        lock (KeysLock)
        {
            PreviousKeys.Clear();
            foreach (var kvp in CurrentKeys)
                PreviousKeys[kvp.Key] = kvp.Value;
        }

        // Сохраняем предыдущее состояние мыши
        lock (MouseLock)
        {
            PreviousMouse.Clear();
            foreach (var kvp in CurrentMouse)
                PreviousMouse[kvp.Key] = kvp.Value;
        }
    }

    #endregion

    #region ---- Очистка колбэков ----

    private void ClearCallbacks()
    {
        if (CallbacksCleared) return;
        if (!GLFW.IsInitialized) return;

        try
        {
            if (GLFW.TryGetFunction<GLFW.glfwSetKeyCallbackDelegate>("glfwSetKeyCallback", out var setKey) && setKey is not null)
                setKey(Window, default!);  // ✅ Используем default

            if (GLFW.TryGetFunction<GLFW.glfwSetCharCallbackDelegate>("glfwSetCharCallback", out var setChar) && setChar is not null)
                setChar(Window, default!);

            if (GLFW.TryGetFunction<GLFW.glfwSetMouseButtonCallbackDelegate>("glfwSetMouseButtonCallback", out var setMouse) && setMouse is not null)
                setMouse(Window, default!);

            if (GLFW.TryGetFunction<GLFW.glfwSetCursorPosCallbackDelegate>("glfwSetCursorPosCallback", out var setCursor) && setCursor is not null)
                setCursor(Window, default!);

            if (GLFW.TryGetFunction<GLFW.glfwSetScrollCallbackDelegate>("glfwSetScrollCallback", out var setScroll) && setScroll is not null)
                setScroll(Window, default!);

            CallbacksCleared = true;
            Diagnostics.Debug("GLFW колбэки сброшены");
        }
        catch (Exception ex)
        {
            Diagnostics.Debug($"Ошибка при сбросе колбэков: {ex.Message}");
        }
    }

    #endregion

    #region ---- IDisposable & Finalizer ----

    ~GLFWInputBackend()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (Disposed) return;

        ClearCallbacks();

        if (disposing)
        {
            lock (KeysLock)
            {
                CurrentKeys.Clear();
                PreviousKeys.Clear();
            }

            lock (MouseLock)
            {
                CurrentMouse.Clear();
                PreviousMouse.Clear();
            }

            KeyCallback = null;
            CharCallback = null;
            MouseButtonCallback = null;
            CursorPosCallback = null;
            ScrollCallback = null;
        }

        Disposed = true;
    }

    #endregion
}