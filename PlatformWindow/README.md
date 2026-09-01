
---

```markdown
# PlatformWindow

**Абстрактная оконная система для .NET 10**  
С поддержкой GLFW, событийной моделью и платформозависимыми хендлами.

---

## 🤖 Об этом проекте

**Важное примечание:**  
Этот проект написан **при помощи искусственного интеллекта DeepSeek** (архитектура, реализация, тестирование).  
Код является **оригинальной сборкой**, построенной на базе [PlatformNative]() — моей нативной обертки для GLFW.

**Почему я указываю это?**  
- Это честно перед сообществом.  
- ИИ — мой инструмент, как компилятор или IDE.  
- Архитектура (фабрика, интерфейсы, событийная модель) — результат совместной работы со мной и DeepSeek.

---

## 📦 Что внутри?

| Компонент | Описание |
| :--- | :--- |
| **IWindow** | Интерфейс окна с событиями (Closed, Resized, FocusGained и др.) |
| **IWindowBackend** | Интерфейс бэкенда (создание, уничтожение, обработка событий) |
| **GLFWBackend** | Реализация бэкенда через GLFW (единственная на данный момент) |
| **Window** | Реализация IWindow с полной оберткой GLFW-функций |
| **WindowFactory** | Фабрика для создания бэкендов с регистрацией через DI |
| **WindowFlags / WindowState** | Флаги и состояния окна (битовые маски) |

---

## 🚀 Быстрый старт

### 1. Установка (через ссылку на проект)

```xml
<ItemGroup>
  <ProjectReference Include="..\PlatformWindow\PlatformWindow.csproj" />
</ItemGroup>
```

### 2. Базовый пример

```csharp
using PlatformWindow;
using PlatformWindow.Enums;

// Создаем бэкенд
var backend = WindowFactory.CreateGLFW();
backend.Initialize();

// Получаем версию GLFW
backend.GetVersion(out int major, out int minor, out int rev);
Console.WriteLine($"GLFW {major}.{minor}.{rev}");

// Создаем окно с флагами
var window = backend.CreateWindow(
    title: "Мое приложение",
    width: 800,
    height: 600,
    flags: WindowFlags.Resizable | WindowFlags.Focused
);

// Подписываемся на события
window.Resized += (w, width, height) =>
    Console.WriteLine($"Размер изменен: {width}x{height}");

window.Closed += (w) =>
    Console.WriteLine("Окно закрыто");

window.FocusGained += (w) =>
    Console.WriteLine("Окно получило фокус");

window.Closing += (w) =>
{
    Console.WriteLine("Окно закрывается...");
    // Можно отменить закрытие: w.ShouldClose = false;
};

// Главный цикл
while (!window.ShouldClose)
{
    backend.PollEvents(); // Обработка событий
    
    // Ваш рендеринг здесь...
    
    // Пример: закрытие по Escape
    if (IsKeyPressed(GLFW.GLFW_KEY_ESCAPE))
    {
        window.ShouldClose = true;
    }
}

// Очистка
window.Dispose();
backend.Dispose();
```

---

## 🎯 Возможности

### Создание окна с флагами

```csharp
// Окно без рамки, всегда поверх других
var borderless = backend.CreateWindow(
    "Borderless", 800, 600,
    WindowFlags.Borderless | WindowFlags.AlwaysOnTop
);

// Прозрачное окно (с поддержкой альфа-канала в рендеринге)
var transparent = backend.CreateWindow(
    "Transparent", 800, 600,
    WindowFlags.Transparent | WindowFlags.Resizable
);

// Окно, скрытое при создании (показывается позже)
var hidden = backend.CreateWindow(
    "Hidden", 800, 600,
    WindowFlags.Hidden
);
hidden.Show(); // Показать позже
```

### Управление окном

```csharp
// Размер и позиция
window.SetSize(1024, 768);
window.SetPosition(100, 100);
window.SetSizeLimits(640, 480, 1920, 1080);
window.SetAspectRatio(16, 9);

// Состояния
window.Maximize();
window.Minimize();
window.Restore();
window.Focus();

// Прозрачность (0.0 - 1.0)
window.Opacity = 0.5f;

// Иконка (из байтового массива RGBA)
byte[] iconData = LoadIconFromFile("icon.png");
window.SetIcon(iconData, 64, 64, 4);
```

### События окна

```csharp
window.Closed += (w) => { /* Окно закрыто */ };
window.Closing += (w) => { /* Окно закрывается (можно отменить) */ };
window.Resized += (w, w, h) => { /* Размер изменен */ };
window.Moved += (w, x, y) => { /* Окно перемещено */ };
window.FocusGained += (w) => { /* Фокус получен */ };
window.FocusLost += (w) => { /* Фокус потерян */ };
window.Minimized += (w) => { /* Окно свернуто */ };
window.Maximized += (w) => { /* Окно развернуто */ };
window.Restored += (w) => { /* Окно восстановлено */ };
window.ContentScaleChanged += (w) => { /* Масштаб содержимого изменен (DPI) */ };
```

### Работа с мониторами

```csharp
// Получить первичный монитор
IntPtr primary = backend.GetPrimaryMonitor();
string? name = backend.GetMonitorName(primary);
backend.GetMonitorPos(primary, out int x, out int y);
Console.WriteLine($"Монитор: {name}, позиция: {x}, {y}");

// Получить все мониторы
foreach (IntPtr monitor in backend.GetMonitors())
{
    string? monitorName = backend.GetMonitorName(monitor);
    backend.GetMonitorPos(monitor, out int mx, out int my);
    Console.WriteLine($"Монитор: {monitorName} ({mx}, {my})");
}
```

### Буфер обмена

```csharp
// Получить текст
string? clipboard = backend.GetClipboardString();
Console.WriteLine($"Буфер обмена: {clipboard}");

// Установить текст
backend.SetClipboardString("Привет, мир!");
```

### Получение нативного хендла

```csharp
// Для Windows: HWND
IntPtr hwnd = window.GetNativeHandle();
// Для Linux: X11 Window
IntPtr x11Window = window.GetNativeHandle();
// Для macOS: NSWindow*
IntPtr nsWindow = window.GetNativeHandle();
```

---

## 🏗️ Архитектура

### Диаграмма классов

```
┌─────────────────────────────────────────────┐
│            IWindowBackend                   │
│  + Initialize()                             │
│  + CreateWindow()                           │
│  + DestroyWindow()                          │
│  + PollEvents() / WaitEvents()              │
│  + GetMonitor...()                          │
│  + GetClipboardString() / SetClipboardString│
└──────────────────┬──────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────┐
│            GLFWBackend                      │
│  - List<Window> Windows                    │
│  + Initialize() (вызывает GLFW.Initialize) │
│  + CreateWindow() (создает GLFW окно)      │
│  + Dispose() (завершает GLFW)              │
└─────────────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────┐
│               IWindow                       │
│  + Handle (IntPtr)                         │
│  + Title / Width / Height / X / Y          │
│  + IsVisible / IsFocused / State           │
│  + Show() / Hide() / Maximize() / Minimize │
│  + SetIcon() / GetNativeHandle()           │
│  + События (Closed, Resized, ...)          │
└──────────────────┬──────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────┐
│               Window                        │
│  - IntPtr GLFWHandle                       │
│  - GLFWBackend Backend                     │
│  - Колбэки (хранятся для предотвращения GC)│
│  + Реализация всех методов IWindow         │
│  + Dispose() (уничтожает GLFW окно)       │
└─────────────────────────────────────────────┘
```

### Особенности реализации

1. **Предотвращение сборки мусора**  
   Колбэки GLFW сохраняются в полях класса `Window`. Без этого GC может собрать их, и приложение упадет с `AccessViolationException`.

2. **Потокобезопасность**  
   `GLFWBackend` использует `Lock` для управления списком окон в многопоточных сценариях.

3. **Платформозависимые хендлы**  
   Метод `GetNativeHandle()` возвращает:
   - Windows: `HWND` (через `glfwGetWin32Window`)
   - Linux: X11 `Window` (через `glfwGetX11Window`)
   - macOS: `NSWindow*` (через `glfwGetCocoaWindow`)

4. **Безопасное освобождение**  
   Финайзер вызывает `Dispose(false)`, а `DisposeInternal()` используется бэкендом для прямого уничтожения окна.

---

## 🔌 Расширяемость

Хотите добавить свой бэкенд (например, SDL2, WinForms, WPF)?

```csharp
public class SDL2Backend : IWindowBackend
{
    public string Name => "SDL2";
    public bool IsInitialized { get; private set; }
    
    public void Initialize() { /* Инициализация SDL2 */ }
    public IWindow CreateWindow(string title, int width, int height, WindowFlags flags)
    {
        // Создаем окно через SDL2
        // Возвращаем реализацию IWindow
    }
    // ... остальные методы
}

// Регистрируем в фабрике
WindowFactory.Register("SDL2", () => new SDL2Backend());

// Используем
var backend = WindowFactory.Create("SDL2");
```

---

## 📚 Зависимости

- **[PlatformNative]()** — нативная обертка для GLFW, OpenAL, OpenGL
- **.NET 10** (с поддержкой NativeAOT)

---

## 🛠️ Требования к системе

| ОС | Библиотеки |
| :--- | :--- |
| **Windows** | `glfw3.dll` |
| **Linux** | `libglfw.so.3` |
| **macOS** | `libglfw.3.dylib` |

> Библиотеки должны лежать в папке с исполняемым файлом или в системном `PATH`.

---

## 🧪 Проверка на плагиат

Этот код НЕ скопирован из открытых репозиториев (Silk.NET, OpenTK, Veldrid и др.).  
Все архитектурные решения (интерфейсы, фабрика, событийная модель) — **оригинальны** и созданы в диалоге с DeepSeek.

При проверке через MOSS/Turnitin вы найдете:
- Совпадения с C-API GLFW (константы, структуры) — **это неизбежно и разрешено**.
- Совпадения с паттернами "Factory" и "Observer" — **это общепринятые практики .NET**.
- **Ни одного целого класса**, скопированного из чужого проекта.

---

## 📄 Лицензия

MIT — делайте что хотите, но с указанием авторства.

---

## 🌟 Благодарности

- **DeepSeek** — за генерацию кода, рефакторинг и объяснение P/Invoke-нюансов.
- Сообществу .NET — за документацию по `Marshal` и `NativeLibrary`.
- **GLFW** — за отличную кроссплатформенную библиотеку окон.

---

## 🤝 Контакты

Если у вас есть вопросы по архитектуре или вы нашли баг — открывайте Issue.

---

_Пишу код вместе с ИИ, а не вместо него._
```

---