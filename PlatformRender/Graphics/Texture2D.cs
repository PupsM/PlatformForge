using PlatformNative.Native;
using PlatformRender.Enums;
using System.Runtime.InteropServices;

namespace PlatformRender.Graphics;

/// <summary>
/// 2D текстура
/// </summary>
public sealed class Texture2D : IDisposable
{
    private uint Handle;
    private readonly int TextureWidth;
    private readonly int TextureHeight;
    private readonly PixelFormat TextureFormat;
    private bool Disposed;

    public int Width => TextureWidth;
    public int Height => TextureHeight;
    public PixelFormat Format => TextureFormat;

    public Texture2D(int width, int height, PixelFormat format = PixelFormat.R8G8B8A8)
    {
        if (!OpenGL.IsInitialized)
            throw new InvalidOperationException("OpenGL не инициализирован");

        TextureWidth = width;
        TextureHeight = height;
        TextureFormat = format;

        uint texture = 0;
        OpenGL.GenTextures(1, ref texture);
        Handle = texture;

        Bind(0);

        OpenGL.TexParameteri(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, (int)OpenGL.GL_LINEAR);
        OpenGL.TexParameteri(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, (int)OpenGL.GL_LINEAR);
        OpenGL.TexParameteri(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, (int)OpenGL.GL_REPEAT);
        OpenGL.TexParameteri(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, (int)OpenGL.GL_REPEAT);

        Unbind();
    }

    public void SetData(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        Bind(0);

        uint glFormat;
        uint glInternalFormat;
        uint glType;

        switch (TextureFormat)
        {
            case PixelFormat.R8:
                glFormat = OpenGL.GL_RED;
                glInternalFormat = OpenGL.GL_R8;
                glType = OpenGL.GL_UNSIGNED_BYTE;
                break;
            case PixelFormat.R8G8B8:
                glFormat = OpenGL.GL_RGB;
                glInternalFormat = OpenGL.GL_RGB8;
                glType = OpenGL.GL_UNSIGNED_BYTE;
                break;
            case PixelFormat.R8G8B8A8:
                glFormat = OpenGL.GL_RGBA;
                glInternalFormat = OpenGL.GL_RGBA8;
                glType = OpenGL.GL_UNSIGNED_BYTE;
                break;
            case PixelFormat.R16:
                glFormat = OpenGL.GL_RED;
                glInternalFormat = OpenGL.GL_R16;
                glType = OpenGL.GL_UNSIGNED_SHORT;
                break;
            case PixelFormat.R16G16B16A16:
                glFormat = OpenGL.GL_RGBA;
                glInternalFormat = OpenGL.GL_RGBA16;
                glType = OpenGL.GL_UNSIGNED_SHORT;
                break;
            default:
                glFormat = OpenGL.GL_RGBA;
                glInternalFormat = OpenGL.GL_RGBA8;
                glType = OpenGL.GL_UNSIGNED_BYTE;
                break;
        }

        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        try
        {
            IntPtr ptr = handle.AddrOfPinnedObject();
            OpenGL.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, (int)glInternalFormat, TextureWidth, TextureHeight, 0, glFormat, glType, ptr);
        }
        finally
        {
            handle.Free();
        }

        OpenGL.GenerateMipmap(OpenGL.GL_TEXTURE_2D);
        Unbind();
    }

    public void Bind(int slot = 0)
    {
        OpenGL.ActiveTexture((uint)(OpenGL.GL_TEXTURE0 + slot));
        OpenGL.BindTexture(OpenGL.GL_TEXTURE_2D, Handle);
    }

    public static void Unbind()
    {
        OpenGL.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
    }

    public void GenerateMipmaps()
    {
        Bind(0);
        OpenGL.GenerateMipmap(OpenGL.GL_TEXTURE_2D);
        Unbind();
    }

    public void Dispose()
    {
        if (Disposed) return;

        if (Handle != 0 && OpenGL.IsInitialized)
        {
            OpenGL.DeleteTextures(1, ref Handle);
        }

        Disposed = true;
        GC.SuppressFinalize(this);
    }
}