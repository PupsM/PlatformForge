using PlatformRender.Core;
using System.Numerics;

namespace PlatformRender.Graphics;

/// <summary>
/// Материал (шейдер + параметры + текстуры)
/// </summary>
public sealed class Material(ShaderProgram program) : IDisposable
{
    private readonly ShaderProgram MaterialProgram = program ?? throw new ArgumentNullException(nameof(program));
    private readonly Dictionary<string, object> Uniforms = [];
    private bool Disposed;

    public ShaderProgram Program => MaterialProgram;
    public Texture2D? Texture { get; set; }
    public Color Color { get; set; } = Color.White;
    public float Metallic { get; set; } = 0f;
    public float Roughness { get; set; } = 1f;

    public void SetUniform(string name, float value)
    {
        Uniforms[name] = value;
    }

    public void SetUniform(string name, Vector2 value)
    {
        Uniforms[name] = value;
    }

    public void SetUniform(string name, Vector3 value)
    {
        Uniforms[name] = value;
    }

    public void SetUniform(string name, Vector4 value)
    {
        Uniforms[name] = value;
    }

    public void SetUniform(string name, Matrix4x4 value)
    {
        Uniforms[name] = value;
    }

    public void SetUniform(string name, Texture2D texture)
    {
        Uniforms[name] = texture;
    }

    /// <summary>
    /// Установить uniform цвета (Vector4)
    /// </summary>
    public void SetUniform(string name, Color color)
    {
        Uniforms[name] = new Vector4(color.R, color.G, color.B, color.A);
    }

    public void Apply()
    {
        MaterialProgram.Bind();

        // Передаём все пользовательские uniform'ы
        foreach (var kvp in Uniforms)
        {
            switch (kvp.Value)
            {
                case float f:
                    MaterialProgram.SetUniform(kvp.Key, f);
                    break;
                case Vector2 v2:
                    MaterialProgram.SetUniform(kvp.Key, v2);
                    break;
                case Vector3 v3:
                    MaterialProgram.SetUniform(kvp.Key, v3);
                    break;
                case Vector4 v4:
                    MaterialProgram.SetUniform(kvp.Key, v4);
                    break;
                case Matrix4x4 m:
                    MaterialProgram.SetUniform(kvp.Key, m);
                    break;
                case Texture2D tex:
                    MaterialProgram.SetUniform(kvp.Key, tex, 0);
                    break;
            }
        }

        // Базовые параметры
        MaterialProgram.SetUniform("u_Color", new Vector4(Color.R, Color.G, Color.B, Color.A));
        MaterialProgram.SetUniform("u_Metallic", Metallic);
        MaterialProgram.SetUniform("u_Roughness", Roughness);

        // Текстура материала
        if (Texture is not null)
        {
            MaterialProgram.SetUniform("u_Texture", Texture, 0);
        }
    }

    public void Dispose()
    {
        if (Disposed) return;

        Uniforms.Clear();
        ShaderProgram.Unbind();  // ✅ Используем имя класса

        Disposed = true;
        GC.SuppressFinalize(this);
    }
}