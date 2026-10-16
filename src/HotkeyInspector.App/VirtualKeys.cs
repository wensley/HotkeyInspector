namespace HotkeyInspector.App;

/// <summary>虚拟键码（VK）到规范 token 的映射，token 与 <c>ComboKey</c> 约定保持一致。</summary>
internal static class VirtualKeys
{
    private const uint VkShift = 0x10;
    private const uint VkControl = 0x11;
    private const uint VkMenu = 0x12;
    private const uint VkLShift = 0xA0;
    private const uint VkRShift = 0xA1;
    private const uint VkLControl = 0xA2;
    private const uint VkRControl = 0xA3;
    private const uint VkLMenu = 0xA4;
    private const uint VkRMenu = 0xA5;
    private const uint VkLWin = 0x5B;
    private const uint VkRWin = 0x5C;

    private static readonly Dictionary<uint, string> Named = new()
    {
        [0x08] = "Backspace",
        [0x09] = "Tab",
        [0x0D] = "Enter",
        [0x13] = "Pause",
        [0x1B] = "Esc",
        [0x20] = "Space",
        [0x21] = "PageUp",
        [0x22] = "PageDown",
        [0x23] = "End",
        [0x24] = "Home",
        [0x25] = "Left",
        [0x26] = "Up",
        [0x27] = "Right",
        [0x28] = "Down",
        [0x2C] = "PrintScreen",
        [0x2D] = "Insert",
        [0x2E] = "Delete",
        [0x5D] = "ContextMenu",
        [0x60] = "NumPad0",
        [0x61] = "NumPad1",
        [0x62] = "NumPad2",
        [0x63] = "NumPad3",
        [0x64] = "NumPad4",
        [0x65] = "NumPad5",
        [0x66] = "NumPad6",
        [0x67] = "NumPad7",
        [0x68] = "NumPad8",
        [0x69] = "NumPad9",
        [0x6A] = "Multiply",
        [0x6B] = "Plus",
        [0x6D] = "Minus",
        [0x6E] = "Period",
        [0x6F] = "Divide",
        [0xBA] = "Semicolon",
        [0xBB] = "Equal",
        [0xBC] = "Comma",
        [0xBD] = "Minus",
        [0xBE] = "Period",
        [0xBF] = "Slash",
        [0xC0] = "Backquote",
        [0xDB] = "BracketLeft",
        [0xDC] = "Backslash",
        [0xDD] = "BracketRight",
        [0xDE] = "Quote",
    };

    /// <summary>返回该键若是修饰键时的规范名（Win/Ctrl/Alt/Shift），否则返回 null。</summary>
    public static string? ModifierOf(uint vk) => vk switch
    {
        VkShift or VkLShift or VkRShift => "Shift",
        VkControl or VkLControl or VkRControl => "Ctrl",
        VkMenu or VkLMenu or VkRMenu => "Alt",
        VkLWin or VkRWin => "Win",
        _ => null
    };

    /// <summary>返回非修饰键的规范 token；未知键返回 null。</summary>
    public static string? Name(uint vk)
    {
        if (vk is >= 0x41 and <= 0x5A) // A-Z
        {
            return ((char)vk).ToString();
        }

        if (vk is >= 0x30 and <= 0x39) // 0-9
        {
            return ((char)vk).ToString();
        }

        if (vk is >= 0x70 and <= 0x87) // F1-F24
        {
            return $"F{vk - 0x70 + 1}";
        }

        return Named.TryGetValue(vk, out var name) ? name : null;
    }
}
