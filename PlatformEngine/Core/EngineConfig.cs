using PlatformAudio.Enums;
using PlatformContext;
using PlatformContext.Enums;
using PlatformWindow.Enums;

namespace PlatformEngine.Core;

/// <summary>
/// Конфигурация PlatformEngine
/// </summary>
public class EngineConfig
{
    // ---- Общие настройки ----
    public string Title { get; set; } = "Platform Engine";
    public ModuleFlags Modules { get; set; } = ModuleFlags.Default;

    // ---- Окно (только если ModuleFlags.Window) ----
    public int Width { get; set; } = 1280;
    public int Height { get; set; } = 720;
    public WindowFlags WindowFlags { get; set; } = WindowFlags.Resizable | WindowFlags.Focused;

    // ---- Графика (только если ModuleFlags.Graphics) ----
    public GraphicsApi GraphicsApi { get; set; } = GraphicsApi.OpenGL;
    public int OpenGLMajor { get; set; } = 3;
    public int OpenGLMinor { get; set; } = 3;
    public ContextProfile OpenGLProfile { get; set; } = ContextProfile.Core;
    public int SwapInterval { get; set; } = 1;

    // ---- Аудио (только если ModuleFlags.Audio) ----
    public DistanceModel AudioDistanceModel { get; set; } = DistanceModel.InverseDistanceClamped;
    public float AudioDopplerFactor { get; set; } = 1.0f;
    public float AudioSpeedOfSound { get; set; } = 343.0f;

    // ---- Ресурсы ----
    public string ResourcePath { get; set; } = "Resources/";
    public bool EnableResourceCache { get; set; } = true;

    // 🔮 БУДУЩИЕ СЕКЦИИ (можно добавлять)
    // public FontConfig Fonts { get; set; }
    // public UIConfig UI { get; set; }
}