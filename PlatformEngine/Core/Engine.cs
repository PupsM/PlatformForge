using PlatformAudio.Interfaces;
using PlatformContext;
using PlatformInput.Interfaces;
using PlatformRender;
using PlatformRender.Camera;
using PlatformRender.Graphics;
using PlatformWindow;
using PlatformWindow.Enums;

namespace PlatformEngine.Core;

/// <summary>
/// Статический фасад PlatformEngine
/// </summary>
public static class Engine
{
    private static EngineInstance? Instance;

    /// <summary>Инициализирован ли движок</summary>
    public static bool IsInitialized => Instance is not null && Instance.IsInitialized;

    /// <summary>Запущен ли движок</summary>
    public static bool IsRunning => Instance is not null && Instance.IsRunning;

    /// <summary>Дельта времени (время между кадрами)</summary>
    public static float DeltaTime => Instance?.DeltaTime ?? 0;

    /// <summary>Общее время работы движка</summary>
    public static float TotalTime => Instance?.TotalTime ?? 0;

    /// <summary>Активные модули</summary>
    public static ModuleFlags ActiveModules => Instance?.ActiveModules ?? ModuleFlags.None;

    // ---- Инициализация ----
    public static void Initialize(EngineConfig config)
    {
        if (Instance is not null)
            throw new InvalidOperationException("Движок уже инициализирован");

        Instance = new EngineInstance(config);
    }

    public static void Initialize(string title, int width = 1280, int height = 720, ModuleFlags modules = ModuleFlags.Default)
    {
        Initialize(new EngineConfig
        {
            Title = title,
            Width = width,
            Height = height,
            Modules = modules
        });
    }

    // ---- Жизненный цикл ----
    public static void Run()
    {
        if (Instance is null)
            throw new InvalidOperationException("Движок не инициализирован");

        Instance.Run();
    }

    public static void Shutdown()
    {
        Instance?.Shutdown();
        Instance = null;
    }

    // ---- Прокси к подсистемам ----
    public static IWindowBackend Window
    {
        get
        {
            if (Instance is null)
                throw new InvalidOperationException("Движок не инициализирован");
            if (Instance.Window is null)
                throw new InvalidOperationException("Модуль окна не загружен");
            return Instance.Window;
        }
    }

    public static IWindow MainWindow
    {
        get
        {
            if (Instance is null)
                throw new InvalidOperationException("Движок не инициализирован");
            if (Instance.MainWindow is null)
                throw new InvalidOperationException("Модуль окна не загружен");
            return Instance.MainWindow;
        }
    }

    public static IInput Input
    {
        get
        {
            if (Instance is null)
                throw new InvalidOperationException("Движок не инициализирован");
            if (Instance.Input is null)
                throw new InvalidOperationException("Модуль ввода не загружен");
            return Instance.Input;
        }
    }

    public static IGraphicsContext Context
    {
        get
        {
            if (Instance is null)
                throw new InvalidOperationException("Движок не инициализирован");
            if (Instance.Context is null)
                throw new InvalidOperationException("Модуль графики не загружен");
            return Instance.Context;
        }
    }

    public static IRenderer Renderer
    {
        get
        {
            if (Instance is null)
                throw new InvalidOperationException("Движок не инициализирован");
            if (Instance.Renderer is null)
                throw new InvalidOperationException("Модуль графики не загружен");
            return Instance.Renderer;
        }
    }

    public static IAudio Audio
    {
        get
        {
            if (Instance is null)
                throw new InvalidOperationException("Движок не инициализирован");
            if (Instance.Audio is null)
                throw new InvalidOperationException("Модуль аудио не загружен");
            return Instance.Audio;
        }
    }

    // ---- Ресурсы ----
    public static Texture2D LoadTexture(string path)
    {
        if (Instance is null)
            throw new InvalidOperationException("Движок не инициализирован");
        return Instance.LoadTexture(path);
    }

    public static ISoundBuffer LoadSound(string path)
    {
        if (Instance is null)
            throw new InvalidOperationException("Движок не инициализирован");
        return Instance.LoadSound(path);
    }

    public static ShaderProgram LoadShader(string vertexPath, string fragmentPath)
    {
        if (Instance is null)
            throw new InvalidOperationException("Движок не инициализирован");
        return Instance.LoadShader(vertexPath, fragmentPath);
    }

    public static Mesh LoadMesh(string path)
    {
        if (Instance is null)
            throw new InvalidOperationException("Движок не инициализирован");
        return Instance.LoadMesh(path);
    }

    public static void ClearCache()
    {
        if (Instance is null)
            throw new InvalidOperationException("Движок не инициализирован");
        Instance.ClearCache();
    }

    // ---- Примитивы ----
    public static Mesh CreateQuad(float width = 1f, float height = 1f)
    {
        if (Instance is null)
            throw new InvalidOperationException("Движок не инициализирован");
        return Instance.CreateQuad(width, height);
    }

    public static Mesh CreateCube(float size = 1f)
    {
        if (Instance is null)
            throw new InvalidOperationException("Движок не инициализирован");
        return Instance.CreateCube(size);
    }

    public static Mesh CreateSphere(float radius = 0.5f, int segments = 32)
    {
        if (Instance is null)
            throw new InvalidOperationException("Движок не инициализирован");
        return Instance.CreateSphere(radius, segments);
    }

    public static Mesh CreatePlane(float width = 1f, float height = 1f, int segmentsX = 1, int segmentsY = 1)
    {
        if (Instance is null)
            throw new InvalidOperationException("Движок не инициализирован");
        return Instance.CreatePlane(width, height, segmentsX, segmentsY);
    }

    // ---- Камеры ----
    public static PerspectiveCamera CreatePerspectiveCamera(float fov = 60f, float aspect = 1.333f, float near = 0.1f, float far = 100f)
        => EngineInstance.CreatePerspectiveCamera(fov, aspect, near, far);

    public static OrthographicCamera CreateOrthographicCamera(float left = -10f, float right = 10f, float bottom = -10f, float top = 10f, float near = -100f, float far = 100f)
        => EngineInstance.CreateOrthographicCamera(left, right, bottom, top, near, far);

    // ---- Дополнительные окна ----
    public static IWindow CreateWindow(string title, int width, int height, WindowFlags flags = WindowFlags.Default)
    {
        if (Instance is null)
            throw new InvalidOperationException("Движок не инициализирован");
        return Instance.CreateWindow(title, width, height, flags);
    }

    public static void DestroyWindow(IWindow window)
    {
        if (Instance is null)
            throw new InvalidOperationException("Движок не инициализирован");
        Instance.DestroyWindow(window);
    }

    // ---- События ----
    public static event EventHandler<FrameEventArgs>? OnUpdate
    {
        add
        {
            if (Instance is null)
                throw new InvalidOperationException("Движок не инициализирован");
            Instance.Update += value;
        }
        remove
        {
            if (Instance is null)
                throw new InvalidOperationException("Движок не инициализирован");
            Instance.Update -= value;
        }
    }

    public static event EventHandler? OnRender
    {
        add
        {
            if (Instance is null)
                throw new InvalidOperationException("Движок не инициализирован");
            Instance.Render += value;
        }
        remove
        {
            if (Instance is null)
                throw new InvalidOperationException("Движок не инициализирован");
            Instance.Render -= value;
        }
    }

    public static event EventHandler<ResizeEventArgs>? OnResize
    {
        add
        {
            if (Instance is null)
                throw new InvalidOperationException("Движок не инициализирован");
            Instance.Resize += value;
        }
        remove
        {
            if (Instance is null)
                throw new InvalidOperationException("Движок не инициализирован");
            Instance.Resize -= value;
        }
    }

    public static event EventHandler? OnShutdown
    {
        add
        {
            if (Instance is null)
                throw new InvalidOperationException("Движок не инициализирован");
            Instance.ShutdownEvent += value;
        }
        remove
        {
            if (Instance is null)
                throw new InvalidOperationException("Движок не инициализирован");
            Instance.ShutdownEvent -= value;
        }
    }

    // ---- Расширяемость ----
    public static void RegisterModule(IEngineModule module)
    {
        if (Instance is null)
            throw new InvalidOperationException("Движок не инициализирован");
        Instance.RegisterModule(module);
    }

    public static T? GetModule<T>(ModuleFlags flag) where T : class, IEngineModule
    {
        if (Instance is null)
            throw new InvalidOperationException("Движок не инициализирован");
        return Instance.GetModule<T>(flag);
    }

    public static bool IsModuleLoaded(ModuleFlags flag)
    {
        if (Instance is null)
            throw new InvalidOperationException("Движок не инициализирован");
        return Instance.IsModuleLoaded(flag);
    }
}