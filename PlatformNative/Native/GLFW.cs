using PlatformNative.Core;
using PlatformNative.Core.Library;
using System.Runtime.InteropServices;

namespace PlatformNative.Native;

/// <summary>
/// Нативная обёртка для GLFW 3.5.1
/// </summary>
public static class GLFW
{
    #region ---- Хост ----

    private sealed class GLFWHost : Host<GLFWHost, GLFWLibrary>
    {
        protected override string LibraryKey => "GLFW";
        protected override Func<string, IntPtr> Resolver => Manager.ResolveGLFW;
        protected override Func<bool> Loader => Manager.LoadGLFW;

        protected override bool InitializeLibrary()
        {
            if (!TryGetFunction<glfwInitDelegate>("glfwInit", out var init) || init is null)
            {
                Diagnostics.Warning("GLFW: glfwInit не найден");
                return false;
            }

            if (init() == 0)
            {
                Diagnostics.Warning("GLFW: glfwInit() вернул ошибку");
                return false;
            }

            if (TryGetFunction<glfwSetErrorCallbackDelegate>("glfwSetErrorCallback", out var setError) && setError is not null)
            {
                setError(OnError);
            }

            Diagnostics.Info($"GLFW {GetVersionString()} инициализирован");
            return true;
        }

        protected override void ShutdownLibrary()
        {
            if (TryGetFunction<glfwTerminateDelegate>("glfwTerminate", out var terminate) && terminate is not null)
            {
                terminate();
                Diagnostics.Info("GLFW завершён");
            }
        }

        private static string GetVersionString()
        {
            if (TryGetFunction<glfwGetVersionDelegate>("glfwGetVersion", out var getVersion) && getVersion is not null)
            {
                getVersion(out int major, out int minor, out int rev);
                return $"{major}.{minor}.{rev}";
            }
            return "unknown";
        }

        private static void OnError(int error, IntPtr description)
        {
            string? desc = Marshal.PtrToStringAnsi(description);
            Diagnostics.Error($"GLFW Error ({error}): {desc ?? "unknown"}");
        }
    }

    private sealed class GLFWLibrary : Base
    {
        protected override Func<string, IntPtr> Resolver => Manager.ResolveGLFW;
    }

    #endregion

    #region ---- Публичные методы ----
    
    public static bool IsInitialized => GLFWHost.IsInitializedStatic;

    public static bool Initialize() => GLFWHost.InitializeStatic();
    public static void Terminate() => GLFWHost.ShutdownStatic();

    public static T LoadFunction<T>(string name) where T : Delegate
        => GLFWHost.LoadFunction<T>(name);

    public static bool TryGetFunction<T>(string name, out T? del) where T : Delegate
        => GLFWHost.TryGetFunction(name, out del);

    public static void ClearCache()
        => GLFWHost.ClearCache();

    public static void Cleanup()
        => GLFWHost.Cleanup();

    #endregion

    #region ---- Делегаты ----

    // ===== Инициализация =====
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int glfwInitDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwTerminateDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetErrorCallbackDelegate(glfwErrorFun callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwErrorFun(int error, IntPtr description);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr glfwGetProcAddressDelegate(string procname);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwGetVersionDelegate(out int major, out int minor, out int rev);

    // ===== Окна =====
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr glfwCreateWindowDelegate(int width, int height, string title, IntPtr monitor, IntPtr share);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwDestroyWindowDelegate(IntPtr window);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetWindowTitleDelegate(IntPtr window, string title);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr glfwGetWindowTitleDelegate(IntPtr window);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwGetWindowSizeDelegate(IntPtr window, out int width, out int height);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetWindowSizeDelegate(IntPtr window, int width, int height);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwGetWindowPosDelegate(IntPtr window, out int xpos, out int ypos);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetWindowPosDelegate(IntPtr window, int xpos, int ypos);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwShowWindowDelegate(IntPtr window);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwHideWindowDelegate(IntPtr window);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int glfwGetWindowAttribDelegate(IntPtr window, int attrib);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwIconifyWindowDelegate(IntPtr window);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwMaximizeWindowDelegate(IntPtr window);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwRestoreWindowDelegate(IntPtr window);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetWindowSizeLimitsDelegate(IntPtr window, int minWidth, int minHeight, int maxWidth, int maxHeight);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetWindowAspectRatioDelegate(IntPtr window, int numer, int denom);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwFocusWindowDelegate(IntPtr window);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwRequestWindowAttentionDelegate(IntPtr window);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate float glfwGetWindowOpacityDelegate(IntPtr window);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetWindowOpacityDelegate(IntPtr window, float opacity);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetWindowIconDelegate(IntPtr window, int count, IntPtr images);

    // ===== Контекст OpenGL =====
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwMakeContextCurrentDelegate(IntPtr window);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSwapBuffersDelegate(IntPtr window);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSwapIntervalDelegate(int interval);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwWindowHintDelegate(int hint, int value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwDefaultWindowHintsDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr glfwGetCurrentContextDelegate();

    // ===== События =====
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwPollEventsDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwWaitEventsDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwWaitEventsTimeoutDelegate(double timeout);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwPostEmptyEventDelegate();

    // ===== Ввод =====
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int glfwGetKeyNameDelegate(int key, int scancode);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetCursorPosDelegate(IntPtr window, double xpos, double ypos);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetInputModeDelegate(IntPtr window, int mode, int value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int glfwGetKeyDelegate(IntPtr window, int key);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int glfwGetMouseButtonDelegate(IntPtr window, int button);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwGetCursorPosDelegate(IntPtr window, out double xpos, out double ypos);

    // ===== Колбэки =====
    public delegate void GlfwKeyFun(IntPtr window, int key, int scancode, int action, int mods);
    public delegate void GlfwCharFun(IntPtr window, uint codepoint);
    public delegate void GlfwMouseButtonFun(IntPtr window, int button, int action, int mods);
    public delegate void GlfwCursorPosFun(IntPtr window, double xpos, double ypos);
    public delegate void GlfwScrollFun(IntPtr window, double xoffset, double yoffset);
    public delegate void GlfwWindowCloseFun(IntPtr window);
    public delegate void GlfwWindowSizeFun(IntPtr window, int width, int height);
    public delegate void GlfwWindowPosFun(IntPtr window, int xpos, int ypos);
    public delegate void GlfwWindowFocusFun(IntPtr window, int focused);
    public delegate void GlfwWindowIconifyFun(IntPtr window, int iconified);
    public delegate void GlfwWindowMaximizeFun(IntPtr window, int maximized);
    public delegate void GlfwWindowContentScaleFun(IntPtr window, float xscale, float yscale);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetKeyCallbackDelegate(IntPtr window, GlfwKeyFun? callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetCharCallbackDelegate(IntPtr window, GlfwCharFun? callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetMouseButtonCallbackDelegate(IntPtr window, GlfwMouseButtonFun? callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetCursorPosCallbackDelegate(IntPtr window, GlfwCursorPosFun? callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetScrollCallbackDelegate(IntPtr window, GlfwScrollFun? callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetWindowCloseCallbackDelegate(IntPtr window, GlfwWindowCloseFun? callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetWindowSizeCallbackDelegate(IntPtr window, GlfwWindowSizeFun? callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetWindowPosCallbackDelegate(IntPtr window, GlfwWindowPosFun? callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetWindowFocusCallbackDelegate(IntPtr window, GlfwWindowFocusFun? callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetWindowIconifyCallbackDelegate(IntPtr window, GlfwWindowIconifyFun? callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetWindowMaximizeCallbackDelegate(IntPtr window, GlfwWindowMaximizeFun? callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetWindowContentScaleCallbackDelegate(IntPtr window, GlfwWindowContentScaleFun? callback);

    // ===== Буфер обмена =====
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr glfwGetClipboardStringDelegate(IntPtr window);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetClipboardStringDelegate(IntPtr window, string text);

    // ===== Мониторы =====
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr glfwGetPrimaryMonitorDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr glfwGetMonitorsDelegate(out int count);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwGetMonitorPosDelegate(IntPtr monitor, out int xpos, out int ypos);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwGetMonitorPhysicalSizeDelegate(IntPtr monitor, out int widthMM, out int heightMM);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr glfwGetMonitorNameDelegate(IntPtr monitor);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwGetMonitorWorkareaDelegate(IntPtr monitor, out int xpos, out int ypos, out int width, out int height);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr glfwGetWindowMonitorDelegate(IntPtr window);

    // ===== Нативные хендлы =====
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr glfwGetWin32WindowDelegate(IntPtr window);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr glfwGetX11WindowDelegate(IntPtr window);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr glfwGetCocoaWindowDelegate(IntPtr window);

    // ===== Дополнительные функции =====
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void glfwSetWindowShouldCloseDelegate(IntPtr window, int value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int glfwWindowShouldCloseDelegate(IntPtr window);

    #endregion

    #region ---- Константы ----

    public const int GLFW_RESIZABLE = 0x00020003;
    public const int GLFW_VISIBLE = 0x00020004;
    public const int GLFW_DECORATED = 0x00020005;
    public const int GLFW_FOCUSED = 0x00020001;
    public const int GLFW_MAXIMIZED = 0x00020008;
    public const int GLFW_FLOATING = 0x00020007;
    public const int GLFW_ICONIFIED = 0x00020002;
    public const int GLFW_TRANSPARENT_FRAMEBUFFER = 0x0002000A;

    public const int GLFW_CONTEXT_VERSION_MAJOR = 0x00022002;
    public const int GLFW_CONTEXT_VERSION_MINOR = 0x00022003;
    public const int GLFW_OPENGL_PROFILE = 0x00022008;
    public const int GLFW_OPENGL_CORE_PROFILE = 0x00032001;
    public const int GLFW_OPENGL_COMPAT_PROFILE = 0x00032002;
    public const int GLFW_OPENGL_FORWARD_COMPAT = 0x00022006;

    public const int GLFW_PRESS = 1;
    public const int GLFW_RELEASE = 0;
    public const int GLFW_REPEAT = 2;

    public const int GLFW_MOUSE_BUTTON_LEFT = 0;
    public const int GLFW_MOUSE_BUTTON_RIGHT = 1;
    public const int GLFW_MOUSE_BUTTON_MIDDLE = 2;

    public const int GLFW_CURSOR = 0x00033001;
    public const int GLFW_CURSOR_NORMAL = 0x00034001;
    public const int GLFW_CURSOR_HIDDEN = 0x00034002;
    public const int GLFW_CURSOR_DISABLED = 0x00034003;

    public const int GLFW_KEY_SPACE = 32;
    public const int GLFW_KEY_APOSTROPHE = 39;
    public const int GLFW_KEY_COMMA = 44;
    public const int GLFW_KEY_MINUS = 45;
    public const int GLFW_KEY_PERIOD = 46;
    public const int GLFW_KEY_SLASH = 47;
    public const int GLFW_KEY_0 = 48;
    public const int GLFW_KEY_1 = 49;
    public const int GLFW_KEY_2 = 50;
    public const int GLFW_KEY_3 = 51;
    public const int GLFW_KEY_4 = 52;
    public const int GLFW_KEY_5 = 53;
    public const int GLFW_KEY_6 = 54;
    public const int GLFW_KEY_7 = 55;
    public const int GLFW_KEY_8 = 56;
    public const int GLFW_KEY_9 = 57;
    public const int GLFW_KEY_SEMICOLON = 59;
    public const int GLFW_KEY_EQUAL = 61;
    public const int GLFW_KEY_A = 65;
    public const int GLFW_KEY_B = 66;
    public const int GLFW_KEY_C = 67;
    public const int GLFW_KEY_D = 68;
    public const int GLFW_KEY_E = 69;
    public const int GLFW_KEY_F = 70;
    public const int GLFW_KEY_G = 71;
    public const int GLFW_KEY_H = 72;
    public const int GLFW_KEY_I = 73;
    public const int GLFW_KEY_J = 74;
    public const int GLFW_KEY_K = 75;
    public const int GLFW_KEY_L = 76;
    public const int GLFW_KEY_M = 77;
    public const int GLFW_KEY_N = 78;
    public const int GLFW_KEY_O = 79;
    public const int GLFW_KEY_P = 80;
    public const int GLFW_KEY_Q = 81;
    public const int GLFW_KEY_R = 82;
    public const int GLFW_KEY_S = 83;
    public const int GLFW_KEY_T = 84;
    public const int GLFW_KEY_U = 85;
    public const int GLFW_KEY_V = 86;
    public const int GLFW_KEY_W = 87;
    public const int GLFW_KEY_X = 88;
    public const int GLFW_KEY_Y = 89;
    public const int GLFW_KEY_Z = 90;
    public const int GLFW_KEY_LEFT_BRACKET = 91;
    public const int GLFW_KEY_BACKSLASH = 92;
    public const int GLFW_KEY_RIGHT_BRACKET = 93;
    public const int GLFW_KEY_GRAVE_ACCENT = 96;

    public const int GLFW_KEY_ESCAPE = 256;
    public const int GLFW_KEY_ENTER = 257;
    public const int GLFW_KEY_TAB = 258;
    public const int GLFW_KEY_BACKSPACE = 259;
    public const int GLFW_KEY_INSERT = 260;
    public const int GLFW_KEY_DELETE = 261;
    public const int GLFW_KEY_RIGHT = 262;
    public const int GLFW_KEY_LEFT = 263;
    public const int GLFW_KEY_DOWN = 264;
    public const int GLFW_KEY_UP = 265;
    public const int GLFW_KEY_PAGE_UP = 266;
    public const int GLFW_KEY_PAGE_DOWN = 267;
    public const int GLFW_KEY_HOME = 268;
    public const int GLFW_KEY_END = 269;
    public const int GLFW_KEY_CAPS_LOCK = 280;
    public const int GLFW_KEY_SCROLL_LOCK = 281;
    public const int GLFW_KEY_NUM_LOCK = 282;
    public const int GLFW_KEY_PRINT_SCREEN = 283;
    public const int GLFW_KEY_PAUSE = 284;

    public const int GLFW_KEY_F1 = 290;
    public const int GLFW_KEY_F2 = 291;
    public const int GLFW_KEY_F3 = 292;
    public const int GLFW_KEY_F4 = 293;
    public const int GLFW_KEY_F5 = 294;
    public const int GLFW_KEY_F6 = 295;
    public const int GLFW_KEY_F7 = 296;
    public const int GLFW_KEY_F8 = 297;
    public const int GLFW_KEY_F9 = 298;
    public const int GLFW_KEY_F10 = 299;
    public const int GLFW_KEY_F11 = 300;
    public const int GLFW_KEY_F12 = 301;
    public const int GLFW_KEY_F13 = 302;
    public const int GLFW_KEY_F14 = 303;
    public const int GLFW_KEY_F15 = 304;
    public const int GLFW_KEY_F16 = 305;
    public const int GLFW_KEY_F17 = 306;
    public const int GLFW_KEY_F18 = 307;
    public const int GLFW_KEY_F19 = 308;
    public const int GLFW_KEY_F20 = 309;
    public const int GLFW_KEY_F21 = 310;
    public const int GLFW_KEY_F22 = 311;
    public const int GLFW_KEY_F23 = 312;
    public const int GLFW_KEY_F24 = 313;
    public const int GLFW_KEY_F25 = 314;

    public const int GLFW_MOD_SHIFT = 0x0001;
    public const int GLFW_MOD_CONTROL = 0x0002;
    public const int GLFW_MOD_ALT = 0x0004;
    public const int GLFW_MOD_SUPER = 0x0008;
    public const int GLFW_MOD_CAPS_LOCK = 0x0010;
    public const int GLFW_MOD_NUM_LOCK = 0x0020;

    #endregion

    #region ---- Структуры ----

    [StructLayout(LayoutKind.Sequential)]
    public struct GLFWimage
    {
        public int width;
        public int height;
        public IntPtr pixels;
    }

    #endregion
}