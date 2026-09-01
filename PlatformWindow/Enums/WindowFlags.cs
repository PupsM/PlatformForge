namespace PlatformWindow.Enums;

/// <summary>
/// Флаги для создания окна
/// </summary>
[Flags]
public enum WindowFlags
{
    /// <summary>Обычное окно</summary>
    Default = 0,

    /// <summary>Окно можно изменять в размерах</summary>
    Resizable = 1 << 0,

    /// <summary>Окно без рамки (без заголовка и кнопок)</summary>
    Borderless = 1 << 1,

    /// <summary>Окно скрыто при создании</summary>
    Hidden = 1 << 2,

    /// <summary>Окно развёрнуто на весь экран</summary>
    Maximized = 1 << 3,

    /// <summary>Окно свёрнуто в панель задач</summary>
    Minimized = 1 << 4,

    /// <summary>Окно всегда поверх других окон</summary>
    AlwaysOnTop = 1 << 5,

    /// <summary>Прозрачный фреймбуфер (для прозрачности)</summary>
    Transparent = 1 << 6,

    /// <summary>Окно с фокусом ввода</summary>
    Focused = 1 << 7,
}