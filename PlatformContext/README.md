
---

```markdown
# PlatformContext

**Абстрактная система графических контекстов для .NET 10**  
С поддержкой OpenGL, профилей (Core/Compatibility/ES) и расширяемой архитектурой.

---

## 🤖 Об этом проекте

**Важное примечание:**  
Этот проект написан **при помощи искусственного интеллекта DeepSeek** (архитектура, реализация, тестирование).  
Код является **оригинальной сборкой**, построенной на базе [PlatformNative]() и [PlatformWindow]().

**Почему я указываю это?**  
- Это честно перед сообществом.  
- ИИ — мой инструмент, как компилятор или IDE.  
- Архитектура (фабрика контекстов, поддержка профилей, разделение окна и контекста) — результат совместной работы со мной и DeepSeek.

---

## 📦 Что внутри?

| Компонент | Описание |
| :--- | :--- |
| **IGraphicsContext** | Интерфейс графического контекста (MakeCurrent, SwapBuffers, VSync, расширения) |
| **OpenGLContext** | Реализация OpenGL контекста с поддержкой Core/Compatibility/ES профилей |
| **GraphicsFactory** | Фабрика для создания графических контекстов с регистрацией |
| **GraphicsApi** | Enum типов графических API (OpenGL, Vulkan, DirectX, Metal) |
| **ContextProfile** | Enum профилей OpenGL (Core, Compatibility, ES) |

---

## 🚀 Быстрый старт

### 1. Установка (через ссылку на проект)

```xml
<ItemGroup>
  <ProjectReference Include="..\PlatformContext\PlatformContext.csproj" />
</ItemGroup>
```

### 2. Базовый пример

```csharp
using PlatformContext;
using PlatformContext.Enums;
using PlatformWindow;

// Создаем окно через PlatformWindow
var backend = WindowFactory.CreateGLFW();
backend.Initialize();
var window = backend.CreateWindow("Graphics Demo", 800, 600);

// Создаем OpenGL контекст (Core Profile 3.3)
var context = GraphicsFactory.CreateOpenGL(3, 3, ContextProfile.Core);

// Привязываем контекст к окну
context.MakeCurrent(window.Handle);

// Включаем VSync
context.SetSwapInterval(1);

// Получаем информацию о контексте
Console.WriteLine($"Контекст: {context.Name}");
Console.WriteLine($"API: {context.Api}");
Console.WriteLine($"Инициализирован: {context.IsInitialized}");
Console.WriteLine($"Нативный хендл: {context.Handle:X8}");

// Получаем адрес функции расширения
IntPtr glDebugProc = context.GetExtensionFunction("glDebugMessageCallback");

// Главный цикл рендеринга
while (!window.ShouldClose)
{
    // Очищаем экран (через PlatformNative.OpenGL)
    OpenGL.ClearColor(0.2f, 0.3f, 0.8f, 1.0f);
    OpenGL.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
    
    // Ваш рендеринг здесь...
    
    // Меняем буферы местами
    context.SwapBuffers();
    
    // Обрабатываем события
    backend.PollEvents();
}

// Очистка
context.Dispose();
window.Dispose();
backend.Dispose();
```

---

## 🎯 Возможности

### Создание OpenGL контекстов

```csharp
// Универсальный метод (с параметрами по умолчанию 3.3 Core)
var context1 = GraphicsFactory.CreateOpenGL();

// Core Profile 3.3 (рекомендуемый минимум)
var context33 = GraphicsFactory.CreateOpenGL33Core();

// Core Profile 4.6 (для HDR и современных фич)
var context46 = GraphicsFactory.CreateOpenGL46Core();

// Произвольная версия Core Profile
var contextCustom = GraphicsFactory.CreateOpenGLCore(4, 5);

// Compatibility Profile (для старого кода)
var compatContext = GraphicsFactory.CreateOpenGLCompat(3, 3);

// OpenGL ES (для мобильных устройств)
var esContext = GraphicsFactory.CreateOpenGLES(2, 0);
```

### Использование фабрики по имени

```csharp
// Регистрируем свои контексты
GraphicsFactory.Register("MyCustomGL", () => new OpenGLContext(4, 6, ContextProfile.Core));

// Создаем по имени
var context = GraphicsFactory.Create("MyCustomGL");

// Проверяем регистрацию
if (GraphicsFactory.IsRegistered("OpenGL46"))
{
    var ctx = GraphicsFactory.Create("OpenGL46");
}

// Получаем список зарегистрированных бэкендов
foreach (string name in GraphicsFactory.GetRegisteredNames())
{
    Console.WriteLine($"Доступен: {name}");
}
```

### Управление контекстом

```csharp
// Привязка к окну
context.MakeCurrent(window.Handle);

// Проверка инициализации
if (context.IsInitialized)
{
    Console.WriteLine($"Контекст активен: {context.Name}");
}

// Получение нативного хендла (HGLRC/GLXContext/NSOpenGLContext)
IntPtr nativeHandle = context.Handle;

// Смена буферов
context.SwapBuffers();

// VSync (0 = выключен, 1 = включен, >1 = каждый N-й кадр)
context.SetSwapInterval(1);
int interval = context.GetSwapInterval();

// Получение адреса функции расширения
IntPtr glCreateShaderPtr = context.GetExtensionFunction("glCreateShader");
```

### Несколько окон с разными контекстами

```csharp
// Окно 1: редактирование (OpenGL 4.6 Core)
var window1 = WindowFactory.CreateGLFW();
window1.Initialize();
var mainWindow = window1.CreateWindow("Main", 1920, 1080, WindowFlags.Resizable);

var context1 = GraphicsFactory.CreateOpenGL46Core();
context1.MakeCurrent(mainWindow.Handle);

// Окно 2: отладка (OpenGL 3.3 Compatibility, для старого кода)
var window2 = WindowFactory.CreateGLFW();
window2.Initialize();
var debugWindow = window2.CreateWindow("Debug", 800, 600, WindowFlags.AlwaysOnTop);

var context2 = GraphicsFactory.CreateOpenGL(3, 3, ContextProfile.Compatibility);
context2.MakeCurrent(debugWindow.Handle);

// В цикле переключаемся между контекстами
while (!mainWindow.ShouldClose && !debugWindow.ShouldClose)
{
    // Рендерим в главное окно
    context1.MakeCurrent(mainWindow.Handle);
    // ... рендеринг ...
    context1.SwapBuffers();
    
    // Рендерим в окно отладки
    context2.MakeCurrent(debugWindow.Handle);
    // ... рендеринг ...
    context2.SwapBuffers();
    
    window1.PollEvents();
    window2.PollEvents();
}
```

---

## 🏗️ Архитектура

### Диаграмма классов

```
┌─────────────────────────────────────────────────────────────┐
│                    IGraphicsContext                         │
│  + Api : GraphicsApi                                       │
│  + Name : string                                           │
│  + IsInitialized : bool                                    │
│  + Handle : IntPtr                                         │
│  + MakeCurrent(IntPtr)                                     │
│  + SwapBuffers()                                           │
│  + SetSwapInterval(int)                                    │
│  + GetSwapInterval() : int                                 │
│  + GetExtensionFunction(string) : IntPtr                   │
└──────────────────────────┬─────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                    OpenGLContext                            │
│  - IntPtr Window                                           │
│  - int SwapInterval                                        │
│  - bool Initialized                                        │
│  - bool Disposed                                           │
│  + Major : int                                             │
│  + Minor : int                                             │
│  + Profile : ContextProfile                                │
│  + Конструктор(int major, int minor, ContextProfile)       │
│  + Реализация всех методов IGraphicsContext                │
│  + Dispose()                                               │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                    GraphicsFactory                          │
│  + Register(string, Func<IGraphicsContext>)                │
│  + Create(string) : IGraphicsContext?                      │
│  + CreateOpenGL(...) : IGraphicsContext                    │
│  + CreateOpenGLCore(int, int) : IGraphicsContext           │
│  + CreateOpenGLCompat(int, int) : IGraphicsContext         │
│  + CreateOpenGLES(int, int) : IGraphicsContext             │
│  + CreateOpenGL33Core() : IGraphicsContext                 │
│  + CreateOpenGL46Core() : IGraphicsContext                 │
│  + IsRegistered(string) : bool                             │
│  + GetRegisteredNames() : IEnumerable<string>              │
└─────────────────────────────────────────────────────────────┘
```

### Особенности реализации

1. **Разделение окна и контекста**  
   В отличие от OpenTK/Silk.NET, где окно и контекст тесно связаны, здесь они разделены. Один контекст можно использовать с несколькими окнами или переключаться между ними.

2. **Поддержка профилей OpenGL**  
   - **Core Profile** — современный OpenGL (рекомендуется)
   - **Compatibility Profile** — с поддержкой устаревших функций
   - **ES** — OpenGL для встраиваемых/мобильных систем

3. **Primary Constructor (C# 12)**  
   Конструктор `OpenGLContext` использует современный синтаксис C# 12, делая код более компактным и читаемым.

4. **Проверка валидности окна**  
   При вызове `MakeCurrent` проверяется, что переданный хендл действительно принадлежит GLFW. Это предотвращает краши при случайной передаче левого хендла.

5. **Нативный хендл контекста**  
   Свойство `Handle` возвращает реальный хендл контекста (HGLRC/GLXContext/NSOpenGLContext) через `glfwGetCurrentContext`.

6. **Безопасное освобождение**  
   При `Dispose` контекст отключается от текущего окна (`glfwMakeContextCurrent(IntPtr.Zero)`), предотвращая вызовы на уже удаленном объекте.

---

## 🔌 Расширяемость

Хотите добавить свой графический API (например, Vulkan, DirectX или Metal)?

```csharp
public class VulkanContext : IGraphicsContext
{
    public GraphicsApi Api => GraphicsApi.Vulkan;
    public string Name => "Vulkan 1.3";
    public bool IsInitialized { get; private set; }
    public IntPtr Handle { get; private set; }
    
    public void MakeCurrent(IntPtr windowHandle)
    {
        // Создаем Vulkan поверхность для окна
        // ...
        IsInitialized = true;
    }
    
    public void SwapBuffers()
    {
        // Vulkan использует Present вместо SwapBuffers
        // ...
    }
    
    public void SetSwapInterval(int interval)
    {
        // Vulkan использует VK_EXT_present_mode
        // ...
    }
    
    public int GetSwapInterval() => 1;
    
    public IntPtr GetExtensionFunction(string name)
    {
        // vkGetInstanceProcAddr / vkGetDeviceProcAddr
        // ...
    }
    
    public void Dispose() { /* Очистка */ }
}

// Регистрируем в фабрике
GraphicsFactory.Register("Vulkan", () => new VulkanContext());

// Используем
var context = GraphicsFactory.Create("Vulkan");
```

---

## 📚 Зависимости

- **[PlatformNative]()** — нативная обертка для GLFW и OpenGL
- **[PlatformWindow]()** — оконная система (передает хендл окна)
- **.NET 10**

---

## 🛠️ Требования к системе

| ОС | Библиотеки |
| :--- | :--- |
| **Windows** | `glfw3.dll`, поддержка OpenGL 3.3+ |
| **Linux** | `libglfw.so.3`, поддержка OpenGL 3.3+ |
| **macOS** | `libglfw.3.dylib`, поддержка OpenGL 3.3+ |

---

## 🔒 Ограничения

На данный момент `PlatformContext` **работает только с окнами GLFW** (через `PlatformWindow`). Это связано с тем, что реализация использует GLFW-функции для управления контекстом.

**В будущем планируется:**
- Поддержка Vulkan через `VulkanContext`
- Поддержка DirectX через `DirectXContext`
- Поддержка Metal через `MetalContext`

---

## 🧪 Проверка на плагиат

Этот код НЕ скопирован из открытых репозиториев (Silk.NET, OpenTK, Veldrid и др.).  
Все архитектурные решения (разделение окна и контекста, фабрика, проверка хендлов) — **оригинальны** и созданы в диалоге с DeepSeek.

При проверке через MOSS/Turnitin вы найдете:
- Совпадения с терминологией OpenGL (Core/Compatibility/ES) — **это неизбежно и разрешено**.
- Совпадения с паттернами "Factory" и "Strategy" — **это общепринятые практики .NET**.
- **Ни одного целого класса**, скопированного из чужого проекта.

---

## 📄 Лицензия

MIT — делайте что хотите, но с указанием авторства.

---

## 🌟 Благодарности

- **DeepSeek** — за генерацию кода, рефакторинг и объяснение P/Invoke-нюансов.
- Сообществу .NET — за документацию по `IDisposable` и `ObjectDisposedException`.
- **GLFW** — за отличную кроссплатформенную библиотеку окон и контекстов.

---

## 🤝 Контакты

Если у вас есть вопросы по архитектуре или вы нашли баг — открывайте Issue.

---

_Пишу код вместе с ИИ, а не вместо него._
```

---