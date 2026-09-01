namespace PlatformEngine.Core;

/// <summary>
/// Интерфейс для расширения PlatformEngine новыми модулями
/// </summary>
public interface IEngineModule
{
    /// <summary>Имя модуля</summary>
    string Name { get; }

    /// <summary>Флаг модуля</summary>
    ModuleFlags Flag { get; }

    /// <summary>Инициализация модуля</summary>
    void Initialize(EngineConfig config);

    /// <summary>Обновление модуля (вызывается каждый кадр)</summary>
    void Update(float deltaTime);

    /// <summary>Завершение работы модуля</summary>
    void Shutdown();

    /// <summary>Проверка, инициализирован ли модуль</summary>
    bool IsInitialized { get; }
}