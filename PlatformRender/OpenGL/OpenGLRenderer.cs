using PlatformContext;
using PlatformNative.Core;
using PlatformNative.Native;
using PlatformRender.Core;
using PlatformRender.Enums;
using PlatformRender.Graphics;
using System.Numerics;

namespace PlatformRender;

/// <summary>
/// Рендерер на OpenGL
/// </summary>
public sealed class OpenGLRenderer : IRenderer
{
    private readonly IGraphicsContext RendererContext;
    private bool Initialized;
    private bool Disposed;
    private readonly RenderCapabilities RendererCapabilities = new();
    private Matrix4x4 RendererProjectionMatrix = Matrix4x4.Identity;
    private Matrix4x4 RendererViewMatrix = Matrix4x4.Identity;
    private Matrix4x4 RendererModelMatrix = Matrix4x4.Identity;
    private ShaderProgram? CurrentProgram;
    private int RendererViewportWidth = 800;
    private int RendererViewportHeight = 600;

    #region ---- Свойства ----

    public bool IsInitialized => Initialized;
    public bool IsDisposed => Disposed;
    public RenderCapabilities Capabilities => RendererCapabilities;
    public IGraphicsContext Context => RendererContext;

    public Matrix4x4 ProjectionMatrix
    {
        get => RendererProjectionMatrix;
        set => RendererProjectionMatrix = value;
    }

    public Matrix4x4 ViewMatrix
    {
        get => RendererViewMatrix;
        set => RendererViewMatrix = value;
    }

    public Matrix4x4 ModelMatrix
    {
        get => RendererModelMatrix;
        set => RendererModelMatrix = value;
    }

    public Color ClearColor { get; set; } = new(0.1f, 0.1f, 0.15f, 1.0f);

    public int ViewportWidth => RendererViewportWidth;
    public int ViewportHeight => RendererViewportHeight;

    #endregion

    #region ---- Конструктор ----

    public OpenGLRenderer(IGraphicsContext context)
    {
        if (context.Api != GraphicsApi.OpenGL)
            throw new InvalidOperationException($"Ожидался OpenGL контекст, получен {context.Api}");

        RendererContext = context ?? throw new ArgumentNullException(nameof(context));
    }

    #endregion

    #region ---- Инициализация ----

    public void Initialize()
    {
        if (Initialized) return;

        if (!RendererContext.IsInitialized)
            throw new InvalidOperationException("Контекст не инициализирован");

        // ✅ Проверяем, что OpenGL уже инициализирован
        if (!OpenGL.IsInitialized)
            throw new InvalidOperationException("OpenGL не инициализирован. Убедитесь, что контекст создан.");

        DetectCapabilities();
        SetDefaultState();

        Initialized = true;
        Diagnostics.Info($"OpenGL рендерер инициализирован: {RendererCapabilities.Renderer}");
    }

    private void DetectCapabilities()
    {
        const uint GL_SHADING_LANGUAGE_VERSION = 0x8B8C;

        if (OpenGL.TryGetFunction<OpenGL.glGetStringDelegate>("glGetString", out var getString) && getString is not null)
        {
            RendererCapabilities.Renderer = GetStringValue(getString, OpenGL.GL_RENDERER);
            RendererCapabilities.Vendor = GetStringValue(getString, OpenGL.GL_VENDOR);
            RendererCapabilities.Version = GetStringValue(getString, OpenGL.GL_VERSION);
            RendererCapabilities.GLSLVersion = GetStringValue(getString, GL_SHADING_LANGUAGE_VERSION);
        }

        if (OpenGL.TryGetFunction<OpenGL.glGetIntegervDelegate>("glGetIntegerv", out var getInt) && getInt is not null)
        {
            RendererCapabilities.MaxTextureSize = GetIntValue(getInt, OpenGL.GL_MAX_TEXTURE_SIZE);
            RendererCapabilities.MaxVertexAttributes = GetIntValue(getInt, OpenGL.GL_MAX_VERTEX_ATTRIBS);
            RendererCapabilities.MaxUniformBufferSize = GetIntValue(getInt, OpenGL.GL_MAX_UNIFORM_BLOCK_SIZE);
            RendererCapabilities.MaxShaderStorageBufferSize = GetIntValue(getInt, OpenGL.GL_MAX_SHADER_STORAGE_BLOCK_SIZE);
        }
    }

    private static string GetStringValue(OpenGL.glGetStringDelegate getString, uint name)
    {
        IntPtr ptr = getString(name);
        return ptr != IntPtr.Zero ? System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr) ?? string.Empty : string.Empty;
    }

    private static int GetIntValue(OpenGL.glGetIntegervDelegate getInt, uint pname)
    {
        int value = 0;
        getInt(pname, ref value);
        return value;
    }

    private void SetDefaultState()
    {
        const uint GL_DEPTH_TEST = 0x0B71;
        const uint GL_CULL_FACE = 0x0B44;
        const uint GL_BLEND = 0x0BE2;
        const uint GL_SRC_ALPHA = 0x0302;
        const uint GL_ONE_MINUS_SRC_ALPHA = 0x0303;

        if (OpenGL.TryGetFunction<OpenGL.glEnableDelegate>("glEnable", out var enable) && enable is not null)
        {
            enable(GL_DEPTH_TEST);
            enable(GL_CULL_FACE);
            enable(GL_BLEND);
        }

        if (OpenGL.TryGetFunction<OpenGL.glBlendFuncDelegate>("glBlendFunc", out var blendFunc) && blendFunc is not null)
        {
            blendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        }

        if (OpenGL.TryGetFunction<OpenGL.glClearColorDelegate>("glClearColor", out var clearColor) && clearColor is not null)
        {
            clearColor(ClearColor.R, ClearColor.G, ClearColor.B, ClearColor.A);
        }
    }

    #endregion

    #region ---- Основные методы ----

    public void BeginFrame()
    {
        EnsureInitialized();

        const uint GL_COLOR_BUFFER_BIT = 0x00004000;
        const uint GL_DEPTH_BUFFER_BIT = 0x00000100;

        if (OpenGL.TryGetFunction<OpenGL.glClearDelegate>("glClear", out var clear) && clear is not null)
        {
            clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        }

        if (OpenGL.TryGetFunction<OpenGL.glClearColorDelegate>("glClearColor", out var clearColor) && clearColor is not null)
        {
            clearColor(ClearColor.R, ClearColor.G, ClearColor.B, ClearColor.A);
        }
    }

    public void EndFrame()
    {
        // Ничего не делаем — SwapBuffers вызывается из контекста
    }

    public void SetViewport(int x, int y, int width, int height)
    {
        EnsureInitialized();

        RendererViewportWidth = width;
        RendererViewportHeight = height;

        if (OpenGL.TryGetFunction<OpenGL.glViewportDelegate>("glViewport", out var viewport) && viewport is not null)
        {
            viewport(x, y, width, height);
        }
    }

    #endregion

    #region ---- Рендеринг ----

    public void DrawMesh(Mesh mesh, ShaderProgram? program = null, Texture2D? texture = null)
    {
        EnsureInitialized();

        if (mesh is null || !mesh.HasVertices) return;

        var activeProgram = program ?? mesh.Program;
        if (activeProgram is null) return;

        activeProgram.Bind();
        CurrentProgram = activeProgram;

        activeProgram.SetUniform("u_Projection", RendererProjectionMatrix);
        activeProgram.SetUniform("u_View", RendererViewMatrix);
        activeProgram.SetUniform("u_Model", RendererModelMatrix);

        var activeTexture = texture ?? mesh.Texture;
        if (activeTexture is not null)
        {
            activeProgram.SetUniform("u_Texture", activeTexture, 0);
        }

        mesh.Draw();
    }

    public void DrawMesh(Mesh mesh, Material material)
    {
        EnsureInitialized();

        if (mesh is null || material is null || !mesh.HasVertices) return;

        material.Apply();
        material.SetUniform("u_Projection", RendererProjectionMatrix);
        material.SetUniform("u_View", RendererViewMatrix);
        material.SetUniform("u_Model", RendererModelMatrix);

        mesh.Draw();
    }

    #endregion

    #region ---- Создание объектов ----

    public Shader CreateShader(ShaderType type, string source)
    {
        EnsureInitialized();
        var shader = new Shader(type, source);
        shader.Compile();
        return shader;
    }

    public ShaderProgram CreateProgram(Shader vertex, Shader fragment)
    {
        EnsureInitialized();
        return new ShaderProgram(vertex, fragment);
    }

    public Texture2D CreateTexture2D(int width, int height, PixelFormat format = PixelFormat.R8G8B8A8)
    {
        EnsureInitialized();
        return new Texture2D(width, height, format);
    }

    public Mesh CreateMesh(Vertex[] vertices, uint[]? indices = null)
    {
        EnsureInitialized();
        var mesh = new Mesh();
        mesh.SetVertices(vertices);
        if (indices is not null)
        {
            mesh.SetIndices(indices);
        }
        return mesh;
    }

    #endregion

    #region ---- Примитивы ----

    public Mesh CreateQuad(float width = 1f, float height = 1f)
        => Primitives.Quad.Create(width, height);

    public Mesh CreateCube(float size = 1f)
        => Primitives.Cube.Create(size);

    public Mesh CreateSphere(float radius = 0.5f, int segments = 32)
        => Primitives.Sphere.Create(radius, segments);

    public Mesh CreatePlane(float width = 1f, float height = 1f, int segmentsX = 1, int segmentsY = 1)
        => Primitives.Plane.Create(width, height, segmentsX, segmentsY);

    #endregion

    #region ---- Вспомогательные методы ----

    private void EnsureInitialized()
    {
        if (!Initialized)
            throw new InvalidOperationException("Рендерер не инициализирован. Вызовите Initialize().");
        if (!Disposed)
            return;
        throw new ObjectDisposedException(nameof(OpenGLRenderer));
    }

    #endregion

    #region ---- IDisposable ----

    public void Dispose()
    {
        if (Disposed) return;

        if (CurrentProgram is not null)
        {
            ShaderProgram.Unbind();  // ✅ Используем имя класса
            CurrentProgram = null;
        }

        Disposed = true;
        Diagnostics.Info("OpenGL рендерер освобождён");
        GC.SuppressFinalize(this);
    }

    #endregion
}