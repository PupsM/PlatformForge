using PlatformNative.Core;
using PlatformNative.Core.Library;
using System.Runtime.InteropServices;
using System.Text;

namespace PlatformNative.Native;

/// <summary>
/// Нативная обёртка для базовых функций OpenGL
/// </summary>
public static class OpenGL
{
    #region ---- Хост ----

    private sealed class OpenGLHost : Host<OpenGLHost, OpenGLLibrary>
    {
        protected override string LibraryKey => "OpenGL";
        protected override Func<string, IntPtr> Resolver => OpenGLLibrary.ResolveOpenGLStatic;
        protected override Func<bool> Loader => static () => true;

        protected override bool InitializeLibrary()
        {
            if (GLFW.IsInitialized)
            {
                if (TryGetFunction<glGetStringDelegate>("glGetString", out var getString) && getString is not null)
                {
                    IntPtr versionPtr = getString(GL_VERSION);
                    if (versionPtr != IntPtr.Zero)
                    {
                        Diagnostics.Info("OpenGL инициализирован (через GLFW)");
                        LoadAllFunctions();
                        return true;
                    }
                }
            }

            if (TryGetFunction<glGetStringDelegate>("glGetString", out var getStringDirect) && getStringDirect is not null)
            {
                IntPtr versionPtr = getStringDirect(GL_VERSION);
                if (versionPtr != IntPtr.Zero)
                {
                    Diagnostics.Info("OpenGL инициализирован (прямая загрузка)");
                    LoadAllFunctions();
                    return true;
                }
            }

            Diagnostics.Warning("OpenGL: glGetString не найден или не работает");
            return false;
        }

        protected override void ShutdownLibrary()
        {
            // OpenGL не требует явного завершения
        }

        private static void LoadAllFunctions()
        {
            LoadFunction(ref GLClearColor, "glClearColor");
            LoadFunction(ref GLClear, "glClear");
            LoadFunction(ref GLViewport, "glViewport");
            LoadFunction(ref GLGetString, "glGetString");
            LoadFunction(ref GLGetIntegerv, "glGetIntegerv");
            LoadFunction(ref GLGetFloatv, "glGetFloatv");
            LoadFunction(ref GLGetError, "glGetError");
            LoadFunction(ref GLFlush, "glFlush");
            LoadFunction(ref GLFinish, "glFinish");

            LoadFunction(ref GLGetStringi, "glGetStringi");
            LoadFunction(ref GLGenBuffers, "glGenBuffers");
            LoadFunction(ref GLDeleteBuffers, "glDeleteBuffers");
            LoadFunction(ref GLBindBuffer, "glBindBuffer");
            LoadFunction(ref GLBufferData, "glBufferData");
            LoadFunction(ref GLGenVertexArrays, "glGenVertexArrays");
            LoadFunction(ref GLDeleteVertexArrays, "glDeleteVertexArrays");
            LoadFunction(ref GLBindVertexArray, "glBindVertexArray");
            LoadFunction(ref GLEnableVertexAttribArray, "glEnableVertexAttribArray");
            LoadFunction(ref GLDisableVertexAttribArray, "glDisableVertexAttribArray");
            LoadFunction(ref GLVertexAttribPointer, "glVertexAttribPointer");
            LoadFunction(ref GLDrawArrays, "glDrawArrays");
            LoadFunction(ref GLDrawElements, "glDrawElements");

            LoadFunction(ref GLEnable, "glEnable");
            LoadFunction(ref GLDisable, "glDisable");
            LoadFunction(ref GLBlendFunc, "glBlendFunc");
            LoadFunction(ref GLDepthFunc, "glDepthFunc");
            LoadFunction(ref GLDepthMask, "glDepthMask");
            LoadFunction(ref GLCullFace, "glCullFace");
            LoadFunction(ref GLFrontFace, "glFrontFace");
            LoadFunction(ref GLPolygonMode, "glPolygonMode");
            LoadFunction(ref GLLineWidth, "glLineWidth");
            LoadFunction(ref GLPointSize, "glPointSize");
            LoadFunction(ref GLScissor, "glScissor");
            LoadFunction(ref GLStencilFunc, "glStencilFunc");
            LoadFunction(ref GLStencilOp, "glStencilOp");
            LoadFunction(ref GLStencilMask, "glStencilMask");

            LoadFunction(ref GLCreateShader, "glCreateShader");
            LoadFunction(ref GLShaderSource, "glShaderSource");
            LoadFunction(ref GLCompileShader, "glCompileShader");
            LoadFunction(ref GLGetShaderiv, "glGetShaderiv");
            LoadFunction(ref GLGetShaderInfoLog, "glGetShaderInfoLog");
            LoadFunction(ref GLDeleteShader, "glDeleteShader");

            LoadFunction(ref GLCreateProgram, "glCreateProgram");
            LoadFunction(ref GLAttachShader, "glAttachShader");
            LoadFunction(ref GLLinkProgram, "glLinkProgram");
            LoadFunction(ref GLGetProgramiv, "glGetProgramiv");
            LoadFunction(ref GLGetProgramInfoLog, "glGetProgramInfoLog");
            LoadFunction(ref GLDeleteProgram, "glDeleteProgram");
            LoadFunction(ref GLUseProgram, "glUseProgram");

            LoadFunction(ref GLGetUniformLocation, "glGetUniformLocation");
            LoadFunction(ref GLUniform1f, "glUniform1f");
            LoadFunction(ref GLUniform2f, "glUniform2f");
            LoadFunction(ref GLUniform3f, "glUniform3f");
            LoadFunction(ref GLUniform4f, "glUniform4f");
            LoadFunction(ref GLUniformMatrix4fv, "glUniformMatrix4fv");
            LoadFunction(ref GLUniform1i, "glUniform1i");

            LoadFunction(ref GLGenTextures, "glGenTextures");
            LoadFunction(ref GLDeleteTextures, "glDeleteTextures");
            LoadFunction(ref GLBindTexture, "glBindTexture");
            LoadFunction(ref GLTexParameteri, "glTexParameteri");
            LoadFunction(ref GLTexImage2D, "glTexImage2D");
            LoadFunction(ref GLGenerateMipmap, "glGenerateMipmap");
            LoadFunction(ref GLActiveTexture, "glActiveTexture");

            Diagnostics.Info("OpenGL: все функции загружены");
        }

        private static void LoadFunction<T>(ref T? field, string name) where T : Delegate
        {
            if (field is not null) return;

            if (!TryGetFunction<T>(name, out var func) || func is null)
            {
                Diagnostics.Warning($"OpenGL: не удалось загрузить функцию {name}");
                return;
            }

            field = func;
        }
    }

    private sealed class OpenGLLibrary : Base
    {
        protected override Func<string, IntPtr> Resolver => ResolveOpenGLStatic;

        public static IntPtr ResolveOpenGLStatic(string name)
        {
            if (GLFW.IsInitialized)
            {
                if (GLFW.TryGetFunction<GLFW.glfwGetProcAddressDelegate>("glfwGetProcAddress", out var getProc) && getProc is not null)
                {
                    IntPtr ptr = getProc(name);
                    if (ptr != IntPtr.Zero)
                        return ptr;
                }
            }

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (NativeLibrary.TryLoad("opengl32.dll", out var handle))
                    {
                        if (NativeLibrary.TryGetExport(handle, name, out IntPtr ptr) && ptr != IntPtr.Zero)
                            return ptr;
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    string[] libs = ["libGL.so.1", "libGL.so"];
                    foreach (var lib in libs)
                    {
                        if (NativeLibrary.TryLoad(lib, out var handle))
                        {
                            if (NativeLibrary.TryGetExport(handle, name, out IntPtr ptr) && ptr != IntPtr.Zero)
                                return ptr;
                        }
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    if (NativeLibrary.TryLoad("/System/Library/Frameworks/OpenGL.framework/OpenGL", out var handle))
                    {
                        if (NativeLibrary.TryGetExport(handle, name, out IntPtr ptr) && ptr != IntPtr.Zero)
                            return ptr;
                    }
                }
            }
            catch
            {
                // Игнорируем
            }

            return IntPtr.Zero;
        }
    }

    #endregion

    #region ---- Публичные методы ----
    
    public static bool IsInitialized
        => OpenGLHost.IsInitializedStatic;

    public static bool Initialize() 
        => OpenGLHost.InitializeStatic();


    public static T LoadFunction<T>(string name) where T : Delegate 
        => OpenGLHost.LoadFunction<T>(name);

    public static bool TryGetFunction<T>(string name, out T? del) where T : Delegate 
        => OpenGLHost.TryGetFunction(name, out del);

    public static void ClearCache() 
        => OpenGLHost.ClearCache();

    public static void Cleanup()
        => OpenGLHost.Cleanup();

    #endregion

    #region ---- Обёртки для функций ----

    public static void ClearColor(float red, float green, float blue, float alpha)
        => GLClearColor?.Invoke(red, green, blue, alpha);

    public static void Clear(uint mask)
        => GLClear?.Invoke(mask);

    public static void Viewport(int x, int y, int width, int height)
        => GLViewport?.Invoke(x, y, width, height);

    public static IntPtr GetString(uint name)
        => GLGetString?.Invoke(name) ?? IntPtr.Zero;

    public static void GetIntegerv(uint pname, ref int data)
        => GLGetIntegerv?.Invoke(pname, ref data);

    public static void GetFloatv(uint pname, IntPtr data)
        => GLGetFloatv?.Invoke(pname, data);

    public static uint GetError()
        => GLGetError?.Invoke() ?? 0;

    public static void Flush()
        => GLFlush?.Invoke();

    public static void Finish()
        => GLFinish?.Invoke();

    public static void GenBuffers(int n, ref uint buffers)
        => GLGenBuffers?.Invoke(n, ref buffers);

    public static void DeleteBuffers(int n, ref uint buffers)
        => GLDeleteBuffers?.Invoke(n, ref buffers);

    public static void BindBuffer(uint target, uint buffer)
        => GLBindBuffer?.Invoke(target, buffer);

    public static void BufferData(uint target, int size, IntPtr data, uint usage)
        => GLBufferData?.Invoke(target, size, data, usage);

    public static void GenVertexArrays(int n, ref uint arrays)
        => GLGenVertexArrays?.Invoke(n, ref arrays);

    public static void DeleteVertexArrays(int n, ref uint arrays)
        => GLDeleteVertexArrays?.Invoke(n, ref arrays);

    public static void BindVertexArray(uint array)
        => GLBindVertexArray?.Invoke(array);

    public static void EnableVertexAttribArray(uint index)
        => GLEnableVertexAttribArray?.Invoke(index);

    public static void DisableVertexAttribArray(uint index)
        => GLDisableVertexAttribArray?.Invoke(index);

    public static void VertexAttribPointer(uint index, int size, uint type, byte normalized, int stride, IntPtr pointer)
        => GLVertexAttribPointer?.Invoke(index, size, type, normalized, stride, pointer);

    public static void DrawArrays(uint mode, int first, int count)
        => GLDrawArrays?.Invoke(mode, first, count);

    public static void DrawElements(uint mode, int count, uint type, IntPtr indices)
        => GLDrawElements?.Invoke(mode, count, type, indices);

    public static void Enable(uint cap)
        => GLEnable?.Invoke(cap);

    public static void Disable(uint cap)
        => GLDisable?.Invoke(cap);

    public static void BlendFunc(uint sfactor, uint dfactor)
        => GLBlendFunc?.Invoke(sfactor, dfactor);

    public static void DepthFunc(uint func)
        => GLDepthFunc?.Invoke(func);

    public static void DepthMask(byte flag)
        => GLDepthMask?.Invoke(flag);

    public static void CullFace(uint mode)
        => GLCullFace?.Invoke(mode);

    public static void FrontFace(uint mode)
        => GLFrontFace?.Invoke(mode);

    public static void PolygonMode(uint face, uint mode)
        => GLPolygonMode?.Invoke(face, mode);

    public static void LineWidth(float width)
        => GLLineWidth?.Invoke(width);

    public static void PointSize(float size)
        => GLPointSize?.Invoke(size);

    public static void Scissor(int x, int y, int width, int height)
        => GLScissor?.Invoke(x, y, width, height);

    public static void StencilFunc(uint func, int @ref, uint mask)
        => GLStencilFunc?.Invoke(func, @ref, mask);

    public static void StencilOp(uint fail, uint zfail, uint zpass)
        => GLStencilOp?.Invoke(fail, zfail, zpass);

    public static void StencilMask(uint mask)
        => GLStencilMask?.Invoke(mask);

    public static uint CreateShader(uint shaderType)
        => GLCreateShader?.Invoke(shaderType) ?? 0;

    public static void ShaderSource(uint shader, int count, ref string @string, IntPtr length)
        => GLShaderSource?.Invoke(shader, count, ref @string, length);

    public static void CompileShader(uint shader)
        => GLCompileShader?.Invoke(shader);

    public static void GetShaderiv(uint shader, uint pname, ref int param)
        => GLGetShaderiv?.Invoke(shader, pname, ref param);

    public static void GetShaderInfoLog(uint shader, int maxLength, out int length, StringBuilder infoLog)
    {
        if (GLGetShaderInfoLog is not null)
        {
            GLGetShaderInfoLog(shader, maxLength, out length, infoLog);
        }
        else
        {
            length = 0;
        }
    }

    public static void DeleteShader(uint shader)
        => GLDeleteShader?.Invoke(shader);

    public static uint CreateProgram()
        => GLCreateProgram?.Invoke() ?? 0;

    public static void AttachShader(uint program, uint shader)
        => GLAttachShader?.Invoke(program, shader);

    public static void LinkProgram(uint program)
        => GLLinkProgram?.Invoke(program);

    public static void GetProgramiv(uint program, uint pname, ref int param)
        => GLGetProgramiv?.Invoke(program, pname, ref param);

    public static void GetProgramInfoLog(uint program, int maxLength, out int length, StringBuilder infoLog)
    {
        if (GLGetProgramInfoLog is not null)
        {
            GLGetProgramInfoLog(program, maxLength, out length, infoLog);
        }
        else
        {
            length = 0;
        }
    }

    public static void DeleteProgram(uint program)
        => GLDeleteProgram?.Invoke(program);

    public static void UseProgram(uint program)
        => GLUseProgram?.Invoke(program);

    public static int GetUniformLocation(uint program, string name)
        => GLGetUniformLocation?.Invoke(program, name) ?? -1;

    public static void Uniform1f(int location, float v0)
        => GLUniform1f?.Invoke(location, v0);

    public static void Uniform2f(int location, float v0, float v1)
        => GLUniform2f?.Invoke(location, v0, v1);

    public static void Uniform3f(int location, float v0, float v1, float v2)
        => GLUniform3f?.Invoke(location, v0, v1, v2);

    public static void Uniform4f(int location, float v0, float v1, float v2, float v3)
        => GLUniform4f?.Invoke(location, v0, v1, v2, v3);

    public static void UniformMatrix4fv(int location, int count, byte transpose, IntPtr value)
        => GLUniformMatrix4fv?.Invoke(location, count, transpose, value);

    public static void Uniform1i(int location, int v0)
        => GLUniform1i?.Invoke(location, v0);

    public static void GenTextures(int n, ref uint textures)
        => GLGenTextures?.Invoke(n, ref textures);

    public static void DeleteTextures(int n, ref uint textures)
        => GLDeleteTextures?.Invoke(n, ref textures);

    public static void BindTexture(uint target, uint texture)
        => GLBindTexture?.Invoke(target, texture);

    public static void TexParameteri(uint target, uint pname, int param)
        => GLTexParameteri?.Invoke(target, pname, param);

    public static void TexImage2D(uint target, int level, int internalFormat, int width, int height, int border, uint format, uint type, IntPtr data)
        => GLTexImage2D?.Invoke(target, level, internalFormat, width, height, border, format, type, data);

    public static void GenerateMipmap(uint target)
        => GLGenerateMipmap?.Invoke(target);

    public static void ActiveTexture(uint texture)
        => GLActiveTexture?.Invoke(texture);

    #endregion

    #region ---- Делегаты ----

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glClearColorDelegate(float red, float green, float blue, float alpha);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glClearDelegate(uint mask);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glViewportDelegate(int x, int y, int width, int height);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr glGetStringDelegate(uint name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glGetIntegervDelegate(uint pname, ref int data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glGetFloatvDelegate(uint pname, IntPtr data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint glGetErrorDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glFlushDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glFinishDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr glGetStringiDelegate(uint name, uint index);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glGenBuffersDelegate(int n, ref uint buffers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glDeleteBuffersDelegate(int n, ref uint buffers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glBindBufferDelegate(uint target, uint buffer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glBufferDataDelegate(uint target, int size, IntPtr data, uint usage);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glGenVertexArraysDelegate(int n, ref uint arrays);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glDeleteVertexArraysDelegate(int n, ref uint arrays);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glBindVertexArrayDelegate(uint array);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glEnableVertexAttribArrayDelegate(uint index);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glDisableVertexAttribArrayDelegate(uint index);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glVertexAttribPointerDelegate(uint index, int size, uint type, byte normalized, int stride, IntPtr pointer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glDrawArraysDelegate(uint mode, int first, int count);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glDrawElementsDelegate(uint mode, int count, uint type, IntPtr indices);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glEnableDelegate(uint cap);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glDisableDelegate(uint cap);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glBlendFuncDelegate(uint sfactor, uint dfactor);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glDepthFuncDelegate(uint func);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glDepthMaskDelegate(byte flag);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glCullFaceDelegate(uint mode);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glFrontFaceDelegate(uint mode);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glPolygonModeDelegate(uint face, uint mode);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glLineWidthDelegate(float width);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glPointSizeDelegate(float size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glScissorDelegate(int x, int y, int width, int height);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glStencilFuncDelegate(uint func, int @ref, uint mask);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glStencilOpDelegate(uint fail, uint zfail, uint zpass);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glStencilMaskDelegate(uint mask);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint glCreateShaderDelegate(uint shaderType);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glShaderSourceDelegate(uint shader, int count, ref string @string, IntPtr length);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glCompileShaderDelegate(uint shader);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glGetShaderivDelegate(uint shader, uint pname, ref int param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glGetShaderInfoLogDelegate(uint shader, int maxLength, out int length, StringBuilder infoLog);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glDeleteShaderDelegate(uint shader);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint glCreateProgramDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glAttachShaderDelegate(uint program, uint shader);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glLinkProgramDelegate(uint program);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glGetProgramivDelegate(uint program, uint pname, ref int param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glGetProgramInfoLogDelegate(uint program, int maxLength, out int length, StringBuilder infoLog);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glDeleteProgramDelegate(uint program);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glUseProgramDelegate(uint program);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int glGetUniformLocationDelegate(uint program, string name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glUniform1fDelegate(int location, float v0);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glUniform2fDelegate(int location, float v0, float v1);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glUniform3fDelegate(int location, float v0, float v1, float v2);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glUniform4fDelegate(int location, float v0, float v1, float v2, float v3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glUniformMatrix4fvDelegate(int location, int count, byte transpose, IntPtr value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glUniform1iDelegate(int location, int v0);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glGenTexturesDelegate(int n, ref uint textures);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glDeleteTexturesDelegate(int n, ref uint textures);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glBindTextureDelegate(uint target, uint texture);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glTexParameteriDelegate(uint target, uint pname, int param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glTexImage2DDelegate(uint target, int level, int internalFormat, int width, int height, int border, uint format, uint type, IntPtr data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glGenerateMipmapDelegate(uint target);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glActiveTextureDelegate(uint texture);

    #endregion

    #region ---- Поля для функций ----

    private static glClearColorDelegate? GLClearColor;
    private static glClearDelegate? GLClear;
    private static glViewportDelegate? GLViewport;
    private static glGetStringDelegate? GLGetString;
    private static glGetIntegervDelegate? GLGetIntegerv;
    private static glGetFloatvDelegate? GLGetFloatv;
    private static glGetErrorDelegate? GLGetError;
    private static glFlushDelegate? GLFlush;
    private static glFinishDelegate? GLFinish;

    private static glGetStringiDelegate? GLGetStringi;
    private static glGenBuffersDelegate? GLGenBuffers;
    private static glDeleteBuffersDelegate? GLDeleteBuffers;
    private static glBindBufferDelegate? GLBindBuffer;
    private static glBufferDataDelegate? GLBufferData;
    private static glGenVertexArraysDelegate? GLGenVertexArrays;
    private static glDeleteVertexArraysDelegate? GLDeleteVertexArrays;
    private static glBindVertexArrayDelegate? GLBindVertexArray;
    private static glEnableVertexAttribArrayDelegate? GLEnableVertexAttribArray;
    private static glDisableVertexAttribArrayDelegate? GLDisableVertexAttribArray;
    private static glVertexAttribPointerDelegate? GLVertexAttribPointer;
    private static glDrawArraysDelegate? GLDrawArrays;
    private static glDrawElementsDelegate? GLDrawElements;

    private static glEnableDelegate? GLEnable;
    private static glDisableDelegate? GLDisable;
    private static glBlendFuncDelegate? GLBlendFunc;
    private static glDepthFuncDelegate? GLDepthFunc;
    private static glDepthMaskDelegate? GLDepthMask;
    private static glCullFaceDelegate? GLCullFace;
    private static glFrontFaceDelegate? GLFrontFace;
    private static glPolygonModeDelegate? GLPolygonMode;
    private static glLineWidthDelegate? GLLineWidth;
    private static glPointSizeDelegate? GLPointSize;
    private static glScissorDelegate? GLScissor;
    private static glStencilFuncDelegate? GLStencilFunc;
    private static glStencilOpDelegate? GLStencilOp;
    private static glStencilMaskDelegate? GLStencilMask;

    private static glCreateShaderDelegate? GLCreateShader;
    private static glShaderSourceDelegate? GLShaderSource;
    private static glCompileShaderDelegate? GLCompileShader;
    private static glGetShaderivDelegate? GLGetShaderiv;
    private static glGetShaderInfoLogDelegate? GLGetShaderInfoLog;
    private static glDeleteShaderDelegate? GLDeleteShader;

    private static glCreateProgramDelegate? GLCreateProgram;
    private static glAttachShaderDelegate? GLAttachShader;
    private static glLinkProgramDelegate? GLLinkProgram;
    private static glGetProgramivDelegate? GLGetProgramiv;
    private static glGetProgramInfoLogDelegate? GLGetProgramInfoLog;
    private static glDeleteProgramDelegate? GLDeleteProgram;
    private static glUseProgramDelegate? GLUseProgram;

    private static glGetUniformLocationDelegate? GLGetUniformLocation;
    private static glUniform1fDelegate? GLUniform1f;
    private static glUniform2fDelegate? GLUniform2f;
    private static glUniform3fDelegate? GLUniform3f;
    private static glUniform4fDelegate? GLUniform4f;
    private static glUniformMatrix4fvDelegate? GLUniformMatrix4fv;
    private static glUniform1iDelegate? GLUniform1i;

    private static glGenTexturesDelegate? GLGenTextures;
    private static glDeleteTexturesDelegate? GLDeleteTextures;
    private static glBindTextureDelegate? GLBindTexture;
    private static glTexParameteriDelegate? GLTexParameteri;
    private static glTexImage2DDelegate? GLTexImage2D;
    private static glGenerateMipmapDelegate? GLGenerateMipmap;
    private static glActiveTextureDelegate? GLActiveTexture;

    #endregion

    #region ---- Константы ----

    // ===== Очистка =====
    public const uint GL_COLOR_BUFFER_BIT = 0x00004000;
    public const uint GL_DEPTH_BUFFER_BIT = 0x00000100;
    public const uint GL_STENCIL_BUFFER_BIT = 0x00000400;

    // ===== Информация =====
    public const uint GL_VERSION = 0x1F02;
    public const uint GL_EXTENSIONS = 0x1F03;
    public const uint GL_RENDERER = 0x1F01;
    public const uint GL_VENDOR = 0x1F00;
    public const uint GL_NUM_EXTENSIONS = 0x821D;
    public const uint GL_SHADING_LANGUAGE_VERSION = 0x8B8C;

    // ===== Ошибки =====
    public const uint GL_NO_ERROR = 0x00000000;
    public const uint GL_INVALID_ENUM = 0x0500;
    public const uint GL_INVALID_VALUE = 0x0501;
    public const uint GL_INVALID_OPERATION = 0x0502;
    public const uint GL_STACK_OVERFLOW = 0x0503;
    public const uint GL_STACK_UNDERFLOW = 0x0504;
    public const uint GL_OUT_OF_MEMORY = 0x0505;

    // ===== Буферы =====
    public const uint GL_ARRAY_BUFFER = 0x8892;
    public const uint GL_ELEMENT_ARRAY_BUFFER = 0x8893;
    public const uint GL_STREAM_DRAW = 0x88E0;
    public const uint GL_STREAM_READ = 0x88E1;
    public const uint GL_STREAM_COPY = 0x88E2;
    public const uint GL_STATIC_DRAW = 0x88E4;
    public const uint GL_STATIC_READ = 0x88E5;
    public const uint GL_STATIC_COPY = 0x88E6;
    public const uint GL_DYNAMIC_DRAW = 0x88E8;
    public const uint GL_DYNAMIC_READ = 0x88E9;
    public const uint GL_DYNAMIC_COPY = 0x88EA;

    // ===== Вершинные атрибуты =====
    public const uint GL_FLOAT = 0x1406;
    public const uint GL_UNSIGNED_BYTE = 0x1401;
    public const uint GL_UNSIGNED_SHORT = 0x1403;
    public const uint GL_UNSIGNED_INT = 0x1405;

    // ===== Режимы рисования =====
    public const uint GL_POINTS = 0x0000;
    public const uint GL_LINES = 0x0001;
    public const uint GL_LINE_LOOP = 0x0002;
    public const uint GL_LINE_STRIP = 0x0003;
    public const uint GL_TRIANGLES = 0x0004;
    public const uint GL_TRIANGLE_STRIP = 0x0005;
    public const uint GL_TRIANGLE_FAN = 0x0006;

    // ===== Состояния =====
    public const uint GL_BLEND = 0x0BE2;
    public const uint GL_CULL_FACE = 0x0B44;
    public const uint GL_DEPTH_TEST = 0x0B71;
    public const uint GL_STENCIL_TEST = 0x0B90;
    public const uint GL_SCISSOR_TEST = 0x0C11;

    // ===== Blend функции =====
    public const uint GL_SRC_ALPHA = 0x0302;
    public const uint GL_ONE_MINUS_SRC_ALPHA = 0x0303;
    public const uint GL_ONE = 0x0001;
    public const uint GL_ZERO = 0x0000;

    // ===== Cull face =====
    public const uint GL_BACK = 0x0405;
    public const uint GL_FRONT = 0x0404;
    public const uint GL_FRONT_AND_BACK = 0x0408;

    // ===== Front face =====
    public const uint GL_CW = 0x0900;
    public const uint GL_CCW = 0x0901;

    // ===== Polygon mode =====
    public const uint GL_FILL = 0x1B02;
    public const uint GL_LINE = 0x1B01;
    public const uint GL_POINT = 0x1B00;

    // ===== Depth =====
    public const uint GL_ALWAYS = 0x0207;
    public const uint GL_NEVER = 0x0200;
    public const uint GL_LESS = 0x0201;
    public const uint GL_EQUAL = 0x0202;
    public const uint GL_LEQUAL = 0x0203;
    public const uint GL_GREATER = 0x0204;
    public const uint GL_NOTEQUAL = 0x0205;
    public const uint GL_GEQUAL = 0x0206;

    // ===== Stencil =====
    public const uint GL_KEEP = 0x1E00;
    public const uint GL_REPLACE = 0x1E01;
    public const uint GL_INCR = 0x1E02;
    public const uint GL_DECR = 0x1E03;
    public const uint GL_INCR_WRAP = 0x8507;
    public const uint GL_DECR_WRAP = 0x8508;

    // ===== Шейдеры =====
    public const uint GL_VERTEX_SHADER = 0x8B31;
    public const uint GL_FRAGMENT_SHADER = 0x8B30;
    public const uint GL_GEOMETRY_SHADER = 0x8DD9;
    public const uint GL_COMPUTE_SHADER = 0x91B9;
    public const uint GL_TESS_CONTROL_SHADER = 0x8E88;
    public const uint GL_TESS_EVALUATION_SHADER = 0x8E87;

    // ===== Шейдеры (статусы) =====
    public const uint GL_COMPILE_STATUS = 0x8B82;
    public const uint GL_LINK_STATUS = 0x8B82;
    public const uint GL_INFO_LOG_LENGTH = 0x8B84;

    // ===== Текстуры =====
    public const uint GL_TEXTURE_2D = 0x0DE1;
    public const uint GL_TEXTURE_MIN_FILTER = 0x2800;
    public const uint GL_TEXTURE_MAG_FILTER = 0x2801;
    public const uint GL_TEXTURE_WRAP_S = 0x2802;
    public const uint GL_TEXTURE_WRAP_T = 0x2803;
    public const uint GL_LINEAR = 0x2601;
    public const uint GL_NEAREST = 0x2600;
    public const uint GL_LINEAR_MIPMAP_LINEAR = 0x2703;
    public const uint GL_LINEAR_MIPMAP_NEAREST = 0x2701;
    public const uint GL_NEAREST_MIPMAP_LINEAR = 0x2702;
    public const uint GL_NEAREST_MIPMAP_NEAREST = 0x2700;
    public const uint GL_REPEAT = 0x2901;
    public const uint GL_CLAMP_TO_EDGE = 0x812F;
    public const uint GL_MIRRORED_REPEAT = 0x8370;

    // ===== Текстуры (форматы) =====
    public const uint GL_RGB = 0x1907;
    public const uint GL_RGBA = 0x1908;
    public const uint GL_RED = 0x1903;
    public const uint GL_R8 = 0x8229;
    public const uint GL_R16 = 0x822A;
    public const uint GL_RGB8 = 0x8051;
    public const uint GL_RGBA8 = 0x8058;
    public const uint GL_RGBA16 = 0x805B;
    public const uint GL_RGBA16F = 0x881A;
    public const uint GL_RGB16F = 0x881B;
    public const uint GL_RGBA32F = 0x8814;
    public const uint GL_DEPTH_COMPONENT = 0x1902;
    public const uint GL_DEPTH_COMPONENT16 = 0x81A5;
    public const uint GL_DEPTH_COMPONENT24 = 0x81A6;
    public const uint GL_DEPTH_COMPONENT32 = 0x81A7;
    public const uint GL_DEPTH24_STENCIL8 = 0x88F0;
    public const uint GL_DEPTH32_STENCIL8 = 0x8CAD;

    // ===== Текстуры (слоты) =====
    public const uint GL_TEXTURE0 = 0x84C0;

    // ===== Compare modes =====
    public const uint GL_COMPARE_REF_TO_TEXTURE = 0x884E;
    public const uint GL_TEXTURE_COMPARE_MODE = 0x884C;
    public const uint GL_TEXTURE_COMPARE_FUNC = 0x884D;

    // ===== Framebuffer =====
    public const uint GL_FRAMEBUFFER = 0x8D40;
    public const uint GL_READ_FRAMEBUFFER = 0x8CA8;
    public const uint GL_DRAW_FRAMEBUFFER = 0x8CA9;
    public const uint GL_COLOR_ATTACHMENT0 = 0x8CE0;
    public const uint GL_DEPTH_ATTACHMENT = 0x8D00;
    public const uint GL_STENCIL_ATTACHMENT = 0x8D20;
    public const uint GL_DEPTH_STENCIL_ATTACHMENT = 0x821A;
    public const uint GL_FRAMEBUFFER_COMPLETE = 0x8CD5;
    public const uint GL_FRAMEBUFFER_INCOMPLETE_ATTACHMENT = 0x8CD6;
    public const uint GL_FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT = 0x8CD7;
    public const uint GL_FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER = 0x8CDB;
    public const uint GL_FRAMEBUFFER_INCOMPLETE_READ_BUFFER = 0x8CDC;
    public const uint GL_FRAMEBUFFER_UNSUPPORTED = 0x8CDD;
    public const uint GL_FRAMEBUFFER_INCOMPLETE_MULTISAMPLE = 0x8D56;

    // ===== Renderbuffer =====
    public const uint GL_RENDERBUFFER = 0x8D41;
    public const uint GL_RENDERBUFFER_WIDTH = 0x8D42;
    public const uint GL_RENDERBUFFER_HEIGHT = 0x8D43;
    public const uint GL_RENDERBUFFER_INTERNAL_FORMAT = 0x8D44;
    public const uint GL_RENDERBUFFER_SAMPLES = 0x8CAB;

    // ===== Read pixels =====
    public const uint GL_PACK_ALIGNMENT = 0x0D05;
    public const uint GL_UNPACK_ALIGNMENT = 0x0CF5;

    // ===== Pixel formats =====
    public const uint GL_BGRA = 0x80E1;
    public const uint GL_BGR = 0x80E0;

    // ===== Capabilities =====
    public const uint GL_MAX_TEXTURE_SIZE = 0x0D33;
    public const uint GL_MAX_VERTEX_ATTRIBS = 0x8869;
    public const uint GL_MAX_UNIFORM_BLOCK_SIZE = 0x85A8;
    public const uint GL_MAX_SHADER_STORAGE_BLOCK_SIZE = 0x90E1;
    public const uint GL_MAX_COMPUTE_WORK_GROUP_INVOCATIONS = 0x90EB;

    #endregion
}