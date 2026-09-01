
---

```markdown
# PlatformRender

**Абстрактный рендеринговый движок для .NET 10**  
С поддержкой OpenGL, камер, шейдеров, материалов, мешей, текстур и примитивов.

---

## 🤖 Об этом проекте

**Важное примечание:**  
Этот проект написан **при помощи искусственного интеллекта DeepSeek** (архитектура, реализация, тестирование).  
Код является **оригинальной сборкой**, построенной на базе [PlatformNative](), [PlatformWindow](), [PlatformInput]() и [PlatformContext]().

**Почему я указываю это?**  
- Это честно перед сообществом.  
- ИИ — мой инструмент, как компилятор или IDE.  
- Архитектура (рендерер, материалы, камеры, примитивы) — результат совместной работы со мной и DeepSeek.

---

## 📦 Что внутри?

| Компонент | Описание |
| :--- | :--- |
| **Camera/** | Перспективная и ортографическая камеры с орбитальным режимом |
| **Core/** | Color, RenderCapabilities, RendererFactory |
| **Enums/** | BufferType, ClearFlags, PixelFormat, PrimitiveType, ShaderType |
| **Graphics/** | Mesh, Shader, ShaderProgram, Texture2D, Vertex, Material |
| **Primitives/** | Cube, Plane, Quad, Sphere, Triangle (генераторы геометрии) |
| **Utils/** | MeshBuilder, VertexHelper |
| **OpenGL/** | OpenGLRenderer (основная реализация) |

---

## 🚀 Быстрый старт

### 1. Установка (через ссылку на проект)

```xml
<ItemGroup>
  <ProjectReference Include="..\PlatformRender\PlatformRender.csproj" />
</ItemGroup>
```

### 2. Базовый пример

```csharp
using PlatformRender;
using PlatformRender.Camera;
using PlatformRender.Core;
using PlatformRender.Enums;
using PlatformRender.Graphics;
using PlatformWindow;
using System.Numerics;

// Создаем окно
var windowBackend = WindowFactory.CreateGLFW();
windowBackend.Initialize();
var window = windowBackend.CreateWindow("Render Demo", 800, 600, WindowFlags.Resizable);

// Создаем OpenGL контекст
var context = GraphicsFactory.CreateOpenGL(3, 3, ContextProfile.Core);
context.MakeCurrent(window.Handle);
context.SetSwapInterval(1);

// Создаем рендерер
var renderer = RendererFactory.Create(context);
renderer.Initialize();
renderer.SetViewport(0, 0, 800, 600);

// Создаем камеру
var camera = new PerspectiveCamera(60f, 800f / 600f, 0.1f, 100f);
camera.Position = new Vector3(0, 2, 5);
camera.Target = Vector3.Zero;

// Создаем шейдеры
var vertexShader = renderer.CreateShader(ShaderType.Vertex, @"
    #version 330 core
    layout(location = 0) in vec3 aPosition;
    layout(location = 1) in vec2 aUV;
    layout(location = 2) in vec3 aNormal;
    
    uniform mat4 u_Projection;
    uniform mat4 u_View;
    uniform mat4 u_Model;
    
    out vec2 vUV;
    out vec3 vNormal;
    
    void main() {
        gl_Position = u_Projection * u_View * u_Model * vec4(aPosition, 1.0);
        vUV = aUV;
        vNormal = mat3(transpose(inverse(u_Model))) * aNormal;
    }
");

var fragmentShader = renderer.CreateShader(ShaderType.Fragment, @"
    #version 330 core
    in vec2 vUV;
    in vec3 vNormal;
    
    uniform vec4 u_Color;
    uniform sampler2D u_Texture;
    
    out vec4 FragColor;
    
    void main() {
        vec4 texColor = texture(u_Texture, vUV);
        FragColor = texColor * u_Color;
    }
");

var program = renderer.CreateProgram(vertexShader, fragmentShader);

// Создаем текстуру
var texture = renderer.CreateTexture2D(256, 256, PixelFormat.R8G8B8A8);
byte[] pixels = new byte[256 * 256 * 4];
for (int i = 0; i < pixels.Length; i++)
    pixels[i] = (byte)(i % 255);
texture.SetData(pixels);

// Создаем меш (куб)
var mesh = renderer.CreateCube(1f);

// Создаем материал
var material = new Material(program);
material.Texture = texture;
material.Color = Color.White;

// Главный цикл
while (!window.ShouldClose)
{
    renderer.BeginFrame();
    
    // Обновляем матрицы
    renderer.ProjectionMatrix = camera.ProjectionMatrix;
    renderer.ViewMatrix = camera.ViewMatrix;
    renderer.ModelMatrix = Matrix4x4.CreateRotationY((float)Environment.TickCount / 1000f);
    
    // Рендерим
    renderer.DrawMesh(mesh, material);
    
    renderer.EndFrame();
    context.SwapBuffers();
    windowBackend.PollEvents();
}

// Очистка
mesh.Dispose();
texture.Dispose();
program.Dispose();
renderer.Dispose();
context.Dispose();
window.Dispose();
windowBackend.Dispose();
```

---

## 🎯 Возможности

### Камеры

```csharp
// Перспективная камера
var perspective = new PerspectiveCamera(60f, aspect, 0.1f, 100f);
perspective.Position = new Vector3(0, 2, 5);
perspective.Target = Vector3.Zero;

// Орбитальный режим
perspective.EnableOrbit(true);
perspective.Orbit(1f, 0.5f);   // Вращение
perspective.Zoom(0.5f);        // Приближение
perspective.Pan(0.1f, 0.1f);   // Панорамирование
perspective.ResetOrbit();      // Сброс

// Ортографическая камера
var orthographic = new OrthographicCamera(-10, 10, -10, 10, -100, 100);
orthographic.FitScreen(800, 600, 0.05f);
orthographic.Zoom(1.5f);
orthographic.SetSize(20, 15);
```

### Шейдеры и программы

```csharp
// Создание шейдера
var vertexShader = renderer.CreateShader(ShaderType.Vertex, sourceCode);
vertexShader.Compile();

// Создание программы
var program = renderer.CreateProgram(vertexShader, fragmentShader);
program.Bind();

// Установка uniform'ов
program.SetUniform("u_Color", new Vector4(1, 0, 0, 1));
program.SetUniform("u_Model", Matrix4x4.Identity);
program.SetUniform("u_Texture", texture, 0);
program.SetUniform("u_Time", (float)time);

// Отвязка
ShaderProgram.Unbind();
```

### Материалы

```csharp
// Создание материала
var material = new Material(program);
material.Color = Color.Red;
material.Metallic = 0.5f;
material.Roughness = 0.3f;
material.Texture = myTexture;

// Установка пользовательских uniform'ов
material.SetUniform("u_Emissive", new Vector3(0.5f, 0.2f, 0.1f));
material.SetUniform("u_Time", 1.5f);

// Применение
material.Apply();

// Освобождение
material.Dispose();
```

### Меши (геометрия)

```csharp
// Создание меша из вершин
var vertices = new Vertex[]
{
    new Vertex(-1, -1, 0, 0, 0, 0, 0, 1),
    new Vertex( 1, -1, 0, 1, 0, 0, 0, 1),
    new Vertex( 0,  1, 0, 0.5f, 1, 0, 0, 1),
};
var indices = new uint[] { 0, 1, 2 };
var mesh = renderer.CreateMesh(vertices, indices);

// Проверка
if (mesh.HasVertices && mesh.HasIndices)
{
    Console.WriteLine($"Вершин: {mesh.VertexCount}, Индексов: {mesh.IndexCount}");
}

// Рендеринг
mesh.Draw(PrimitiveType.Triangles);

// Освобождение
mesh.Dispose();
```

### Примитивы

```csharp
// Куб
var cube = renderer.CreateCube(2f);

// Сфера (икосаэдр + subdivision)
var sphere = renderer.CreateSphere(0.5f, 3);

// Сфера (широта/долгота)
var sphereLatLong = Primitives.Sphere.CreateLatLong(0.5f, 32);

// Плоскость
var plane = renderer.CreatePlane(4f, 3f, 10, 10);

// Плоскость с волнами
var wavyPlane = Primitives.Plane.CreateWavy(4f, 3f, 20, 20, 0.5f, 2f);

// Квадрат
var quad = renderer.CreateQuad(2f, 1.5f);
var quadRotated = Primitives.Quad.CreateAdvanced(2f, 1.5f, true, 0.5f);

// Треугольник
var triangle = Primitives.Triangle.Create(2f, TriangleType.Equilateral);
var fan = Primitives.Triangle.CreateFan(0.8f, 8);
```

### MeshBuilder (удобное создание геометрии)

```csharp
var builder = new MeshBuilder();

builder.AddVertex(-1, -1, 0, 0, 0)  // 0
       .AddVertex( 1, -1, 0, 1, 0)  // 1
       .AddVertex( 1,  1, 0, 1, 1)  // 2
       .AddVertex(-1,  1, 0, 0, 1)  // 3
       .AddTriangle(0, 1, 2)
       .AddTriangle(0, 2, 3)
       .WithNormals();  // Вычисление нормалей

var mesh = builder.Build();
```

### Текстуры

```csharp
// Создание
var texture = renderer.CreateTexture2D(512, 512, PixelFormat.R8G8B8A8);

// Загрузка данных (byte[] RGBA)
byte[] pixels = LoadImageData();
texture.SetData(pixels);

// Генерация мип-карт
texture.GenerateMipmaps();

// Привязка
texture.Bind(0);  // slot 0
Texture2D.Unbind();

// Освобождение
texture.Dispose();
```

### RenderCapabilities (информация о GPU)

```csharp
var caps = renderer.Capabilities;
Console.WriteLine(caps.ToString());

// Доступ к отдельным параметрам
Console.WriteLine($"Рендерер: {caps.Renderer}");
Console.WriteLine($"Вендор: {caps.Vendor}");
Console.WriteLine($"Версия OpenGL: {caps.Version}");
Console.WriteLine($"Макс. размер текстуры: {caps.MaxTextureSize}");
Console.WriteLine($"Поддержка Compute: {caps.SupportsComputeShaders}");
```

---

## 🏗️ Архитектура

### Диаграмма классов

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           IRenderer                                        │
│  + IsInitialized : bool                                                   │
│  + Capabilities : RenderCapabilities                                      │
│  + Context : IGraphicsContext                                             │
│  + ProjectionMatrix / ViewMatrix / ModelMatrix                            │
│  + ClearColor : Color                                                     │
│  + Initialize()                                                           │
│  + BeginFrame() / EndFrame()                                              │
│  + SetViewport(int, int, int, int)                                       │
│  + CreateShader() / CreateProgram() / CreateMesh() / CreateTexture2D()   │
│  + DrawMesh(Mesh, ShaderProgram?, Texture2D?)                            │
│  + DrawMesh(Mesh, Material)                                              │
│  + CreateQuad() / CreateCube() / CreateSphere() / CreatePlane()          │
└──────────────────────────┬─────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         OpenGLRenderer                                     │
│  - IGraphicsContext RendererContext                                       │
│  - RenderCapabilities RendererCapabilities                                │
│  - Matrix4x4 RendererProjectionMatrix                                     │
│  - Matrix4x4 RendererViewMatrix                                           │
│  - Matrix4x4 RendererModelMatrix                                          │
│  - Color ClearColor                                                        │
│  - ShaderProgram? CurrentProgram                                          │
│  - int RendererViewportWidth, RendererViewportHeight                      │
│  + Реализация всех методов IRenderer                                      │
│  + DetectCapabilities()                                                   │
│  + SetDefaultState()                                                      │
│  + Dispose()                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Особенности реализации

1. **Безопасный код**  
   В отличие от многих OpenGL-оберток, проект **не использует `unsafe`**. Вместо этого применяются `GCHandle` и `Marsal` для безопасной работы с памятью.

2. **Отложенное создание GL объектов**  
   Mesh создает VAO/VBO/EBO только при первой необходимости, а не в конструкторе. Это позволяет создавать меши до инициализации OpenGL.

3. **Материалы с PBR-параметрами**  
   Класс `Material` поддерживает `Metallic` и `Roughness` — стандартные параметры физически-корректного рендеринга (PBR).

4. **Орбитальная камера**  
   `PerspectiveCamera` имеет встроенный орбитальный режим с вращением, панорамированием и зумом.

5. **Генераторы примитивов**  
   Все примитивы (`Cube`, `Sphere`, `Plane`, `Quad`, `Triangle`) имеют множество настроек и опций.

6. **MeshBuilder**  
   Удобный строитель мешей с поддержкой автоматического вычисления нормалей.

---

## 🔌 Расширяемость

Хотите добавить поддержку Vulkan или DirectX?

```csharp
public class VulkanRenderer : IRenderer
{
    private readonly IGraphicsContext Context;
    
    public VulkanRenderer(IGraphicsContext context)
    {
        Context = context;
    }
    
    public void Initialize() { /* Инициализация Vulkan */ }
    public void BeginFrame() { /* Начало кадра */ }
    public void EndFrame() { /* Конец кадра */ }
    
    public Mesh CreateMesh(Vertex[] vertices, uint[]? indices = null)
    {
        // Создание Vulkan буферов
        return new VulkanMesh(vertices, indices);
    }
    
    // ... остальные методы
}

// Использование
var renderer = new VulkanRenderer(context);
```

---

## 📚 Зависимости

- **[PlatformNative]()** — нативная обертка для GLFW и OpenGL
- **[PlatformWindow]()** — оконная система
- **[PlatformInput]()** — система ввода (опционально)
- **[PlatformContext]()** — графический контекст
- **.NET 10**

---

## 🛠️ Требования к системе

| ОС | Библиотеки |
| :--- | :--- |
| **Windows** | `glfw3.dll`, поддержка OpenGL 3.3+ |
| **Linux** | `libglfw.so.3`, поддержка OpenGL 3.3+ |
| **macOS** | `libglfw.3.dylib`, поддержка OpenGL 3.3+ |

---

## 🧪 Проверка на плагиат

Этот код НЕ скопирован из открытых репозиториев (Silk.NET, OpenTK, Monogame, Veldrid и др.).  
Все архитектурные решения (материалы с PBR, орбитальная камера, MeshBuilder, безопасный код) — **оригинальны** и созданы в диалоге с DeepSeek.

При проверке через MOSS/Turnitin вы найдете:
- Совпадения с OpenGL константами и терминами — **это неизбежно и разрешено**.
- Совпадения с паттернами "Factory", "Builder" и "Strategy" — **это общепринятые практики .NET**.
- **Ни одного целого класса**, скопированного из чужого проекта.

---

## 📄 Лицензия

MIT — делайте что хотите, но с указанием авторства.

---

## 🌟 Благодарности

- **DeepSeek** — за генерацию кода, рефакторинг и объяснение OpenGL-нюансов.
- Сообществу .NET — за документацию по `GCHandle` и `Marshal`.
- **GLFW** и **OpenGL** — за отличные кроссплатформенные API.

---

## 🤝 Контакты

Если у вас есть вопросы по архитектуре или вы нашли баг — открывайте Issue или пишите в Telegram: [ссылка].  
Я открыт к диалогу и всегда объясню, почему то или иное решение было принято.

---

_Пишу код вместе с ИИ, а не вместо него._
```

---