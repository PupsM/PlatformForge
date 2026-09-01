using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace PlatformNative.Core;

/// <summary>
/// Загрузчик нативных функций с кешированием
/// </summary>
[RequiresDynamicCode("Uses Marshal.GetDelegateForFunctionPointer")]
public sealed class Loader : IDisposable
{
    #region ---- Поля ----

    private readonly Func<string, IntPtr> Resolver;
    private readonly Dictionary<string, Delegate> Cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock CacheLock = new();
    private bool Disposed;

    #endregion

    #region ---- Конструктор ----

    public Loader(Func<string, IntPtr> resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        Resolver = resolver;
    }

    #endregion

    #region ---- Свойства ----

    public int CacheSize
    {
        get
        {
            lock (CacheLock) return Cache.Count;
        }
    }

    public bool IsDisposed => Disposed;

    #endregion

    #region ---- Публичные методы ----

    public T LoadFunction<T>(string name) where T : Delegate
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Имя функции не может быть пустым", nameof(name));

        if (typeof(T) == typeof(Delegate))
            throw new ArgumentException("Используйте конкретный тип делегата", nameof(T));

        lock (CacheLock)
        {
            if (Cache.TryGetValue(name, out Delegate? cached))
            {
                if (cached is T typedDelegate)
                    return typedDelegate;

                throw new InvalidOperationException(
                    $"Тип делегата в кеше ({cached.GetType().Name}) не соответствует запрошенному ({typeof(T).Name})");
            }

            IntPtr ptr = Resolver(name);
            if (ptr == IntPtr.Zero)
                throw new NotSupportedException($"Функция '{name}' не найдена");

            T delegateInstance = Marshal.GetDelegateForFunctionPointer<T>(ptr);
            Cache[name] = delegateInstance;

            Diagnostics.Debug($"Загружена функция: {name} ({typeof(T).Name})");
            return delegateInstance;
        }
    }

    public bool TryGetFunction<T>(string name, [NotNullWhen(true)] out T? del) where T : Delegate
    {
        del = default;

        if (Disposed || string.IsNullOrEmpty(name))
            return false;

        if (typeof(T) == typeof(Delegate))
            throw new ArgumentException("Используйте конкретный тип делегата", nameof(T));

        lock (CacheLock)
        {
            if (Cache.TryGetValue(name, out Delegate? cached))
            {
                if (cached is T typedDelegate)
                {
                    del = typedDelegate;
                    return true;
                }
                return false;
            }

            IntPtr ptr = Resolver(name);
            if (ptr == IntPtr.Zero)
                return false;

            T delegateInstance = Marshal.GetDelegateForFunctionPointer<T>(ptr);
            Cache[name] = delegateInstance;
            del = delegateInstance;

            Diagnostics.Debug($"Загружена функция: {name} ({typeof(T).Name})");
            return true;
        }
    }

    public void ClearCache()
    {
        lock (CacheLock)
        {
            Cache.Clear();
        }
    }

    public void Dispose()
    {
        if (Disposed) return;
        ClearCache();
        Disposed = true;
        GC.SuppressFinalize(this);
    }

    #endregion
}