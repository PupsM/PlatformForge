using PlatformAudio.Enums;
using PlatformAudio.Interfaces;
using PlatformEngine.Core;
using PlatformImage.Core;
using PlatformImage.IO;
using PlatformRender.Enums;
using PlatformRender.Graphics;
using PixelFormat = PlatformRender.Enums.PixelFormat;

namespace PlatformEngine.Resources;

/// <summary>
/// Управление ресурсами с автоматическим кешированием
/// </summary>
public sealed class ResourceManager(EngineInstance engine) : IDisposable
{
    private readonly EngineInstance Engine = engine ?? throw new ArgumentNullException(nameof(engine));
    private readonly Dictionary<string, Texture2D> TextureCache = [];
    private readonly Dictionary<string, ISoundBuffer> SoundCache = [];
    private readonly Dictionary<string, ShaderProgram> ShaderCache = [];
    private readonly Dictionary<string, Mesh> MeshCache = [];
    private readonly Lock Lock = new();
    private bool Disposed;

    /// <summary>
    /// Загрузить текстуру из файла (с кешированием)
    /// </summary>
    public Texture2D LoadTexture(string path)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be empty", nameof(path));

        lock (Lock)
        {
            if (TextureCache.TryGetValue(path, out var cached))
                return cached;

            if (Engine.Renderer is null)
                throw new InvalidOperationException("Renderer not initialized");

            using var image = ImageLoader.Load(path);
            var texture = Engine.Renderer.CreateTexture2D(
                image.Width,
                image.Height,
                PixelFormat.R8G8B8A8
            );
            texture.SetData(image.Data.ToArray());

            TextureCache[path] = texture;
            return texture;
        }
    }

    /// <summary>
    /// Загрузить звук из файла (с кешированием)
    /// </summary>
    public ISoundBuffer LoadSound(string path)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be empty", nameof(path));

        lock (Lock)
        {
            if (SoundCache.TryGetValue(path, out var cached))
                return cached;

            if (Engine.Audio is null)
                throw new InvalidOperationException("Audio not initialized");

            var data = File.ReadAllBytes(path);
            var buffer = Engine.Audio.CreateBuffer();

            // Автоопределение формата по расширению
            var ext = Path.GetExtension(path).ToLowerInvariant();
            var format = ext switch
            {
                ".wav" => AudioFormat.Stereo16,
                ".ogg" => AudioFormat.Stereo16,
                ".mp3" => AudioFormat.Stereo16,
                _ => AudioFormat.Stereo16
            };

            buffer.SetData(data, format, 44100);
            SoundCache[path] = buffer;
            return buffer;
        }
    }

    /// <summary>
    /// Загрузить шейдерную программу (с кешированием)
    /// </summary>
    public ShaderProgram LoadShader(string vertexPath, string fragmentPath)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (string.IsNullOrEmpty(vertexPath) || string.IsNullOrEmpty(fragmentPath))
            throw new ArgumentException("Paths cannot be empty");

        string key = $"{vertexPath}|{fragmentPath}";

        lock (Lock)
        {
            if (ShaderCache.TryGetValue(key, out var cached))
                return cached;

            if (Engine.Renderer is null)
                throw new InvalidOperationException("Renderer not initialized");

            var vertexSource = File.ReadAllText(vertexPath);
            var fragmentSource = File.ReadAllText(fragmentPath);

            var vertexShader = Engine.Renderer.CreateShader(ShaderType.Vertex, vertexSource);
            var fragmentShader = Engine.Renderer.CreateShader(ShaderType.Fragment, fragmentSource);
            var program = Engine.Renderer.CreateProgram(vertexShader, fragmentShader);

            ShaderCache[key] = program;
            return program;
        }
    }

    /// <summary>
    /// Загрузить меш из файла (с кешированием)
    /// </summary>
    public Mesh LoadMesh(string path)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be empty", nameof(path));

        lock (Lock)
        {
            if (MeshCache.TryGetValue(path, out var cached))
                return cached;

            // TODO: Реализовать загрузку OBJ/FBX/glTF
            // Пока возвращаем куб как заглушку
            var mesh = Engine.CreateCube(1f);
            MeshCache[path] = mesh;
            return mesh;
        }
    }

    /// <summary>
    /// Очистить кеш ресурсов
    /// </summary>
    public void ClearCache()
    {
        lock (Lock)
        {
            foreach (var tex in TextureCache.Values) tex.Dispose();
            foreach (var sound in SoundCache.Values) sound.Dispose();
            foreach (var shader in ShaderCache.Values) shader.Dispose();
            foreach (var mesh in MeshCache.Values) mesh.Dispose();

            TextureCache.Clear();
            SoundCache.Clear();
            ShaderCache.Clear();
            MeshCache.Clear();
        }
    }

    public void Dispose()
    {
        if (Disposed) return;
        ClearCache();
        Disposed = true;
        GC.SuppressFinalize(this);
    }
}