using PlatformWindow.Enums;

namespace PlatformWindow;

/// <summary>
/// Интерфейс бэкенда оконной системы
/// </summary>
public interface IWindowBackend : IDisposable
{
    /// <summary>Имя бэкенда</summary>
    string Name { get; }

    /// <summary>Инициализирован ли бэкенд</summary>
    bool IsInitialized { get; }

    /// <summary>Инициализация бэкенда</summary>
    void Initialize();

    /// <summary>Создать окно</summary>
    IWindow CreateWindow(string title, int width, int height, WindowFlags flags = WindowFlags.Default);

    /// <summary>Уничтожить окно</summary>
    void DestroyWindow(IWindow window);

    /// <summary>Обработать события</summary>
    void PollEvents();

    /// <summary>Ожидать события</summary>
    void WaitEvents();

    /// <summary>Ожидать события с таймаутом</summary>
    void WaitEventsTimeout(double timeout);

    /// <summary>Получить версию бэкенда</summary>
    void GetVersion(out int major, out int minor, out int rev);

    /// <summary>Получить первичный монитор</summary>
    IntPtr GetPrimaryMonitor();

    /// <summary>Получить все мониторы</summary>
    IEnumerable<IntPtr> GetMonitors();

    /// <summary>Получить имя монитора</summary>
    string? GetMonitorName(IntPtr monitor);

    /// <summary>Получить позицию монитора</summary>
    void GetMonitorPos(IntPtr monitor, out int x, out int y);

    /// <summary>Получить текст из буфера обмена</summary>
    string? GetClipboardString();

    /// <summary>Установить текст в буфер обмена</summary>
    void SetClipboardString(string text);

    /// <summary>Получить адрес функции</summary>
    IntPtr GetProcAddress(string name);
}