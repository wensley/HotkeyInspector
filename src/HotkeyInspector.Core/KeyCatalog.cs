namespace HotkeyInspector.Core;

/// <summary>
/// 单一键位词表：token → 虚拟键码 → 规范标签。
/// 解析器、捕获器与知识库归一化都从这里取词，避免三套词表互相漂移。
/// </summary>
public static class KeyCatalog
{
    /// <summary>修饰键的规范顺序，用于生成稳定的 DisplayText。</summary>
    public static readonly string[] ModifierOrder = ["Win", "Ctrl", "Alt", "Shift"];

    private static readonly Dictionary<string, (string Label, HotkeyModifier Modifier)> Modifiers = BuildModifiers();
    private static readonly Dictionary<string, (uint VirtualKey, string Label)> Keys = BuildKeys();
    private static readonly Dictionary<uint, string> LabelsByVirtualKey = BuildLabelsByVirtualKey();

    public static bool TryGetModifier(string token, out string label, out HotkeyModifier modifier)
    {
        if (Modifiers.TryGetValue(token.Trim(), out var info))
        {
            label = info.Label;
            modifier = info.Modifier;
            return true;
        }

        label = string.Empty;
        modifier = HotkeyModifier.None;
        return false;
    }

    public static bool TryGetKey(string token, out uint virtualKey, out string label)
    {
        if (Keys.TryGetValue(token.Trim(), out var info))
        {
            virtualKey = info.VirtualKey;
            label = info.Label;
            return true;
        }

        virtualKey = 0;
        label = string.Empty;
        return false;
    }

    /// <summary>返回虚拟键码对应的规范标签；未知键返回 null。</summary>
    public static string? LabelFor(uint virtualKey)
        => LabelsByVirtualKey.TryGetValue(virtualKey, out var label) ? label : null;

    /// <summary>返回该键若是修饰键时的规范名（Win/Ctrl/Alt/Shift），否则返回 null。</summary>
    public static string? ModifierOf(uint virtualKey) => virtualKey switch
    {
        0x10 or 0xA0 or 0xA1 => "Shift",
        0x11 or 0xA2 or 0xA3 => "Ctrl",
        0x12 or 0xA4 or 0xA5 => "Alt",
        0x5B or 0x5C => "Win",
        _ => null
    };

    /// <summary>返回非修饰键的规范标签；修饰键或未知键返回 null。</summary>
    public static string? NameOf(uint virtualKey)
        => ModifierOf(virtualKey) is null ? LabelFor(virtualKey) : null;

    private static Dictionary<string, (string Label, HotkeyModifier Modifier)> BuildModifiers()
    {
        var map = new Dictionary<string, (string, HotkeyModifier)>(StringComparer.OrdinalIgnoreCase);
        void add(string[] tokens, string label, HotkeyModifier modifier)
        {
            foreach (var token in tokens)
            {
                map[token] = (label, modifier);
            }
        }

        add(["ALT", "OPTION", "OPT"], "Alt", HotkeyModifier.Alt);
        add(["CTRL", "CONTROL"], "Ctrl", HotkeyModifier.Control);
        add(["SHIFT"], "Shift", HotkeyModifier.Shift);
        add(["WIN", "WINDOWS", "META"], "Win", HotkeyModifier.Win);
        return map;
    }

    private static Dictionary<string, (uint VirtualKey, string Label)> BuildKeys()
    {
        var keys = new Dictionary<string, (uint, string)>(StringComparer.OrdinalIgnoreCase);

        void add(uint virtualKey, string label, params string[] tokens)
        {
            foreach (var token in tokens)
            {
                keys[token] = (virtualKey, label);
            }
        }

        add(0x08, "Backspace", "BACKSPACE", "BACK");
        add(0x09, "Tab", "TAB");
        add(0x0D, "Enter", "ENTER", "RETURN");
        add(0x13, "Pause", "PAUSE");
        add(0x1B, "Esc", "ESC", "ESCAPE");
        add(0x20, "Space", "SPACE", "SPACEBAR");
        add(0x21, "PageUp", "PAGEUP", "PGUP");
        add(0x22, "PageDown", "PAGEDOWN", "PGDN");
        add(0x23, "End", "END");
        add(0x24, "Home", "HOME");
        add(0x25, "Left", "LEFT", "\u2190", "ARROWLEFT");
        add(0x26, "Up", "UP", "\u2191", "ARROWUP");
        add(0x27, "Right", "RIGHT", "\u2192", "ARROWRIGHT");
        add(0x28, "Down", "DOWN", "\u2193", "ARROWDOWN");
        add(0x2C, "PrintScreen", "PRINTSCREEN", "PRTSC", "PRINT");
        add(0x2D, "Insert", "INSERT", "INS");
        add(0x2E, "Delete", "DELETE", "DEL");
        add(0x5D, "ContextMenu", "CONTEXTMENU", "MENU", "APPS");

        add(0xBA, "Semicolon", "SEMICOLON", ";");
        add(0xBB, "Equal", "EQUAL", "=", "PLUS");
        add(0xBC, "Comma", "COMMA", ",");
        add(0xBD, "Minus", "MINUS", "-");
        add(0xBE, "Period", "PERIOD", ".");
        add(0xBF, "Slash", "SLASH", "/");
        add(0xC0, "Backquote", "BACKQUOTE", "BACKTICK", "`");
        add(0xDB, "BracketLeft", "BRACKETLEFT", "[");
        add(0xDC, "Backslash", "BACKSLASH", "\\");
        add(0xDD, "BracketRight", "BRACKETRIGHT", "]");
        add(0xDE, "Quote", "QUOTE", "'");

        add(0x60, "NumPad0", "NUMPAD0", "NUM0");
        add(0x61, "NumPad1", "NUMPAD1", "NUM1");
        add(0x62, "NumPad2", "NUMPAD2", "NUM2");
        add(0x63, "NumPad3", "NUMPAD3", "NUM3");
        add(0x64, "NumPad4", "NUMPAD4", "NUM4");
        add(0x65, "NumPad5", "NUMPAD5", "NUM5");
        add(0x66, "NumPad6", "NUMPAD6", "NUM6");
        add(0x67, "NumPad7", "NUMPAD7", "NUM7");
        add(0x68, "NumPad8", "NUMPAD8", "NUM8");
        add(0x69, "NumPad9", "NUMPAD9", "NUM9");
        add(0x6A, "Multiply", "MULTIPLY");
        add(0x6B, "NumpadPlus", "NUMPADPLUS", "ADD");
        add(0x6D, "NumpadMinus", "NUMPADMINUS", "SUBTRACT");
        add(0x6E, "NumpadDecimal", "NUMPADDECIMAL", "DECIMAL");
        add(0x6F, "Divide", "DIVIDE");

        for (var digit = 0; digit <= 9; digit++)
        {
            keys[digit.ToString()] = ((uint)'0' + (uint)digit, digit.ToString());
        }

        for (var letter = 'A'; letter <= 'Z'; letter++)
        {
            keys[letter.ToString()] = (letter, letter.ToString());
        }

        for (var functionKey = 1; functionKey <= 24; functionKey++)
        {
            var label = $"F{functionKey}";
            keys[label] = (0x70u + (uint)functionKey - 1, label);
        }

        return keys;
    }

    private static Dictionary<uint, string> BuildLabelsByVirtualKey()
    {
        var labels = new Dictionary<uint, string>();
        foreach (var entry in Keys.Values)
        {
            labels.TryAdd(entry.VirtualKey, entry.Label);
        }

        return labels;
    }
}
