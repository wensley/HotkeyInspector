namespace HotkeyInspector.Core;

[Flags]
public enum HotkeyModifier : uint
{
    None = 0,
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Win = 0x0008,
    NoRepeat = 0x4000
}
