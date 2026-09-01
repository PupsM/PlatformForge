namespace PlatformEngine.Core;

/// <summary>
/// Флаги модулей для подключения
/// </summary>
[Flags]
public enum ModuleFlags
{
    /// <summary>Без модулей (только ядро)</summary>
    None = 0,

    /// <summary>Оконная система</summary>
    Window = 1 << 0,

    /// <summary>Система ввода</summary>
    Input = 1 << 1,

    /// <summary>Графика (Context + Render)</summary>
    Graphics = 1 << 2,

    /// <summary>Аудио</summary>
    Audio = 1 << 3,

    /// <summary>Загрузка изображений</summary>
    Image = 1 << 4,

    // 🔮 БУДУЩИЕ МОДУЛИ (можно добавлять)
    // Fonts = 1 << 5,
    // UI = 1 << 6,
    // Physics = 1 << 7,
    // Networking = 1 << 8,

    /// <summary>Консольное приложение (окно + ввод)</summary>
    Console = Window | Input,

    /// <summary>Стандартный набор (окно + ввод + графика + изображения)</summary>
    Default = Window | Input | Graphics | Image,

    /// <summary>Полный набор (все модули)</summary>
    Full = Window | Input | Graphics | Audio | Image,
}