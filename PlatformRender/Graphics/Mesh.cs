using PlatformNative.Core;
using PlatformNative.Native;
using PlatformRender.Enums;
using System.Runtime.InteropServices;

namespace PlatformRender.Graphics;

/// <summary>
/// Меш (геометрия)
/// </summary>
public sealed class Mesh : IDisposable
{
    private uint VAO;
    private uint VBO;
    private uint EBO;
    private int VrtexCount;
    private int MeshIndexCount;
    private bool MeshHasIndices;
    private bool MeshHasVertices;
    private bool Disposed;
    private bool GlObjectsCreated;

    public ShaderProgram? Program { get; set; }
    public Texture2D? Texture { get; set; }
    public int VertexCount => VrtexCount;
    public int IndexCount => MeshIndexCount;
    public bool HasVertices => MeshHasVertices;
    public bool HasIndices => MeshHasIndices;

    public Mesh()
    {
        if (!OpenGL.IsInitialized)
        {
            Diagnostics.Warning("OpenGL не инициализирован, Mesh создан без GL объектов");
            return;
        }
        CreateGLObjects();
    }

    private void CreateGLObjects()
    {
        if (GlObjectsCreated) return;

        // Создаём VAO
        if (OpenGL.TryGetFunction<OpenGL.glGenVertexArraysDelegate>("glGenVertexArrays", out var genVAO) && genVAO is not null)
        {
            uint vao = 0;
            genVAO(1, ref vao);
            VAO = vao;
        }

        // Создаём VBO и EBO
        if (OpenGL.TryGetFunction<OpenGL.glGenBuffersDelegate>("glGenBuffers", out var genBuffer) && genBuffer is not null)
        {
            uint vbo = 0;
            uint ebo = 0;
            genBuffer(1, ref vbo);
            genBuffer(1, ref ebo);
            VBO = vbo;
            EBO = ebo;
        }

        GlObjectsCreated = true;
        BindVAO();
        UnbindVAO();
    }

    private void EnsureGlObjects()
    {
        if (!GlObjectsCreated && OpenGL.IsInitialized)
        {
            CreateGLObjects();
        }
    }

    private void BindVAO()
    {
        EnsureGlObjects();
        if (OpenGL.TryGetFunction<OpenGL.glBindVertexArrayDelegate>("glBindVertexArray", out var bindVAO) && bindVAO is not null)
        {
            bindVAO(VAO);
        }
    }

    private static void UnbindVAO()
    {
        if (OpenGL.TryGetFunction<OpenGL.glBindVertexArrayDelegate>("glBindVertexArray", out var bindVAO) && bindVAO is not null)
        {
            bindVAO(0);
        }
    }

    public void SetVertices(Vertex[] vertices)
    {
        SetVertices(vertices, vertices.Length);
    }

    public void SetVertices(Vertex[] vertices, int count)
    {
        if (!OpenGL.IsInitialized)
        {
            Diagnostics.Warning("OpenGL не инициализирован, вершины не будут загружены");
            return;
        }

        EnsureGlObjects();
        VrtexCount = count;
        MeshHasVertices = true;
        var size = count * Vertex.SizeInBytes;

        BindVAO();

        if (OpenGL.TryGetFunction<OpenGL.glBindBufferDelegate>("glBindBuffer", out var bindBuffer) && bindBuffer is not null)
        {
            bindBuffer(OpenGL.GL_ARRAY_BUFFER, VBO);
        }

        if (OpenGL.TryGetFunction<OpenGL.glBufferDataDelegate>("glBufferData", out var bufferData) && bufferData is not null)
        {
            var handle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
            try
            {
                bufferData(OpenGL.GL_ARRAY_BUFFER, size, handle.AddrOfPinnedObject(), OpenGL.GL_STATIC_DRAW);
            }
            finally
            {
                handle.Free();
            }
        }

        SetupVertexAttributes();
        UnbindVAO();
    }

    private static void SetupVertexAttributes()
    {
        // Позиция (location = 0)
        if (OpenGL.TryGetFunction<OpenGL.glVertexAttribPointerDelegate>("glVertexAttribPointer", out var attribPointer) && attribPointer is not null)
        {
            attribPointer(0, 3, OpenGL.GL_FLOAT, 0, Vertex.SizeInBytes, IntPtr.Zero);
        }
        if (OpenGL.TryGetFunction<OpenGL.glEnableVertexAttribArrayDelegate>("glEnableVertexAttribArray", out var enableAttrib) && enableAttrib is not null)
        {
            enableAttrib(0);
        }

        // UV (location = 1)
        if (OpenGL.TryGetFunction<OpenGL.glVertexAttribPointerDelegate>("glVertexAttribPointer", out var attribPointer2) && attribPointer2 is not null)
        {
            attribPointer2(1, 2, OpenGL.GL_FLOAT, 0, Vertex.SizeInBytes, (IntPtr)(3 * sizeof(float)));
        }
        if (OpenGL.TryGetFunction<OpenGL.glEnableVertexAttribArrayDelegate>("glEnableVertexAttribArray", out var enableAttrib2) && enableAttrib2 is not null)
        {
            enableAttrib2(1);
        }

        // Нормаль (location = 2)
        if (OpenGL.TryGetFunction<OpenGL.glVertexAttribPointerDelegate>("glVertexAttribPointer", out var attribPointer3) && attribPointer3 is not null)
        {
            attribPointer3(2, 3, OpenGL.GL_FLOAT, 0, Vertex.SizeInBytes, (IntPtr)(5 * sizeof(float)));
        }
        if (OpenGL.TryGetFunction<OpenGL.glEnableVertexAttribArrayDelegate>("glEnableVertexAttribArray", out var enableAttrib3) && enableAttrib3 is not null)
        {
            enableAttrib3(2);
        }
    }

    public void SetIndices(uint[] indices)
    {
        SetIndices(indices, indices.Length);
    }

    public void SetIndices(uint[] indices, int count)
    {
        if (!OpenGL.IsInitialized)
        {
            Diagnostics.Warning("OpenGL не инициализирован, индексы не будут загружены");
            return;
        }

        EnsureGlObjects();
        MeshIndexCount = count;
        MeshHasIndices = true;
        var size = count * sizeof(uint);

        BindVAO();

        if (OpenGL.TryGetFunction<OpenGL.glBindBufferDelegate>("glBindBuffer", out var bindBuffer) && bindBuffer is not null)
        {
            bindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, EBO);
        }

        if (OpenGL.TryGetFunction<OpenGL.glBufferDataDelegate>("glBufferData", out var bufferData) && bufferData is not null)
        {
            var handle = GCHandle.Alloc(indices, GCHandleType.Pinned);
            try
            {
                bufferData(OpenGL.GL_ELEMENT_ARRAY_BUFFER, size, handle.AddrOfPinnedObject(), OpenGL.GL_STATIC_DRAW);
            }
            finally
            {
                handle.Free();
            }
        }

        UnbindVAO();
    }

    public void Draw(PrimitiveType type = PrimitiveType.Triangles)
    {
        if (!MeshHasVertices) return;
        if (!OpenGL.IsInitialized) return;

        BindVAO();

        uint glMode = type switch
        {
            PrimitiveType.Points => OpenGL.GL_POINTS,
            PrimitiveType.Lines => OpenGL.GL_LINES,
            PrimitiveType.LineStrip => OpenGL.GL_LINE_STRIP,
            PrimitiveType.LineLoop => OpenGL.GL_LINE_LOOP,
            PrimitiveType.Triangles => OpenGL.GL_TRIANGLES,
            PrimitiveType.TriangleStrip => OpenGL.GL_TRIANGLE_STRIP,
            PrimitiveType.TriangleFan => OpenGL.GL_TRIANGLE_FAN,
            _ => OpenGL.GL_TRIANGLES
        };

        if (MeshHasIndices)
        {
            if (OpenGL.TryGetFunction<OpenGL.glDrawElementsDelegate>("glDrawElements", out var drawElements) && drawElements is not null)
            {
                drawElements(glMode, MeshIndexCount, OpenGL.GL_UNSIGNED_INT, IntPtr.Zero);
            }
        }
        else
        {
            if (OpenGL.TryGetFunction<OpenGL.glDrawArraysDelegate>("glDrawArrays", out var drawArrays) && drawArrays is not null)
            {
                drawArrays(glMode, 0, VrtexCount);
            }
        }

        UnbindVAO();
    }

    public void Dispose()
    {
        if (Disposed) return;

        if (OpenGL.IsInitialized && GlObjectsCreated)
        {
            if (OpenGL.TryGetFunction<OpenGL.glDeleteVertexArraysDelegate>("glDeleteVertexArrays", out var deleteVAO) && deleteVAO is not null)
            {
                deleteVAO(1, ref VAO);
            }

            if (OpenGL.TryGetFunction<OpenGL.glDeleteBuffersDelegate>("glDeleteBuffers", out var deleteBuffer) && deleteBuffer is not null)
            {
                deleteBuffer(1, ref VBO);
                if (MeshHasIndices)
                    deleteBuffer(1, ref EBO);
            }
        }

        Disposed = true;
        GC.SuppressFinalize(this);
    }
}