namespace HotkeyInspector.Core;

public static class KnownHotkeyCatalog
{
    private static readonly Dictionary<string, KnownHotkeyInfo> KnownHotkeys = BuildCatalog();

    public static KnownHotkeyInfo UnknownOccupied { get; } = new(
        "未知应用",
        "该快捷键已被注册，但 Windows 未公开占用应用信息。正在尝试通过当前运行的进程列表推断占用方。");

    public static KnownHotkeyInfo Available { get; } = new("无", "该快捷键当前可注册为全局快捷键。");

    public static KnownHotkeyInfo Invalid { get; } = new("无", "快捷键格式无效。");

    public static KnownHotkeyInfo Lookup(HotkeyDefinition hotkey)
    {
        return KnownHotkeys.TryGetValue(hotkey.DisplayText, out var info)
            ? info
            : UnknownOccupied;
    }

    public static IEnumerable<KeyValuePair<string, KnownHotkeyInfo>> GetAllEntries() => KnownHotkeys;

    private static Dictionary<string, KnownHotkeyInfo> BuildCatalog()
    {
        var d = new Dictionary<string, KnownHotkeyInfo>(StringComparer.OrdinalIgnoreCase);

        AddSystemHotkeys(d);
        AddBrowserHotkeys(d);
        AddScreenshotRecordingHotkeys(d);
        AddCommunicationHotkeys(d);
        AddDeveloperHotkeys(d);
        AddProductivityHotkeys(d);
        AddLauncherHotkeys(d);
        AddMediaHotkeys(d);
        AddGamingHotkeys(d);
        AddThirdPartyHotkeys(d);

        return d;
    }

    private static void AddSystemHotkeys(Dictionary<string, KnownHotkeyInfo> d)
    {
        // Win key - desktop / shell
        d["Win+D"] = new("Windows 资源管理器", "显示或隐藏桌面。", ["explorer"], "系统");
        d["Win+E"] = new("Windows 文件资源管理器", "打开文件资源管理器窗口。", ["explorer"], "系统");
        d["Win+I"] = new("Windows 设置", "打开系统设置应用。", ["SystemSettings"], "系统");
        d["Win+L"] = new("Windows 登录界面", "锁定电脑，返回登录屏幕。", ["LogonUI"], "系统");
        d["Win+R"] = new("Windows", "打开“运行”对话框。", ["ShellExperienceHost"], "系统");
        d["Win+S"] = new("Windows 搜索", "打开 Windows 搜索侧栏（或搜索主页）。", ["SearchHost", "SearchApp"], "系统");
        d["Win+V"] = new("Windows 剪贴板历史", "打开剪贴板历史记录面板。", ["ShellExperienceHost"], "系统");
        d["Win+X"] = new("Windows", "打开快速链接菜单（开始按钮右键菜单）。", ["ShellExperienceHost"], "系统");

        // Win + letter
        d["Win+A"] = new("Windows 快速设置", "打开快速设置面板（网络/音量/亮度等）。", ["ShellExperienceHost"], "系统");
        d["Win+B"] = new("Windows 任务栏", "聚焦任务栏通知区域（系统托盘），可使用方向键操作。", ["explorer"], "系统");
        d["Win+C"] = new("Microsoft Copilot", "打开 Microsoft Copilot 侧边栏。", ["Copilot"], "系统");
        d["Win+F"] = new("Windows 反馈中心", "打开反馈中心应用。", ["FeedbackHub"], "系统");
        d["Win+G"] = new("Xbox Game Bar", "打开 Xbox Game Bar 游戏覆盖层。", ["GameBar"], "系统");
        d["Win+H"] = new("Windows 语音输入", "打开语音输入（听写）功能。", ["ShellExperienceHost"], "系统");
        d["Win+K"] = new("Windows 投屏", "打开投屏到无线显示器/音频设备侧栏。", ["ShellExperienceHost"], "系统");
        d["Win+N"] = new("Windows 通知中心", "打开通知中心和日历面板。", ["ShellExperienceHost"], "系统");
        d["Win+O"] = new("Windows 锁定方向", "锁定或解锁设备显示方向（平板模式）。", ["explorer"], "系统");
        d["Win+P"] = new("Windows 投影", "打开投影选项侧栏（仅屏幕/复制/扩展/仅第二屏幕）。", ["ShellExperienceHost"], "系统");
        d["Win+T"] = new("Windows 任务栏", "循环浏览任务栏中的应用预览。", ["explorer"], "系统");
        d["Win+W"] = new("Windows 小组件", "打开小组件面板（新闻/天气/日历等）。", ["Widgets"], "系统");
        d["Win+Y"] = new("Windows", "在桌面版和 Mixed Reality 版之间切换输入。", [], "系统");
        d["Win+Z"] = new("Windows 贴靠布局", "打开窗口贴靠布局选择器。", ["ShellExperienceHost"], "系统");

        // Win + punctuation
        d["Win+Comma"] = new("Windows", "临时速览桌面（按住时），松开后恢复。", ["explorer"], "系统");
        d["Win+Period"] = new("Windows 表情符号面板", "打开表情符号/颜文字/剪贴板面板。", ["ShellExperienceHost"], "系统");
        d["Win+Semicolon"] = new("Windows 表情符号面板", "打开表情符号/颜文字/剪贴板面板（同 Win+.）。", ["ShellExperienceHost"], "系统");

        // Win + Shift
        d["Win+Shift+S"] = new("截图工具", "打开屏幕截图选择工具栏（矩形/任意形状/窗口/全屏）。", ["SnippingTool"], "系统");
        d["Win+Shift+M"] = new("Windows", "还原所有最小化窗口。", ["explorer"], "系统");
        d["Win+Shift+T"] = new("Windows 任务栏", "循环浏览任务栏中的应用预览（反向顺序）。", ["explorer"], "系统");
        d["Win+Shift+Q"] = new("Windows", "打开或关闭自动 HDR（如硬件支持）。", [], "系统");
        d["Win+Shift+Up"] = new("Windows", "将活动窗口垂直拉伸到顶部和底部边缘。", ["explorer"], "系统");
        d["Win+Shift+Down"] = new("Windows", "还原垂直拉伸的窗口，或最小化窗口。", ["explorer"], "系统");
        d["Win+Shift+Left"] = new("Windows", "将活动窗口移动到左侧显示器。", ["explorer"], "系统");
        d["Win+Shift+Right"] = new("Windows", "将活动窗口移动到右侧显示器。", ["explorer"], "系统");

        // Win + Ctrl
        d["Win+Ctrl+D"] = new("Windows", "创建新的虚拟桌面。", ["explorer"], "系统");
        d["Win+Ctrl+F4"] = new("Windows", "关闭当前虚拟桌面。", ["explorer"], "系统");
        d["Win+Ctrl+Left"] = new("Windows", "切换到左侧虚拟桌面。", ["explorer"], "系统");
        d["Win+Ctrl+Right"] = new("Windows", "切换到右侧虚拟桌面。", ["explorer"], "系统");
        d["Win+Ctrl+F"] = new("Windows", "在域网络中搜索计算机。", ["explorer"], "系统");
        d["Win+Ctrl+Shift+B"] = new("Windows 图形驱动", "重置图形驱动程序（屏幕会闪烁）。", [], "系统");

        // Win + Alt
        d["Win+Alt+D"] = new("Windows 任务栏", "显示或隐藏桌面上的日期和时间。", ["explorer"], "系统");
        d["Win+Alt+R"] = new("Xbox Game Bar", "开始或停止屏幕录制。", ["GameBar"], "系统");
        d["Win+Alt+G"] = new("Xbox Game Bar", "录制最近 30 秒的游戏或应用画面（后台录制）。", ["GameBar"], "系统");
        d["Win+Alt+T"] = new("Xbox Game Bar", "显示或隐藏录制计时器。", ["GameBar"], "系统");
        d["Win+Alt+PrtSc"] = new("Xbox Game Bar", "截取当前游戏或应用窗口的屏幕截图。", ["GameBar"], "系统");
        d["Win+Alt+B"] = new("Xbox Game Bar", "打开或关闭 HDR 校准。", ["GameBar"], "系统");
        d["Win+Alt+K"] = new("Xbox Game Bar", "静音或取消静音麦克风。", ["GameBar"], "系统");

        // Win + number (taskbar)
        for (var i = 1; i <= 9; i++)
        {
            d[$"Win+{i}"] = new("Windows 任务栏", $"打开或切换到任务栏上第 {i} 个固定的应用。", ["explorer"], "系统");
            d[$"Win+Shift+{i}"] = new("Windows 任务栏", $"打开任务栏上第 {i} 个应用的新实例。", ["explorer"], "系统");
            d[$"Win+Alt+{i}"] = new("Windows 任务栏", $"打开任务栏上第 {i} 个应用的跳转列表。", ["explorer"], "系统");
            d[$"Win+Ctrl+{i}"] = new("Windows 任务栏", $"切换到任务栏上第 {i} 个应用的最后一个活动窗口。", ["explorer"], "系统");
        }
        d["Win+0"] = new("Windows 任务栏", "打开或切换到任务栏上第 10 个固定的应用。", ["explorer"], "系统");
        d["Win+Shift+0"] = new("Windows 任务栏", "打开任务栏上第 10 个应用的新实例。", ["explorer"], "系统");
        d["Win+Alt+0"] = new("Windows 任务栏", "打开任务栏上第 10 个应用的跳转列表。", ["explorer"], "系统");
        d["Win+Ctrl+0"] = new("Windows 任务栏", "切换到任务栏上第 10 个应用的最后一个活动窗口。", ["explorer"], "系统");

        // Alt combinations
        d["Alt+Tab"] = new("Windows 任务切换器", "切换打开的窗口（按住 Alt 再按 Tab 循环）。", ["explorer"], "系统");
        d["Alt+Shift+Tab"] = new("Windows 任务切换器", "反向切换打开的窗口。", ["explorer"], "系统");
        d["Alt+F4"] = new("Windows", "关闭当前活动窗口或应用；在桌面按此键打开关机对话框。", ["explorer"], "系统");
        // Alt+Space is a per-app system menu shortcut, not a global hotkey registration.
        // The global hotkey version (launchers) is defined in AddLauncherHotkeys.
        d["Alt+Enter"] = new("Windows 资源管理器 / 应用", "打开选中项目的属性对话框；在命令提示符中切换全屏。属于前台应用内快捷键，一般不注册为全局热键。", ["explorer"], "系统");
        d["Alt+Esc"] = new("Windows", "按打开顺序循环切换窗口项。", ["explorer"], "系统");
        d["Alt+Shift"] = new("Windows 输入法", "切换输入法/键盘布局（如已配置）。", ["ctfmon"], "系统");

        // Ctrl + Alt combinations
        d["Ctrl+Alt+Delete"] = new("Windows 安全界面", "打开安全选项屏幕（锁定/切换用户/注销/任务管理器/更改密码）。", ["winlogon"], "系统");
        d["Ctrl+Shift+Esc"] = new("任务管理器 / 其他工具", "直接打开任务管理器。也被某些工具用作全局快捷键覆盖，如 Everything、TranslucentTB、MPC-HC/BE 等。", ["Taskmgr", "Everything", "TranslucentTB", "mpc-hc", "mpc-be64"], "系统");
        d["F1"] = new("Windows/应用帮助", "打开当前应用的帮助文档；在资源管理器中按 F1 打开 Windows 帮助。", [], "系统");
        d["F2"] = new("Windows 资源管理器", "重命名选中的文件或文件夹。", [], "系统");
        d["F3"] = new("Windows 搜索", "在资源管理器中打开搜索框；在浏览器中打开查找栏。", [], "系统");
        d["F4"] = new("Windows 资源管理器", "在资源管理器中打开地址栏下拉列表；Alt+F4 关闭窗口。", [], "系统");
        d["F5"] = new("Windows/浏览器", "刷新当前窗口或页面。", [], "系统");
        d["F6"] = new("Windows/浏览器", "循环浏览窗口或页面中的可聚焦元素（地址栏、标签页、内容区）。", [], "系统");
        d["F10"] = new("Windows", "激活当前的菜单栏（如文件/编辑/查看等）。", [], "系统");
        d["F11"] = new("Windows/浏览器", "切换全屏模式。", [], "系统");

        // PrintScreen
        d["PrintScreen"] = new("Windows 截图 / 第三方截图工具", "截取整个屏幕到剪贴板；如安装了 ShareX、Greenshot 等，可能被其接管并打开截图工具。", ["ShareX", "Greenshot"], "系统");

        // Other useful
        d["Ctrl+Esc"] = new("Windows", "打开开始菜单（等效于按 Win 键）。", ["ShellExperienceHost"], "系统");
    }

    private static void AddBrowserHotkeys(Dictionary<string, KnownHotkeyInfo> d)
    {
        // Common browser hotkeys - many browsers register these
        d["Ctrl+T"] = new("浏览器", "打开新标签页。", ["chrome", "msedge", "firefox", "brave", "opera", "iexplore"], "浏览器");
        d["Ctrl+N"] = new("浏览器", "打开新窗口。", ["chrome", "msedge", "firefox", "brave", "opera", "iexplore"], "浏览器");
        d["Ctrl+W"] = new("浏览器", "关闭当前标签页。", ["chrome", "msedge", "firefox", "brave", "opera", "iexplore"], "浏览器");
        d["Ctrl+Shift+T"] = new("浏览器", "重新打开最近关闭的标签页。", ["chrome", "msedge", "firefox", "brave", "opera"], "浏览器");
        d["Ctrl+Tab"] = new("浏览器", "切换到下一个标签页。", ["chrome", "msedge", "firefox", "brave", "opera"], "浏览器");
        d["Ctrl+Shift+Tab"] = new("浏览器", "切换到上一个标签页。", ["chrome", "msedge", "firefox", "brave", "opera"], "浏览器");
        d["Ctrl+D"] = new("浏览器", "将当前页面添加为书签。", ["chrome", "msedge", "firefox", "brave", "opera"], "浏览器");
        d["Ctrl+Shift+B"] = new("浏览器", "显示或隐藏书签栏。", ["chrome", "msedge", "firefox", "brave", "opera"], "浏览器");
        d["Ctrl+H"] = new("浏览器", "打开浏览历史记录。", ["chrome", "msedge", "firefox", "brave", "opera"], "浏览器");
        d["Ctrl+J"] = new("浏览器", "打开下载记录。", ["chrome", "msedge", "firefox", "brave", "opera"], "浏览器");
        d["Ctrl+L"] = new("浏览器", "聚焦地址栏。", ["chrome", "msedge", "firefox", "brave", "opera"], "浏览器");
        d["Ctrl+F"] = new("浏览器", "在当前页面查找文本。", ["chrome", "msedge", "firefox", "brave", "opera"], "浏览器");
        d["Ctrl+P"] = new("浏览器", "打印当前页面。", ["chrome", "msedge", "firefox", "brave", "opera", "iexplore"], "浏览器");
        d["Ctrl+S"] = new("浏览器", "保存当前页面到本地。", ["chrome", "msedge", "firefox", "brave", "opera"], "浏览器");
        d["Ctrl+U"] = new("浏览器", "查看当前页面源代码。", ["chrome", "msedge", "firefox", "brave", "opera"], "浏览器");
        d["Ctrl+R"] = new("浏览器", "刷新当前页面（等效于 F5）。", ["chrome", "msedge", "firefox", "brave", "opera"], "浏览器");
        d["Ctrl+Shift+R"] = new("浏览器", "强制刷新（忽略缓存）。", ["chrome", "msedge", "firefox", "brave", "opera"], "浏览器");
        d["Ctrl+Shift+I"] = new("浏览器开发者工具", "打开开发者工具（DevTools）。", ["chrome", "msedge", "firefox", "brave", "opera"], "浏览器");
        d["Ctrl+Shift+J"] = new("浏览器", "打开控制台（DevTools Console）。", ["chrome", "msedge", "brave", "opera"], "浏览器");
        d["Ctrl+Shift+Delete"] = new("浏览器", "打开清除浏览数据对话框。", ["chrome", "msedge", "firefox", "brave", "opera"], "浏览器");

        // Numbered tab switching
        for (var i = 1; i <= 8; i++)
        {
            d[$"Ctrl+{i}"] = new("浏览器", $"切换到第 {i} 个标签页。", ["chrome", "msedge", "firefox", "brave", "opera"], "浏览器");
        }
        d["Ctrl+9"] = new("浏览器", "切换到最后一个标签页。", ["chrome", "msedge", "firefox", "brave", "opera"], "浏览器");
    }

    private static void AddScreenshotRecordingHotkeys(Dictionary<string, KnownHotkeyInfo> d)
    {
        // ShareX
        d["Shift+PrintScreen"] = new("ShareX", "截取矩形区域。", ["ShareX"], "截图/录制");
        d["Alt+PrintScreen"] = new("ShareX", "截取活动窗口。", ["ShareX"], "截图/录制");
        d["Ctrl+PrintScreen"] = new("ShareX", "截取窗口/控件。", ["ShareX"], "截图/录制");

        // Snipaste
        d["F1"] = new("Snipaste", "开始截图。", ["Snipaste"], "截图/录制");
        d["F3"] = new("Snipaste", "将截取的图片贴到屏幕上（贴图）。", ["Snipaste"], "截图/录制");
        d["Shift+F1"] = new("Snipaste", "截取控件/窗口。", ["Snipaste"], "截图/录制");
        d["Ctrl+F1"] = new("Snipaste", "截取矩形区域（同上）。", ["Snipaste"], "截图/录制");
        d["Ctrl+Shift+F1"] = new("Snipaste", "截取全屏。", ["Snipaste"], "截图/录制");
        d["Esc"] = new("Snipaste", "退出截图/贴图模式。", ["Snipaste"], "截图/录制");

        // Greenshot
        d["Alt+PrintScreen"] = new("Greenshot", "截取活动窗口。", ["Greenshot"], "截图/录制");
        d["Ctrl+PrintScreen"] = new("Greenshot", "截取矩形区域。", ["Greenshot"], "截图/录制");

        // OBS Studio
        d["Ctrl+Shift+F5"] = new("OBS Studio", "开始或停止录制。", ["obs64", "obs32"], "截图/录制");
        d["Ctrl+Shift+F6"] = new("OBS Studio", "开始或停止推流。", ["obs64", "obs32"], "截图/录制");
        d["Ctrl+Shift+F7"] = new("OBS Studio", "暂停录制。", ["obs64", "obs32"], "截图/录制");
        d["Ctrl+Shift+F8"] = new("OBS Studio", "切换场景。", ["obs64", "obs32"], "截图/录制");
        d["Ctrl+Shift+F9"] = new("OBS Studio", "切换场景（快速）。", ["obs64", "obs32"], "截图/录制");
        d["Ctrl+Shift+F10"] = new("OBS Studio", "隐藏或显示捕获画面。", ["obs64", "obs32"], "截图/录制");
        d["Ctrl+Shift+F11"] = new("OBS Studio", "切换全屏预览。", ["obs64", "obs32"], "截图/录制");
        d["Ctrl+Shift+F12"] = new("OBS Studio", "截取当前画面（来源截图）。", ["obs64", "obs32"], "截图/录制");

        // NVIDIA GeForce Experience / ShadowPlay
        d["Alt+F2"] = new("NVIDIA GeForce Experience", "打开游戏滤镜（Ansel/Freestyle）。", ["nvidiashare"], "截图/录制");
        d["Alt+F3"] = new("NVIDIA GeForce Experience", "切换游戏滤镜开关。", ["nvidiashare"], "截图/录制");
        d["Alt+F1"] = new("NVIDIA GeForce Experience", "截取游戏屏幕截图。", ["nvidiashare"], "截图/录制");
        d["Alt+Shift+F9"] = new("NVIDIA GeForce Experience", "开始或停止即时重放录制。", ["nvidiashare"], "截图/录制");
        d["Alt+F9"] = new("NVIDIA GeForce Experience", "开始或停止手动录制。", ["nvidiashare"], "截图/录制");
        d["Alt+Z"] = new("NVIDIA GeForce Experience", "打开 GeForce Experience 覆盖层。", ["nvidiashare"], "截图/录制");

        // AMD Adrenalin
        d["Alt+R"] = new("AMD Software: Adrenalin", "打开 AMD 驱动覆盖层。", ["amdsoftware"], "截图/录制");
        d["Alt+Shift+R"] = new("AMD Software: Adrenalin", "开始或停止录制。", ["amdsoftware"], "截图/录制");
        d["Alt+Shift+S"] = new("AMD Software: Adrenalin", "截取屏幕截图。", ["amdsoftware"], "截图/录制");
        d["Alt+Shift+L"] = new("AMD Software: Adrenalin", "打开或关闭性能叠加层。", ["amdsoftware"], "截图/录制");
        d["Alt+Shift+B"] = new("AMD Software: Adrenalin", "开始或停止即时重放。", ["amdsoftware"], "截图/录制");
    }

    private static void AddCommunicationHotkeys(Dictionary<string, KnownHotkeyInfo> d)
    {
        // Discord
        d["Ctrl+`"] = new("Discord", "切换 Discord 覆盖层。", ["discord"], "即时通讯");
        d["Ctrl+Alt+A"] = new("Discord", "静音或取消静音麦克风。", ["discord"], "即时通讯");
        d["Ctrl+Shift+A"] = new("Discord", "打开或关闭主动降噪（Krisp）。", ["discord"], "即时通讯");
        d["Ctrl+Alt+S"] = new("Discord", "静音或取消静音扬声器/耳机。", ["discord"], "即时通讯");
        d["Ctrl+Shift+F"] = new("Discord", "搜索消息。", ["discord"], "即时通讯");
        d["Ctrl+Shift+M"] = new("Discord", "静音或取消静音麦克风（同 Ctrl+Alt+A）。", ["discord"], "即时通讯");
        d["Ctrl+Shift+D"] = new("Discord", "切换到开发者模式。", ["discord"], "即时通讯");
        d["Ctrl+I"] = new("Discord", "忽略用户。", ["discord"], "即时通讯");
        d["Ctrl+B"] = new("Discord", "创建私密频道。", ["discord"], "即时通讯");
        d["Ctrl+U"] = new("Discord", "上传文件。", ["discord"], "即时通讯");
        d["Ctrl+E"] = new("Discord", "编辑最后一条消息。", ["discord"], "即时通讯");
        d["Ctrl+K"] = new("Discord", "快速跳转到频道/用户/消息。", ["discord"], "即时通讯");
        d["Ctrl+Shift+X"] = new("Discord", "切换全屏模式。", ["discord"], "即时通讯");

        // Teams
        d["Ctrl+Shift+M"] = new("Microsoft Teams", "静音或取消静音麦克风。", ["Teams"], "即时通讯");
        d["Ctrl+Shift+E"] = new("Microsoft Teams", "打开或关闭摄像头。", ["Teams"], "即时通讯");
        d["Ctrl+Shift+Space"] = new("Microsoft Teams", "举手或放下手。", ["Teams"], "即时通讯");
        d["Ctrl+Shift+K"] = new("Microsoft Teams", "共享屏幕。", ["Teams"], "即时通讯");
        d["Ctrl+Shift+H"] = new("Microsoft Teams", "显示或隐藏聊天侧栏。", ["Teams"], "即时通讯");
        d["Ctrl+Shift+Esc"] = new("Microsoft Teams", "打开活动日志。", ["Teams"], "即时通讯");
        d["Ctrl+E"] = new("Microsoft Teams", "搜索消息/频道/文件/人员。", ["Teams"], "即时通讯");
        d["Ctrl+G"] = new("Microsoft Teams", "固定或取消固定频道。", ["Teams"], "即时通讯");
        d["Ctrl+Shift+A"] = new("Microsoft Teams", "接受来电。", ["Teams"], "即时通讯");
        d["Ctrl+Shift+D"] = new("Microsoft Teams", "拒绝来电。", ["Teams"], "即时通讯");
        d["Ctrl+N"] = new("Microsoft Teams", "开始新聊天。", ["Teams"], "即时通讯");

        // Slack
        d["Ctrl+Shift+M"] = new("Slack", "静音或取消静音麦克风（通话中）。", ["slack"], "即时通讯");
        d["Ctrl+Shift+V"] = new("Slack", "打开或关闭摄像头（通话中）。", ["slack"], "即时通讯");
        d["Ctrl+K"] = new("Slack", "快速跳转到频道/用户/对话。", ["slack"], "即时通讯");
        d["Ctrl+Shift+K"] = new("Slack", "跳转到未读消息。", ["slack"], "即时通讯");
        d["Ctrl+Shift+N"] = new("Slack", "创建新频道。", ["slack"], "即时通讯");
        d["Ctrl+Shift+P"] = new("Slack", "创建私人频道。", ["slack"], "即时通讯");
        d["Ctrl+T"] = new("Slack", "跳转到频道浏览器。", ["slack"], "即时通讯");
        d["Ctrl+Shift+E"] = new("Slack", "编辑最后发送的消息。", ["slack"], "即时通讯");
        d["Ctrl+Shift+F"] = new("Slack", "搜索消息和文件。", ["slack"], "即时通讯");
        d["Ctrl+Shift+T"] = new("Slack", "切换讨论线程侧栏。", ["slack"], "即时通讯");

        // Zoom
        d["Alt+A"] = new("Zoom", "静音或取消静音麦克风。", ["Zoom"], "即时通讯");
        d["Alt+V"] = new("Zoom", "开始或停止视频。", ["Zoom"], "即时通讯");
        d["Alt+S"] = new("Zoom", "共享屏幕。", ["Zoom"], "即时通讯");
        d["Alt+Pause"] = new("Zoom", "暂停或恢复屏幕共享。", ["Zoom"], "即时通讯");
        d["Alt+R"] = new("Zoom", "开始或停止本地录制。", ["Zoom"], "即时通讯");
        d["Alt+C"] = new("Zoom", "开始或停止云录制。", ["Zoom"], "即时通讯");
        d["Alt+Shift+T"] = new("Zoom", "暂停或恢复录制。", ["Zoom"], "即时通讯");
        d["Alt+H"] = new("Zoom", "显示或隐藏聊天面板。", ["Zoom"], "即时通讯");
        d["Alt+F2"] = new("Zoom", "切换图库视图/演讲者视图。", ["Zoom"], "即时通讯");
        d["Alt+F"] = new("Zoom", "进入/退出全屏模式。", ["Zoom"], "即时通讯");
        d["Alt+Shift+M"] = new("Zoom", "切换到最小化视图。", ["Zoom"], "即时通讯");
        d["Alt+I"] = new("Zoom", "打开邀请窗口。", ["Zoom"], "即时通讯");
        d["Alt+Y"] = new("Zoom", "举手/放下手。", ["Zoom"], "即时通讯");
        d["Alt+U"] = new("Zoom", "显示参与者列表。", ["Zoom"], "即时通讯");
        d["Ctrl+Shift+Space"] = new("Zoom", "激活静音快捷键（PTT 模式）。", ["Zoom"], "即时通讯");

        // Telegram
        d["Ctrl+N"] = new("Telegram", "新建私信。", ["Telegram"], "即时通讯");
        d["Ctrl+G"] = new("Telegram", "新建群组。", ["Telegram"], "即时通讯");
        d["Ctrl+Shift+G"] = new("Telegram", "新建频道。", ["Telegram"], "即时通讯");
        d["Ctrl+K"] = new("Telegram", "跳转到对话（快速切换）。", ["Telegram"], "即时通讯");
        d["Ctrl+F"] = new("Telegram", "搜索当前对话消息。", ["Telegram"], "即时通讯");
        d["Ctrl+Shift+F"] = new("Telegram", "全局搜索消息。", ["Telegram"], "即时通讯");
        d["Ctrl+Shift+M"] = new("Telegram", "静音对话。", ["Telegram"], "即时通讯");
        d["Ctrl+Up"] = new("Telegram", "跳转到上一对话。", ["Telegram"], "即时通讯");
        d["Ctrl+Down"] = new("Telegram", "跳转到下一对话。", ["Telegram"], "即时通讯");

        // WhatsApp Desktop
        d["Ctrl+N"] = new("WhatsApp", "开始新聊天。", ["WhatsApp"], "即时通讯");
        d["Ctrl+Shift+N"] = new("WhatsApp", "创建新群组。", ["WhatsApp"], "即时通讯");
        d["Ctrl+Shift+["] = new("WhatsApp", "切换到上一个聊天。", ["WhatsApp"], "即时通讯");
        d["Ctrl+Shift+]"] = new("WhatsApp", "切换到下一个聊天。", ["WhatsApp"], "即时通讯");
        d["Ctrl+F"] = new("WhatsApp", "搜索消息。", ["WhatsApp"], "即时通讯");
        d["Ctrl+K"] = new("WhatsApp", "搜索聊天。", ["WhatsApp"], "即时通讯");
        d["Ctrl+E"] = new("WhatsApp", "存档聊天。", ["WhatsApp"], "即时通讯");
        d["Ctrl+Shift+M"] = new("WhatsApp", "静音聊天。", ["WhatsApp"], "即时通讯");
        d["Ctrl+Shift+U"] = new("WhatsApp", "标记为未读。", ["WhatsApp"], "即时通讯");
        d["Ctrl+Delete"] = new("WhatsApp", "删除聊天。", ["WhatsApp"], "即时通讯");
    }

    private static void AddDeveloperHotkeys(Dictionary<string, KnownHotkeyInfo> d)
    {
        // VS Code
        d["Ctrl+Shift+P"] = new("VS Code", "打开命令面板。", ["Code"], "开发者工具");
        d["Ctrl+P"] = new("VS Code", "快速打开文件（按名称搜索）。", ["Code"], "开发者工具");
        d["Ctrl+B"] = new("VS Code", "切换侧边栏显示。", ["Code"], "开发者工具");
        d["Ctrl+`"] = new("VS Code", "打开或关闭终端面板。", ["Code"], "开发者工具");
        d["Ctrl+Shift+`"] = new("VS Code", "创建新的终端实例。", ["Code"], "开发者工具");
        d["Ctrl+Shift+E"] = new("VS Code", "聚焦资源管理器（文件树）。", ["Code"], "开发者工具");
        d["Ctrl+Shift+X"] = new("VS Code", "打开扩展商店面板。", ["Code"], "开发者工具");
        d["Ctrl+Shift+G"] = new("VS Code", "打开源代码管理（Git）面板。", ["Code"], "开发者工具");
        d["Ctrl+Shift+D"] = new("VS Code", "打开运行和调试面板。", ["Code"], "开发者工具");
        d["Ctrl+Shift+F"] = new("VS Code", "全局搜索文件内容。", ["Code"], "开发者工具");
        d["Ctrl+H"] = new("VS Code", "查找替换。", ["Code"], "开发者工具");
        d["Ctrl+Shift+H"] = new("VS Code", "全局查找替换。", ["Code"], "开发者工具");
        d["Ctrl+Shift+O"] = new("VS Code", "跳转到符号定义。", ["Code"], "开发者工具");
        d["Ctrl+G"] = new("VS Code", "跳转到指定行。", ["Code"], "开发者工具");
        d["Ctrl+Shift+\\"] = new("VS Code", "跳转到匹配的括号。", ["Code"], "开发者工具");
        d["Ctrl+Shift+]"] = new("VS Code", "展开或折叠代码区域。", ["Code"], "开发者工具");
        d["Ctrl+D"] = new("VS Code", "选择下一个匹配项（多光标编辑）。", ["Code"], "开发者工具");
        d["Ctrl+Shift+L"] = new("VS Code", "选择所有匹配项（多光标编辑）。", ["Code"], "开发者工具");
        d["Alt+Up"] = new("VS Code", "向上移动当前行。", ["Code"], "开发者工具");
        d["Alt+Down"] = new("VS Code", "向下移动当前行。", ["Code"], "开发者工具");
        d["Alt+Shift+Up"] = new("VS Code", "向上复制当前行。", ["Code"], "开发者工具");
        d["Alt+Shift+Down"] = new("VS Code", "向下复制当前行。", ["Code"], "开发者工具");
        d["Ctrl+Shift+K"] = new("VS Code", "删除当前行。", ["Code"], "开发者工具");
        d["Ctrl+Enter"] = new("VS Code", "在当前行下方插入新行。", ["Code"], "开发者工具");
        d["Ctrl+Shift+Enter"] = new("VS Code", "在当前行上方插入新行。", ["Code"], "开发者工具");
        d["Ctrl+/"] = new("VS Code", "切换行注释。", ["Code"], "开发者工具");
        d["Ctrl+Shift+/"] = new("VS Code", "切换块注释。", ["Code"], "开发者工具");
        d["F5"] = new("VS Code", "开始调试。", ["Code"], "开发者工具");
        d["Ctrl+F5"] = new("VS Code", "开始执行（不调试）。", ["Code"], "开发者工具");
        d["Shift+F5"] = new("VS Code", "停止调试。", ["Code"], "开发者工具");
        d["F9"] = new("VS Code", "切换断点。", ["Code"], "开发者工具");
        d["F10"] = new("VS Code", "单步跳过。", ["Code"], "开发者工具");
        d["F11"] = new("VS Code", "单步进入。", ["Code"], "开发者工具");
        d["Shift+F11"] = new("VS Code", "单步跳出。", ["Code"], "开发者工具");
        d["Ctrl+Shift+5"] = new("VS Code", "拆分编辑器窗口。", ["Code"], "开发者工具");

        // Visual Studio
        d["Ctrl+Shift+B"] = new("Visual Studio", "生成解决方案。", ["devenv"], "开发者工具");
        d["F5"] = new("Visual Studio", "开始调试。", ["devenv"], "开发者工具");
        d["Ctrl+F5"] = new("Visual Studio", "开始执行（不调试）。", ["devenv"], "开发者工具");
        d["Shift+F5"] = new("Visual Studio", "停止调试。", ["devenv"], "开发者工具");
        d["F9"] = new("Visual Studio", "切换断点。", ["devenv"], "开发者工具");
        d["F10"] = new("Visual Studio", "单步跳过。", ["devenv"], "开发者工具");
        d["F11"] = new("Visual Studio", "单步进入。", ["devenv"], "开发者工具");
        d["Shift+F11"] = new("Visual Studio", "单步跳出。", ["devenv"], "开发者工具");
        d["Ctrl+K, Ctrl+D"] = new("Visual Studio", "格式化文档。", ["devenv"], "开发者工具");
        d["Ctrl+K, Ctrl+C"] = new("Visual Studio", "注释选定代码。", ["devenv"], "开发者工具");
        d["Ctrl+K, Ctrl+U"] = new("Visual Studio", "取消注释选定代码。", ["devenv"], "开发者工具");
        d["Ctrl+M, Ctrl+M"] = new("Visual Studio", "展开或折叠代码区域。", ["devenv"], "开发者工具");
        d["Ctrl+R, Ctrl+W"] = new("Visual Studio", "显示或隐藏监视窗口。", ["devenv"], "开发者工具");
        d["Ctrl+Alt+E"] = new("Visual Studio", "打开异常设置窗口。", ["devenv"], "开发者工具");
        d["Ctrl+Alt+L"] = new("Visual Studio", "打开解决方案资源管理器。", ["devenv"], "开发者工具");
        d["Ctrl+\\, Ctrl+E"] = new("Visual Studio", "打开错误列表。", ["devenv"], "开发者工具");
        d["Ctrl+Alt+X"] = new("Visual Studio", "打开工具箱。", ["devenv"], "开发者工具");
        d["Ctrl+\\, M"] = new("Visual Studio", "显示或隐藏选项卡式文档。", ["devenv"], "开发者工具");

        // JetBrains IDEs (IntelliJ, Rider, WebStorm, PyCharm, etc.)
        d["Ctrl+Shift+A"] = new("JetBrains IDE", "查找操作（打开所有命令搜索）。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Ctrl+N"] = new("JetBrains IDE", "按名称搜索类。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Ctrl+Shift+N"] = new("JetBrains IDE", "按名称搜索文件。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Ctrl+Alt+L"] = new("JetBrains IDE", "格式化代码。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Ctrl+Alt+Shift+L"] = new("JetBrains IDE", "显示格式化对话框。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Ctrl+R"] = new("JetBrains IDE", "替换当前文件内容。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Ctrl+Shift+R"] = new("JetBrains IDE", "全局替换。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["F12"] = new("JetBrains IDE", "跳转到上一个打开的文件。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Ctrl+F12"] = new("JetBrains IDE", "显示文件结构。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Shift+F6"] = new("JetBrains IDE", "重命名符号（重构）。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Ctrl+Alt+O"] = new("JetBrains IDE", "优化导入。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Ctrl+E"] = new("JetBrains IDE", "最近打开的文件。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Ctrl+Shift+E"] = new("JetBrains IDE", "最近编辑的文件。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Shift+F10"] = new("JetBrains IDE", "运行当前配置。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Shift+F9"] = new("JetBrains IDE", "调试当前配置。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Ctrl+Shift+F10"] = new("JetBrains IDE", "运行上下文最近的配置。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Ctrl+D"] = new("JetBrains IDE", "复制当前行或选定内容。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Ctrl+Y"] = new("JetBrains IDE", "删除当前行。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Ctrl+Shift+Up"] = new("JetBrains IDE", "向上移动语句（代码移动）。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Ctrl+Shift+Down"] = new("JetBrains IDE", "向下移动语句。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Ctrl+Alt+B"] = new("JetBrains IDE", "跳转到实现（实现类/方法）。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Ctrl+U"] = new("JetBrains IDE", "跳转到父类/父方法。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Ctrl+Shift+I"] = new("JetBrains IDE", "查看定义（内联预览）。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Alt+F7"] = new("JetBrains IDE", "查找用法。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Ctrl+B"] = new("JetBrains IDE", "跳转到声明。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");
        d["Ctrl+F8"] = new("JetBrains IDE", "切换断点。", ["jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64"], "开发者工具");

        // Windows Terminal
        d["Ctrl+Shift+1"] = new("Windows Terminal", "打开新终端标签页（Profile 1）。", ["WindowsTerminal"], "开发者工具");
        d["Ctrl+Shift+2"] = new("Windows Terminal", "打开新终端标签页（Profile 2）。", ["WindowsTerminal"], "开发者工具");
        d["Ctrl+Shift+D"] = new("Windows Terminal", "复制当前标签页（拆分窗格）。", ["WindowsTerminal"], "开发者工具");
        d["Ctrl+Shift+W"] = new("Windows Terminal", "关闭当前标签页或窗格。", ["WindowsTerminal"], "开发者工具");
        d["Alt+Shift+D"] = new("Windows Terminal", "垂直拆分当前窗格。", ["WindowsTerminal"], "开发者工具");
        d["Alt+Shift+-"] = new("Windows Terminal", "水平拆分当前窗格。", ["WindowsTerminal"], "开发者工具");
        d["Ctrl+Shift+Space"] = new("Windows Terminal", "打开搜索（查找终端输出）。", ["WindowsTerminal"], "开发者工具");
        d["Ctrl+,"] = new("Windows Terminal", "打开设置页面。", ["WindowsTerminal"], "开发者工具");
    }

    private static void AddProductivityHotkeys(Dictionary<string, KnownHotkeyInfo> d)
    {
        // Microsoft PowerToys
        d["Alt+Space"] = new("PowerToys PowerToys Run", "打开 PowerToys Run 快速启动器。", ["PowerToys"], "生产力工具");
        d["Win+Ctrl+T"] = new("PowerToys", "打开 PowerToys Peek（快速预览文件）。", ["PowerToys"], "生产力工具");
        d["Win+Ctrl+E"] = new("PowerToys", "打开 PowerToys 颜色选择器。", ["PowerToys"], "生产力工具");
        d["Win+Shift+T"] = new("PowerToys", "打开文本提取器（OCR 截图识别文字）。", ["PowerToys"], "生产力工具");
        d["Win+Shift+C"] = new("PowerToys", "打开颜色选择器。", ["PowerToys"], "生产力工具");
        d["Win+Ctrl+Shift+Q"] = new("PowerToys", "打开视频会议静音（麦克风/摄像头一键控制）。", ["PowerToys"], "生产力工具");
        d["Win+Ctrl+Alt+F"] = new("PowerToys", "将鼠标指针置于焦点中心。", ["PowerToys"], "生产力工具");
        d["Win+Ctrl+Alt+T"] = new("PowerToys", "始终置顶当前窗口。", ["PowerToys"], "生产力工具");
        d["Win+Ctrl+Shift+R"] = new("PowerToys", "运行 PowerRename（批量重命名）。", ["PowerToys"], "生产力工具");
        d["Win+Shift+M"] = new("PowerToys", "打开鼠标高亮/点击指示器。", ["PowerToys"], "生产力工具");

        // AutoHotkey (user-defined, common defaults)
        d["Ctrl+Alt+O"] = new("AutoHotkey（用户自定义）", "打开常用文件夹或应用（取决于用户脚本定义）。", ["AutoHotkey64", "AutoHotkey32", "AutoHotkeyUX"], "生产力工具");
        d["Ctrl+Alt+K"] = new("AutoHotkey（用户自定义）", "搜索选中文本在 Google 中（取决于用户脚本定义）。", ["AutoHotkey64", "AutoHotkey32", "AutoHotkeyUX"], "生产力工具");
        d["Ctrl+Alt+C"] = new("AutoHotkey（用户自定义）", "复制选中文本为纯文本（取决于用户脚本定义）。", ["AutoHotkey64", "AutoHotkey32", "AutoHotkeyUX"], "生产力工具");

        // Everything (voidtools)
        d["Ctrl+Shift+F"] = new("Everything", "打开 Everything 搜索窗口。", ["Everything"], "生产力工具");
        d["Win+Shift+F"] = new("Everything", "打开 Everything 搜索窗口（可选配置）。", ["Everything"], "生产力工具");

        // Ditto Clipboard Manager
        d["Ctrl+`"] = new("Ditto 剪贴板管理器", "打开 Ditto 剪贴板历史窗口。", ["Ditto"], "生产力工具");
        d["Ctrl+Shift+V"] = new("Ditto 剪贴板管理器", "以 Ditto 菜单粘贴（替代系统粘贴）。", ["Ditto"], "生产力工具");

        // EarTrumpet (volume control)
        d["Ctrl+Shift+E"] = new("EarTrumpet", "打开 EarTrumpet 音量控制面板。", ["EarTrumpet"], "生产力工具");

        // Listary (file search)
        d["Ctrl+G"] = new("Listary", "跳转到文件管理器中当前路径。", ["Listary"], "生产力工具");
        d["Ctrl+Shift+K"] = new("Listary", "打开 Listary 快速搜索。", ["Listary"], "生产力工具");
        d["Ctrl+Shift+F"] = new("Listary", "在文件管理器中搜索当前文件夹。", ["Listary"], "生产力工具");
        d["Win+F"] = new("Listary", "打开 Listary 搜索窗口（可选配置）。", ["Listary"], "生产力工具");

        // DisplayFusion
        d["Win+Ctrl+Shift+Left"] = new("DisplayFusion", "将窗口移动到左显示器并跨越。", ["DisplayFusion"], "生产力工具");
        d["Win+Ctrl+Shift+Right"] = new("DisplayFusion", "将窗口移动到右显示器并跨越。", ["DisplayFusion"], "生产力工具");
        d["Win+Ctrl+Shift+Up"] = new("DisplayFusion", "将窗口移动到上方显示器并跨越。", ["DisplayFusion"], "生产力工具");
        d["Win+Ctrl+Shift+Down"] = new("DisplayFusion", "将窗口移动到下方显示器并跨越。", ["DisplayFusion"], "生产力工具");
        d["Win+Ctrl+Shift+M"] = new("DisplayFusion", "将窗口移动到下一个显示器。", ["DisplayFusion"], "生产力工具");

        // Wox (launcher)
        d["Alt+Space"] = new("Wox", "打开 Wox 快速启动器。", ["Wox"], "生产力工具");
    }

    private static void AddLauncherHotkeys(Dictionary<string, KnownHotkeyInfo> d)
    {
        // General launcher hotkeys
        d["Alt+Space"] = new("快速启动器（通用）", "打开应用启动器——PowerToys Run/Flow Launcher/Wox/Keyboad 等。", ["PowerToys", "flow.launcher", "Wox", "Keyboad"], "启动器");
    }

    private static void AddMediaHotkeys(Dictionary<string, KnownHotkeyInfo> d)
    {
        // Spotify
        d["Ctrl+Left"] = new("Spotify", "上一首。", ["Spotify"], "媒体");
        d["Ctrl+Right"] = new("Spotify", "下一首。", ["Spotify"], "媒体");
        d["Ctrl+Up"] = new("Spotify", "增加音量。", ["Spotify"], "媒体");
        d["Ctrl+Down"] = new("Spotify", "减小音量。", ["Spotify"], "媒体");
        d["Ctrl+Shift+Left"] = new("Spotify", "快退。", ["Spotify"], "媒体");
        d["Ctrl+Shift+Right"] = new("Spotify", "快进。", ["Spotify"], "媒体");
        d["Alt+Left"] = new("Spotify", "返回到上一个页面。", ["Spotify"], "媒体");
        d["Alt+Right"] = new("Spotify", "前进到下一个页面。", ["Spotify"], "媒体");
        d["Ctrl+L"] = new("Spotify", "聚焦搜索框。", ["Spotify"], "媒体");

        // MPC-HC / MPC-BE (empty, shares Ctrl+Shift+Esc with Task Manager)
    }

    private static void AddGamingHotkeys(Dictionary<string, KnownHotkeyInfo> d)
    {
        // Steam
        d["Shift+Tab"] = new("Steam", "在游戏中打开 Steam 覆盖层。", ["Steam"], "游戏");
        d["Ctrl+Shift+F"] = new("Steam", "在游戏内显示 FPS 计数器（如启用）。", ["Steam"], "游戏");
        d["Ctrl+Tab"] = new("Steam", "在 Steam 界面中切换视图。", ["Steam"], "游戏");

        // Xbox Game Bar (already added in system section)

        // SteelSeries GG / Sonar
        d["Alt+Shift+E"] = new("SteelSeries GG", "打开或关闭 Sonar 均衡器。", ["SteelSeriesGG"], "游戏");
    }

    private static void AddThirdPartyHotkeys(Dictionary<string, KnownHotkeyInfo> d)
    {
        // Logitech G HUB
        d["Ctrl+Shift+F1"] = new("Logitech G HUB", "切换 G HUB 配置（鼠标灵敏度/DPI 切换）。", ["lghub"], "外设驱动");
        d["Ctrl+Shift+F2"] = new("Logitech G HUB", "切换 G HUB 配置 2。", ["lghub"], "外设驱动");
        d["Ctrl+Shift+F3"] = new("Logitech G HUB", "切换 G HUB 配置 3。", ["lghub"], "外设驱动");
        d["Ctrl+Shift+F4"] = new("Logitech G HUB", "切换 G HUB 配置 4。", ["lghub"], "外设驱动");

        // Razer Synapse
        d["Ctrl+Shift+F10"] = new("Razer Synapse", "切换 Razer 设备配置文件。", ["RazerSynapse", "RazerSynapse3"], "外设驱动");
        d["Ctrl+Shift+F11"] = new("Razer Synapse", "切换 Razer 灯光效果。", ["RazerSynapse", "RazerSynapse3"], "外设驱动");

        // Corsair iCUE
        d["Ctrl+Shift+F12"] = new("Corsair iCUE", "切换 iCUE 灯光配置文件。", ["iCUE"], "外设驱动");
        d["Ctrl+Shift+F10"] = new("Corsair iCUE", "切换 iCUE 硬件配置文件。", ["iCUE"], "外设驱动");

        // TranslucentTB (empty, shares Ctrl+Shift+Esc with Task Manager)
    }
}
