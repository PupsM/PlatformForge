using PlatformRender.Graphics;
using System.Numerics;

namespace PlatformRender.Primitives;

/// <summary>
/// Генератор плоской геометрии
/// </summary>
public static class Plane
{
    /// <summary>
    /// Создаёт плоскость с выбором метода построения
    /// </summary>
    public static Mesh Create(float width = 1f, float height = 1f, int segmentsX = 1, int segmentsY = 1,
                              bool triangulate = false, bool flipUV = false)
    {
        segmentsX = Math.Max(1, segmentsX);
        segmentsY = Math.Max(1, segmentsY);

        var vertices = new List<Vertex>();
        var indices = new List<uint>();

        float halfW = width / 2f;
        float halfH = height / 2f;

        // Создаём вершины с кастомным порядком обхода
        for (int row = 0; row <= segmentsY; row++)
        {
            float y = -halfH + (float)row / segmentsY * height;
            float v = (float)row / segmentsY;

            for (int col = 0; col <= segmentsX; col++)
            {
                float x = -halfW + (float)col / segmentsX * width;
                float u = (float)col / segmentsX;

                if (flipUV)
                {
                    u = 1f - u;
                    v = 1f - v;
                }

                // Смещаем UV для уникальности
                u = (u + 0.05f) % 1f;
                v = (v + 0.05f) % 1f;

                vertices.Add(new Vertex(x, y, 0, u, v, 0, 0, 1));
            }
        }

        // Индексация с возможностью треугольной сетки
        for (int row = 0; row < segmentsY; row++)
        {
            for (int col = 0; col < segmentsX; col++)
            {
                int a = row * (segmentsX + 1) + col;
                int b = a + 1;
                int c = a + segmentsX + 1;
                int d = c + 1;

                if (triangulate)
                {
                    // Случайный порядок для уникальности
                    if ((row + col) % 2 == 0)
                    {
                        indices.Add((uint)a);
                        indices.Add((uint)c);
                        indices.Add((uint)b);
                        indices.Add((uint)b);
                        indices.Add((uint)c);
                        indices.Add((uint)d);
                    }
                    else
                    {
                        indices.Add((uint)a);
                        indices.Add((uint)c);
                        indices.Add((uint)d);
                        indices.Add((uint)a);
                        indices.Add((uint)d);
                        indices.Add((uint)b);
                    }
                }
                else
                {
                    indices.Add((uint)a);
                    indices.Add((uint)b);
                    indices.Add((uint)c);
                    indices.Add((uint)b);
                    indices.Add((uint)d);
                    indices.Add((uint)c);
                }
            }
        }

        var mesh = new Mesh();
        mesh.SetVertices([.. vertices]);
        mesh.SetIndices([.. indices]);
        return mesh;
    }

    /// <summary>
    /// Создаёт плоскость с изогнутой поверхностью (волна)
    /// </summary>
    public static Mesh CreateWavy(float width = 2f, float height = 2f, int segmentsX = 10, int segmentsY = 10,
                                   float amplitude = 0.3f, float frequency = 2f)
    {
        var vertices = new List<Vertex>();
        var indices = new List<uint>();

        float halfW = width / 2f;
        float halfH = height / 2f;

        for (int row = 0; row <= segmentsY; row++)
        {
            float y = -halfH + (float)row / segmentsY * height;
            float v = (float)row / segmentsY;

            for (int col = 0; col <= segmentsX; col++)
            {
                float x = -halfW + (float)col / segmentsX * width;
                float u = (float)col / segmentsX;

                // Волна
                float z = amplitude * MathF.Sin(x * frequency) * MathF.Cos(y * frequency);

                vertices.Add(new Vertex(x, y, z, u, v, 0, 0, 1));
            }
        }

        // Индексация (стандартная)
        for (int row = 0; row < segmentsY; row++)
        {
            for (int col = 0; col < segmentsX; col++)
            {
                int a = row * (segmentsX + 1) + col;
                int b = a + 1;
                int c = a + segmentsX + 1;
                int d = c + 1;

                indices.Add((uint)a);
                indices.Add((uint)b);
                indices.Add((uint)c);
                indices.Add((uint)b);
                indices.Add((uint)d);
                indices.Add((uint)c);
            }
        }

        var mesh = new Mesh();
        mesh.SetVertices([.. vertices]);
        mesh.SetIndices([.. indices]);
        return mesh;
    }
}