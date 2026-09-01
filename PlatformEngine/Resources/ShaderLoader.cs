using PlatformRender;
using PlatformRender.Enums;
using PlatformRender.Graphics;

namespace PlatformEngine.Resources;

/// <summary>
/// Загрузчик шейдеров
/// </summary>
public static class ShaderLoader
{
    public static ShaderProgram Load(IRenderer renderer, string vertexPath, string fragmentPath)
    {
        ArgumentNullException.ThrowIfNull(renderer);

        var vertexSource = File.ReadAllText(vertexPath);
        var fragmentSource = File.ReadAllText(fragmentPath);

        var vertexShader = renderer.CreateShader(ShaderType.Vertex, vertexSource);
        var fragmentShader = renderer.CreateShader(ShaderType.Fragment, fragmentSource);
        var program = renderer.CreateProgram(vertexShader, fragmentShader);

        return program;
    }
}