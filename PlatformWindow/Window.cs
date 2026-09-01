using System.Runtime.InteropServices;
using PlatformNative.Core;
using PlatformNative.Native;
using PlatformWindow.Enums;

namespace PlatformWindow;

/// <summary>
/// Реализация окна через GLFW
/// </summary>
public sealed class Window : IWindow
{
    private readonly IntPtr GLFWHandle;
    private readonly GLFWBackend? Backend;
    private bool GLFWShouldClose;
    private bool Disposed;

    // ---- Хранение колбэков для предотвращения GC ----
    private GLFW.GlfwWindowCloseFun? GLFWCloseCallback;
    private GLFW.GlfwWindowSizeFun? GLFWSizeCallback;
    private GLFW.GlfwWindowPosFun? GLFWPosCallback;
    private GLFW.GlfwWindowFocusFun? GLFWFocusCallback;
    private GLFW.GlfwWindowIconifyFun? GLFWIconifyCallback;
    private GLFW.GlfwWindowMaximizeFun? GLFWMaximizeCallback;
    private GLFW.GlfwWindowContentScaleFun? GLFWScaleCallback;

    internal Window(IntPtr handle, GLFWBackend? backend = null)
    {
        GLFWHandle = handle;
        Backend = backend;

        // Инициализируем колбэки
        SetupCallbacks();
    }

    // ---- Публичные свойства с колбэками для доступа из GLFWBackend ----

    internal GLFW.GlfwWindowCloseFun CloseCallback => GLFWCloseCallback!;
    internal GLFW.GlfwWindowSizeFun SizeCallback => GLFWSizeCallback!;
    internal GLFW.GlfwWindowPosFun PosCallback => GLFWPosCallback!;
    internal GLFW.GlfwWindowFocusFun FocusCallback => GLFWFocusCallback!;
    internal GLFW.GlfwWindowIconifyFun IconifyCallback => GLFWIconifyCallback!;
    internal GLFW.GlfwWindowMaximizeFun MaximizeCallback => GLFWMaximizeCallback!;
    internal GLFW.GlfwWindowContentScaleFun ScaleCallback => GLFWScaleCallback!;

    internal void SetupCallbacks()
    {
        // Создаём колбэки и сохраняем их в полях
        GLFWCloseCallback = (w) => OnClosing();
        GLFWSizeCallback = (w, width, height) => OnResized(width, height);
        GLFWPosCallback = (w, x, y) => OnMoved(x, y);
        GLFWFocusCallback = (w, focused) =>
        {
            if (focused != 0)
                OnFocusGained();
            else
                OnFocusLost();
        };
        GLFWIconifyCallback = (w, iconified) =>
        {
            if (iconified != 0)
                OnMinimized();
            else
                OnRestored();
        };
        GLFWMaximizeCallback = (w, maximized) =>
        {
            if (maximized != 0)
                OnMaximized();
            else
                OnRestored();
        };
        GLFWScaleCallback = (w, xscale, yscale) => OnContentScaleChanged();
    }

    #region ---- Свойства ----

    public IntPtr Handle => GLFWHandle;

    public string Title
    {
        get
        {
            if (GLFW.TryGetFunction<GLFW.glfwGetWindowTitleDelegate>("glfwGetWindowTitle", out var getTitle) && getTitle is not null)
            {
                IntPtr ptr = getTitle(GLFWHandle);
                return Marshal.PtrToStringAnsi(ptr) ?? string.Empty;
            }
            return string.Empty;
        }
        set
        {
            if (GLFW.TryGetFunction<GLFW.glfwSetWindowTitleDelegate>("glfwSetWindowTitle", out var setTitle) && setTitle is not null)
            {
                setTitle(GLFWHandle, value);
            }
        }
    }

    public int Width
    {
        get
        {
            if (GLFW.TryGetFunction<GLFW.glfwGetWindowSizeDelegate>("glfwGetWindowSize", out var getSize) && getSize is not null)
            {
                getSize(GLFWHandle, out int width, out int _);
                return width;
            }
            return 0;
        }
    }

    public int Height
    {
        get
        {
            if (GLFW.TryGetFunction<GLFW.glfwGetWindowSizeDelegate>("glfwGetWindowSize", out var getSize) && getSize is not null)
            {
                getSize(GLFWHandle, out int _, out int height);
                return height;
            }
            return 0;
        }
    }

    public int X
    {
        get
        {
            if (GLFW.TryGetFunction<GLFW.glfwGetWindowPosDelegate>("glfwGetWindowPos", out var getPos) && getPos is not null)
            {
                getPos(GLFWHandle, out int x, out int _);
                return x;
            }
            return 0;
        }
    }

    public int Y
    {
        get
        {
            if (GLFW.TryGetFunction<GLFW.glfwGetWindowPosDelegate>("glfwGetWindowPos", out var getPos) && getPos is not null)
            {
                getPos(GLFWHandle, out int _, out int y);
                return y;
            }
            return 0;
        }
    }

    public bool IsVisible
    {
        get
        {
            if (GLFW.TryGetFunction<GLFW.glfwGetWindowAttribDelegate>("glfwGetWindowAttrib", out var getAttrib) && getAttrib is not null)
            {
                return getAttrib(GLFWHandle, GLFW.GLFW_VISIBLE) != 0;
            }
            return false;
        }
        set
        {
            if (value)
                Show();
            else
                Hide();
        }
    }

    public bool IsFocused
    {
        get
        {
            if (GLFW.TryGetFunction<GLFW.glfwGetWindowAttribDelegate>("glfwGetWindowAttrib", out var getAttrib) && getAttrib is not null)
            {
                return getAttrib(GLFWHandle, GLFW.GLFW_FOCUSED) != 0;
            }
            return false;
        }
    }

    public bool IsMinimized => State == WindowState.Minimized;
    public bool IsMaximized => State == WindowState.Maximized;

    public WindowState State
    {
        get
        {
            if (GLFW.TryGetFunction<GLFW.glfwGetWindowAttribDelegate>("glfwGetWindowAttrib", out var getAttrib) && getAttrib is not null)
            {
                if (getAttrib(GLFWHandle, GLFW.GLFW_ICONIFIED) != 0)
                    return WindowState.Minimized;
                if (getAttrib(GLFWHandle, GLFW.GLFW_MAXIMIZED) != 0)
                    return WindowState.Maximized;
                if (!IsVisible)
                    return WindowState.Hidden;
                return WindowState.Normal;
            }
            return WindowState.Normal;
        }
    }

    public float Opacity
    {
        get
        {
            if (GLFW.TryGetFunction<GLFW.glfwGetWindowOpacityDelegate>("glfwGetWindowOpacity", out var getOpacity) && getOpacity is not null)
            {
                return getOpacity(GLFWHandle);
            }
            return 1.0f;
        }
        set
        {
            if (GLFW.TryGetFunction<GLFW.glfwSetWindowOpacityDelegate>("glfwSetWindowOpacity", out var setOpacity) && setOpacity is not null)
            {
                setOpacity(GLFWHandle, Math.Clamp(value, 0f, 1f));
            }
        }
    }

    public bool ShouldClose
    {
        get
        {
            if (GLFW.TryGetFunction<GLFW.glfwWindowShouldCloseDelegate>("glfwWindowShouldClose", out var getClose) && getClose is not null)
            {
                return getClose(GLFWHandle) != 0;
            }
            return GLFWShouldClose;
        }
        set
        {
            GLFWShouldClose = value;
            if (GLFW.TryGetFunction<GLFW.glfwSetWindowShouldCloseDelegate>("glfwSetWindowShouldClose", out var setClose) && setClose is not null)
            {
                setClose(GLFWHandle, value ? 1 : 0);
            }
        }
    }

    #endregion

    #region ---- Методы ----

    public void Show()
    {
        if (GLFW.TryGetFunction<GLFW.glfwShowWindowDelegate>("glfwShowWindow", out var show) && show is not null)
        {
            show(GLFWHandle);
        }
    }

    public void Hide()
    {
        if (GLFW.TryGetFunction<GLFW.glfwHideWindowDelegate>("glfwHideWindow", out var hide) && hide is not null)
        {
            hide(GLFWHandle);
        }
    }

    public void Maximize()
    {
        if (GLFW.TryGetFunction<GLFW.glfwMaximizeWindowDelegate>("glfwMaximizeWindow", out var maximize) && maximize is not null)
        {
            maximize(GLFWHandle);
        }
    }

    public void Minimize()
    {
        if (GLFW.TryGetFunction<GLFW.glfwIconifyWindowDelegate>("glfwIconifyWindow", out var iconify) && iconify is not null)
        {
            iconify(GLFWHandle);
        }
    }

    public void Restore()
    {
        if (GLFW.TryGetFunction<GLFW.glfwRestoreWindowDelegate>("glfwRestoreWindow", out var restore) && restore is not null)
        {
            restore(GLFWHandle);
        }
    }

    public void SetSize(int width, int height)
    {
        if (GLFW.TryGetFunction<GLFW.glfwSetWindowSizeDelegate>("glfwSetWindowSize", out var setSize) && setSize is not null)
        {
            setSize(GLFWHandle, width, height);
        }
    }

    public void SetPosition(int x, int y)
    {
        if (GLFW.TryGetFunction<GLFW.glfwSetWindowPosDelegate>("glfwSetWindowPos", out var setPos) && setPos is not null)
        {
            setPos(GLFWHandle, x, y);
        }
    }

    public void SetSizeLimits(int minWidth, int minHeight, int maxWidth, int maxHeight)
    {
        if (GLFW.TryGetFunction<GLFW.glfwSetWindowSizeLimitsDelegate>("glfwSetWindowSizeLimits", out var setLimits) && setLimits is not null)
        {
            setLimits(GLFWHandle, minWidth, minHeight, maxWidth, maxHeight);
        }
    }

    public void SetAspectRatio(int numer, int denom)
    {
        if (GLFW.TryGetFunction<GLFW.glfwSetWindowAspectRatioDelegate>("glfwSetWindowAspectRatio", out var setAspect) && setAspect is not null)
        {
            setAspect(GLFWHandle, numer, denom);
        }
    }

    public void Focus()
    {
        if (GLFW.TryGetFunction<GLFW.glfwFocusWindowDelegate>("glfwFocusWindow", out var focus) && focus is not null)
        {
            focus(GLFWHandle);
        }
    }

    public void RequestAttention()
    {
        if (GLFW.TryGetFunction<GLFW.glfwRequestWindowAttentionDelegate>("glfwRequestWindowAttention", out var request) && request is not null)
        {
            request(GLFWHandle);
        }
    }

    public void SetIcon(byte[] pixels, int width, int height, int channels = 4)
    {
        if (pixels is null || pixels.Length == 0)
        {
            Diagnostics.Warning("SetIcon: пиксели не могут быть пустыми");
            return;
        }

        if (channels != 3 && channels != 4)
        {
            Diagnostics.Warning($"SetIcon: неподдерживаемое количество каналов {channels}. Используйте 3 (RGB) или 4 (RGBA)");
            return;
        }

        int expectedSize = width * height * channels;
        if (pixels.Length != expectedSize)
        {
            Diagnostics.Warning($"SetIcon: размер пикселей {pixels.Length} не соответствует {width}x{height}x{channels} = {expectedSize}");
            return;
        }

        try
        {
            IntPtr imagePtr = Marshal.AllocHGlobal(Marshal.SizeOf<GLFW.GLFWimage>());
            try
            {
                IntPtr pixelsPtr = Marshal.AllocHGlobal(pixels.Length);
                try
                {
                    Marshal.Copy(pixels, 0, pixelsPtr, pixels.Length);

                    var image = new GLFW.GLFWimage
                    {
                        width = width,
                        height = height,
                        pixels = pixelsPtr
                    };

                    Marshal.StructureToPtr(image, imagePtr, false);

                    if (GLFW.TryGetFunction<GLFW.glfwSetWindowIconDelegate>("glfwSetWindowIcon", out var setIcon) && setIcon is not null)
                    {
                        setIcon(GLFWHandle, 1, imagePtr);
                        Diagnostics.Debug($"Иконка установлена {width}x{height} с {channels} каналами");
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(pixelsPtr);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(imagePtr);
            }
        }
        catch (Exception ex)
        {
            Diagnostics.Warning($"SetIcon ошибка: {ex.Message}");
        }
    }

    public IntPtr GetNativeHandle()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (GLFW.TryGetFunction<GLFW.glfwGetWin32WindowDelegate>("glfwGetWin32Window", out var getWin32) && getWin32 is not null)
                {
                    return getWin32(GLFWHandle);
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (GLFW.TryGetFunction<GLFW.glfwGetX11WindowDelegate>("glfwGetX11Window", out var getX11) && getX11 is not null)
                {
                    return getX11(GLFWHandle);
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (GLFW.TryGetFunction<GLFW.glfwGetCocoaWindowDelegate>("glfwGetCocoaWindow", out var getCocoa) && getCocoa is not null)
                {
                    return getCocoa(GLFWHandle);
                }
            }
        }
        catch (Exception ex)
        {
            Diagnostics.Warning($"GetNativeHandle ошибка: {ex.Message}");
        }
        return GLFWHandle;
    }

    public void PollEvents()
    {
        if (GLFW.TryGetFunction<GLFW.glfwPollEventsDelegate>("glfwPollEvents", out var pollEvents) && pollEvents is not null)
        {
            pollEvents();
        }
    }

    #endregion

    #region ---- Внутренние методы для событий ----

    internal void OnClosed()
        => Closed?.Invoke(this);

    internal void OnResized(int width, int height)
        => Resized?.Invoke(this, width, height);

    internal void OnMoved(int x, int y)
        => Moved?.Invoke(this, x, y);

    internal void OnFocusGained()
        => FocusGained?.Invoke(this);

    internal void OnFocusLost()
        => FocusLost?.Invoke(this);

    internal void OnMinimized()
        => Minimized?.Invoke(this);

    internal void OnMaximized()
        => Maximized?.Invoke(this);

    internal void OnRestored()
        => Restored?.Invoke(this);

    internal void OnClosing()
        => Closing?.Invoke(this);

    internal void OnContentScaleChanged()
        => ContentScaleChanged?.Invoke(this);

    #endregion

    #region ---- События ----

    public event Action<IWindow>? Closed;
    public event Action<IWindow, int, int>? Resized;
    public event Action<IWindow, int, int>? Moved;
    public event Action<IWindow>? FocusGained;
    public event Action<IWindow>? FocusLost;
    public event Action<IWindow>? Minimized;
    public event Action<IWindow>? Maximized;
    public event Action<IWindow>? Restored;
    public event Action<IWindow>? Closing;
    public event Action<IWindow>? ContentScaleChanged;

    #endregion

    #region ---- IDisposable & Finalizer ----

    ~Window()
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

        if (disposing)
        {
            // Освобождаем управляемые ресурсы (если есть)
            // В данном случае колбэки не требуют явного освобождения
        }

        // Освобождаем неуправляемые ресурсы
        if (Backend is not null)
        {
            // Бэкенд управляет уничтожением окна
            Backend.DestroyWindow(this);
        }
        else
        {
            // Если бэкенд отсутствует, уничтожаем окно напрямую
            if (GLFW.TryGetFunction<GLFW.glfwDestroyWindowDelegate>("glfwDestroyWindow", out var destroy) && destroy is not null)
            {
                try
                {
                    destroy(GLFWHandle);
                }
                catch (Exception ex)
                {
                    Diagnostics.Warning($"Ошибка при уничтожении окна: {ex.Message}");
                }
            }
        }

        Disposed = true;
    }

    /// <summary>
    /// Внутренний метод для уничтожения окна (используется бэкендом)
    /// </summary>
    internal void DisposeInternal()
    {
        if (Disposed) return;

        if (GLFW.TryGetFunction<GLFW.glfwDestroyWindowDelegate>("glfwDestroyWindow", out var destroy) && destroy is not null)
        {
            destroy(GLFWHandle);
        }

        Disposed = true;
        // GC.SuppressFinalize(this) - УБРАНО! Это должен делать только Dispose()
    }

    #endregion
}