namespace HotkeyInspector.Core;

public sealed record SystemReservedHotkey(string Combo, LocalizedText Note, bool Uncapturable);

/// <summary>
/// Windows 自身优先处理的组合。它们不一定出现在 <see cref="KnownHotkeyCatalog"/> 中
/// （例如仅修饰键的 Win、Alt+Tab），因此单独维护，供界面展示"系统保留"清单。
/// </summary>
public static class SystemReservedHotkeys
{
    public static IReadOnlyList<SystemReservedHotkey> All { get; } =
    [
        new("Win", new("打开或关闭“开始”菜单", "Open or close the Start menu"), false),
        new("Alt+Tab", new("切换打开的窗口", "Switch between open windows"), false),
        new("Ctrl+Esc", new("打开“开始”菜单", "Open the Start menu"), false),
        new("Win+Tab", new("打开任务视图", "Open Task View"), false),
        new("Alt+F4", new("关闭活动窗口", "Close the active window"), false),
        new("Win+D", new("显示或隐藏桌面", "Show or hide the desktop"), false),
        new("Win+L", new("锁定电脑", "Lock the computer"), false),
        new("Win+Shift+S", new("区域截图到剪贴板", "Screenshot a region to clipboard"), false),
        new("PrintScreen", new("全屏截图到剪贴板", "Screenshot the screen to clipboard"), false),
        new("Ctrl+Shift+Esc", new("打开任务管理器", "Open Task Manager"), false),
        new("Ctrl+Alt+Delete", new("打开安全屏幕（安全注意序列）", "Open the security screen (secure attention sequence)"), true),
    ];
}
