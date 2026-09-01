using PlatformRender.Graphics;
using System.Numerics;

namespace PlatformRender.Utils;

/// <summary>
/// Построитель мешей (удобный способ создания геометрии)
/// </summary>
public class MeshBuilder
{
    private readonly List<Vertex> Vertices = [];
    private readonly List<uint> Indices = [];
    private uint CurrentIndex = 0;

    public MeshBuilder Clear()
    {
        Vertices.Clear();
        Indices.Clear();
        CurrentIndex = 0;
        return this;
    }

    public MeshBuilder AddVertex(float x, float y, float z, float u = 0, float v = 0, float nx = 0, float ny = 0, float nz = 0)
    {
        Vertices.Add(new Vertex(x, y, z, u, v, nx, ny, nz));
        CurrentIndex++;
        return this;
    }

    public MeshBuilder AddVertex(Vertex vertex)
    {
        Vertices.Add(vertex);
        CurrentIndex++;
        return this;
    }

    public MeshBuilder AddTriangle(uint i0, uint i1, uint i2)
    {
        Indices.Add(i0);
        Indices.Add(i1);
        Indices.Add(i2);
        return this;
    }

    public MeshBuilder AddQuad(uint i0, uint i1, uint i2, uint i3)
    {
        Indices.Add(i0);
        Indices.Add(i1);
        Indices.Add(i2);
        Indices.Add(i0);
        Indices.Add(i2);
        Indices.Add(i3);
        return this;
    }

    public Mesh Build()
    {
        var mesh = new Mesh();
        mesh.SetVertices([.. Vertices]);
        if (Indices.Count > 0)
        {
            mesh.SetIndices([.. Indices]);
        }
        return mesh;
    }

    /// <summary>
    /// Вычисляет нормали для всех вершин на основе индексов
    /// </summary>
    public MeshBuilder WithNormals()
    {
        if (Indices.Count == 0 || Vertices.Count == 0)
            return this;

        // Инициализируем массив нормалей
        var normals = new Vector3[Vertices.Count];

        // Для каждого треугольника вычисляем нормаль и добавляем к вершинам
        for (int i = 0; i < Indices.Count; i += 3)
        {
            uint i0 = Indices[i];
            uint i1 = Indices[i + 1];
            uint i2 = Indices[i + 2];

            // Проверяем, что индексы валидны
            if (i0 >= Vertices.Count || i1 >= Vertices.Count || i2 >= Vertices.Count)
                continue;

            var v0 = new Vector3(Vertices[(int)i0].X, Vertices[(int)i0].Y, Vertices[(int)i0].Z);
            var v1 = new Vector3(Vertices[(int)i1].X, Vertices[(int)i1].Y, Vertices[(int)i1].Z);
            var v2 = new Vector3(Vertices[(int)i2].X, Vertices[(int)i2].Y, Vertices[(int)i2].Z);

            var edge1 = v1 - v0;
            var edge2 = v2 - v0;
            var normal = Vector3.Normalize(Vector3.Cross(edge1, edge2));

            // Добавляем нормаль к каждой вершине треугольника
            normals[i0] += normal;
            normals[i1] += normal;
            normals[i2] += normal;
        }

        // Нормализуем все нормали
        for (int i = 0; i < normals.Length; i++)
        {
            if (normals[i] != Vector3.Zero)
                normals[i] = Vector3.Normalize(normals[i]);
            else
                normals[i] = Vector3.UnitZ; // fallback
        }

        // Обновляем вершины с новыми нормалями
        for (int i = 0; i < Vertices.Count; i++)
        {
            var v = Vertices[i];
            Vertices[i] = new Vertex(
                v.X, v.Y, v.Z,
                v.U, v.V,
                normals[i].X, normals[i].Y, normals[i].Z
            );
        }

        return this;
    }
}