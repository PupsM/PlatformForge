using PlatformContext.Enums;
using PlatformNative.Core;
using PlatformNative.Native;

namespace PlatformContext;

/// <summary>
/// Реализация OpenGL контекста
/// </summary>
/// <remarks>
/// Создаёт OpenGL контекст с указанным профилем
/// </remarks>
public sealed class OpenGLContext(int major = 3, int minor = 3, ContextProfile profile = ContextProfile.Core) : IGraphicsContext
{
    private IntPtr Window;
    private int SwapInterval = 1;
    private bool Disposed;
    private bool Initialized;

    public GraphicsApi Api => GraphicsApi.OpenGL;

    public string Name => $"OpenGL {Major}.{Minor} {Profile}";

    public bool IsInitialized => Initialized && Window != IntPtr.Zero;

    public IntPtr Handle
    {
        get
        {
            if (!Initialized || Window == IntPtr.Zero)
                return IntPtr.Zero;

            if (GLFW.TryGetFunction<GLFW.glfwGetCurrentContextDelegate>("glfwGetCurrentContext", out var getCurrent) && getCurrent is not null)
            {
                return getCurrent();
            }
            return IntPtr.Zero;
        }
    }

    public int Major { get; } = major;
    public int Minor { get; } = minor;
    public ContextProfile Profile { get; } = profile;

    public void MakeCurrent(IntPtr windowHandle)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (windowHandle == IntPtr.Zero)
            throw new ArgumentException("Window handle cannot be zero", nameof(windowHandle));

        // Проверяем валидность окна
        if (GLFW.TryGetFunction<GLFW.glfwGetWindowAttribDelegate>("glfwGetWindowAttrib", out var getAttrib) && getAttrib is not null)
        {
            try
            {
                getAttrib(windowHandle, GLFW.GLFW_FOCUSED);
            }
            catch
            {
                throw new ArgumentException("Window handle is not a valid GLFW window", nameof(windowHandle));
            }
        }

        Window = windowHandle;

        // Настраиваем GLFW для OpenGL
        if (GLFW.TryGetFunction<GLFW.glfwWindowHintDelegate>("glfwWindowHint", out var hint) && hint is not null)
        {
            hint(GLFW.GLFW_CONTEXT_VERSION_MAJOR, Major);
            hint(GLFW.GLFW_CONTEXT_VERSION_MINOR, Minor);

            if (Profile == ContextProfile.Core)
            {
                hint(GLFW.GLFW_OPENGL_PROFILE, GLFW.GLFW_OPENGL_CORE_PROFILE);
            }
            else if (Profile == ContextProfile.Compatibility)
            {
                hint(GLFW.GLFW_OPENGL_PROFILE, GLFW.GLFW_OPENGL_COMPAT_PROFILE);
            }
            // Для ES: GLFW сам выберет подходящий контекст (EGL/GLX/WGL)

            hint(GLFW.GLFW_OPENGL_FORWARD_COMPAT, 1);
        }

        // Делаем контекст текущим
        if (GLFW.TryGetFunction<GLFW.glfwMakeContextCurrentDelegate>("glfwMakeContextCurrent", out var makeCurrent) && makeCurrent is not null)
        {
            makeCurrent(Window);
        }

        // Инициализируем OpenGL
        if (!OpenGL.IsInitialized)
        {
            OpenGL.Initialize();
        }

        SetSwapInterval(SwapInterval);
        Initialized = true;
        Diagnostics.Info($"OpenGL контекст создан: {Major}.{Minor} {Profile}");
    }

    public void SwapBuffers()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (!Initialized || Window == IntPtr.Zero)
            throw new InvalidOperationException("Контекст не инициализирован или не привязан к окну");

        if (GLFW.TryGetFunction<GLFW.glfwSwapBuffersDelegate>("glfwSwapBuffers", out var swapBuffers) && swapBuffers is not null)
        {
            swapBuffers(Window);
        }
    }

    public void SetSwapInterval(int interval)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        SwapInterval = interval;

        if (GLFW.TryGetFunction<GLFW.glfwSwapIntervalDelegate>("glfwSwapInterval", out var swapInterval) && swapInterval is not null)
        {
            swapInterval(interval);
        }
    }

    public int GetSwapInterval()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        return SwapInterval;
    }

    public IntPtr GetExtensionFunction(string name)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (string.IsNullOrEmpty(name))
            return IntPtr.Zero;

        if (GLFW.TryGetFunction<GLFW.glfwGetProcAddressDelegate>("glfwGetProcAddress", out var getProc) && getProc is not null)
        {
            return getProc(name);
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (Disposed) return;

        try
        {
            if (GLFW.TryGetFunction<GLFW.glfwMakeContextCurrentDelegate>("glfwMakeContextCurrent", out var makeCurrent) && makeCurrent is not null)
            {
                makeCurrent(IntPtr.Zero);
            }
        }
        catch (Exception ex)
        {
            Diagnostics.Debug($"Ошибка при отключении контекста: {ex.Message}");
        }

        Window = IntPtr.Zero;
        Initialized = false;
        Disposed = true;

        Diagnostics.Info("OpenGL контекст освобождён");
        GC.SuppressFinalize(this);
    }
}