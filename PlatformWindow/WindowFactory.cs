using PlatformNative.Core;

namespace PlatformWindow;

/// <summary>
/// Фабрика для создания оконных бэкендов
/// </summary>
public static class WindowFactory
{
    private static readonly Lock Lock = new();
    private static readonly Dictionary<string, Func<IWindowBackend>> Factories = [];

    #region ---- Регистрация ----

    public static void Register(string name, Func<IWindowBackend> factory)
    {
        lock (Lock)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Имя не может быть пустым", nameof(name));

            Factories[name] = factory ?? throw new ArgumentNullException(nameof(factory));
            Diagnostics.Debug($"Зарегистрирован оконный бэкенд: {name}");
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

    public static IWindowBackend? Create(string name)
    {
        lock (Lock)
        {
            if (Factories.TryGetValue(name, out var factory))
                return factory();
        }

        Diagnostics.Warning($"Оконный бэкенд '{name}' не зарегистрирован");
        return null;
    }

    #endregion

    #region ---- GLFW (уникальные методы) ----

    public static IWindowBackend CreateGLFW()
        => new GLFWBackend();

    public static IWindowBackend CreateDefault()
    {
        if (IsRegistered("GLFW"))
            return CreateGLFW();

        return CreateGLFW();
    }

    public static IWindowBackend CreateBorderlessBackend()
    {
        return CreateGLFW();
    }

    #endregion

    #region ---- Статическая инициализация ----

    static WindowFactory()
    {
        Register("GLFW", () => new GLFWBackend());
        Register("Default", () => new GLFWBackend());
    }

    #endregion
}