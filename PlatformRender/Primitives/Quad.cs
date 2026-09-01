using PlatformRender.Graphics;
using System.Numerics;

namespace PlatformRender.Primitives;

/// <summary>
/// Генератор квадратной геометрии
/// </summary>
public static class Quad
{
    /// <summary>
    /// Создаёт квадрат (плоскость) с текстурными координатами
    /// </summary>
    public static Mesh Create(float width = 1f, float height = 1f)
    {
        float w = width * 0.5f;
        float h = height * 0.5f;

        // 4 вершины: позиция + UV + нормаль (смотрит вперёд)
        var vertices = new Vertex[]
        {
            // Левый-нижний  (0, 0)
            new(-w, -h, 0, 0, 0, 0, 0, 1),
            // Правый-нижний (1, 0)
            new( w, -h, 0, 1, 0, 0, 0, 1),
            // Правый-верхний (1, 1)
            new( w,  h, 0, 1, 1, 0, 0, 1),
            // Левый-верхний  (0, 1)
            new(-w,  h, 0, 0, 1, 0, 0, 1),
        };

        // Два треугольника: 0-1-2 и 0-2-3
        // Против часовой стрелки для видимой стороны
        var indices = new uint[]
        {
            0, 1, 2,  // Треугольник 1
            0, 2, 3   // Треугольник 2
        };

        var mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetIndices(indices);
        return mesh;
    }

    /// <summary>
    /// Создаёт квадрат с возможностью центрирования и поворота
    /// </summary>
    public static Mesh CreateAdvanced(float width = 1f, float height = 1f,
                                       bool centered = true, float rotation = 0f)
    {
        float w = width * 0.5f;
        float h = height * 0.5f;

        // Позиции вершин
        Vector3[] positions;

        if (centered)
        {
            positions =
            [
                new(-w, -h, 0),  // 0: Левый-нижний
                new( w, -h, 0),  // 1: Правый-нижний
                new( w,  h, 0),  // 2: Правый-верхний
                new(-w,  h, 0)   // 3: Левый-верхний
            ];
        }
        else
        {
            positions =
            [
                new(0, 0, 0),     // 0: Левый-нижний
                new(width, 0, 0), // 1: Правый-нижний
                new(width, height, 0), // 2: Правый-верхний
                new(0, height, 0) // 3: Левый-верхний
            ];
        }

        // Поворот (если нужен)
        if (rotation != 0)
        {
            var rot = Matrix4x4.CreateRotationZ(rotation);
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = Vector3.Transform(positions[i], rot);
            }
        }

        var vertices = new Vertex[]
        {
            new(positions[0].X, positions[0].Y, positions[0].Z, 0, 0, 0, 0, 1),
            new(positions[1].X, positions[1].Y, positions[1].Z, 1, 0, 0, 0, 1),
            new(positions[2].X, positions[2].Y, positions[2].Z, 1, 1, 0, 0, 1),
            new(positions[3].X, positions[3].Y, positions[3].Z, 0, 1, 0, 0, 1),
        };

        var indices = new uint[] { 0, 1, 2, 0, 2, 3 };

        var mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetIndices(indices);
        return mesh;
    }

    /// <summary>
    /// Создаёт квадрат для 2D спрайтов (удобная обёртка)
    /// </summary>
    public static Mesh CreateSprite(float width = 1f, float height = 1f)
    {
        return Create(width, height);
    }
}