namespace PlatformImage.Core;

/// <summary>
/// Простая диагностика для ImageLoader
/// </summary>
public static class Diagnostics
{
    private static readonly Lock Lock = new();
    private static Action<string>? LogHandler;
    private static LogLevel DiagnosticsCurrentLevel = LogLevel.Info;

    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Debug = 4
    }

    /// <summary>
    /// Уровень логирования
    /// </summary>
    public static LogLevel CurrentLevel
    {
        get => DiagnosticsCurrentLevel;
        set => DiagnosticsCurrentLevel = value;
    }

    /// <summary>
    /// Обработчик логов (для интеграции с внешней системой)
    /// </summary>
    public static event Action<string>? OnLog
    {
        add
        {
            lock (Lock)
            {
                LogHandler += value;
            }
        }
        remove
        {
            lock (Lock)
            {
                LogHandler -= value;
            }
        }
    }

    private static void Log(LogLevel level, string message)
    {
        if (level > DiagnosticsCurrentLevel) return;

        string prefix = level switch
        {
            LogLevel.Error => "[ERROR]",
            LogLevel.Warning => "[WARN] ",
            LogLevel.Info => "[INFO] ",
            LogLevel.Debug => "[DEBUG]",
            _ => "[LOG]  "
        };

        string formattedMessage = $"{prefix} {message}";

        // Вывод в консоль
        try
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {formattedMessage}");
        }
        catch
        {
            // Игнорируем ошибки вывода
        }

        // Вызов внешнего обработчика
        LogHandler?.Invoke(formattedMessage);

        // Вывод в System.Diagnostics
        System.Diagnostics.Debug.WriteLine(formattedMessage);
    }

    public static void Debug(string message)
        => Log(LogLevel.Debug, message);

    public static void Info(string message)
        => Log(LogLevel.Info, message);

    public static void Warning(string message)
        => Log(LogLevel.Warning, message);

    public static void Error(string message)
        => Log(LogLevel.Error, message);
}