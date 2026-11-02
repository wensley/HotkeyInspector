namespace HotkeyInspector.Core;

/// <summary>
/// 键位组合归一化。用于把「解析器产出的 DisplayText」与「知识库中的 k 值」映射到同一套
/// 规范形式，从而可跨写法匹配（修饰键统一排序为 Win、Ctrl、Alt、Shift；键名统一取自
/// <see cref="KeyCatalog"/>）。平台专属写法（Cmd/Command/Super）保持原样，避免在 Windows
/// 上把 macOS/Linux 快捷键误匹配到 Win 组合。
/// </summary>
public static class ComboKey
{
    public static string Canon(string token)
    {
        var trimmed = token.Trim();
        if (trimmed.Length == 0)
        {
            return trimmed;
        }

        if (KeyCatalog.TryGetModifier(trimmed, out var modifierLabel, out _))
        {
            return modifierLabel;
        }

        if (KeyCatalog.TryGetKey(trimmed, out _, out var keyLabel))
        {
            return keyLabel;
        }

        if (trimmed.Length == 1 && char.IsAsciiLetter(trimmed[0]))
        {
            return char.ToUpperInvariant(trimmed[0]).ToString();
        }

        return trimmed;
    }

    public static string Normalize(string combo)
    {
        if (string.IsNullOrWhiteSpace(combo))
        {
            return string.Empty;
        }

        var modifiers = new List<string>();
        var keys = new List<string>();

        foreach (var part in combo.Split('+'))
        {
            var token = Canon(part);
            if (token.Length == 0)
            {
                continue;
            }

            if (Array.IndexOf(KeyCatalog.ModifierOrder, token) >= 0)
            {
                if (!modifiers.Contains(token, StringComparer.Ordinal))
                {
                    modifiers.Add(token);
                }
            }
            else
            {
                keys.Add(token);
            }
        }

        modifiers.Sort((a, b) => Array.IndexOf(KeyCatalog.ModifierOrder, a).CompareTo(Array.IndexOf(KeyCatalog.ModifierOrder, b)));
        return string.Join('+', modifiers.Concat(keys));
    }
}
