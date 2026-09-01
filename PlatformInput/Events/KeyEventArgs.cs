using PlatformInput.Enums;

namespace PlatformInput.Events;

/// <summary>
/// Аргументы события клавиатуры
/// </summary>
public class KeyEventArgs(Key key, int scancode, int mods, bool isRepeat) : EventArgs
{
    public Key Key { get; } = key;
    public int Scancode { get; } = scancode;
    public int Mods { get; } = mods;
    public bool IsRepeat { get; } = isRepeat;
}