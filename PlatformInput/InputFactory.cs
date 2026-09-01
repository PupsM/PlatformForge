using PlatformInput.Interfaces;
using PlatformNative.Core;

namespace PlatformInput;

/// <summary>
/// Фабрика для создания бэкендов ввода
/// </summary>
public static class InputFactory
{
    private static readonly Lock Lock = new();
    private static readonly Dictionary<string, Func<IntPtr, IInput>> Factories = [];

    #region ---- Регистрация ----

    public static void Register(string name, Func<IntPtr, IInput> factory)
    {
        lock (Lock)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Имя не может быть пустым", nameof(name));

            Factories[name] = factory ?? throw new ArgumentNullException(nameof(factory));
            Diagnostics.Debug($"Зарегистрирован бэкенд ввода: {name}");
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
            return [.. Factories.Keys];  // ✅ Без LINQ
        }
    }

    #endregion

    #region ---- Создание ----

    public static IInput? Create(string name, IntPtr window)
    {
        lock (Lock)
        {
            if (Factories.TryGetValue(name, out var factory))
                return factory(window);
        }

        Diagnostics.Warning($"Бэкенд ввода '{name}' не зарегистрирован");
        return null;
    }

    #endregion

    #region ---- GLFW (уникальные методы) ----

    public static IInput CreateGLFW(IntPtr window)
        => new GLFWInputBackend(window);

    public static IInput CreateGLFWWithGamepadSupport(IntPtr window)
    {
        var backend = new GLFWInputBackend(window);
        return backend;
    }

    public static IInput CreateGLFWWithAutoUpdate(IntPtr window)
    {
        return new GLFWInputBackend(window);
    }

    public static IInput CreateDefault(IntPtr window)
    {
        if (IsRegistered("GLFW"))
            return CreateGLFW(window);

        return CreateGLFW(window);
    }

    #endregion

    #region ---- Статическая инициализация ----

    static InputFactory()
    {
        Register("GLFW", w => new GLFWInputBackend(w));
        Register("Default", w => new GLFWInputBackend(w));
    }

    #endregion
}