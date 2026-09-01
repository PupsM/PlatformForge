using System.Diagnostics.CodeAnalysis;

namespace PlatformNative.Core.Library;

/// <summary>
/// Базовый класс для всех нативных библиотек
/// </summary>
public abstract class Base : IDisposable
{
    #region ---- Поля ----

    private Loader? Loader;
    private bool Initialized;
    private bool Disposed;

    #endregion

    #region ---- Свойства ----

    protected abstract Func<string, IntPtr> Resolver { get; }

    public bool IsInitialized => Initialized;
    public bool IsDisposed => Disposed;

    #endregion

    #region ---- Защищённые методы ----

    internal void SetInitialized(bool value) => Initialized = value;

    protected Loader GetLoader()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (Loader is null || Loader.IsDisposed)
        {
            Loader = new Loader(Resolver);
        }

        return Loader;
    }

    #endregion

    #region ---- Публичные методы ----

    public T LoadFunction<T>(string name) where T : Delegate
        => GetLoader().LoadFunction<T>(name);

    public bool TryGetFunction<T>(string name, out T? del) where T : Delegate
        => GetLoader().TryGetFunction(name, out del);

    public void ClearCache()
        => GetLoader().ClearCache();

    #endregion

    #region ---- Статические методы ----

    public static bool LoadLibrary(string libraryKey, Func<bool> loader)
    {
        try
        {
            return loader();
        }
        catch (Exception ex)
        {
            Diagnostics.Debug($"Ошибка загрузки {libraryKey}: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region ---- IDisposable ----

    public void Dispose()
    {
        if (Disposed) return;

        try
        {
            Loader?.Dispose();
            Loader = null;
        }
        catch (Exception ex)
        {
            Diagnostics.Debug($"Ошибка освобождения {GetType().Name}: {ex.Message}");
        }

        Disposed = true;
        GC.SuppressFinalize(this);
    }

    #endregion
}