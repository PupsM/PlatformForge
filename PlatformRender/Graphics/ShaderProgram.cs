using PlatformNative;
using PlatformNative.Core;
using PlatformNative.Native;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace PlatformRender.Graphics;

/// <summary>
/// Шейдерная программа
/// </summary>
public sealed class ShaderProgram : IDisposable
{
    private readonly uint ProgramHandle;
    private readonly bool Linked;
    private readonly Dictionary<string, int> UniformCache = [];
    private readonly Shader? VertexShader;
    private readonly Shader? FragmentShader;
    private bool Disposed;

    public uint Handle => ProgramHandle;
    public bool IsLinked => Linked;

    public ShaderProgram(Shader vertex, Shader fragment)
    {
        VertexShader = vertex;
        FragmentShader = fragment;

        if (!vertex.IsCompiled) vertex.Compile();
        if (!fragment.IsCompiled) fragment.Compile();

        ProgramHandle = OpenGL.CreateProgram();
        if (ProgramHandle == 0)
        {
            Diagnostics.Error("Не удалось создать программу");
            return;
        }

        OpenGL.AttachShader(ProgramHandle, vertex.Handle);
        OpenGL.AttachShader(ProgramHandle, fragment.Handle);
        OpenGL.LinkProgram(ProgramHandle);

        // Проверка линковки
        int linked = 0;
        OpenGL.GetProgramiv(ProgramHandle, OpenGL.GL_LINK_STATUS, ref linked);

        if (linked == 0)
        {
            int length = 0;
            OpenGL.GetProgramiv(ProgramHandle, OpenGL.GL_INFO_LOG_LENGTH, ref length);
            if (length > 0)
            {
                var infoLog = new StringBuilder(length);
                OpenGL.GetProgramInfoLog(ProgramHandle, length, out _, infoLog);
                Diagnostics.Error($"Ошибка линковки программы:\n{infoLog}");
            }
            return;
        }

        Linked = true;
        Diagnostics.Debug("Шейдерная программа слинкована");
    }

    public void Bind()
    {
        if (!Linked) return;
        OpenGL.UseProgram(ProgramHandle);
    }

    public static void Unbind()
    {
        OpenGL.UseProgram(0);
    }

    private int GetUniformLocation(string name)
    {
        if (UniformCache.TryGetValue(name, out var location))
            return location;

        location = OpenGL.GetUniformLocation(ProgramHandle, name);
        UniformCache[name] = location;
        return location;
    }

    public void SetUniform(string name, float value)
    {
        var loc = GetUniformLocation(name);
        if (loc < 0) return;
        OpenGL.Uniform1f(loc, value);
    }

    public void SetUniform(string name, Vector2 value)
    {
        var loc = GetUniformLocation(name);
        if (loc < 0) return;
        OpenGL.Uniform2f(loc, value.X, value.Y);
    }

    public void SetUniform(string name, Vector3 value)
    {
        var loc = GetUniformLocation(name);
        if (loc < 0) return;
        OpenGL.Uniform3f(loc, value.X, value.Y, value.Z);
    }

    public void SetUniform(string name, Vector4 value)
    {
        var loc = GetUniformLocation(name);
        if (loc < 0) return;
        OpenGL.Uniform4f(loc, value.X, value.Y, value.Z, value.W);
    }

    public void SetUniform(string name, Matrix4x4 value)
    {
        var loc = GetUniformLocation(name);
        if (loc < 0) return;

        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<Matrix4x4>());
        try
        {
            Marshal.StructureToPtr(value, ptr, false);
            OpenGL.UniformMatrix4fv(loc, 1, 0, ptr);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    public void SetUniform(string name, Texture2D texture, int slot = 0)
    {
        var loc = GetUniformLocation(name);
        if (loc < 0) return;

        // Активируем текстурный юнит
        const uint GL_TEXTURE0 = 0x84C0;
        OpenGL.ActiveTexture(GL_TEXTURE0 + (uint)slot);
        OpenGL.Uniform1i(loc, slot);
        texture.Bind(slot);
    }

    public void SetUniform(string name, int value)
    {
        var loc = GetUniformLocation(name);
        if (loc < 0) return;
        OpenGL.Uniform1i(loc, value);
    }

    public void Dispose()
    {
        if (Disposed) return;

        if (ProgramHandle != 0)
        {
            OpenGL.DeleteProgram(ProgramHandle);
        }

        Disposed = true;
        GC.SuppressFinalize(this);
    }
}