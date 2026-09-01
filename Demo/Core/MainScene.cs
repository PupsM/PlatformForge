using PlatformAudio.Interfaces;
using PlatformEngine;
using PlatformEngine.Core;
using PlatformInput.Enums;
using PlatformRender;
using PlatformRender.Camera;
using PlatformRender.Core;
using PlatformRender.Graphics;
using System.Numerics;

namespace Demo.Core;

public class MainScene
{
    private readonly Mesh Cube;
    private readonly Mesh Sphere;
    private readonly ShaderProgram Shader;
    private readonly PerspectiveCamera Camera;
    private readonly CubeController Controller;
    private readonly Texture2D? Texture;
    private readonly ISoundBuffer? Sound;
    private readonly ISoundSource? SoundSource;
    private float RotationAngle = 0f;

    public MainScene()
    {
        // ---- 1. Создаем объекты ----
        Cube = Engine.CreateCube(1.5f);
        Sphere = Engine.CreateSphere(0.8f, 32);

        // ---- 2. Загружаем шейдеры ----
        Shader = Engine.LoadShader(
            "Resources/Shaders/vertex.glsl", 
            "Resources/Shaders/fragment.glsl"
            );

        // ---- 3. Загружаем текстуру ----
        try
        {
            Texture = Engine.LoadTexture("Resources/Textures/wood.png");
        }
        catch
        {
            // Текстура не найдена, используем цвет
        }

        // ---- 4. Загружаем звук ----
        try
        {
            Sound = Engine.LoadSound("Resources/Sounds/click.wav");
            SoundSource = Engine.Audio.CreateSource();
            SoundSource.BindBuffer(Sound);
            SoundSource.Gain = 1.0f;
            SoundSource.Pitch = 1.0f;
            SoundSource.Looping = false;
            SoundSource.Position = (0, 0, 0);
        }
        catch
        {
            // Звук не найден
        }

        // ---- 5. Создаем камеру с орбитой ----
        Camera = Engine.CreatePerspectiveCamera(60f, 1280f / 720f, 0.1f, 100f);
        Camera.Position = new Vector3(3, 2, 5);
        Camera.LookAt(Vector3.Zero);
        Camera.EnableOrbit(true);

        // ---- 6. Создаем контроллер для куба ----
        Controller = new CubeController(Cube);

        // ---- 7. Обновляем матрицы ----
        UpdateMatrices();
    }

    public void Update(float deltaTime)
    {
        Controller.Update(deltaTime);
        RotationAngle += deltaTime * 30f;
        Camera.Orbit(deltaTime * 15f, 0f);
    }

    public void Render()
    {
        Engine.Renderer.ClearColor = new Color(0.1f, 0.15f, 0.2f, 1.0f);
        Engine.Renderer.BeginFrame();

        Shader.Bind();

        Shader.SetUniform("uView", Engine.Renderer.ViewMatrix);
        Shader.SetUniform("uProjection", Engine.Renderer.ProjectionMatrix);
        Shader.SetUniform("uLightPos", new Vector3(5f, 5f, 5f));

        // ---- Рисуем куб ----
        var cubeMatrix = Controller.GetModelMatrix();
        Shader.SetUniform("uModel", cubeMatrix);
        Shader.SetUniform("uColor", new Vector3(0.2f, 0.6f, 1.0f));

        if (Texture is not null)
        {
            Shader.SetUniform("uTexture", Texture, 0);
            Shader.SetUniform("uUseTexture", 1);
        }
        else
        {
            Shader.SetUniform("uUseTexture", 0);
        }

        Engine.Renderer.DrawMesh(Cube, Shader);

        // ---- Рисуем сферу ----
        var sphereMatrix = Matrix4x4.CreateRotationY(RotationAngle * (float)Math.PI / 180f) *
                          Matrix4x4.CreateTranslation(new Vector3(2.5f, 0, 0));
        Shader.SetUniform("uModel", sphereMatrix);
        Shader.SetUniform("uColor", new Vector3(1.0f, 0.3f, 0.3f));
        Shader.SetUniform("uUseTexture", 0);
        Engine.Renderer.DrawMesh(Sphere, Shader);

        Engine.Renderer.EndFrame();
    }

    public void Resize(int width, int height)
    {
        Camera.Aspect = (float)width / height;
        UpdateMatrices();
    }

    public void PlaySound()
    {
        if (SoundSource is not null)
        {
            SoundSource.Rewind();
            SoundSource.Play();
        }
    }

    private void UpdateMatrices()
    {
        Engine.Renderer.ViewMatrix = Camera.ViewMatrix;
        Engine.Renderer.ProjectionMatrix = Camera.ProjectionMatrix;
    }
}