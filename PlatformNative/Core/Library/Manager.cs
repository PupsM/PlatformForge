using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace PlatformNative.Core.Library;

/// <summary>
/// Менеджер загрузки нативных библиотек (расширяемый)
/// </summary>
public static class Manager
{
    private static readonly Dictionary<string, LibraryInfo> Libraries = [];
    private static readonly Lock Lock = new();
    private static readonly AsyncLocal<bool> IsResolving = new();

    #region ---- Регистрация библиотек ----

    public static void RegisterLibrary(string key, string[] names, Func<string, IntPtr> resolver)
    {
        lock (Lock)
        {
            if (Libraries.ContainsKey(key))
                throw new InvalidOperationException($"Библиотека '{key}' уже зарегистрирована");

            Libraries[key] = new LibraryInfo
            {
                Names = names ?? throw new ArgumentNullException(nameof(names)),
                Resolver = resolver ?? throw new ArgumentNullException(nameof(resolver))
            };
        }
    }

    public static bool IsRegistered(string key)
    {
        lock (Lock)
        {
            return Libraries.ContainsKey(key);
        }
    }

    #endregion

    #region ---- Загрузка библиотек ----

    public static bool LoadLibrary(string key)
    {
        lock (Lock)
        {
            if (!Libraries.TryGetValue(key, out var info))
            {
                Diagnostics.Warning($"Библиотека '{key}' не зарегистрирована");
                return false;
            }

            if (info.Loaded)
                return true;

            foreach (string name in info.Names)
            {
                if (NativeLibrary.TryLoad(name, out IntPtr handle))
                {
                    info.Handle = handle;
                    info.Loaded = true;
                    Diagnostics.Debug($"Загружена {key}: {name}");
                    return true;
                }
            }

            Diagnostics.Warning($"Не удалось загрузить {key}");
            return false;
        }
    }

    public static void UnloadLibrary(string key)
    {
        lock (Lock)
        {
            if (!Libraries.TryGetValue(key, out var info))
                return;

            if (info.Handle != IntPtr.Zero)
            {
                try
                {
                    NativeLibrary.Free(info.Handle);
                }
                catch (Exception ex)
                {
                    Diagnostics.Debug($"Ошибка выгрузки {key}: {ex.Message}");
                }
                info.Handle = IntPtr.Zero;
            }

            info.Loaded = false;
        }
    }

    public static IntPtr Resolve(string key, string name)
    {
        if (IsResolving.Value)
        {
            Diagnostics.Warning(
                $"Обнаружена рекурсия при разрешении функции '{name}' в библиотеке '{key}'. Возвращаем IntPtr.Zero.");
            return IntPtr.Zero;
        }

        lock (Lock)
        {
            if (!Libraries.TryGetValue(key, out var info) || !info.Loaded)
                return IntPtr.Zero;

            if (info.Handle != IntPtr.Zero)
            {
                if (NativeLibrary.TryGetExport(info.Handle, name, out IntPtr ptr) && ptr != IntPtr.Zero)
                    return ptr;
            }

            try
            {
                IsResolving.Value = true;
                return info.Resolver(name);
            }
            finally
            {
                IsResolving.Value = false;
            }
        }
    }

    #endregion

    #region ---- Очистка ----

    public static void Cleanup()
    {
        lock (Lock)
        {
            foreach (var key in Libraries.Keys.ToArray())
            {
                UnloadLibrary(key);
            }
            Libraries.Clear();
            IsResolving.Value = false;
        }
    }

    #endregion

    #region ---- Статическая инициализация (GLFW + OpenAL) ----

    static Manager()
    {
        RegisterLibrary("GLFW", GetGLFWNativeNames(), ResolveGLFWInternal);
        RegisterLibrary("OpenAL", GetOpenALNativeNames(), ResolveOpenALInternal);
    }

    private static IntPtr ResolveGLFWInternal(string name)
    {
        lock (Lock)
        {
            if (!Libraries.ContainsKey("GLFW"))
            {
                RegisterLibrary("GLFW", GetGLFWNativeNames(), ResolveGLFWInternal);
            }

            if (Libraries.TryGetValue("GLFW", out var info) && info.Loaded && info.Handle != IntPtr.Zero)
            {
                if (NativeLibrary.TryGetExport(info.Handle, name, out IntPtr ptr))
                    return ptr;
            }
            return IntPtr.Zero;
        }
    }

    private static IntPtr ResolveOpenALInternal(string name)
    {
        lock (Lock)
        {
            if (!Libraries.ContainsKey("OpenAL"))
            {
                RegisterLibrary("OpenAL", GetOpenALNativeNames(), ResolveOpenALInternal);
            }

            if (Libraries.TryGetValue("OpenAL", out var info) && info.Loaded && info.Handle != IntPtr.Zero)
            {
                if (NativeLibrary.TryGetExport(info.Handle, name, out IntPtr ptr))
                    return ptr;
            }
            return IntPtr.Zero;
        }
    }

    private static string[] GetGLFWNativeNames()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return ["glfw3.dll"];
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return ["libglfw.so.3", "libglfw.so"];
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return ["libglfw.3.dylib"];
        return [];
    }

    private static string[] GetOpenALNativeNames()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return ["soft_oal.dll", "openal32.dll"];
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return ["libopenal.so.1", "libopenal.so"];
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return ["libopenal.1.dylib"];
        return [];
    }

    #endregion

    #region ---- Удобные методы для встроенных библиотек ----

    public static bool LoadGLFW() => LoadLibrary("GLFW");
    public static bool LoadOpenAL() => LoadLibrary("OpenAL");
    public static IntPtr ResolveGLFW(string name) => Resolve("GLFW", name);
    public static IntPtr ResolveOpenAL(string name) => Resolve("OpenAL", name);

    #endregion
}