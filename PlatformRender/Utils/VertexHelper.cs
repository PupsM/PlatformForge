using PlatformRender.Graphics;

namespace PlatformRender.Utils;

/// <summary>
/// Вспомогательные методы для создания вершин
/// </summary>
public static class VertexHelper
{
    public static Vertex Position(float x, float y, float z)
        => new(x, y, z, 0, 0, 0, 0, 0);

    public static Vertex PositionUV(float x, float y, float z, float u, float v)
        => new(x, y, z, u, v, 0, 0, 0);

    public static Vertex Full(float x, float y, float z, float u, float v, float nx, float ny, float nz)
        => new(x, y, z, u, v, nx, ny, nz);
}