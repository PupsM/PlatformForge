using PlatformRender.Graphics;
using System.Numerics;

namespace PlatformRender.Primitives;

/// <summary>
/// Генератор кубической геометрии
/// </summary>
public static class Cube
{
    /// <summary>
    /// Создаёт куб с возможностью настройки
    /// </summary>
    public static Mesh Create(float size = 1f, bool generateUV = true, bool generateNormals = true, bool invertNormals = false)
    {
        float h = size * 0.5f;
        var vertices = new List<Vertex>();
        var indices = new List<uint>();

        // 6 граней: каждая грань — это 4 вершины и 2 треугольника
        var faces = new (Vector3 normal, Vector3[] corners, (float u, float v)[] uvs)[]
        {
            // Перед (Z+)
            (new Vector3(0, 0, 1),
             new Vector3[] { new(-1, -1, 1), new(1, -1, 1), new(1, 1, 1), new(-1, 1, 1) },
             new (float u, float v)[] { (0,0), (1,0), (1,1), (0,1) }),
             
            // Зад (Z-)
            (new Vector3(0, 0, -1),
             new Vector3[] { new(1, -1, -1), new(-1, -1, -1), new(-1, 1, -1), new(1, 1, -1) },
             new (float u, float v)[] { (1,0), (0,0), (0,1), (1,1) }),
             
            // Право (X+)
            (new Vector3(1, 0, 0),
             new Vector3[] { new(1, -1, 1), new(1, -1, -1), new(1, 1, -1), new(1, 1, 1) },
             new (float u, float v)[] { (0,0), (1,0), (1,1), (0,1) }),
             
            // Лево (X-)
            (new Vector3(-1, 0, 0),
             new Vector3[] { new(-1, -1, -1), new(-1, -1, 1), new(-1, 1, 1), new(-1, 1, -1) },
             new (float u, float v)[] { (1,0), (0,0), (0,1), (1,1) }),
             
            // Верх (Y+)
            (new Vector3(0, 1, 0),
             new Vector3[] { new(-1, 1, 1), new(1, 1, 1), new(1, 1, -1), new(-1, 1, -1) },
             new (float u, float v)[] { (0,0), (1,0), (1,1), (0,1) }),
             
            // Низ (Y-)
            (new Vector3(0, -1, 0),
             new Vector3[] { new(-1, -1, -1), new(1, -1, -1), new(1, -1, 1), new(-1, -1, 1) },
             new (float u, float v)[] { (1,0), (0,0), (0,1), (1,1) })
        };

        uint vertexOffset = 0;

        foreach (var face in faces)
        {
            Vector3 normal = invertNormals ? -face.normal : face.normal;

            // Добавляем 4 вершины грани
            for (int i = 0; i < 4; i++)
            {
                Vector3 pos = face.corners[i] * h;
                float u = generateUV ? face.uvs[i].u : 0;
                float v = generateUV ? face.uvs[i].v : 0;
                float nx = generateNormals ? normal.X : 0;
                float ny = generateNormals ? normal.Y : 0;
                float nz = generateNormals ? normal.Z : 0;

                vertices.Add(new Vertex(pos.X, pos.Y, pos.Z, u, v, nx, ny, nz));
            }

            // Индексы для двух треугольников (0-1-2 и 0-2-3)
            indices.Add(vertexOffset + 0);
            indices.Add(vertexOffset + 1);
            indices.Add(vertexOffset + 2);

            indices.Add(vertexOffset + 0);
            indices.Add(vertexOffset + 2);
            indices.Add(vertexOffset + 3);

            vertexOffset += 4;
        }

        var mesh = new Mesh();
        mesh.SetVertices([.. vertices]);
        mesh.SetIndices([.. indices]);
        return mesh;
    }
}