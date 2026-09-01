using PlatformNative.Core;
using PlatformNative.Core.Library;
using System.Diagnostics.CodeAnalysis;

namespace PlatformNative;

/// <summary>
/// Хост для нативной библиотеки с синглтон-доступом
/// </summary>
/// <typeparam name="THost">Тип хоста (наследник)</typeparam>
/// <typeparam name="TLibrary">Тип библиотеки (наследник NativeLibraryBase)</typeparam>
public abstract class Host<THost, TLibrary>
    where THost : Host<THost, TLibrary>, new()
    where TLibrary : Base, new()
{
    #region ---- Поля ----

    private static THost? HostInstance;
    private static readonly Lock Lock = new();

    #endregion

    #region ---- Защищённые свойства ----

    protected abstract string LibraryKey { get; }
    protected abstract Func<string, IntPtr> Resolver { get; }
    protected abstract Func<bool> Loader { get; }

    #endregion

    #region ---- Свойства экземпляра ----

    public TLibrary Library { get; private set; } = new();
    public bool IsInitialized => Library.IsInitialized && !Library.IsDisposed;

    #endregion

    #region ---- Статические методы доступа ----

    protected static THost Instance
    {
        get
        {
            lock (Lock)
            {
                if (HostInstance is null || HostInstance.Library.IsDisposed)
                {
                    HostInstance = new THost
                    {
                        Library = new TLibrary()
                    };
                }
                return HostInstance;
            }
        }
    }

    #endregion

    #region ---- Абстрактные методы ----

    protected abstract bool InitializeLibrary();
    protected abstract void ShutdownLibrary();

    #endregion

    #region ---- Публичные методы хоста ----

    public bool Initialize()
    {
        if (IsInitialized) return true;

        if (!Base.LoadLibrary(LibraryKey, Loader))
            return false;

        if (!InitializeLibrary())
            return false;

        Library.SetInitialized(true);
        Diagnostics.Info($"{LibraryKey} инициализирован");
        return true;
    }

    public void Shutdown()
    {
        if (!IsInitialized) return;

        ShutdownLibrary();
        Library.SetInitialized(false);
        Diagnostics.Info($"{LibraryKey} завершён");
    }

    #endregion

    #region ---- Статические публичные методы ----

    public static bool InitializeStatic() => Instance.Initialize();
    public static void ShutdownStatic() => Instance.Shutdown();
    public static bool IsInitializedStatic => Instance.IsInitialized;

    public static T LoadFunction<T>(string name) where T : Delegate
    {
        var instance = Instance;
        if (instance.Library is null || instance.Library.IsDisposed)
            throw new InvalidOperationException("Библиотека не инициализирована или уже освобождена");
        return instance.Library.LoadFunction<T>(name);
    }

    public static bool TryGetFunction<T>(string name, out T? del) where T : Delegate
    {
        var instance = Instance;
        if (instance.Library is null || instance.Library.IsDisposed)
        {
            del = default;
            return false;
        }
        return instance.Library.TryGetFunction(name, out del);
    }

    public static void ClearCache()
    {
        var instance = Instance;
        if (instance.Library is null || instance.Library.IsDisposed)
            return;
        instance.Library.ClearCache();
    }

    public static void Cleanup()
    {
        lock (Lock)
        {
            if (HostInstance is not null)
            {
                HostInstance.Shutdown();
                HostInstance.Library.Dispose();
                HostInstance = null;
            }
        }
    }

    #endregion
}