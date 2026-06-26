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
        ["PrintScreen"] = new("Windows 截图", "截取屏幕或打开截图体验，具体取决于系统设置。"),

        ["Win+Shift+M"] = new("Windows", "还原所有最小化窗口。"),
        ["Win+Shift+T"] = new("Windows 任务栏", "循环浏览任务栏中的应用预览。"),
        ["Win+Shift+1"] = new("Windows 任务栏", "打开任务栏上第 1 个应用的新实例。"),
        ["Win+Shift+2"] = new("Windows 任务栏", "打开任务栏上第 2 个应用的新实例。"),
        ["Win+Shift+3"] = new("Windows 任务栏", "打开任务栏上第 3 个应用的新实例。"),
        ["Win+Shift+4"] = new("Windows 任务栏", "打开任务栏上第 4 个应用的新实例。"),
        ["Win+Shift+5"] = new("Windows 任务栏", "打开任务栏上第 5 个应用的新实例。"),
        ["Win+Shift+6"] = new("Windows 任务栏", "打开任务栏上第 6 个应用的新实例。"),
        ["Win+Shift+7"] = new("Windows 任务栏", "打开任务栏上第 7 个应用的新实例。"),
        ["Win+Shift+8"] = new("Windows 任务栏", "打开任务栏上第 8 个应用的新实例。"),
        ["Win+Shift+9"] = new("Windows 任务栏", "打开任务栏上第 9 个应用的新实例。"),
        ["Win+Shift+0"] = new("Windows 任务栏", "打开任务栏上第 10 个应用的新实例。"),

        ["Win+Shift+Left"] = new("Windows", "将窗口移动到左侧显示器。"),
        ["Win+Shift+Right"] = new("Windows", "将窗口移动到右侧显示器。"),
        ["Win+Shift+Up"] = new("Windows", "将窗口拉伸到顶部和底部。"),
        ["Win+Shift+Down"] = new("Windows", "还原或最小化窗口。"),

        ["Win+Ctrl+D"] = new("Windows", "创建新的虚拟桌面。"),
        ["Win+Ctrl+F4"] = new("Windows", "关闭当前虚拟桌面。"),
        ["Win+Ctrl+Left"] = new("Windows", "切换到左侧虚拟桌面。"),
        ["Win+Ctrl+Right"] = new("Windows", "切换到右侧虚拟桌面。"),

        ["Win+Alt+D"] = new("Windows", "显示或隐藏桌面上的日期和时间。"),
        ["Win+Alt+1"] = new("Windows 任务栏", "打开任务栏上第 1 个应用的跳转列表。"),
        ["Win+Alt+2"] = new("Windows 任务栏", "打开任务栏上第 2 个应用的跳转列表。"),
        ["Win+Alt+3"] = new("Windows 任务栏", "打开任务栏上第 3 个应用的跳转列表。"),
        ["Win+Alt+4"] = new("Windows 任务栏", "打开任务栏上第 4 个应用的跳转列表。"),
        ["Win+Alt+5"] = new("Windows 任务栏", "打开任务栏上第 5 个应用的跳转列表。"),
        ["Win+Alt+6"] = new("Windows 任务栏", "打开任务栏上第 6 个应用的跳转列表。"),
        ["Win+Alt+7"] = new("Windows 任务栏", "打开任务栏上第 7 个应用的跳转列表。"),
        ["Win+Alt+8"] = new("Windows 任务栏", "打开任务栏上第 8 个应用的跳转列表。"),
        ["Win+Alt+9"] = new("Windows 任务栏", "打开任务栏上第 9 个应用的跳转列表。"),
        ["Win+Alt+0"] = new("Windows 任务栏", "打开任务栏上第 10 个应用的跳转列表。"),

        ["Win+A"] = new("Windows 操作中心", "打开快速设置。"),
        ["Win+B"] = new("Windows", "聚焦任务栏通知区域。"),
        ["Win+C"] = new("Microsoft Copilot", "打开 Microsoft Copilot。"),
        ["Win+F"] = new("Windows 反馈中心", "打开反馈中心。"),
        ["Win+G"] = new("Xbox Game Bar", "打开 Xbox Game Bar。"),
        ["Win+H"] = new("Windows", "打开语音输入。"),
        ["Win+K"] = new("Windows", "打开投屏。"),
        ["Win+N"] = new("Windows", "打开通知中心。"),
        ["Win+O"] = new("Windows", "锁定设备方向。"),
        ["Win+P"] = new("Windows", "打开投影选项。"),
        ["Win+Shift+Q"] = new("Windows", "打开或关闭自动 HDR。"),
        ["Win+T"] = new("Windows 任务栏", "循环浏览任务栏中的应用。"),
        ["Win+W"] = new("Windows", "打开小组件面板。"),
        ["Win+Y"] = new("Windows", "切换输入。"),
        ["Win+Z"] = new("Windows", "打开贴靠布局。"),
        ["Win+Comma"] = new("Windows", "临时速览桌面。"),
        ["Win+Period"] = new("Windows", "打开表情符号面板。"),

        ["Alt+Shift"] = new("Windows 输入法", "切换输入法/键盘布局。"),

        ["Ctrl+Shift+Escape"] = new("任务管理器", "打开任务管理器。"),
        ["Ctrl+Shift+Left"] = new("Windows", "将应用移动到左侧显示器。"),
        ["Ctrl+Shift+Right"] = new("Windows", "将应用移动到右侧显示器。"),
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
