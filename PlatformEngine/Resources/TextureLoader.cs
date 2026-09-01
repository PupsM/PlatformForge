using PlatformImage.Core;
using PlatformImage.IO;
using PlatformRender;
using PlatformRender.Enums;
using PlatformRender.Graphics;
using PixelFormat = PlatformRender.Enums.PixelFormat;

namespace PlatformEngine.Resources;

/// <summary>
/// Загрузчик текстур
/// </summary>
public static class TextureLoader
{
    public static Texture2D Load(IRenderer renderer, string path)
    {
        ArgumentNullException.ThrowIfNull(renderer);

        using var image = ImageLoader.Load(path);
        var texture = renderer.CreateTexture2D(
            image.Width,
            image.Height,
            PixelFormat.R8G8B8A8
        );
        texture.SetData(image.Data.ToArray());
        return texture;
    }

    public static async Task<Texture2D> LoadAsync(IRenderer renderer, string path)
    {
        return await Task.Run(() => Load(renderer, path));
    }
}