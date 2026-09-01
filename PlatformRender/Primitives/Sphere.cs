using PlatformRender.Graphics;
using System.Numerics;

namespace PlatformRender.Primitives;

/// <summary>
/// Генератор сферической геометрии
/// </summary>
public static class Sphere
{
    /// <summary>
    /// Создаёт сферу с использованием икосаэдра и subdivision
    /// </summary>
    public static Mesh Create(float radius = 0.5f, int subdivisions = 3)
    {
        if (subdivisions < 0) subdivisions = 0;
        if (subdivisions > 6) subdivisions = 6;

        var (vertices, indices) = CreateIcosahedron(radius);

        for (int i = 0; i < subdivisions; i++)
        {
            (vertices, indices) = Subdivide(vertices, indices, radius);
        }

        var mesh = new Mesh();
        mesh.SetVertices([.. vertices]);
        mesh.SetIndices([.. indices]);
        return mesh;
    }

    /// <summary>
    /// Создаёт сферу по стандартному методу (широта/долгота)
    /// </summary>
    public static Mesh CreateLatLong(float radius = 0.5f, int segments = 32, bool flipUV = false)
    {
        segments = Math.Max(3, segments);
        var vertices = new List<Vertex>();
        var indices = new List<uint>();

        for (int lat = 0; lat <= segments; lat++)
        {
            float theta = (float)lat / segments * MathF.PI;
            float sinTheta = MathF.Sin(theta);
            float cosTheta = MathF.Cos(theta);

            for (int lon = 0; lon <= segments; lon++)
            {
                float phi = (float)lon / segments * 2 * MathF.PI;
                float sinPhi = MathF.Sin(phi);
                float cosPhi = MathF.Cos(phi);

                float x = radius * sinTheta * cosPhi;
                float y = radius * cosTheta;
                float z = radius * sinTheta * sinPhi;

                float u = flipUV ? 1f - (float)lon / segments : (float)lon / segments;
                float v = flipUV ? 1f - (float)lat / segments : (float)lat / segments;

                vertices.Add(new Vertex(x, y, z, u, v, x / radius, y / radius, z / radius));
            }
        }

        for (int lat = 0; lat < segments; lat++)
        {
            for (int lon = 0; lon < segments; lon++)
            {
                int a = lat * (segments + 1) + lon;
                int b = a + segments + 1;
                int c = a + 1;
                int d = b + 1;

                indices.Add((uint)a);
                indices.Add((uint)b);
                indices.Add((uint)c);
                indices.Add((uint)c);
                indices.Add((uint)b);
                indices.Add((uint)d);
            }
        }

        var mesh = new Mesh();
        mesh.SetVertices([.. vertices]);
        mesh.SetIndices([.. indices]);
        return mesh;
    }

    // ---- Приватные методы для икосаэдра ----

    private static (Vertex[] vertices, uint[] indices) CreateIcosahedron(float radius)
    {
        float phi = (1.0f + MathF.Sqrt(5.0f)) * 0.5f;
        float norm = MathF.Sqrt(1.0f + phi * phi);
        float invNorm = 1.0f / norm;

        var baseVerts = new Vector3[]
        {
            new Vector3(-1,  phi, 0) * invNorm,
            new Vector3( 1,  phi, 0) * invNorm,
            new Vector3(-1, -phi, 0) * invNorm,
            new Vector3( 1, -phi, 0) * invNorm,
            new Vector3(0, -1,  phi) * invNorm,
            new Vector3(0,  1,  phi) * invNorm,
            new Vector3(0, -1, -phi) * invNorm,
            new Vector3(0,  1, -phi) * invNorm,
            new Vector3( phi, 0, -1) * invNorm,
            new Vector3( phi, 0,  1) * invNorm,
            new Vector3(-phi, 0, -1) * invNorm,
            new Vector3(-phi, 0,  1) * invNorm
        };

        for (int i = 0; i < baseVerts.Length; i++)
            baseVerts[i] *= radius;

        var vertices = new List<Vertex>();
        foreach (var v in baseVerts)
        {
            var n = Vector3.Normalize(v);
            vertices.Add(new Vertex(v.X, v.Y, v.Z, 0, 0, n.X, n.Y, n.Z));
        }

        var baseIndices = new uint[]
        {
            0,11,5, 0,5,1, 0,1,7, 0,7,10, 0,10,11,
            1,5,9, 5,11,4, 11,10,2, 10,7,6, 7,1,8,
            3,9,4, 3,4,2, 3,2,6, 3,6,8, 3,8,9,
            4,9,5, 2,4,11, 6,2,10, 8,6,7, 9,8,1
        };

        return (vertices.ToArray(), baseIndices);
    }

    private static (Vertex[] vertices, uint[] indices) Subdivide(Vertex[] vertices, uint[] indices, float radius)
    {
        var newVerts = new List<Vertex>(vertices);
        var newIndices = new List<uint>();
        var cache = new Dictionary<(uint, uint), uint>();

        for (int i = 0; i < indices.Length; i += 3)
        {
            uint i0 = indices[i];
            uint i1 = indices[i + 1];
            uint i2 = indices[i + 2];

            uint i01 = GetMidpoint(i0, i1, newVerts, cache, radius);
            uint i12 = GetMidpoint(i1, i2, newVerts, cache, radius);
            uint i20 = GetMidpoint(i2, i0, newVerts, cache, radius);

            newIndices.Add(i0);
            newIndices.Add(i01);
            newIndices.Add(i20);

            newIndices.Add(i1);
            newIndices.Add(i12);
            newIndices.Add(i01);

            newIndices.Add(i2);
            newIndices.Add(i20);
            newIndices.Add(i12);

            newIndices.Add(i01);
            newIndices.Add(i12);
            newIndices.Add(i20);
        }

        return (newVerts.ToArray(), newIndices.ToArray());
    }

    private static uint GetMidpoint(uint i0, uint i1, List<Vertex> vertices,
                                     Dictionary<(uint, uint), uint> cache, float radius)
    {
        var key = i0 < i1 ? (i0, i1) : (i1, i0);
        if (cache.TryGetValue(key, out var cached))
            return cached;

        var v0 = new Vector3(vertices[(int)i0].X, vertices[(int)i0].Y, vertices[(int)i0].Z);
        var v1 = new Vector3(vertices[(int)i1].X, vertices[(int)i1].Y, vertices[(int)i1].Z);

        var mid = (v0 + v1) / 2f;
        var normalized = Vector3.Normalize(mid) * radius;

        vertices.Add(new Vertex(normalized.X, normalized.Y, normalized.Z, 0, 0,
                                normalized.X / radius, normalized.Y / radius, normalized.Z / radius));
        uint index = (uint)(vertices.Count - 1);
        cache[key] = index;
        return index;
    }
}