using PlatformContext;
using PlatformRender.Core;
using PlatformRender.Enums;
using PlatformRender.Graphics;
using System.Numerics;

namespace PlatformRender;

/// <summary>
/// Интерфейс рендерера
/// </summary>
public interface IRenderer : IDisposable
{
    /// <summary>Инициализирован ли рендерер</summary>
    bool IsInitialized { get; }

    /// <summary>Информация о возможностях GPU</summary>
    RenderCapabilities Capabilities { get; }

    /// <summary>Графический контекст</summary>
    IGraphicsContext Context { get; }

    /// <summary>Матрица проекции</summary>
    Matrix4x4 ProjectionMatrix { get; set; }

    /// <summary>Матрица вида</summary>
    Matrix4x4 ViewMatrix { get; set; }

    /// <summary>Матрица модели</summary>
    Matrix4x4 ModelMatrix { get; set; }

    /// <summary>Цвет очистки</summary>
    Color ClearColor { get; set; }

    /// <summary>Инициализация рендерера</summary>
    void Initialize();

    /// <summary>Начать кадр</summary>
    void BeginFrame();

    /// <summary>Закончить кадр</summary>
    void EndFrame();

    /// <summary>Установить область просмотра</summary>
    void SetViewport(int x, int y, int width, int height);

    // ---- Создание объектов ----

    Shader CreateShader(ShaderType type, string source);
    ShaderProgram CreateProgram(Shader vertex, Shader fragment);
    Mesh CreateMesh(Vertex[] vertices, uint[]? indices = null);
    Texture2D CreateTexture2D(int width, int height, PixelFormat format = PixelFormat.R8G8B8A8);

    // ---- Рендеринг ----

    void DrawMesh(Mesh mesh, ShaderProgram? program = null, Texture2D? texture = null);
    void DrawMesh(Mesh mesh, Material material);

    // ---- Примитивы ----

    Mesh CreateQuad(float width = 1f, float height = 1f);
    Mesh CreateCube(float size = 1f);
    Mesh CreateSphere(float radius = 0.5f, int segments = 32);
    Mesh CreatePlane(float width = 1f, float height = 1f, int segmentsX = 1, int segmentsY = 1);
}