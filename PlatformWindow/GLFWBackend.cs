using System.Runtime.InteropServices;
using PlatformNative.Core;
using PlatformNative.Native;
using PlatformWindow.Enums;

namespace PlatformWindow;

/// <summary>
/// Бэкенд окон через GLFW
/// </summary>
public sealed class GLFWBackend : IWindowBackend
{
    private bool Initialized;
    private bool Disposed;
    private readonly List<Window> Windows = [];
    private readonly Lock WindowsLock = new();

    public string Name => "GLFW";
    public bool IsInitialized => Initialized;

    #region ---- Инициализация ----

    public void Initialize()
    {
        if (Initialized) return;

        if (!GLFW.IsInitialized)
        {
            if (!GLFW.Initialize())
            {
                throw new InvalidOperationException("Не удалось инициализировать GLFW");
            }
        }

        Initialized = true;
        Diagnostics.Info("GLFW бэкенд инициализирован");
    }

    public void GetVersion(out int major, out int minor, out int rev)
    {
        major = 0;
        minor = 0;
        rev = 0;

        EnsureInitialized();

        if (GLFW.TryGetFunction<GLFW.glfwGetVersionDelegate>("glfwGetVersion", out var getVersion) && getVersion is not null)
        {
            getVersion(out major, out minor, out rev);
        }
    }

    #endregion

    #region ---- Создание окон ----

    public IWindow CreateWindow(string title, int width, int height, WindowFlags flags = WindowFlags.Default)
    {
        EnsureInitialized();

        if (!GLFW.TryGetFunction<GLFW.glfwWindowHintDelegate>("glfwWindowHint", out var hint) || hint is null)
            throw new InvalidOperationException("glfwWindowHint не найден");

        if (!GLFW.TryGetFunction<GLFW.glfwCreateWindowDelegate>("glfwCreateWindow", out var create) || create is null)
            throw new InvalidOperationException("glfwCreateWindow не найден");

        hint(GLFW.GLFW_RESIZABLE, (flags & WindowFlags.Resizable) != 0 ? 1 : 0);
        hint(GLFW.GLFW_VISIBLE, (flags & WindowFlags.Hidden) != 0 ? 0 : 1);
        hint(GLFW.GLFW_DECORATED, (flags & WindowFlags.Borderless) != 0 ? 0 : 1);
        hint(GLFW.GLFW_FLOATING, (flags & WindowFlags.AlwaysOnTop) != 0 ? 1 : 0);
        hint(GLFW.GLFW_TRANSPARENT_FRAMEBUFFER, (flags & WindowFlags.Transparent) != 0 ? 1 : 0);

        if ((flags & WindowFlags.Maximized) != 0)
            hint(GLFW.GLFW_MAXIMIZED, 1);
        if ((flags & WindowFlags.Minimized) != 0)
            hint(GLFW.GLFW_ICONIFIED, 1);
        if ((flags & WindowFlags.Focused) != 0)
            hint(GLFW.GLFW_FOCUSED, 1);

        IntPtr handle = create(width, height, title, IntPtr.Zero, IntPtr.Zero);

        if (handle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Не удалось создать GLFW окно");
        }

        var window = new Window(handle, this);

        lock (WindowsLock)
        {
            Windows.Add(window);
        }

        SetupCallbacks(handle, window);

        Diagnostics.Info($"GLFW окно создано: {handle:X8} ({width}x{height})");
        return window;
    }

    public void DestroyWindow(IWindow window)
    {
        if (window is null) return;

        if (window is not Window w) return;

        lock (WindowsLock)
        {
            if (!Windows.Contains(w))
            {
                return;
            }
            Windows.Remove(w);
            w.DisposeInternal();
            Diagnostics.Debug($"GLFW окно уничтожено: {w.Handle:X8}");
        }
    }

    #endregion

    #region ---- Настройка колбэков ----

    private static void SetupCallbacks(IntPtr handle, Window window)
    {
        if (GLFW.TryGetFunction<GLFW.glfwSetWindowCloseCallbackDelegate>("glfwSetWindowCloseCallback", out var setClose) && setClose is not null)
        {
            setClose(handle, window.CloseCallback);
        }

        if (GLFW.TryGetFunction<GLFW.glfwSetWindowSizeCallbackDelegate>("glfwSetWindowSizeCallback", out var setSize) && setSize is not null)
        {
            setSize(handle, window.SizeCallback);
        }

        if (GLFW.TryGetFunction<GLFW.glfwSetWindowPosCallbackDelegate>("glfwSetWindowPosCallback", out var setPos) && setPos is not null)
        {
            setPos(handle, window.PosCallback);
        }

        if (GLFW.TryGetFunction<GLFW.glfwSetWindowFocusCallbackDelegate>("glfwSetWindowFocusCallback", out var setFocus) && setFocus is not null)
        {
            setFocus(handle, window.FocusCallback);
        }

        if (GLFW.TryGetFunction<GLFW.glfwSetWindowIconifyCallbackDelegate>("glfwSetWindowIconifyCallback", out var setIconify) && setIconify is not null)
        {
            setIconify(handle, window.IconifyCallback);
        }

        if (GLFW.TryGetFunction<GLFW.glfwSetWindowMaximizeCallbackDelegate>("glfwSetWindowMaximizeCallback", out var setMaximize) && setMaximize is not null)
        {
            setMaximize(handle, window.MaximizeCallback);
        }

        if (GLFW.TryGetFunction<GLFW.glfwSetWindowContentScaleCallbackDelegate>("glfwSetWindowContentScaleCallback", out var setScale) && setScale is not null)
        {
            setScale(handle, window.ScaleCallback);
        }
    }

    #endregion

    #region ---- События ----

    public void PollEvents()
    {
        EnsureInitialized();

        if (GLFW.TryGetFunction<GLFW.glfwPollEventsDelegate>("glfwPollEvents", out var pollEvents) && pollEvents is not null)
        {
            pollEvents();
        }
    }

    public void WaitEvents()
    {
        EnsureInitialized();

        if (GLFW.TryGetFunction<GLFW.glfwWaitEventsDelegate>("glfwWaitEvents", out var waitEvents) && waitEvents is not null)
        {
            waitEvents();
        }
    }

    public void WaitEventsTimeout(double timeout)
    {
        EnsureInitialized();

        if (GLFW.TryGetFunction<GLFW.glfwWaitEventsTimeoutDelegate>("glfwWaitEventsTimeout", out var waitTimeout) && waitTimeout is not null)
        {
            waitTimeout(timeout);
        }
    }

    #endregion

    #region ---- Мониторы ----

    public IntPtr GetPrimaryMonitor()
    {
        EnsureInitialized();

        if (GLFW.TryGetFunction<GLFW.glfwGetPrimaryMonitorDelegate>("glfwGetPrimaryMonitor", out var getPrimary) && getPrimary is not null)
        {
            return getPrimary();
        }
        return IntPtr.Zero;
    }

    public IEnumerable<IntPtr> GetMonitors()
    {
        EnsureInitialized();

        if (GLFW.TryGetFunction<GLFW.glfwGetMonitorsDelegate>("glfwGetMonitors", out var getMonitors) && getMonitors is not null)
        {
            IntPtr ptr = getMonitors(out int count);
            if (ptr != IntPtr.Zero && count > 0)
            {
                var monitors = new IntPtr[count];
                Marshal.Copy(ptr, monitors, 0, count);
                return monitors;
            }
        }
        return [];
    }

    public string? GetMonitorName(IntPtr monitor)
    {
        EnsureInitialized();

        if (GLFW.TryGetFunction<GLFW.glfwGetMonitorNameDelegate>("glfwGetMonitorName", out var getName) && getName is not null)
        {
            IntPtr ptr = getName(monitor);
            return Marshal.PtrToStringAnsi(ptr);
        }
        return null;
    }

    public void GetMonitorPos(IntPtr monitor, out int x, out int y)
    {
        x = 0;
        y = 0;

        EnsureInitialized();

        if (GLFW.TryGetFunction<GLFW.glfwGetMonitorPosDelegate>("glfwGetMonitorPos", out var getPos) && getPos is not null)
        {
            getPos(monitor, out x, out y);
        }
    }

    #endregion

    #region ---- Буфер обмена ----

    public string? GetClipboardString()
    {
        EnsureInitialized();

        if (GLFW.TryGetFunction<GLFW.glfwGetClipboardStringDelegate>("glfwGetClipboardString", out var getClipboard) && getClipboard is not null)
        {
            IntPtr ptr = getClipboard(IntPtr.Zero);
            return Marshal.PtrToStringAnsi(ptr);
        }
        return null;
    }

    public void SetClipboardString(string text)
    {
        EnsureInitialized();

        if (GLFW.TryGetFunction<GLFW.glfwSetClipboardStringDelegate>("glfwSetClipboardString", out var setClipboard) && setClipboard is not null)
        {
            setClipboard(IntPtr.Zero, text);
        }
    }

    #endregion

    #region ---- Прочее ----

    public IntPtr GetProcAddress(string name)
    {
        EnsureInitialized();

        if (GLFW.TryGetFunction<GLFW.glfwGetProcAddressDelegate>("glfwGetProcAddress", out var getProc) && getProc is not null)
        {
            return getProc(name);
        }
        return IntPtr.Zero;
    }

    #endregion

    #region ---- Приватные методы ----

    private void EnsureInitialized()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (!Initialized)
            throw new InvalidOperationException("GLFWBackend не инициализирован. Вызовите Initialize() перед использованием.");
    }

    #endregion

    #region ---- IDisposable & Finalizer ----

    ~GLFWBackend()
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
            lock (WindowsLock)
            {
                foreach (var window in Windows)  // ✅ Без LINQ
                {
                    window.DisposeInternal();
                }
                Windows.Clear();
            }
        }

        if (Initialized)
        {
            try
            {
                GLFW.Terminate();
            }
            catch (Exception ex)
            {
                Diagnostics.Warning($"Ошибка при завершении GLFW: {ex.Message}");
            }
            Initialized = false;
        }

        Disposed = true;
        Diagnostics.Info("GLFW бэкенд освобождён");
    }

    #endregion
}