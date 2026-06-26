namespace HotkeyInspector.Core;

public static class HotkeyParser
{
    private static readonly Dictionary<string, (string Label, HotkeyModifier Modifier)> Modifiers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ALT"] = ("Alt", HotkeyModifier.Alt),
        ["OPTION"] = ("Alt", HotkeyModifier.Alt),
        ["CTRL"] = ("Ctrl", HotkeyModifier.Control),
        ["CONTROL"] = ("Ctrl", HotkeyModifier.Control),
        ["SHIFT"] = ("Shift", HotkeyModifier.Shift),
        ["WIN"] = ("Win", HotkeyModifier.Win),
        ["WINDOWS"] = ("Win", HotkeyModifier.Win),
        ["META"] = ("Win", HotkeyModifier.Win)
    };

    private static readonly Dictionary<string, uint> Keys = BuildKeys();

    public static HotkeyDefinition Parse(string text)
    {
        var parts = text
            .Replace("-", "+", StringComparison.Ordinal)
            .Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            throw new HotkeyParseException("请输入快捷键，例如 Ctrl+Alt+K。");
        }

        var modifiers = HotkeyModifier.None;
        var modifierLabels = new List<string>();
        uint? virtualKey = null;
        string? keyLabel = null;

        foreach (var rawPart in parts)
        {
            var token = rawPart.Replace(" ", "", StringComparison.Ordinal).ToUpperInvariant();
            if (Modifiers.TryGetValue(token, out var modifier))
            {
                if (!modifiers.HasFlag(modifier.Modifier))
                {
                    modifierLabels.Add(modifier.Label);
                }

                modifiers |= modifier.Modifier;
                continue;
            }

            if (virtualKey is not null)
            {
                throw new HotkeyParseException("一个快捷键只能包含一个非修饰键。");
            }

            if (!Keys.TryGetValue(token, out var vk))
            {
                throw new HotkeyParseException($"不支持的按键：{rawPart}");
            }

            virtualKey = vk;
            keyLabel = DisplayKeyLabel(token);
        }

        if (virtualKey is null || keyLabel is null)
        {
            throw new HotkeyParseException("请添加一个非修饰键，例如 K 或 F9。");
        }

        var label = string.Join("+", modifierLabels.Append(keyLabel));
        return new HotkeyDefinition(modifiers, virtualKey.Value, label);
    }

    private static Dictionary<string, uint> BuildKeys()
    {
        var keys = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
        {
            ["ESC"] = 0x1B,
            ["ESCAPE"] = 0x1B,
            ["TAB"] = 0x09,
            ["SPACE"] = 0x20,
            ["SPACEBAR"] = 0x20,
            ["ENTER"] = 0x0D,
            ["RETURN"] = 0x0D,
            ["BACKSPACE"] = 0x08,
            ["DELETE"] = 0x2E,
            ["DEL"] = 0x2E,
            ["INSERT"] = 0x2D,
            ["INS"] = 0x2D,
            ["HOME"] = 0x24,
            ["END"] = 0x23,
            ["PAGEUP"] = 0x21,
            ["PGUP"] = 0x21,
            ["PAGEDOWN"] = 0x22,
            ["PGDN"] = 0x22,
            ["LEFT"] = 0x25,
            ["UP"] = 0x26,
            ["RIGHT"] = 0x27,
            ["DOWN"] = 0x28,
            ["PRINTSCREEN"] = 0x2C,
            ["PRTSC"] = 0x2C,
            ["PAUSE"] = 0x13,
            ["PERIOD"] = 0xBE,
            ["."] = 0xBE
        };

        for (var digit = 0; digit <= 9; digit++)
        {
            keys[digit.ToString()] = (uint)'0' + (uint)digit;
        }

        for (var letter = 'A'; letter <= 'Z'; letter++)
        {
            keys[letter.ToString()] = letter;
        }

        for (var functionKey = 1; functionKey <= 24; functionKey++)
        {
            keys[$"F{functionKey}"] = 0x70u + (uint)functionKey - 1;
        }

        return keys;
    }

    private static string DisplayKeyLabel(string token) => token switch
    {
        "ESCAPE" => "Esc",
        "RETURN" => "Enter",
        "SPACEBAR" => "Space",
        "DEL" => "Delete",
        "INS" => "Insert",
        "PGUP" => "PageUp",
        "PGDN" => "PageDown",
        "PRTSC" => "PrintScreen",
        "." => "Period",
        _ when token.Length == 1 => token,
        _ when token.StartsWith('F') && token[1..].All(char.IsDigit) => token,
        _ => token[..1] + token[1..].ToLowerInvariant()
    };
}
