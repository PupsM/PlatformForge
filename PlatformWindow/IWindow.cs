using PlatformWindow.Enums;

namespace PlatformWindow;

/// <summary>
/// Интерфейс окна
/// </summary>
public interface IWindow : IDisposable
{
    #region ---- Свойства ----

    /// <summary>Нативный хендл окна (GLFWwindow*)</summary>
    IntPtr Handle { get; }

    /// <summary>Заголовок окна</summary>
    string Title { get; set; }

    /// <summary>Ширина окна</summary>
    int Width { get; }

    /// <summary>Высота окна</summary>
    int Height { get; }

    /// <summary>Позиция X</summary>
    int X { get; }

    /// <summary>Позиция Y</summary>
    int Y { get; }

    /// <summary>Видимость окна</summary>
    bool IsVisible { get; set; }

    /// <summary>Фокус ввода</summary>
    bool IsFocused { get; }

    /// <summary>Свёрнуто ли окно</summary>
    bool IsMinimized { get; }

    /// <summary>Развёрнуто ли окно</summary>
    bool IsMaximized { get; }

    /// <summary>Состояние окна</summary>
    WindowState State { get; }

    /// <summary>Прозрачность окна (0.0 - 1.0)</summary>
    float Opacity { get; set; }

    /// <summary>Флаг закрытия окна</summary>
    bool ShouldClose { get; set; }

    #endregion

    #region ---- Методы ----

    /// <summary>Показать окно</summary>
    void Show();

    /// <summary>Скрыть окно</summary>
    void Hide();

    /// <summary>Развернуть окно</summary>
    void Maximize();

    /// <summary>Свернуть окно</summary>
    void Minimize();

    /// <summary>Восстановить нормальное состояние</summary>
    void Restore();

    /// <summary>Установить размер окна</summary>
    void SetSize(int width, int height);

    /// <summary>Установить позицию окна</summary>
    void SetPosition(int x, int y);

    /// <summary>Установить ограничения размера</summary>
    void SetSizeLimits(int minWidth, int minHeight, int maxWidth, int maxHeight);

    /// <summary>Установить соотношение сторон</summary>
    void SetAspectRatio(int numer, int denom);

    /// <summary>Установить фокус на окно</summary>
    void Focus();

    /// <summary>Запросить внимание (мигание в панели задач)</summary>
    void RequestAttention();

    /// <summary>Установить иконку окна</summary>
    void SetIcon(byte[] pixels, int width, int height, int channels = 4);

    /// <summary>Получить нативный хендл (HWND, X11 Window, NSWindow)</summary>
    IntPtr GetNativeHandle();

    /// <summary>Обработать события окна</summary>
    void PollEvents();

    #endregion

    #region ---- События ----

    /// <summary>Окно закрыто</summary>
    event Action<IWindow>? Closed;

    /// <summary>Окно изменено в размере</summary>
    event Action<IWindow, int, int>? Resized;

    /// <summary>Окно перемещено</summary>
    event Action<IWindow, int, int>? Moved;

    /// <summary>Окно получило фокус</summary>
    event Action<IWindow>? FocusGained;

    /// <summary>Окно потеряло фокус</summary>
    event Action<IWindow>? FocusLost;

    /// <summary>Окно свёрнуто</summary>
    event Action<IWindow>? Minimized;

    /// <summary>Окно развёрнуто</summary>
    event Action<IWindow>? Maximized;

    /// <summary>Окно восстановлено</summary>
    event Action<IWindow>? Restored;

    /// <summary>Окно закрывается</summary>
    event Action<IWindow>? Closing;

    /// <summary>Изменён масштаб содержимого</summary>
    event Action<IWindow>? ContentScaleChanged;

    #endregion
}