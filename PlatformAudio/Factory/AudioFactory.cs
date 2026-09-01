using PlatformAudio.Backend;
using PlatformAudio.Interfaces;
using PlatformNative.Core;

namespace PlatformAudio.Factory;

/// <summary>
/// Фабрика для создания аудиобэкендов
/// </summary>
public static class AudioFactory
{
    private static readonly Lock Lock = new();
    private static readonly Dictionary<string, Func<IAudio>> Factories = [];

    #region ---- Регистрация ----

    public static void Register(string name, Func<IAudio> factory)
    {
        lock (Lock)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Имя не может быть пустым", nameof(name));

            Factories[name] = factory ?? throw new ArgumentNullException(nameof(factory));
            Diagnostics.Debug($"Зарегистрирован аудиобэкенд: {name}");
        }
    }

    public static bool IsRegistered(string name)
    {
        lock (Lock) return Factories.ContainsKey(name);
    }

    public static IEnumerable<string> GetRegisteredNames()
    {
        lock (Lock) return [.. Factories.Keys];  // ✅ Без LINQ
    }

    #endregion

    #region ---- Создание ----

    public static IAudio? Create(string name)
    {
        lock (Lock)
        {
            if (Factories.TryGetValue(name, out var factory))
                return factory();
        }

        Diagnostics.Warning($"Аудиобэкенд '{name}' не зарегистрирован");
        return null;
    }

    #endregion

    #region ---- OpenAL (уникальные методы) ----

    public static IAudio CreateOpenAL()
        => new OpenALBackend();

    public static IAudio CreateDefault()
    {
        if (IsRegistered("OpenAL"))
            return CreateOpenAL();

        return CreateOpenAL();
    }

    #endregion

    #region ---- Статическая инициализация ----

    static AudioFactory()
    {
        Register("OpenAL", () => new OpenALBackend());
        Register("Default", () => new OpenALBackend());
    }

    #endregion
}