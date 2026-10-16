namespace HotkeyInspector.Core;

/// <summary>
/// 键位组合归一化。用于把「解析器产出的 DisplayText」与「知识库中的 k 值」映射到同一套
/// 规范形式，从而可跨写法匹配（例如 Super/Meta/Cmd 一律折叠为 Win，PrtSc/Print/PrintScreen
/// 一律折叠为 PrintScreen，修饰键统一排序为 Win、Ctrl、Alt、Shift）。
/// </summary>
public static class ComboKey
{
    private static readonly string[] ModifierOrder = ["Win", "Ctrl", "Alt", "Shift"];

    private static readonly Dictionary<string, string> Synonyms = new(StringComparer.OrdinalIgnoreCase)
    {
        ["control"] = "Ctrl",
        ["ctrl"] = "Ctrl",
        ["shift"] = "Shift",
        ["alt"] = "Alt",
        ["option"] = "Alt",
        ["opt"] = "Alt",
        ["win"] = "Win",
        ["windows"] = "Win",
        ["super"] = "Win",
        ["meta"] = "Win",
        ["cmd"] = "Win",
        ["command"] = "Win",
        ["escape"] = "Esc",
        ["esc"] = "Esc",
        ["return"] = "Enter",
        ["enter"] = "Enter",
        ["del"] = "Delete",
        ["delete"] = "Delete",
        ["space"] = "Space",
        ["spacebar"] = "Space",
        ["ins"] = "Insert",
        ["insert"] = "Insert",
        ["pgup"] = "PageUp",
        ["pageup"] = "PageUp",
        ["pgdn"] = "PageDown",
        ["pagedown"] = "PageDown",
        ["prtsc"] = "PrintScreen",
        ["print"] = "PrintScreen",
        ["printscreen"] = "PrintScreen",
        ["\u2190"] = "Left",
        ["left"] = "Left",
        ["arrowleft"] = "Left",
        ["\u2192"] = "Right",
        ["right"] = "Right",
        ["arrowright"] = "Right",
        ["\u2191"] = "Up",
        ["up"] = "Up",
        ["arrowup"] = "Up",
        ["\u2193"] = "Down",
        ["down"] = "Down",
        ["arrowdown"] = "Down",
        ["+"] = "Plus",
        ["plus"] = "Plus",
        ["-"] = "Minus",
        ["minus"] = "Minus",
        ["multiply"] = "Multiply",
        ["divide"] = "Divide",
    };

    public static string Canon(string token)
    {
        var trimmed = token.Trim();
        if (trimmed.Length == 0)
        {
            return trimmed;
        }

        if (Synonyms.TryGetValue(trimmed, out var mapped))
        {
            return mapped;
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

            if (Array.IndexOf(ModifierOrder, token) >= 0)
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

        modifiers.Sort((a, b) => Array.IndexOf(ModifierOrder, a).CompareTo(Array.IndexOf(ModifierOrder, b)));
        return string.Join('+', modifiers.Concat(keys));
    }
}
