using System.Runtime.InteropServices;

namespace PlatformRender.Graphics;

/// <summary>
/// Структура вершины (позиция + UV + нормаль)
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Vertex(float x, float y, float z, float u = 0, float v = 0, float nx = 0, float ny = 0, float nz = 0)
{
    public float X = x, Y = y, Z = z;
    public float U = u, V = v;
    public float NX = nx, NY = ny, NZ = nz;
    public static readonly int SizeInBytes = Marshal.SizeOf<Vertex>();
}