
namespace PlatformContext;

/// <summary>
/// Интерфейс графического контекста
/// </summary>
public interface IGraphicsContext : IDisposable
{
    /// <summary>Тип графического API</summary>
    GraphicsApi Api { get; }

    /// <summary>Имя контекста</summary>
    string Name { get; }

    /// <summary>Инициализирован ли контекст</summary>
    bool IsInitialized { get; }

    /// <summary>Нативный хендл контекста</summary>
    IntPtr Handle { get; }

    /// <summary>Сделать контекст текущим для окна</summary>
    void MakeCurrent(IntPtr windowHandle);

    /// <summary>Поменять буферы местами</summary>
    void SwapBuffers();

    /// <summary>Установить интервал смены буферов (VSync)</summary>
    void SetSwapInterval(int interval);

    /// <summary>Получить интервал смены буферов</summary>
    int GetSwapInterval();

    /// <summary>Получить адрес функции расширения</summary>
    IntPtr GetExtensionFunction(string name);
}