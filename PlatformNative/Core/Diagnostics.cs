using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PlatformNative.Core;

/// <summary>
/// Диагностика и логирование для нативного слоя
/// </summary>
public static class Diagnostics
{
    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Debug = 4,
        Trace = 5
    }

    private static LogLevel DiagnosticsCurrentLevel = LogLevel.Info;
    private static readonly Lock Lock = new();
    private static readonly TimeProvider TimeProvider = TimeProvider.System;

    public static event Action<string>? OnLogMessage;

    public static LogLevel CurrentLevel
    {
        get => DiagnosticsCurrentLevel;
        set => DiagnosticsCurrentLevel = value;
    }

    private static void WriteLine(string message)
    {
        try
        {
            Console.WriteLine($"[{TimeProvider.GetLocalNow():HH:mm:ss.fff}] {message}");
        }
        catch
        {
            // Игнорируем
        }

        OnLogMessage?.Invoke(message);
    }

    public static void Log(LogLevel level, string message, Exception? ex = null)
    {
        if (level > DiagnosticsCurrentLevel) return;

        lock (Lock)
        {
            string prefix = level switch
            {
                LogLevel.Error => "[ERROR]",
                LogLevel.Warning => "[WARN] ",
                LogLevel.Info => "[INFO] ",
                LogLevel.Debug => "[DEBUG]",
                LogLevel.Trace => "[TRACE]",
                _ => "[LOG]  "
            };

            WriteLine($"{prefix} {message}");
            if (ex is not null)
            {
                WriteLine($"  └─ {ex.Message}");
                if (level <= LogLevel.Debug && ex.StackTrace is not null)
                {
                    WriteLine($"  └─ {ex.StackTrace}");
                }
            }
        }
    }

    public static void Error(string message, Exception? ex = null) => Log(LogLevel.Error, message, ex);
    public static void Warning(string message, Exception? ex = null) => Log(LogLevel.Warning, message, ex);
    public static void Info(string message) => Log(LogLevel.Info, message);
    public static void Debug(string message) => Log(LogLevel.Debug, message);
    public static void Trace(string message) => Log(LogLevel.Trace, message);

    public static IDisposable Scope(string operationName) => new DiagnosticsScope(operationName);

    private sealed class DiagnosticsScope(string operationName) : IDisposable
    {
        private readonly string OperationName = operationName;
        private readonly Stopwatch Stopwatch = Stopwatch.StartNew();
        private bool Disposed;

        public void Dispose()
        {
            if (Disposed) return;
            Stopwatch.Stop();
            Debug($"◀ Завершено: {OperationName} ({Stopwatch.Elapsed.TotalMilliseconds:F2}ms)");
            Disposed = true;
        }
    }
}