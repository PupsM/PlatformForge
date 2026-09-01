using PlatformEngine.Core;
using PlatformEngine;
using PlatformInput.Events;
using PlatformInput.Enums;

namespace Demo.Core;

public class GameApp
{
    private readonly MainScene Scene;

    public GameApp()
    {
        Engine.Initialize(
            title: "3D Cube Demo",
            width: 1280,
            height: 720,
            modules: ModuleFlags.Default | ModuleFlags.Audio
        );

        Scene = new MainScene();

        Engine.OnUpdate += OnUpdate;
        Engine.OnRender += OnRender;
        Engine.OnResize += OnResize;

        // Подписываемся на события мыши
        Engine.Input.MouseDown += OnMouseDown;
    }

    public static void Run()
        => Engine.Run();

    private void OnUpdate(object? sender, FrameEventArgs e)
        => Scene.Update(e.DeltaTime);

    private void OnRender(object? sender, EventArgs e)
        => Scene.Render();

    private void OnResize(object? sender, ResizeEventArgs e)
        => Scene.Resize(e.Width, e.Height);

    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        // Воспроизводим звук при клике левой кнопкой
        if (e.Button == MouseButton.Right)
        {
            Scene.PlaySound();
        }
    }
}