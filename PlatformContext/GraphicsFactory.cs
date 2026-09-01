using PlatformContext.Enums;
using PlatformNative.Core;

namespace PlatformContext;

/// <summary>
/// Фабрика для создания графических контекстов
/// </summary>
public static class GraphicsFactory
{
    private static readonly Lock Lock = new();
    private static readonly Dictionary<string, Func<IGraphicsContext>> Factories = [];

    #region ---- Регистрация ----

    public static void Register(string name, Func<IGraphicsContext> factory)
    {
        lock (Lock)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Имя не может быть пустым", nameof(name));

            Factories[name] = factory ?? throw new ArgumentNullException(nameof(factory));
            Diagnostics.Debug($"Зарегистрирован графический бэкенд: {name}");
        }
    }

    public static bool IsRegistered(string name)
    {
        lock (Lock)
        {
            return Factories.ContainsKey(name);
        }
    }

    public static IEnumerable<string> GetRegisteredNames()
    {
        lock (Lock)
        {
            return [.. Factories.Keys];
        }
    }

    #endregion

    #region ---- Создание ----

    public static IGraphicsContext? Create(string name)
    {
        lock (Lock)
        {
            if (Factories.TryGetValue(name, out var factory))
                return factory();
        }

        Diagnostics.Warning($"Графический бэкенд '{name}' не зарегистрирован");
        return null;
    }

    #endregion

    #region ---- OpenGL (универсальные методы) ----

    /// <summary>
    /// Создать OpenGL контекст с указанной версией и профилем
    /// </summary>
    public static IGraphicsContext CreateOpenGL(int major = 3, int minor = 3,
                                                  ContextProfile profile = ContextProfile.Core)
        => new OpenGLContext(major, minor, profile);

    /// <summary>
    /// Создать OpenGL Core Profile контекст
    /// </summary>
    public static IGraphicsContext CreateOpenGLCore(int major, int minor)
        => new OpenGLContext(major, minor, ContextProfile.Core);

    /// <summary>
    /// Создать OpenGL Compatibility Profile контекст
    /// </summary>
    public static IGraphicsContext CreateOpenGLCompat(int major, int minor)
        => new OpenGLContext(major, minor, ContextProfile.Compatibility);

    /// <summary>
    /// Создать OpenGL ES контекст
    /// </summary>
    public static IGraphicsContext CreateOpenGLES(int major = 2, int minor = 0)
        => new OpenGLContext(major, minor, ContextProfile.ES);

    /// <summary>
    /// Создать OpenGL 3.3 Core Profile (рекомендуемый минимум)
    /// </summary>
    public static IGraphicsContext CreateOpenGL33Core()
        => new OpenGLContext(3, 3, ContextProfile.Core);

    /// <summary>
    /// Создать OpenGL 4.6 Core Profile (для современных фич)
    /// </summary>
    public static IGraphicsContext CreateOpenGL46Core()
        => new OpenGLContext(4, 6, ContextProfile.Core);

    #endregion

    #region ---- Статическая инициализация ----

    static GraphicsFactory()
    {
        Register("OpenGL", () => new OpenGLContext(3, 3, ContextProfile.Core));
        Register("OpenGL33", () => new OpenGLContext(3, 3, ContextProfile.Core));
        Register("OpenGL46", () => new OpenGLContext(4, 6, ContextProfile.Core));
        Register("OpenGLES", () => new OpenGLContext(2, 0, ContextProfile.ES));
        Register("OpenGL33Compat", () => new OpenGLContext(3, 3, ContextProfile.Compatibility));
        Register("OpenGL46Compat", () => new OpenGLContext(4, 6, ContextProfile.Compatibility));
    }

    #endregion
}