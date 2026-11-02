namespace HotkeyInspector.Core;

public static class HotkeyParser
{
    public static HotkeyDefinition Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new HotkeyParseException(new LocalizedText(
                "请输入快捷键，例如 Ctrl+Alt+K。",
                "Enter a shortcut, for example Ctrl+Alt+K."));
        }

        var modifiers = HotkeyModifier.None;
        var modifierLabels = new List<string>();
        uint? virtualKey = null;
        string? keyLabel = null;

        foreach (var rawPart in SplitParts(text))
        {
            var token = rawPart.Replace(" ", "", StringComparison.Ordinal);
            if (token.Length == 0)
            {
                continue;
            }

            if (KeyCatalog.TryGetModifier(token, out var modifierLabel, out var modifier))
            {
                if (!modifiers.HasFlag(modifier))
                {
                    modifierLabels.Add(modifierLabel);
                }

                modifiers |= modifier;
                continue;
            }

            if (virtualKey is not null)
            {
                throw new HotkeyParseException(new LocalizedText(
                    "一个快捷键只能包含一个非修饰键。",
                    "A shortcut can contain only one non-modifier key."));
            }

            if (!KeyCatalog.TryGetKey(token, out var vk, out var label))
            {
                throw new HotkeyParseException(new LocalizedText(
                    $"不支持的按键：{rawPart}",
                    $"Unsupported key: {rawPart}"));
            }

            virtualKey = vk;
            keyLabel = label;
        }

        if (virtualKey is null || keyLabel is null)
        {
            throw new HotkeyParseException(new LocalizedText(
                "请添加一个非修饰键，例如 K 或 F9。",
                "Add a non-modifier key, for example K or F9."));
        }

        var orderedModifiers = KeyCatalog.ModifierOrder.Where(modifierLabels.Contains);
        var displayText = string.Join("+", orderedModifiers.Append(keyLabel));
        return new HotkeyDefinition(modifiers, virtualKey.Value, displayText);
    }

    /// <summary>
    /// 拆分组合键：优先按 <c>+</c>；没有 <c>+</c> 时把 <c>-</c> 当分隔符。
    /// 因此 <c>Ctrl+-</c> 得到减号键，而 <c>Ctrl-Alt-K</c> 仍按旧写法工作。
    /// </summary>
    private static IEnumerable<string> SplitParts(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.Length == 0)
        {
            yield break;
        }

        if (trimmed == "+")
        {
            yield return "Plus";
            yield break;
        }

        if (!trimmed.Contains('+') && trimmed.Contains('-'))
        {
            foreach (var part in trimmed.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                yield return part;
            }

            yield break;
        }

        // "Ctrl++" 的末尾空段表示加号键本身。
        var normalized = trimmed.EndsWith("++", StringComparison.Ordinal)
            ? trimmed[..^1] + "Plus"
            : trimmed;

        foreach (var part in normalized.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            yield return part;
        }
    }
}
