using System.Windows.Input;

namespace HotkeyInspector.App;

/// <summary>WPF 的 <see cref="Key"/> 与 Core 词表之间的桥接，供输入框和窗口级捕获共用。</summary>
internal static class WpfKeyMapper
{
    public static bool IsModifier(Key key) => key is
        Key.LeftCtrl or Key.RightCtrl or
        Key.LeftAlt or Key.RightAlt or
        Key.LeftShift or Key.RightShift or
        Key.LWin or Key.RWin;

    public static string BuildCombo(Key key) => BuildCombo(key, Keyboard.Modifiers);

    public static string BuildCombo(Key key, ModifierKeys modifiers)
    {
        var parts = new List<string>();
        if (modifiers.HasFlag(ModifierKeys.Windows))
        {
            parts.Add("Win");
        }

        if (modifiers.HasFlag(ModifierKeys.Control))
        {
            parts.Add("Ctrl");
        }

        if (modifiers.HasFlag(ModifierKeys.Alt))
        {
            parts.Add("Alt");
        }

        if (modifiers.HasFlag(ModifierKeys.Shift))
        {
            parts.Add("Shift");
        }

        parts.Add(ToToken(key));
        return string.Join('+', parts);
    }

    public static string ToToken(Key key)
    {
        if (key is >= Key.D0 and <= Key.D9)
        {
            return ((char)('0' + (key - Key.D0))).ToString();
        }

        if (key is >= Key.NumPad0 and <= Key.NumPad9)
        {
            return $"NumPad{key - Key.NumPad0}";
        }

        if (key is >= Key.A and <= Key.Z)
        {
            return key.ToString();
        }

        if (key is >= Key.F1 and <= Key.F24)
        {
            return key.ToString();
        }

        return key switch
        {
            Key.Escape => "Esc",
            Key.Return => "Enter",
            Key.Space => "Space",
            Key.Tab => "Tab",
            Key.Back => "Backspace",
            Key.Delete => "Delete",
            Key.Insert => "Insert",
            Key.Home => "Home",
            Key.End => "End",
            Key.PageUp => "PageUp",
            Key.PageDown => "PageDown",
            Key.Left => "Left",
            Key.Up => "Up",
            Key.Right => "Right",
            Key.Down => "Down",
            Key.PrintScreen => "PrintScreen",
            Key.Pause => "Pause",
            Key.OemPeriod => "Period",
            Key.OemComma => "Comma",
            Key.OemMinus => "Minus",
            Key.OemPlus => "Equal",
            Key.Oem1 => "Semicolon",
            Key.Oem2 => "Slash",
            Key.Oem3 => "Backquote",
            Key.Oem4 => "BracketLeft",
            Key.Oem5 => "Backslash",
            Key.Oem6 => "BracketRight",
            Key.Oem7 => "Quote",
            Key.Add => "NumpadPlus",
            Key.Subtract => "NumpadMinus",
            Key.Multiply => "Multiply",
            Key.Divide => "Divide",
            Key.Decimal => "NumpadDecimal",
            Key.Apps => "ContextMenu",
            _ => key.ToString()
        };
    }
}
