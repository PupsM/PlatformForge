using PlatformRender;
using PlatformRender.Graphics;

namespace PlatformEngine.Resources;

/// <summary>
/// Загрузчик мешей
/// </summary>
public static class MeshLoader
{
    public static Mesh Load(IRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);

        // TODO: Реализовать загрузку OBJ/FBX/glTF
        // Пока возвращаем куб как заглушку
        return renderer.CreateCube(1f);
    }
}