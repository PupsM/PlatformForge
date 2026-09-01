using PlatformRender.Graphics;
using System.Numerics;

namespace PlatformRender.Primitives;

/// <summary>
/// Типы треугольников
/// </summary>
public enum TriangleType
{
    Equilateral,
    Right,
    Isosceles,
    Scalene
}

/// <summary>
/// Генератор треугольной геометрии
/// </summary>
public static class Triangle
{
    /// <summary>
    /// Создаёт треугольник с выбором типа
    /// </summary>
    public static Mesh Create(float size = 1f, TriangleType type = TriangleType.Equilateral, bool centered = true)
    {
        var vertices = type switch
        {
            TriangleType.Equilateral => CreateEquilateral(size, centered),
            TriangleType.Right => CreateRight(size, centered),
            TriangleType.Isosceles => CreateIsosceles(size, centered),
            TriangleType.Scalene => CreateScalene(size, centered),
            _ => CreateEquilateral(size, centered)
        };

        var mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetIndices([0, 1, 2]);
        return mesh;
    }

    private static Vertex[] CreateEquilateral(float size, bool centered)
    {
        float half = size / 2f;
        float height = size * MathF.Sqrt(3f) / 2f;
        float centerOffset = centered ? height / 3f : 0;

        return
        [
            new(-half, -centerOffset, 0, 0, 0, 0, 0, 1),
            new( half, -centerOffset, 0, 1, 0, 0, 0, 1),
            new(    0, height - centerOffset, 0, 0.5f, 1, 0, 0, 1),
        ];
    }

    private static Vertex[] CreateRight(float size, bool centered)
    {
        float half = size / 2f;
        float offset = centered ? -half : 0;

        return
        [
            new(0 + offset, 0 + offset, 0, 0, 0, 0, 0, 1),
            new(size + offset, 0 + offset, 0, 1, 0, 0, 0, 1),
            new(0 + offset, size + offset, 0, 0, 1, 0, 0, 1),
        ];
    }

    private static Vertex[] CreateIsosceles(float size, bool centered)
    {
        float half = size / 2f;
        float offsetY = centered ? -size / 3f : 0;

        return
        [
            new(-half, 0 + offsetY, 0, 0, 0, 0, 0, 1),
            new( half, 0 + offsetY, 0, 1, 0, 0, 0, 1),
            new(0, size + offsetY, 0, 0.5f, 1, 0, 0, 1),
        ];
    }

    private static Vertex[] CreateScalene(float size, bool centered)
    {
        float half = size / 2f;
        float offsetX = centered ? -half / 2f : 0;
        float offsetY = centered ? -half / 2f : 0;

        return
        [
            new(-half + offsetX, -half * 0.5f + offsetY, 0, 0, 0, 0, 0, 1),
            new( half + offsetX, -half * 0.3f + offsetY, 0, 1, 0, 0, 0, 1),
            new(0 + offsetX, half + offsetY, 0, 0.5f, 1, 0, 0, 1),
        ];
    }

    /// <summary>
    /// Создаёт треугольный вентилятор (для дисков)
    /// </summary>
    public static Mesh CreateFan(float radius = 0.5f, int segments = 8)
    {
        segments = Math.Max(3, segments);
        var vertices = new List<Vertex>();
        var indices = new List<uint>();

        // Центр
        vertices.Add(new Vertex(0, 0, 0, 0.5f, 0.5f, 0, 0, 1));

        // Внешние точки
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * 2 * MathF.PI;
            float x = radius * MathF.Cos(angle);
            float y = radius * MathF.Sin(angle);
            float u = 0.5f + 0.5f * MathF.Cos(angle);
            float v = 0.5f + 0.5f * MathF.Sin(angle);

            vertices.Add(new Vertex(x, y, 0, u, v, 0, 0, 1));
        }

        // Индексы
        for (int i = 0; i < segments; i++)
        {
            indices.Add(0);
            indices.Add((uint)(i + 1));
            indices.Add((uint)(i + 2));
        }

        var mesh = new Mesh();
        mesh.SetVertices([.. vertices]);
        mesh.SetIndices([.. indices]);
        return mesh;
    }
}