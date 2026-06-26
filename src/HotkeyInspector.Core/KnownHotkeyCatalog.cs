namespace HotkeyInspector.Core;

public static class KnownHotkeyCatalog
{
    private static readonly Dictionary<string, KnownHotkeyInfo> KnownHotkeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Win+D"] = new("Windows", "显示或隐藏桌面。"),
        ["Win+E"] = new("Windows 文件资源管理器", "打开文件资源管理器。"),
        ["Win+I"] = new("Windows 设置", "打开系统设置。"),
        ["Win+L"] = new("Windows", "锁定电脑。"),
        ["Win+R"] = new("Windows", "打开“运行”对话框。"),
        ["Win+S"] = new("Windows 搜索", "打开 Windows 搜索。"),
        ["Win+V"] = new("Windows 剪贴板", "打开剪贴板历史记录。"),
        ["Win+X"] = new("Windows", "打开快速链接菜单。"),
        ["Win+Shift+S"] = new("截图工具", "打开屏幕截图区域选择工具。"),
        ["Win+Alt+R"] = new("Xbox Game Bar", "开始或停止屏幕录制。"),
        ["Win+Alt+G"] = new("Xbox Game Bar", "录制最近一段游戏或应用画面。"),
        ["Win+Alt+PrtSc"] = new("Xbox Game Bar", "截取当前游戏或应用窗口。"),
        ["Win+Ctrl+Shift+B"] = new("Windows 图形驱动", "重置图形驱动程序。"),
        ["Ctrl+Shift+Esc"] = new("任务管理器", "打开任务管理器。"),
        ["Alt+Tab"] = new("Windows", "切换打开的窗口。"),
        ["Alt+F4"] = new("Windows", "关闭当前窗口或应用。"),
        ["Ctrl+Alt+Delete"] = new("Windows 安全界面", "打开安全选项界面。"),
        ["PrintScreen"] = new("Windows 截图", "截取屏幕或打开截图体验，具体取决于系统设置。")
    };

    public static KnownHotkeyInfo UnknownOccupied { get; } = new("未知应用", "该快捷键已被注册，但 Windows 未公开占用应用信息。");

    public static KnownHotkeyInfo Available { get; } = new("无", "该快捷键当前可注册为全局快捷键。");

    public static KnownHotkeyInfo Invalid { get; } = new("无", "快捷键格式无效。");

    public static KnownHotkeyInfo Lookup(HotkeyDefinition hotkey)
    {
        return KnownHotkeys.TryGetValue(hotkey.DisplayText, out var info)
            ? info
            : UnknownOccupied;
    }
}
