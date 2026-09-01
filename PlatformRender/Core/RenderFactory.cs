using PlatformContext;

namespace PlatformRender.Core;

/// <summary>
/// Фабрика для создания рендереров
/// </summary>
public static class RendererFactory
{
    /// <summary>
    /// Создать рендерер для переданного контекста
    /// </summary>
    public static IRenderer Create(IGraphicsContext context)
    {
        if (context is not null)
            return context.Api switch
            {
                GraphicsApi.OpenGL => new OpenGLRenderer(context),
                _ => throw new NotSupportedException($"API {context.Api} не поддерживается")
            };

        throw new ArgumentNullException(nameof(context));
    }
}