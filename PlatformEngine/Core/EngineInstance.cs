using PlatformAudio;
using PlatformAudio.Enums;
using PlatformAudio.Factory;
using PlatformAudio.Interfaces;
using PlatformContext;
using PlatformContext.Enums;
using PlatformEngine.Resources;
using PlatformImage.Core;
using PlatformImage.IO;
using PlatformInput;
using PlatformInput.Enums;
using PlatformInput.Interfaces;
using PlatformRender;
using PlatformRender.Camera;
using PlatformRender.Core;
using PlatformRender.Enums;
using PlatformRender.Graphics;
using PlatformWindow;
using PlatformWindow.Enums;
using System.Diagnostics;
using System.Numerics;

namespace PlatformEngine.Core;

/// <summary>
/// Экземпляр PlatformEngine (основная логика)
/// </summary>
public sealed class EngineInstance : IDisposable
{
    private readonly EngineConfig Config;
    private readonly Dictionary<ModuleFlags, IEngineModule> Modules = [];
    private bool Disposed;
    private Stopwatch? Stopwatch;
    private float PreviousTime;

    // ---- Прокси к модулям (доступны только если подключены) ----
    public IWindowBackend? Window { get; private set; }
    public IWindow? MainWindow { get; private set; }
    public IInput? Input { get; private set; }
    public IGraphicsContext? Context { get; private set; }
    public IRenderer? Renderer { get; private set; }
    public IAudio? Audio { get; private set; }

    // ---- Управление ресурсами ----
    public ResourceManager Resources { get; }

    // ---- Состояние ----
    public bool IsInitialized { get; private set; }
    public bool IsRunning { get; private set; }
    public float DeltaTime { get; private set; }
    public float TotalTime { get; private set; }
    public ModuleFlags ActiveModules { get; private set; }

    // ---- События ----
    public event EventHandler<FrameEventArgs>? Update;
    public event EventHandler? Render;
    public event EventHandler<ResizeEventArgs>? Resize;
    public event EventHandler? ShutdownEvent;

    public EngineInstance(EngineConfig config)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));
        ActiveModules = config.Modules;
        Resources = new ResourceManager(this);

        InitializeModules();
        IsInitialized = true;
    }

    private void InitializeModules()
    {
        // ---- 1. Окно ----
        if (Config.Modules.HasFlag(ModuleFlags.Window))
        {
            Window = WindowFactory.CreateGLFW();
            Window.Initialize();

            MainWindow = Window.CreateWindow(
                Config.Title,
                Config.Width,
                Config.Height,
                Config.WindowFlags
            );

            // Подписка на событие изменения размера
            MainWindow.Resized += (w, width, height) =>
            {
                OnResize(width, height);
            };
        }

        // ---- 2. Ввод ----
        if (Config.Modules.HasFlag(ModuleFlags.Input) && MainWindow is not null)
        {
            Input = InputFactory.CreateGLFW(MainWindow.Handle);
        }

        // ---- 3. Графика ----
        if (Config.Modules.HasFlag(ModuleFlags.Graphics) && MainWindow is not null)
        {
            Context = GraphicsFactory.CreateOpenGL(
                Config.OpenGLMajor,
                Config.OpenGLMinor,
                Config.OpenGLProfile
            );
            Context.MakeCurrent(MainWindow.Handle);
            Context.SetSwapInterval(Config.SwapInterval);

            Renderer = RendererFactory.Create(Context);
            Renderer.Initialize();
            Renderer.SetViewport(0, 0, Config.Width, Config.Height);
        }

        // ---- 4. Аудио ----
        if (Config.Modules.HasFlag(ModuleFlags.Audio))
        {
            Audio = AudioFactory.CreateOpenAL();
            Audio.Initialize();
            Audio.SetDistanceModel(Config.AudioDistanceModel);
            Audio.SetDopplerFactor(Config.AudioDopplerFactor);
            Audio.SetSpeedOfSound(Config.AudioSpeedOfSound);
        }
    }

    // ---- Ресурсы ----
    public Texture2D LoadTexture(string path)
        => Resources.LoadTexture(path);

    public ISoundBuffer LoadSound(string path)
        => Resources.LoadSound(path);

    public ShaderProgram LoadShader(string vertexPath, string fragmentPath)
        => Resources.LoadShader(vertexPath, fragmentPath);

    public Mesh LoadMesh(string path)
        => Resources.LoadMesh(path);

    public void ClearCache()
        => Resources.ClearCache();

    // ---- Примитивы ----
    public Mesh CreateQuad(float width = 1f, float height = 1f)
    {
        if (Renderer is null)
            throw new InvalidOperationException("Renderer not initialized");
        return Renderer.CreateQuad(width, height);
    }

    public Mesh CreateCube(float size = 1f)
    {
        if (Renderer is null)
            throw new InvalidOperationException("Renderer not initialized");
        return Renderer.CreateCube(size);
    }

    public Mesh CreateSphere(float radius = 0.5f, int segments = 32)
    {
        if (Renderer is null)
            throw new InvalidOperationException("Renderer not initialized");
        return Renderer.CreateSphere(radius, segments);
    }

    public Mesh CreatePlane(float width = 1f, float height = 1f, int segmentsX = 1, int segmentsY = 1)
    {
        if (Renderer is null)
            throw new InvalidOperationException("Renderer not initialized");
        return Renderer.CreatePlane(width, height, segmentsX, segmentsY);
    }

    // ---- Камеры ----
    public static PerspectiveCamera CreatePerspectiveCamera(float fov = 60f, float aspect = 1.333f, float near = 0.1f, float far = 100f)  // <- ИСПРАВЛЕНО: добавлен static
        => new(fov, aspect, near, far);

    public static OrthographicCamera CreateOrthographicCamera(float left = -10f, float right = 10f, float bottom = -10f, float top = 10f, float near = -100f, float far = 100f)  // <- ИСПРАВЛЕНО: добавлен static
        => new(left, right, bottom, top, near, far);

    // ---- Дополнительные окна ----
    public IWindow CreateWindow(string title, int width, int height, WindowFlags flags = WindowFlags.Default)
    {
        if (Window is null)
            throw new InvalidOperationException("Window module not initialized");
        return Window.CreateWindow(title, width, height, flags);
    }

    public void DestroyWindow(IWindow window)
    {
        if (Window is null)
            throw new InvalidOperationException("Window module not initialized");
        Window.DestroyWindow(window);
    }

    // ---- События ----
    private void OnResize(int width, int height)
    {
        Renderer?.SetViewport(0, 0, width, height);
        Resize?.Invoke(this, new ResizeEventArgs(width, height));
    }

    // ---- Главный цикл ----
    public void Run()
    {
        if (!IsInitialized)
            throw new InvalidOperationException("Engine not initialized");

        IsRunning = true;
        Stopwatch = Stopwatch.StartNew();
        PreviousTime = 0;

        while (IsRunning)
        {
            float currentTime = (float)Stopwatch.Elapsed.TotalSeconds;
            DeltaTime = currentTime - PreviousTime;
            PreviousTime = currentTime;
            TotalTime += DeltaTime;

            // Обработка событий окна
            Window?.PollEvents();

            // Проверка закрытия окна
            if (MainWindow?.ShouldClose == true)
            {
                IsRunning = false;
                break;
            }

            // Обновление модулей
            Input?.Update();
            Audio?.Update();

            foreach (var module in Modules.Values)
            {
                module.Update(DeltaTime);
            }

            // Событие обновления
            Update?.Invoke(this, new FrameEventArgs(DeltaTime, TotalTime));

            // Рендеринг
            if (Renderer is not null && Renderer.IsInitialized)
            {
                Renderer.BeginFrame();
                Render?.Invoke(this, EventArgs.Empty);
                Renderer.EndFrame();
                Context?.SwapBuffers();
            }
        }

        Shutdown();
    }

    public void Shutdown()
    {
        if (!IsRunning && !IsInitialized) return;

        IsRunning = false;
        ShutdownEvent?.Invoke(this, EventArgs.Empty);

        // Очистка модулей
        foreach (var module in Modules.Values)
        {
            module.Shutdown();
        }
        Modules.Clear();

        // Очистка ресурсов
        Resources.ClearCache();

        // Очистка в обратном порядке
        Renderer?.Dispose();
        Context?.Dispose();
        Audio?.Dispose();
        Input?.Dispose();
        MainWindow?.Dispose();
        Window?.Dispose();

        IsInitialized = false;
    }

    // ---- Расширяемость ----
    public void RegisterModule(IEngineModule module)
    {
        ArgumentNullException.ThrowIfNull(module);  // <- ИСПРАВЛЕНО: используем ThrowIfNull

        if (Modules.ContainsKey(module.Flag))
            throw new InvalidOperationException($"Module {module.Name} already registered");

        module.Initialize(Config);
        Modules[module.Flag] = module;

        // Обновляем активные флаги
        ActiveModules |= module.Flag;
    }

    public T? GetModule<T>(ModuleFlags flag) where T : class, IEngineModule
    {
        if (Modules.TryGetValue(flag, out var module))
            return module as T;
        return null;
    }

    public bool IsModuleLoaded(ModuleFlags flag)
        => Modules.ContainsKey(flag) || ActiveModules.HasFlag(flag);

    public void Dispose()
    {
        if (Disposed) return;
        Shutdown();
        Disposed = true;
        GC.SuppressFinalize(this);
    }
}