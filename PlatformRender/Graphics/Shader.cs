using PlatformNative.Core;
using PlatformNative.Native;
using PlatformRender.Enums;
using System.Text;

namespace PlatformRender.Graphics;

/// <summary>
/// Шейдер
/// </summary>
public sealed class Shader : IDisposable
{
    private uint ShaderHandle;
    private readonly ShaderType ShaderType;
    private string ShaderSource;
    private bool Compiled;
    private bool Disposed;

    public uint Handle => ShaderHandle;
    public ShaderType Type => ShaderType;
    public string Source => ShaderSource;
    public bool IsCompiled => Compiled;

    public Shader(ShaderType type, string source)
    {
        ShaderType = type;
        ShaderSource = source;
    }

    public bool Compile()
    {
        if (Compiled) return true;

        if (!OpenGL.IsInitialized)
        {
            Diagnostics.Error("OpenGL не инициализирован");
            return false;
        }

        uint glType = ShaderType switch
        {
            ShaderType.Vertex => OpenGL.GL_VERTEX_SHADER,
            ShaderType.Fragment => OpenGL.GL_FRAGMENT_SHADER,
            ShaderType.Geometry => OpenGL.GL_GEOMETRY_SHADER,
            ShaderType.Compute => OpenGL.GL_COMPUTE_SHADER,
            ShaderType.TessellationControl => OpenGL.GL_TESS_CONTROL_SHADER,
            ShaderType.TessellationEvaluation => OpenGL.GL_TESS_EVALUATION_SHADER,
            _ => OpenGL.GL_VERTEX_SHADER
        };

        ShaderHandle = OpenGL.CreateShader(glType);
        if (ShaderHandle == 0)
        {
            Diagnostics.Error("Не удалось создать шейдер");
            return false;
        }

        OpenGL.ShaderSource(ShaderHandle, 1, ref ShaderSource, IntPtr.Zero);
        OpenGL.CompileShader(ShaderHandle);

        // Проверка компиляции
        int compiled = 0;
        OpenGL.GetShaderiv(ShaderHandle, OpenGL.GL_COMPILE_STATUS, ref compiled);

        if (compiled == 0)
        {
            int length = 0;
            OpenGL.GetShaderiv(ShaderHandle, OpenGL.GL_INFO_LOG_LENGTH, ref length);
            if (length > 0)
            {
                var infoLog = new StringBuilder(length);
                OpenGL.GetShaderInfoLog(ShaderHandle, length, out _, infoLog);
                Diagnostics.Error($"Ошибка компиляции {ShaderType} шейдера:\n{infoLog}");
            }
            return false;
        }

        Compiled = true;
        Diagnostics.Debug($"Шейдер {ShaderType} скомпилирован");
        return true;
    }

    public void Dispose()
    {
        if (Disposed) return;

        if (ShaderHandle != 0 && OpenGL.IsInitialized)
        {
            OpenGL.DeleteShader(ShaderHandle);
        }

        Disposed = true;
    }
}