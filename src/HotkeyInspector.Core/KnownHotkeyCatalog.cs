namespace HotkeyInspector.Core;

public static class KnownHotkeyCatalog
{
    private static readonly Dictionary<string, List<KnownHotkeyInfo>> KnownHotkeys = BuildCatalog();

    public static KnownHotkeyInfo UnknownOccupied { get; } = new(
        new LocalizedText("未知应用", "Unknown app"),
        new LocalizedText(
            "该快捷键已被注册，但 Windows 未公开占用应用信息。正在尝试通过当前运行的进程列表推断占用方。",
            "This shortcut is registered, but Windows did not report the owning app. Trying to infer it from the running process list."));

    public static KnownHotkeyInfo Available { get; } = new(
        new LocalizedText("无", "None"),
        new LocalizedText("该快捷键当前可注册为全局快捷键。", "This shortcut can currently be registered as a global hotkey."));

    public static KnownHotkeyInfo Invalid { get; } = new(
        new LocalizedText("无", "None"),
        new LocalizedText("快捷键格式无效。", "The shortcut format is invalid."));

    /// <summary>目录中保留的全部归属记录数（同一组合可以有多条）。</summary>
    public static int EntryCount => KnownHotkeys.Sum(pair => pair.Value.Count);

    /// <summary>存在多个归属的组合数量。</summary>
    public static int ConflictCount => KnownHotkeys.Count(pair => pair.Value.Count > 1);

    public static KnownHotkeyInfo Lookup(HotkeyDefinition hotkey) => Lookup(hotkey.DisplayText);

    /// <summary>
    /// 按归一化键位查找占用方。多个应用共用同一组合时，返回优先级最高者
    /// （系统 &gt; 浏览器 &gt; 截图/录制 &gt; 即时通讯 &gt; 开发者工具 &gt; 生产力 &gt; 启动器 &gt; 媒体 &gt; 游戏 &gt; 外设）。
    /// </summary>
    public static KnownHotkeyInfo Lookup(string combo)
    {
        var matches = LookupAll(combo);
        return matches.Count > 0 ? matches[0] : UnknownOccupied;
    }

    public static IReadOnlyList<KnownHotkeyInfo> LookupAll(string combo)
    {
        var key = ComboKey.Normalize(combo);
        return KnownHotkeys.TryGetValue(key, out var list) ? list : [];
    }

    public static IEnumerable<KeyValuePair<string, KnownHotkeyInfo>> GetAllEntries()
        => KnownHotkeys.SelectMany(pair => pair.Value.Select(info => new KeyValuePair<string, KnownHotkeyInfo>(pair.Key, info)));

    private static Dictionary<string, List<KnownHotkeyInfo>> BuildCatalog()
    {
        var d = new Dictionary<string, List<KnownHotkeyInfo>>(StringComparer.Ordinal);

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

        foreach (var key in d.Keys.ToList())
        {
            d[key] = d[key].OrderBy(CategoryPriority).ToList();
        }

        return d;
    }

    private static void Add(Dictionary<string, List<KnownHotkeyInfo>> d, string key, KnownHotkeyInfo info)
    {
        var normalized = ComboKey.Normalize(key);
        if (normalized.Length == 0)
        {
            return;
        }

        if (!d.TryGetValue(normalized, out var list))
        {
            list = [];
            d[normalized] = list;
        }

        list.Add(info);
    }

    private static int CategoryPriority(KnownHotkeyInfo info) => info.Category?.Zh switch
    {
        "系统" => 0,
        "浏览器" => 1,
        "截图/录制" => 2,
        "即时通讯" => 3,
        "开发者工具" => 4,
        "生产力工具" => 5,
        "启动器" => 6,
        "媒体" => 7,
        "游戏" => 8,
        "外设驱动" => 9,
        _ => 10
    };

    private static void AddSystemHotkeys(Dictionary<string, List<KnownHotkeyInfo>> d)
    {
        var cat = new LocalizedText("系统", "System");
        void a(string k, string oz, string oe, string uz, string ue, string[]? p = null)
            => Add(d, k, new(new LocalizedText(oz, oe), new LocalizedText(uz, ue), p, cat));

        // Win key - desktop / shell
        a("Win+D", "Windows 资源管理器", "Windows Explorer", "显示或隐藏桌面。", "Show or hide the desktop.", ["explorer"]);
        a("Win+E", "Windows 文件资源管理器", "Windows File Explorer", "打开文件资源管理器窗口。", "Open a File Explorer window.", ["explorer"]);
        a("Win+I", "Windows 设置", "Windows Settings", "打开系统设置应用。", "Open the Settings app.", ["SystemSettings"]);
        a("Win+L", "Windows 登录界面", "Windows sign-in screen", "锁定电脑，返回登录屏幕。", "Lock the PC and return to the sign-in screen.", ["LogonUI"]);
        a("Win+R", "Windows", "Windows", "打开“运行”对话框。", "Open the Run dialog box.", ["ShellExperienceHost"]);
        a("Win+S", "Windows 搜索", "Windows Search", "打开 Windows 搜索侧栏（或搜索主页）。", "Open the Windows Search sidebar (or the search home).", ["SearchHost", "SearchApp"]);
        a("Win+V", "Windows 剪贴板历史", "Windows Clipboard History", "打开剪贴板历史记录面板。", "Open the clipboard history panel.", ["ShellExperienceHost"]);
        a("Win+X", "Windows", "Windows", "打开快速链接菜单（开始按钮右键菜单）。", "Open the Quick Link menu (the Start button context menu).", ["ShellExperienceHost"]);

        // Win + letter
        a("Win+A", "Windows 快速设置", "Windows Quick Settings", "打开快速设置面板（网络/音量/亮度等）。", "Open Quick Settings (network, volume, brightness, etc.).", ["ShellExperienceHost"]);
        a("Win+B", "Windows 任务栏", "Windows Taskbar", "聚焦任务栏通知区域（系统托盘），可使用方向键操作。", "Focus the taskbar notification area (system tray); use the arrow keys to navigate.", ["explorer"]);
        a("Win+C", "Microsoft Copilot", "Microsoft Copilot", "打开 Microsoft Copilot 侧边栏。", "Open the Microsoft Copilot sidebar.", ["Copilot"]);
        a("Win+F", "Windows 反馈中心", "Windows Feedback Hub", "打开反馈中心应用。", "Open the Feedback Hub app.", ["FeedbackHub"]);
        a("Win+G", "Xbox Game Bar", "Xbox Game Bar", "打开 Xbox Game Bar 游戏覆盖层。", "Open the Xbox Game Bar overlay.", ["GameBar"]);
        a("Win+H", "Windows 语音输入", "Windows Voice Typing", "打开语音输入（听写）功能。", "Open voice typing (dictation).", ["ShellExperienceHost"]);
        a("Win+K", "Windows 投屏", "Windows Cast", "打开投屏到无线显示器/音频设备侧栏。", "Open the sidebar to cast to a wireless display or audio device.", ["ShellExperienceHost"]);
        a("Win+N", "Windows 通知中心", "Windows Notification Center", "打开通知中心和日历面板。", "Open the notification center and calendar panel.", ["ShellExperienceHost"]);
        a("Win+O", "Windows 锁定方向", "Windows orientation lock", "锁定或解锁设备显示方向（平板模式）。", "Lock or unlock the device display orientation (tablet mode).", ["explorer"]);
        a("Win+P", "Windows 投影", "Windows Project", "打开投影选项侧栏（仅屏幕/复制/扩展/仅第二屏幕）。", "Open the projection options (PC screen only, Duplicate, Extend, Second screen only).", ["ShellExperienceHost"]);
        a("Win+T", "Windows 任务栏", "Windows Taskbar", "循环浏览任务栏中的应用预览。", "Cycle through taskbar app previews.", ["explorer"]);
        a("Win+W", "Windows 小组件", "Windows Widgets", "打开小组件面板（新闻/天气/日历等）。", "Open the Widgets panel (news, weather, calendar, etc.).", ["Widgets"]);
        a("Win+Y", "Windows", "Windows", "在桌面版和 Mixed Reality 版之间切换输入。", "Switch input between desktop and Mixed Reality.", []);
        a("Win+Z", "Windows 贴靠布局", "Windows Snap Layouts", "打开窗口贴靠布局选择器。", "Open the snap layout picker.", ["ShellExperienceHost"]);

        // Win + punctuation
        a("Win+Comma", "Windows", "Windows", "临时速览桌面（按住时），松开后恢复。", "Temporarily peek at the desktop while held; release to restore.", ["explorer"]);
        a("Win+Period", "Windows 表情符号面板", "Windows Emoji Panel", "打开表情符号/颜文字/剪贴板面板。", "Open the emoji, kaomoji, and clipboard panel.", ["ShellExperienceHost"]);
        a("Win+Semicolon", "Windows 表情符号面板", "Windows Emoji Panel", "打开表情符号/颜文字/剪贴板面板（同 Win+.）。", "Open the emoji, kaomoji, and clipboard panel (same as Win+.).", ["ShellExperienceHost"]);

        // Win + Shift
        a("Win+Shift+S", "截图工具", "Snipping Tool", "打开屏幕截图选择工具栏（矩形/任意形状/窗口/全屏）。", "Open the screen-snip toolbar (rectangle, freeform, window, full screen).", ["SnippingTool"]);
        a("Win+Shift+M", "Windows", "Windows", "还原所有最小化窗口。", "Restore all minimized windows.", ["explorer"]);
        a("Win+Shift+T", "Windows 任务栏", "Windows Taskbar", "循环浏览任务栏中的应用预览（反向顺序）。", "Cycle through taskbar app previews (reverse order).", ["explorer"]);
        a("Win+Shift+Q", "Windows", "Windows", "打开或关闭自动 HDR（如硬件支持）。", "Turn Auto HDR on or off (if the hardware supports it).", []);
        a("Win+Shift+Up", "Windows", "Windows", "将活动窗口垂直拉伸到顶部和底部边缘。", "Stretch the active window vertically to the top and bottom edges.", ["explorer"]);
        a("Win+Shift+Down", "Windows", "Windows", "还原垂直拉伸的窗口，或最小化窗口。", "Restore a vertically stretched window, or minimize it.", ["explorer"]);
        a("Win+Shift+Left", "Windows", "Windows", "将活动窗口移动到左侧显示器。", "Move the active window to the left monitor.", ["explorer"]);
        a("Win+Shift+Right", "Windows", "Windows", "将活动窗口移动到右侧显示器。", "Move the active window to the right monitor.", ["explorer"]);

        // Win + Ctrl
        a("Win+Ctrl+D", "Windows", "Windows", "创建新的虚拟桌面。", "Create a new virtual desktop.", ["explorer"]);
        a("Win+Ctrl+F4", "Windows", "Windows", "关闭当前虚拟桌面。", "Close the current virtual desktop.", ["explorer"]);
        a("Win+Ctrl+Left", "Windows", "Windows", "切换到左侧虚拟桌面。", "Switch to the virtual desktop on the left.", ["explorer"]);
        a("Win+Ctrl+Right", "Windows", "Windows", "切换到右侧虚拟桌面。", "Switch to the virtual desktop on the right.", ["explorer"]);
        a("Win+Ctrl+F", "Windows", "Windows", "在域网络中搜索计算机。", "Search for computers on a domain network.", ["explorer"]);
        a("Win+Ctrl+Shift+B", "Windows 图形驱动", "Windows graphics driver", "重置图形驱动程序（屏幕会闪烁）。", "Reset the graphics driver (the screen flashes).", []);

        // Win + Alt
        a("Win+Alt+D", "Windows 任务栏", "Windows Taskbar", "显示或隐藏桌面上的日期和时间。", "Show or hide the date and time on the desktop.", ["explorer"]);
        a("Win+Alt+R", "Xbox Game Bar", "Xbox Game Bar", "开始或停止屏幕录制。", "Start or stop screen recording.", ["GameBar"]);
        a("Win+Alt+G", "Xbox Game Bar", "Xbox Game Bar", "录制最近 30 秒的游戏或应用画面（后台录制）。", "Record the last 30 seconds of gameplay (background recording).", ["GameBar"]);
        a("Win+Alt+T", "Xbox Game Bar", "Xbox Game Bar", "显示或隐藏录制计时器。", "Show or hide the recording timer.", ["GameBar"]);
        a("Win+Alt+PrtSc", "Xbox Game Bar", "Xbox Game Bar", "截取当前游戏或应用窗口的屏幕截图。", "Take a screenshot of the current game or app window.", ["GameBar"]);
        a("Win+Alt+B", "Xbox Game Bar", "Xbox Game Bar", "打开或关闭 HDR 校准。", "Turn HDR calibration on or off.", ["GameBar"]);
        a("Win+Alt+K", "Xbox Game Bar", "Xbox Game Bar", "静音或取消静音麦克风。", "Mute or unmute the microphone.", ["GameBar"]);

        // Win + number (taskbar)
        for (var i = 1; i <= 9; i++)
        {
            a($"Win+{i}", "Windows 任务栏", "Windows Taskbar", $"打开或切换到任务栏上第 {i} 个固定的应用。", $"Open or switch to the pinned app at position {i} on the taskbar.", ["explorer"]);
            a($"Win+Shift+{i}", "Windows 任务栏", "Windows Taskbar", $"打开任务栏上第 {i} 个应用的新实例。", $"Open a new instance of the app at position {i} on the taskbar.", ["explorer"]);
            a($"Win+Alt+{i}", "Windows 任务栏", "Windows Taskbar", $"打开任务栏上第 {i} 个应用的跳转列表。", $"Open the jump list of the app at position {i} on the taskbar.", ["explorer"]);
            a($"Win+Ctrl+{i}", "Windows 任务栏", "Windows Taskbar", $"切换到任务栏上第 {i} 个应用的最后一个活动窗口。", $"Switch to the last active window of the app at position {i} on the taskbar.", ["explorer"]);
        }
        a("Win+0", "Windows 任务栏", "Windows Taskbar", "打开或切换到任务栏上第 10 个固定的应用。", "Open or switch to the pinned app at position 10 on the taskbar.", ["explorer"]);
        a("Win+Shift+0", "Windows 任务栏", "Windows Taskbar", "打开任务栏上第 10 个应用的新实例。", "Open a new instance of the app at position 10 on the taskbar.", ["explorer"]);
        a("Win+Alt+0", "Windows 任务栏", "Windows Taskbar", "打开任务栏上第 10 个应用的跳转列表。", "Open the jump list of the app at position 10 on the taskbar.", ["explorer"]);
        a("Win+Ctrl+0", "Windows 任务栏", "Windows Taskbar", "切换到任务栏上第 10 个应用的最后一个活动窗口。", "Switch to the last active window of the app at position 10 on the taskbar.", ["explorer"]);

        // Alt combinations
        a("Alt+Tab", "Windows 任务切换器", "Windows Task Switcher", "切换打开的窗口（按住 Alt 再按 Tab 循环）。", "Switch between open windows (hold Alt and press Tab to cycle).", ["explorer"]);
        a("Alt+Shift+Tab", "Windows 任务切换器", "Windows Task Switcher", "反向切换打开的窗口。", "Switch between open windows in reverse order.", ["explorer"]);
        a("Alt+F4", "Windows", "Windows", "关闭当前活动窗口或应用；在桌面按此键打开关机对话框。", "Close the active window or app; on the desktop it opens the shutdown dialog.", ["explorer"]);
        a("Alt+Enter", "Windows 资源管理器 / 应用", "Windows Explorer / apps", "打开选中项目的属性对话框；在命令提示符中切换全屏。属于前台应用内快捷键，一般不注册为全局热键。", "Open the Properties dialog for the selected item; toggle fullscreen in Command Prompt. This is a foreground app shortcut, usually not registered as a global hotkey.", ["explorer"]);
        a("Alt+Esc", "Windows", "Windows", "按打开顺序循环切换窗口项。", "Cycle through window items in the order they were opened.", ["explorer"]);
        a("Alt+Shift", "Windows 输入法", "Windows input method", "切换输入法/键盘布局（如已配置）。", "Switch input method or keyboard layout (if configured).", ["ctfmon"]);

        // Ctrl + Alt combinations
        a("Ctrl+Alt+Delete", "Windows 安全界面", "Windows Security screen", "打开安全选项屏幕（锁定/切换用户/注销/任务管理器/更改密码）。", "Open the security options screen (Lock, Switch user, Sign out, Task Manager, Change password).", ["winlogon"]);
        a("Ctrl+Shift+Esc", "任务管理器 / 其他工具", "Task Manager / other tools", "直接打开任务管理器。也被某些工具用作全局快捷键覆盖，如 Everything、TranslucentTB、MPC-HC/BE 等。", "Open Task Manager directly. Also used as a global hotkey override by some tools such as Everything, TranslucentTB, and MPC-HC/BE.", ["Taskmgr", "Everything", "TranslucentTB", "mpc-hc", "mpc-be64"]);
        a("F1", "Windows/应用帮助", "Windows / app help", "打开当前应用的帮助文档；在资源管理器中按 F1 打开 Windows 帮助。", "Open help for the current app; in File Explorer, F1 opens Windows Help.", []);
        a("F2", "Windows 资源管理器", "Windows Explorer", "重命名选中的文件或文件夹。", "Rename the selected file or folder.", []);
        a("F3", "Windows 搜索", "Windows Search", "在资源管理器中打开搜索框；在浏览器中打开查找栏。", "Open the search box in File Explorer; open the find bar in a browser.", []);
        a("F4", "Windows 资源管理器", "Windows Explorer", "在资源管理器中打开地址栏下拉列表；Alt+F4 关闭窗口。", "Open the address bar drop-down list in File Explorer; Alt+F4 closes the window.", []);
        a("F5", "Windows/浏览器", "Windows / browser", "刷新当前窗口或页面。", "Refresh the current window or page.", []);
        a("F6", "Windows/浏览器", "Windows / browser", "循环浏览窗口或页面中的可聚焦元素（地址栏、标签页、内容区）。", "Cycle through focusable elements in a window or page (address bar, tabs, content area).", []);
        a("F10", "Windows", "Windows", "激活当前的菜单栏（如文件/编辑/查看等）。", "Activate the current menu bar (File, Edit, View, etc.).", []);
        a("F11", "Windows/浏览器", "Windows / browser", "切换全屏模式。", "Toggle fullscreen mode.", []);

        // PrintScreen
        a("PrintScreen", "Windows 截图 / 第三方截图工具", "Windows screenshot / third-party tools", "截取整个屏幕到剪贴板；如安装了 ShareX、Greenshot 等，可能被其接管并打开截图工具。", "Capture the entire screen to the clipboard; if ShareX, Greenshot, etc. are installed, they may take over and open their capture tool.", ["ShareX", "Greenshot"]);

        // Other useful
        a("Ctrl+Esc", "Windows", "Windows", "打开开始菜单（等效于按 Win 键）。", "Open the Start menu (equivalent to pressing the Win key).", ["ShellExperienceHost"]);
    }

    private static void AddBrowserHotkeys(Dictionary<string, List<KnownHotkeyInfo>> d)
    {
        var cat = new LocalizedText("浏览器", "Browser");
        void a(string k, string oz, string oe, string uz, string ue, string[]? p = null)
            => Add(d, k, new(new LocalizedText(oz, oe), new LocalizedText(uz, ue), p, cat));

        var browsers = new[] { "chrome", "msedge", "firefox", "brave", "opera" };
        var browsersAndIe = new[] { "chrome", "msedge", "firefox", "brave", "opera", "iexplore" };

        // Common browser hotkeys - many browsers register these
        a("Ctrl+T", "浏览器", "Browser", "打开新标签页。", "Open a new tab.", browsersAndIe);
        a("Ctrl+N", "浏览器", "Browser", "打开新窗口。", "Open a new window.", browsersAndIe);
        a("Ctrl+W", "浏览器", "Browser", "关闭当前标签页。", "Close the current tab.", browsersAndIe);
        a("Ctrl+Shift+T", "浏览器", "Browser", "重新打开最近关闭的标签页。", "Reopen the most recently closed tab.", browsers);
        a("Ctrl+Tab", "浏览器", "Browser", "切换到下一个标签页。", "Switch to the next tab.", browsers);
        a("Ctrl+Shift+Tab", "浏览器", "Browser", "切换到上一个标签页。", "Switch to the previous tab.", browsers);
        a("Ctrl+D", "浏览器", "Browser", "将当前页面添加为书签。", "Bookmark the current page.", browsers);
        a("Ctrl+Shift+B", "浏览器", "Browser", "显示或隐藏书签栏。", "Show or hide the bookmarks bar.", browsers);
        a("Ctrl+H", "浏览器", "Browser", "打开浏览历史记录。", "Open browsing history.", browsers);
        a("Ctrl+J", "浏览器", "Browser", "打开下载记录。", "Open downloads.", browsers);
        a("Ctrl+L", "浏览器", "Browser", "聚焦地址栏。", "Focus the address bar.", browsers);
        a("Ctrl+F", "浏览器", "Browser", "在当前页面查找文本。", "Find text on the current page.", browsers);
        a("Ctrl+P", "浏览器", "Browser", "打印当前页面。", "Print the current page.", browsersAndIe);
        a("Ctrl+S", "浏览器", "Browser", "保存当前页面到本地。", "Save the current page locally.", browsers);
        a("Ctrl+U", "浏览器", "Browser", "查看当前页面源代码。", "View the page source.", browsers);
        a("Ctrl+R", "浏览器", "Browser", "刷新当前页面（等效于 F5）。", "Reload the current page (same as F5).", browsers);
        a("Ctrl+Shift+R", "浏览器", "Browser", "强制刷新（忽略缓存）。", "Hard reload (ignore cache).", browsers);
        a("Ctrl+Shift+I", "浏览器开发者工具", "Browser DevTools", "打开开发者工具（DevTools）。", "Open the developer tools (DevTools).", browsers);
        a("Ctrl+Shift+J", "浏览器", "Browser", "打开控制台（DevTools Console）。", "Open the console (DevTools Console).", ["chrome", "msedge", "brave", "opera"]);
        a("Ctrl+Shift+Delete", "浏览器", "Browser", "打开清除浏览数据对话框。", "Open the Clear Browsing Data dialog.", browsers);

        // Numbered tab switching
        for (var i = 1; i <= 8; i++)
        {
            a($"Ctrl+{i}", "浏览器", "Browser", $"切换到第 {i} 个标签页。", $"Switch to tab {i}.", browsers);
        }
        a("Ctrl+9", "浏览器", "Browser", "切换到最后一个标签页。", "Switch to the last tab.", browsers);
    }

    private static void AddScreenshotRecordingHotkeys(Dictionary<string, List<KnownHotkeyInfo>> d)
    {
        var cat = new LocalizedText("截图/录制", "Screenshot / Recording");
        void a(string k, string oz, string oe, string uz, string ue, string[]? p = null)
            => Add(d, k, new(new LocalizedText(oz, oe), new LocalizedText(uz, ue), p, cat));

        // ShareX
        a("Shift+PrintScreen", "ShareX", "ShareX", "截取矩形区域。", "Capture a rectangular region.", ["ShareX"]);
        a("Alt+PrintScreen", "ShareX", "ShareX", "截取活动窗口。", "Capture the active window.", ["ShareX"]);
        a("Ctrl+PrintScreen", "ShareX", "ShareX", "截取窗口/控件。", "Capture a window or control.", ["ShareX"]);

        // Snipaste
        a("F1", "Snipaste", "Snipaste", "开始截图。", "Start a screenshot.", ["Snipaste"]);
        a("F3", "Snipaste", "Snipaste", "将截取的图片贴到屏幕上（贴图）。", "Paste the captured image onto the screen (pin to screen).", ["Snipaste"]);
        a("Shift+F1", "Snipaste", "Snipaste", "截取控件/窗口。", "Capture a control or window.", ["Snipaste"]);
        a("Ctrl+F1", "Snipaste", "Snipaste", "截取矩形区域（同上）。", "Capture a rectangular region (same as above).", ["Snipaste"]);
        a("Ctrl+Shift+F1", "Snipaste", "Snipaste", "截取全屏。", "Capture the full screen.", ["Snipaste"]);
        a("Esc", "Snipaste", "Snipaste", "退出截图/贴图模式。", "Exit screenshot or pin mode.", ["Snipaste"]);

        // Greenshot
        a("Alt+PrintScreen", "Greenshot", "Greenshot", "截取活动窗口。", "Capture the active window.", ["Greenshot"]);
        a("Ctrl+PrintScreen", "Greenshot", "Greenshot", "截取矩形区域。", "Capture a rectangular region.", ["Greenshot"]);

        // OBS Studio
        a("Ctrl+Shift+F5", "OBS Studio", "OBS Studio", "开始或停止录制。", "Start or stop recording.", ["obs64", "obs32"]);
        a("Ctrl+Shift+F6", "OBS Studio", "OBS Studio", "开始或停止推流。", "Start or stop streaming.", ["obs64", "obs32"]);
        a("Ctrl+Shift+F7", "OBS Studio", "OBS Studio", "暂停录制。", "Pause recording.", ["obs64", "obs32"]);
        a("Ctrl+Shift+F8", "OBS Studio", "OBS Studio", "切换场景。", "Switch scenes.", ["obs64", "obs32"]);
        a("Ctrl+Shift+F9", "OBS Studio", "OBS Studio", "切换场景（快速）。", "Switch scenes (fast).", ["obs64", "obs32"]);
        a("Ctrl+Shift+F10", "OBS Studio", "OBS Studio", "隐藏或显示捕获画面。", "Hide or show the capture source.", ["obs64", "obs32"]);
        a("Ctrl+Shift+F11", "OBS Studio", "OBS Studio", "切换全屏预览。", "Toggle fullscreen preview.", ["obs64", "obs32"]);
        a("Ctrl+Shift+F12", "OBS Studio", "OBS Studio", "截取当前画面（来源截图）。", "Take a source screenshot of the current frame.", ["obs64", "obs32"]);

        // NVIDIA GeForce Experience / ShadowPlay
        a("Alt+F2", "NVIDIA GeForce Experience", "NVIDIA GeForce Experience", "打开游戏滤镜（Ansel/Freestyle）。", "Open game filters (Ansel/Freestyle).", ["nvidiashare"]);
        a("Alt+F3", "NVIDIA GeForce Experience", "NVIDIA GeForce Experience", "切换游戏滤镜开关。", "Toggle game filters on or off.", ["nvidiashare"]);
        a("Alt+F1", "NVIDIA GeForce Experience", "NVIDIA GeForce Experience", "截取游戏屏幕截图。", "Take an in-game screenshot.", ["nvidiashare"]);
        a("Alt+Shift+F9", "NVIDIA GeForce Experience", "NVIDIA GeForce Experience", "开始或停止即时重放录制。", "Start or stop Instant Replay recording.", ["nvidiashare"]);
        a("Alt+F9", "NVIDIA GeForce Experience", "NVIDIA GeForce Experience", "开始或停止手动录制。", "Start or stop manual recording.", ["nvidiashare"]);
        a("Alt+Z", "NVIDIA GeForce Experience", "NVIDIA GeForce Experience", "打开 GeForce Experience 覆盖层。", "Open the GeForce Experience overlay.", ["nvidiashare"]);

        // AMD Adrenalin
        a("Alt+R", "AMD Software: Adrenalin", "AMD Software: Adrenalin", "打开 AMD 驱动覆盖层。", "Open the AMD software overlay.", ["amdsoftware"]);
        a("Alt+Shift+R", "AMD Software: Adrenalin", "AMD Software: Adrenalin", "开始或停止录制。", "Start or stop recording.", ["amdsoftware"]);
        a("Alt+Shift+S", "AMD Software: Adrenalin", "AMD Software: Adrenalin", "截取屏幕截图。", "Take a screenshot.", ["amdsoftware"]);
        a("Alt+Shift+L", "AMD Software: Adrenalin", "AMD Software: Adrenalin", "打开或关闭性能叠加层。", "Toggle the performance overlay.", ["amdsoftware"]);
        a("Alt+Shift+B", "AMD Software: Adrenalin", "AMD Software: Adrenalin", "开始或停止即时重放。", "Start or stop Instant Replay.", ["amdsoftware"]);
    }

    private static void AddCommunicationHotkeys(Dictionary<string, List<KnownHotkeyInfo>> d)
    {
        var cat = new LocalizedText("即时通讯", "Messaging");
        void a(string k, string oz, string oe, string uz, string ue, string[]? p = null)
            => Add(d, k, new(new LocalizedText(oz, oe), new LocalizedText(uz, ue), p, cat));

        // Discord
        a("Ctrl+`", "Discord", "Discord", "切换 Discord 覆盖层。", "Toggle the Discord overlay.", ["discord"]);
        a("Ctrl+Alt+A", "Discord", "Discord", "静音或取消静音麦克风。", "Mute or unmute the microphone.", ["discord"]);
        a("Ctrl+Shift+A", "Discord", "Discord", "打开或关闭主动降噪（Krisp）。", "Toggle noise suppression (Krisp).", ["discord"]);
        a("Ctrl+Alt+S", "Discord", "Discord", "静音或取消静音扬声器/耳机。", "Mute or unmute the speakers/headphones.", ["discord"]);
        a("Ctrl+Shift+F", "Discord", "Discord", "搜索消息。", "Search messages.", ["discord"]);
        a("Ctrl+Shift+M", "Discord", "Discord", "静音或取消静音麦克风（同 Ctrl+Alt+A）。", "Mute or unmute the microphone (same as Ctrl+Alt+A).", ["discord"]);
        a("Ctrl+Shift+D", "Discord", "Discord", "切换到开发者模式。", "Toggle developer mode.", ["discord"]);
        a("Ctrl+I", "Discord", "Discord", "忽略用户。", "Ignore a user.", ["discord"]);
        a("Ctrl+B", "Discord", "Discord", "创建私密频道。", "Create a private channel.", ["discord"]);
        a("Ctrl+U", "Discord", "Discord", "上传文件。", "Upload a file.", ["discord"]);
        a("Ctrl+E", "Discord", "Discord", "编辑最后一条消息。", "Edit the last message.", ["discord"]);
        a("Ctrl+K", "Discord", "Discord", "快速跳转到频道/用户/消息。", "Quick switch to a channel, user, or message.", ["discord"]);
        a("Ctrl+Shift+X", "Discord", "Discord", "切换全屏模式。", "Toggle fullscreen mode.", ["discord"]);

        // Teams
        a("Ctrl+Shift+M", "Microsoft Teams", "Microsoft Teams", "静音或取消静音麦克风。", "Mute or unmute the microphone.", ["Teams"]);
        a("Ctrl+Shift+E", "Microsoft Teams", "Microsoft Teams", "打开或关闭摄像头。", "Turn the camera on or off.", ["Teams"]);
        a("Ctrl+Shift+Space", "Microsoft Teams", "Microsoft Teams", "举手或放下手。", "Raise or lower your hand.", ["Teams"]);
        a("Ctrl+Shift+K", "Microsoft Teams", "Microsoft Teams", "共享屏幕。", "Share your screen.", ["Teams"]);
        a("Ctrl+Shift+H", "Microsoft Teams", "Microsoft Teams", "显示或隐藏聊天侧栏。", "Show or hide the chat sidebar.", ["Teams"]);
        a("Ctrl+Shift+Esc", "Microsoft Teams", "Microsoft Teams", "打开活动日志。", "Open the activity log.", ["Teams"]);
        a("Ctrl+E", "Microsoft Teams", "Microsoft Teams", "搜索消息/频道/文件/人员。", "Search messages, channels, files, or people.", ["Teams"]);
        a("Ctrl+G", "Microsoft Teams", "Microsoft Teams", "固定或取消固定频道。", "Pin or unpin a channel.", ["Teams"]);
        a("Ctrl+Shift+A", "Microsoft Teams", "Microsoft Teams", "接受来电。", "Accept an incoming call.", ["Teams"]);
        a("Ctrl+Shift+D", "Microsoft Teams", "Microsoft Teams", "拒绝来电。", "Decline an incoming call.", ["Teams"]);
        a("Ctrl+N", "Microsoft Teams", "Microsoft Teams", "开始新聊天。", "Start a new chat.", ["Teams"]);

        // Slack
        a("Ctrl+Shift+M", "Slack", "Slack", "静音或取消静音麦克风（通话中）。", "Mute or unmute the microphone (during a call).", ["slack"]);
        a("Ctrl+Shift+V", "Slack", "Slack", "打开或关闭摄像头（通话中）。", "Turn the camera on or off (during a call).", ["slack"]);
        a("Ctrl+K", "Slack", "Slack", "快速跳转到频道/用户/对话。", "Quick switch to a channel, user, or conversation.", ["slack"]);
        a("Ctrl+Shift+K", "Slack", "Slack", "跳转到未读消息。", "Jump to unread messages.", ["slack"]);
        a("Ctrl+Shift+N", "Slack", "Slack", "创建新频道。", "Create a new channel.", ["slack"]);
        a("Ctrl+Shift+P", "Slack", "Slack", "创建私人频道。", "Create a private channel.", ["slack"]);
        a("Ctrl+T", "Slack", "Slack", "跳转到频道浏览器。", "Jump to the channel browser.", ["slack"]);
        a("Ctrl+Shift+E", "Slack", "Slack", "编辑最后发送的消息。", "Edit the last sent message.", ["slack"]);
        a("Ctrl+Shift+F", "Slack", "Slack", "搜索消息和文件。", "Search messages and files.", ["slack"]);
        a("Ctrl+Shift+T", "Slack", "Slack", "切换讨论线程侧栏。", "Toggle the thread sidebar.", ["slack"]);

        // Zoom
        a("Alt+A", "Zoom", "Zoom", "静音或取消静音麦克风。", "Mute or unmute the microphone.", ["Zoom"]);
        a("Alt+V", "Zoom", "Zoom", "开始或停止视频。", "Start or stop the video.", ["Zoom"]);
        a("Alt+S", "Zoom", "Zoom", "共享屏幕。", "Share your screen.", ["Zoom"]);
        a("Alt+Pause", "Zoom", "Zoom", "暂停或恢复屏幕共享。", "Pause or resume screen sharing.", ["Zoom"]);
        a("Alt+R", "Zoom", "Zoom", "开始或停止本地录制。", "Start or stop local recording.", ["Zoom"]);
        a("Alt+C", "Zoom", "Zoom", "开始或停止云录制。", "Start or stop cloud recording.", ["Zoom"]);
        a("Alt+Shift+T", "Zoom", "Zoom", "暂停或恢复录制。", "Pause or resume recording.", ["Zoom"]);
        a("Alt+H", "Zoom", "Zoom", "显示或隐藏聊天面板。", "Show or hide the chat panel.", ["Zoom"]);
        a("Alt+F2", "Zoom", "Zoom", "切换图库视图/演讲者视图。", "Switch between Gallery and Speaker view.", ["Zoom"]);
        a("Alt+F", "Zoom", "Zoom", "进入/退出全屏模式。", "Enter or exit fullscreen mode.", ["Zoom"]);
        a("Alt+Shift+M", "Zoom", "Zoom", "切换到最小化视图。", "Switch to the minimized view.", ["Zoom"]);
        a("Alt+I", "Zoom", "Zoom", "打开邀请窗口。", "Open the invite window.", ["Zoom"]);
        a("Alt+Y", "Zoom", "Zoom", "举手/放下手。", "Raise or lower your hand.", ["Zoom"]);
        a("Alt+U", "Zoom", "Zoom", "显示参与者列表。", "Show the participants list.", ["Zoom"]);
        a("Ctrl+Shift+Space", "Zoom", "Zoom", "激活静音快捷键（PTT 模式）。", "Activate the push-to-talk mute shortcut.", ["Zoom"]);

        // Telegram
        a("Ctrl+N", "Telegram", "Telegram", "新建私信。", "Start a new private chat.", ["Telegram"]);
        a("Ctrl+G", "Telegram", "Telegram", "新建群组。", "Create a new group.", ["Telegram"]);
        a("Ctrl+Shift+G", "Telegram", "Telegram", "新建频道。", "Create a new channel.", ["Telegram"]);
        a("Ctrl+K", "Telegram", "Telegram", "跳转到对话（快速切换）。", "Quick switch between conversations.", ["Telegram"]);
        a("Ctrl+F", "Telegram", "Telegram", "搜索当前对话消息。", "Search messages in the current chat.", ["Telegram"]);
        a("Ctrl+Shift+F", "Telegram", "Telegram", "全局搜索消息。", "Search messages globally.", ["Telegram"]);
        a("Ctrl+Shift+M", "Telegram", "Telegram", "静音对话。", "Mute the conversation.", ["Telegram"]);
        a("Ctrl+Up", "Telegram", "Telegram", "跳转到上一对话。", "Jump to the previous conversation.", ["Telegram"]);
        a("Ctrl+Down", "Telegram", "Telegram", "跳转到下一对话。", "Jump to the next conversation.", ["Telegram"]);

        // WhatsApp Desktop
        a("Ctrl+N", "WhatsApp", "WhatsApp", "开始新聊天。", "Start a new chat.", ["WhatsApp"]);
        a("Ctrl+Shift+N", "WhatsApp", "WhatsApp", "创建新群组。", "Create a new group.", ["WhatsApp"]);
        a("Ctrl+Shift+[", "WhatsApp", "WhatsApp", "切换到上一个聊天。", "Switch to the previous chat.", ["WhatsApp"]);
        a("Ctrl+Shift+]", "WhatsApp", "WhatsApp", "切换到下一个聊天。", "Switch to the next chat.", ["WhatsApp"]);
        a("Ctrl+F", "WhatsApp", "WhatsApp", "搜索消息。", "Search messages.", ["WhatsApp"]);
        a("Ctrl+K", "WhatsApp", "WhatsApp", "搜索聊天。", "Search chats.", ["WhatsApp"]);
        a("Ctrl+E", "WhatsApp", "WhatsApp", "存档聊天。", "Archive the chat.", ["WhatsApp"]);
        a("Ctrl+Shift+M", "WhatsApp", "WhatsApp", "静音聊天。", "Mute the chat.", ["WhatsApp"]);
        a("Ctrl+Shift+U", "WhatsApp", "WhatsApp", "标记为未读。", "Mark as unread.", ["WhatsApp"]);
        a("Ctrl+Delete", "WhatsApp", "WhatsApp", "删除聊天。", "Delete the chat.", ["WhatsApp"]);
    }

    private static void AddDeveloperHotkeys(Dictionary<string, List<KnownHotkeyInfo>> d)
    {
        var cat = new LocalizedText("开发者工具", "Developer Tools");
        void a(string k, string oz, string oe, string uz, string ue, string[]? p = null)
            => Add(d, k, new(new LocalizedText(oz, oe), new LocalizedText(uz, ue), p, cat));

        // VS Code
        a("Ctrl+Shift+P", "VS Code", "VS Code", "打开命令面板。", "Open the Command Palette.", ["Code"]);
        a("Ctrl+P", "VS Code", "VS Code", "快速打开文件（按名称搜索）。", "Quick Open a file (search by name).", ["Code"]);
        a("Ctrl+B", "VS Code", "VS Code", "切换侧边栏显示。", "Toggle the sidebar.", ["Code"]);
        a("Ctrl+`", "VS Code", "VS Code", "打开或关闭终端面板。", "Toggle the terminal panel.", ["Code"]);
        a("Ctrl+Shift+`", "VS Code", "VS Code", "创建新的终端实例。", "Create a new terminal instance.", ["Code"]);
        a("Ctrl+Shift+E", "VS Code", "VS Code", "聚焦资源管理器（文件树）。", "Focus the Explorer (file tree).", ["Code"]);
        a("Ctrl+Shift+X", "VS Code", "VS Code", "打开扩展商店面板。", "Open the Extensions view.", ["Code"]);
        a("Ctrl+Shift+G", "VS Code", "VS Code", "打开源代码管理（Git）面板。", "Open the Source Control (Git) view.", ["Code"]);
        a("Ctrl+Shift+D", "VS Code", "VS Code", "打开运行和调试面板。", "Open the Run and Debug view.", ["Code"]);
        a("Ctrl+Shift+F", "VS Code", "VS Code", "全局搜索文件内容。", "Search across files.", ["Code"]);
        a("Ctrl+H", "VS Code", "VS Code", "查找替换。", "Find and replace.", ["Code"]);
        a("Ctrl+Shift+H", "VS Code", "VS Code", "全局查找替换。", "Replace across files.", ["Code"]);
        a("Ctrl+Shift+O", "VS Code", "VS Code", "跳转到符号定义。", "Go to a symbol.", ["Code"]);
        a("Ctrl+G", "VS Code", "VS Code", "跳转到指定行。", "Go to a specific line.", ["Code"]);
        a("Ctrl+Shift+\\", "VS Code", "VS Code", "跳转到匹配的括号。", "Jump to a matching bracket.", ["Code"]);
        a("Ctrl+Shift+]", "VS Code", "VS Code", "展开或折叠代码区域。", "Expand or collapse a code region.", ["Code"]);
        a("Ctrl+D", "VS Code", "VS Code", "选择下一个匹配项（多光标编辑）。", "Select the next occurrence (multi-cursor editing).", ["Code"]);
        a("Ctrl+Shift+L", "VS Code", "VS Code", "选择所有匹配项（多光标编辑）。", "Select all occurrences (multi-cursor editing).", ["Code"]);
        a("Alt+Up", "VS Code", "VS Code", "向上移动当前行。", "Move the current line up.", ["Code"]);
        a("Alt+Down", "VS Code", "VS Code", "向下移动当前行。", "Move the current line down.", ["Code"]);
        a("Alt+Shift+Up", "VS Code", "VS Code", "向上复制当前行。", "Copy the current line up.", ["Code"]);
        a("Alt+Shift+Down", "VS Code", "VS Code", "向下复制当前行。", "Copy the current line down.", ["Code"]);
        a("Ctrl+Shift+K", "VS Code", "VS Code", "删除当前行。", "Delete the current line.", ["Code"]);
        a("Ctrl+Enter", "VS Code", "VS Code", "在当前行下方插入新行。", "Insert a new line below the current line.", ["Code"]);
        a("Ctrl+Shift+Enter", "VS Code", "VS Code", "在当前行上方插入新行。", "Insert a new line above the current line.", ["Code"]);
        a("Ctrl+/", "VS Code", "VS Code", "切换行注释。", "Toggle a line comment.", ["Code"]);
        a("Ctrl+Shift+/", "VS Code", "VS Code", "切换块注释。", "Toggle a block comment.", ["Code"]);
        a("F5", "VS Code", "VS Code", "开始调试。", "Start debugging.", ["Code"]);
        a("Ctrl+F5", "VS Code", "VS Code", "开始执行（不调试）。", "Start without debugging.", ["Code"]);
        a("Shift+F5", "VS Code", "VS Code", "停止调试。", "Stop debugging.", ["Code"]);
        a("F9", "VS Code", "VS Code", "切换断点。", "Toggle a breakpoint.", ["Code"]);
        a("F10", "VS Code", "VS Code", "单步跳过。", "Step over.", ["Code"]);
        a("F11", "VS Code", "VS Code", "单步进入。", "Step into.", ["Code"]);
        a("Shift+F11", "VS Code", "VS Code", "单步跳出。", "Step out.", ["Code"]);
        a("Ctrl+Shift+5", "VS Code", "VS Code", "拆分编辑器窗口。", "Split the editor window.", ["Code"]);

        // Visual Studio
        a("Ctrl+Shift+B", "Visual Studio", "Visual Studio", "生成解决方案。", "Build the solution.", ["devenv"]);
        a("F5", "Visual Studio", "Visual Studio", "开始调试。", "Start debugging.", ["devenv"]);
        a("Ctrl+F5", "Visual Studio", "Visual Studio", "开始执行（不调试）。", "Start without debugging.", ["devenv"]);
        a("Shift+F5", "Visual Studio", "Visual Studio", "停止调试。", "Stop debugging.", ["devenv"]);
        a("F9", "Visual Studio", "Visual Studio", "切换断点。", "Toggle a breakpoint.", ["devenv"]);
        a("F10", "Visual Studio", "Visual Studio", "单步跳过。", "Step over.", ["devenv"]);
        a("F11", "Visual Studio", "Visual Studio", "单步进入。", "Step into.", ["devenv"]);
        a("Shift+F11", "Visual Studio", "Visual Studio", "单步跳出。", "Step out.", ["devenv"]);
        a("Ctrl+K, Ctrl+D", "Visual Studio", "Visual Studio", "格式化文档。", "Format the document.", ["devenv"]);
        a("Ctrl+K, Ctrl+C", "Visual Studio", "Visual Studio", "注释选定代码。", "Comment the selected code.", ["devenv"]);
        a("Ctrl+K, Ctrl+U", "Visual Studio", "Visual Studio", "取消注释选定代码。", "Uncomment the selected code.", ["devenv"]);
        a("Ctrl+M, Ctrl+M", "Visual Studio", "Visual Studio", "展开或折叠代码区域。", "Expand or collapse a code region.", ["devenv"]);
        a("Ctrl+R, Ctrl+W", "Visual Studio", "Visual Studio", "显示或隐藏监视窗口。", "Show or hide the Watch window.", ["devenv"]);
        a("Ctrl+Alt+E", "Visual Studio", "Visual Studio", "打开异常设置窗口。", "Open the Exception Settings window.", ["devenv"]);
        a("Ctrl+Alt+L", "Visual Studio", "Visual Studio", "打开解决方案资源管理器。", "Open Solution Explorer.", ["devenv"]);
        a("Ctrl+\\, Ctrl+E", "Visual Studio", "Visual Studio", "打开错误列表。", "Open the Error List.", ["devenv"]);
        a("Ctrl+Alt+X", "Visual Studio", "Visual Studio", "打开工具箱。", "Open the Toolbox.", ["devenv"]);
        a("Ctrl+\\, M", "Visual Studio", "Visual Studio", "显示或隐藏选项卡式文档。", "Show or hide the tabbed documents.", ["devenv"]);

        // JetBrains IDEs (IntelliJ, Rider, WebStorm, PyCharm, etc.)
        var jetbrains = new[] { "jetbrainstoolbox", "rider64", "idea64", "webstorm64", "pycharm64" };
        a("Ctrl+Shift+A", "JetBrains IDE", "JetBrains IDE", "查找操作（打开所有命令搜索）。", "Find Action (search all commands).", jetbrains);
        a("Ctrl+N", "JetBrains IDE", "JetBrains IDE", "按名称搜索类。", "Search for a class by name.", jetbrains);
        a("Ctrl+Shift+N", "JetBrains IDE", "JetBrains IDE", "按名称搜索文件。", "Search for a file by name.", jetbrains);
        a("Ctrl+Alt+L", "JetBrains IDE", "JetBrains IDE", "格式化代码。", "Reformat code.", jetbrains);
        a("Ctrl+Alt+Shift+L", "JetBrains IDE", "JetBrains IDE", "显示格式化对话框。", "Show the reformat dialog.", jetbrains);
        a("Ctrl+R", "JetBrains IDE", "JetBrains IDE", "替换当前文件内容。", "Replace within the current file.", jetbrains);
        a("Ctrl+Shift+R", "JetBrains IDE", "JetBrains IDE", "全局替换。", "Replace across files.", jetbrains);
        a("F12", "JetBrains IDE", "JetBrains IDE", "跳转到上一个打开的文件。", "Jump to the previously opened file.", jetbrains);
        a("Ctrl+F12", "JetBrains IDE", "JetBrains IDE", "显示文件结构。", "Show the file structure.", jetbrains);
        a("Shift+F6", "JetBrains IDE", "JetBrains IDE", "重命名符号（重构）。", "Rename a symbol (refactor).", jetbrains);
        a("Ctrl+Alt+O", "JetBrains IDE", "JetBrains IDE", "优化导入。", "Optimize imports.", jetbrains);
        a("Ctrl+E", "JetBrains IDE", "JetBrains IDE", "最近打开的文件。", "Recently opened files.", jetbrains);
        a("Ctrl+Shift+E", "JetBrains IDE", "JetBrains IDE", "最近编辑的文件。", "Recently edited files.", jetbrains);
        a("Shift+F10", "JetBrains IDE", "JetBrains IDE", "运行当前配置。", "Run the current configuration.", jetbrains);
        a("Shift+F9", "JetBrains IDE", "JetBrains IDE", "调试当前配置。", "Debug the current configuration.", jetbrains);
        a("Ctrl+Shift+F10", "JetBrains IDE", "JetBrains IDE", "运行上下文最近的配置。", "Run the context configuration.", jetbrains);
        a("Ctrl+D", "JetBrains IDE", "JetBrains IDE", "复制当前行或选定内容。", "Duplicate the current line or selection.", jetbrains);
        a("Ctrl+Y", "JetBrains IDE", "JetBrains IDE", "删除当前行。", "Delete the current line.", jetbrains);
        a("Ctrl+Shift+Up", "JetBrains IDE", "JetBrains IDE", "向上移动语句（代码移动）。", "Move a statement up.", jetbrains);
        a("Ctrl+Shift+Down", "JetBrains IDE", "JetBrains IDE", "向下移动语句。", "Move a statement down.", jetbrains);
        a("Ctrl+Alt+B", "JetBrains IDE", "JetBrains IDE", "跳转到实现（实现类/方法）。", "Go to implementation.", jetbrains);
        a("Ctrl+U", "JetBrains IDE", "JetBrains IDE", "跳转到父类/父方法。", "Go to the superclass or supermethod.", jetbrains);
        a("Ctrl+Shift+I", "JetBrains IDE", "JetBrains IDE", "查看定义（内联预览）。", "View the definition (inline preview).", jetbrains);
        a("Alt+F7", "JetBrains IDE", "JetBrains IDE", "查找用法。", "Find usages.", jetbrains);
        a("Ctrl+B", "JetBrains IDE", "JetBrains IDE", "跳转到声明。", "Go to declaration.", jetbrains);
        a("Ctrl+F8", "JetBrains IDE", "JetBrains IDE", "切换断点。", "Toggle a breakpoint.", jetbrains);

        // Windows Terminal
        a("Ctrl+Shift+1", "Windows Terminal", "Windows Terminal", "打开新终端标签页（Profile 1）。", "Open a new terminal tab (Profile 1).", ["WindowsTerminal"]);
        a("Ctrl+Shift+2", "Windows Terminal", "Windows Terminal", "打开新终端标签页（Profile 2）。", "Open a new terminal tab (Profile 2).", ["WindowsTerminal"]);
        a("Ctrl+Shift+D", "Windows Terminal", "Windows Terminal", "复制当前标签页（拆分窗格）。", "Duplicate the current tab (split pane).", ["WindowsTerminal"]);
        a("Ctrl+Shift+W", "Windows Terminal", "Windows Terminal", "关闭当前标签页或窗格。", "Close the current tab or pane.", ["WindowsTerminal"]);
        a("Alt+Shift+D", "Windows Terminal", "Windows Terminal", "垂直拆分当前窗格。", "Split the current pane vertically.", ["WindowsTerminal"]);
        a("Alt+Shift+-", "Windows Terminal", "Windows Terminal", "水平拆分当前窗格。", "Split the current pane horizontally.", ["WindowsTerminal"]);
        a("Ctrl+Shift+Space", "Windows Terminal", "Windows Terminal", "打开搜索（查找终端输出）。", "Open search (find in terminal output).", ["WindowsTerminal"]);
        a("Ctrl+,", "Windows Terminal", "Windows Terminal", "打开设置页面。", "Open the settings page.", ["WindowsTerminal"]);
    }

    private static void AddProductivityHotkeys(Dictionary<string, List<KnownHotkeyInfo>> d)
    {
        var cat = new LocalizedText("生产力工具", "Productivity");
        void a(string k, string oz, string oe, string uz, string ue, string[]? p = null)
            => Add(d, k, new(new LocalizedText(oz, oe), new LocalizedText(uz, ue), p, cat));

        // Microsoft PowerToys
        a("Alt+Space", "PowerToys PowerToys Run", "PowerToys Run", "打开 PowerToys Run 快速启动器。", "Open the PowerToys Run launcher.", ["PowerToys"]);
        a("Win+Ctrl+T", "PowerToys", "PowerToys", "打开 PowerToys Peek（快速预览文件）。", "Open PowerToys Peek (quick file preview).", ["PowerToys"]);
        a("Win+Ctrl+E", "PowerToys", "PowerToys", "打开 PowerToys 颜色选择器。", "Open the PowerToys Color Picker.", ["PowerToys"]);
        a("Win+Shift+T", "PowerToys", "PowerToys", "打开文本提取器（OCR 截图识别文字）。", "Open Text Extractor (OCR text from screen).", ["PowerToys"]);
        a("Win+Shift+C", "PowerToys", "PowerToys", "打开颜色选择器。", "Open the Color Picker.", ["PowerToys"]);
        a("Win+Ctrl+Shift+Q", "PowerToys", "PowerToys", "打开视频会议静音（麦克风/摄像头一键控制）。", "Toggle Video Conference Mute (one-key mic/camera control).", ["PowerToys"]);
        a("Win+Ctrl+Alt+F", "PowerToys", "PowerToys", "将鼠标指针置于焦点中心。", "Place the mouse pointer at the focus center.", ["PowerToys"]);
        a("Win+Ctrl+Alt+T", "PowerToys", "PowerToys", "始终置顶当前窗口。", "Always-on-top for the current window.", ["PowerToys"]);
        a("Win+Ctrl+Shift+R", "PowerToys", "PowerToys", "运行 PowerRename（批量重命名）。", "Run PowerRename (bulk rename).", ["PowerToys"]);
        a("Win+Shift+M", "PowerToys", "PowerToys", "打开鼠标高亮/点击指示器。", "Toggle mouse highlighter / click indicator.", ["PowerToys"]);

        // AutoHotkey (user-defined, common defaults)
        var ahk = new[] { "AutoHotkey64", "AutoHotkey32", "AutoHotkeyUX" };
        a("Ctrl+Alt+O", "AutoHotkey（用户自定义）", "AutoHotkey (user-defined)", "打开常用文件夹或应用（取决于用户脚本定义）。", "Open a common folder or app (depends on the user script).", ahk);
        a("Ctrl+Alt+K", "AutoHotkey（用户自定义）", "AutoHotkey (user-defined)", "搜索选中文本在 Google 中（取决于用户脚本定义）。", "Search the selected text on Google (depends on the user script).", ahk);
        a("Ctrl+Alt+C", "AutoHotkey（用户自定义）", "AutoHotkey (user-defined)", "复制选中文本为纯文本（取决于用户脚本定义）。", "Copy the selected text as plain text (depends on the user script).", ahk);

        // Everything (voidtools)
        a("Ctrl+Shift+F", "Everything", "Everything", "打开 Everything 搜索窗口。", "Open the Everything search window.", ["Everything"]);
        a("Win+Shift+F", "Everything", "Everything", "打开 Everything 搜索窗口（可选配置）。", "Open the Everything search window (optional configuration).", ["Everything"]);

        // Ditto Clipboard Manager
        a("Ctrl+`", "Ditto 剪贴板管理器", "Ditto Clipboard Manager", "打开 Ditto 剪贴板历史窗口。", "Open the Ditto clipboard history window.", ["Ditto"]);
        a("Ctrl+Shift+V", "Ditto 剪贴板管理器", "Ditto Clipboard Manager", "以 Ditto 菜单粘贴（替代系统粘贴）。", "Paste with the Ditto menu (replaces system paste).", ["Ditto"]);

        // EarTrumpet (volume control)
        a("Ctrl+Shift+E", "EarTrumpet", "EarTrumpet", "打开 EarTrumpet 音量控制面板。", "Open the EarTrumpet volume control panel.", ["EarTrumpet"]);

        // Listary (file search)
        a("Ctrl+G", "Listary", "Listary", "跳转到文件管理器中当前路径。", "Jump to the current path in the file manager.", ["Listary"]);
        a("Ctrl+Shift+K", "Listary", "Listary", "打开 Listary 快速搜索。", "Open the Listary quick search.", ["Listary"]);
        a("Ctrl+Shift+F", "Listary", "Listary", "在文件管理器中搜索当前文件夹。", "Search the current folder in the file manager.", ["Listary"]);
        a("Win+F", "Listary", "Listary", "打开 Listary 搜索窗口（可选配置）。", "Open the Listary search window (optional configuration).", ["Listary"]);

        // DisplayFusion
        a("Win+Ctrl+Shift+Left", "DisplayFusion", "DisplayFusion", "将窗口移动到左显示器并跨越。", "Move the window to the left monitor and span.", ["DisplayFusion"]);
        a("Win+Ctrl+Shift+Right", "DisplayFusion", "DisplayFusion", "将窗口移动到右显示器并跨越。", "Move the window to the right monitor and span.", ["DisplayFusion"]);
        a("Win+Ctrl+Shift+Up", "DisplayFusion", "DisplayFusion", "将窗口移动到上方显示器并跨越。", "Move the window to the monitor above and span.", ["DisplayFusion"]);
        a("Win+Ctrl+Shift+Down", "DisplayFusion", "DisplayFusion", "将窗口移动到下方显示器并跨越。", "Move the window to the monitor below and span.", ["DisplayFusion"]);
        a("Win+Ctrl+Shift+M", "DisplayFusion", "DisplayFusion", "将窗口移动到下一个显示器。", "Move the window to the next monitor.", ["DisplayFusion"]);

        // Wox (launcher)
        a("Alt+Space", "Wox", "Wox", "打开 Wox 快速启动器。", "Open the Wox launcher.", ["Wox"]);
    }

    private static void AddLauncherHotkeys(Dictionary<string, List<KnownHotkeyInfo>> d)
    {
        var cat = new LocalizedText("启动器", "Launcher");
        void a(string k, string oz, string oe, string uz, string ue, string[]? p = null)
            => Add(d, k, new(new LocalizedText(oz, oe), new LocalizedText(uz, ue), p, cat));

        // General launcher hotkeys
        a("Alt+Space", "快速启动器（通用）", "Quick launcher (generic)", "打开应用启动器——PowerToys Run/Flow Launcher/Wox/Keyboad 等。", "Open an app launcher such as PowerToys Run, Flow Launcher, Wox, or Keypirinha.", ["PowerToys", "flow.launcher", "Wox", "Keyboad"]);
    }

    private static void AddMediaHotkeys(Dictionary<string, List<KnownHotkeyInfo>> d)
    {
        var cat = new LocalizedText("媒体", "Media");
        void a(string k, string oz, string oe, string uz, string ue, string[]? p = null)
            => Add(d, k, new(new LocalizedText(oz, oe), new LocalizedText(uz, ue), p, cat));

        // Spotify
        a("Ctrl+Left", "Spotify", "Spotify", "上一首。", "Previous track.", ["Spotify"]);
        a("Ctrl+Right", "Spotify", "Spotify", "下一首。", "Next track.", ["Spotify"]);
        a("Ctrl+Up", "Spotify", "Spotify", "增加音量。", "Increase volume.", ["Spotify"]);
        a("Ctrl+Down", "Spotify", "Spotify", "减小音量。", "Decrease volume.", ["Spotify"]);
        a("Ctrl+Shift+Left", "Spotify", "Spotify", "快退。", "Rewind.", ["Spotify"]);
        a("Ctrl+Shift+Right", "Spotify", "Spotify", "快进。", "Fast-forward.", ["Spotify"]);
        a("Alt+Left", "Spotify", "Spotify", "返回到上一个页面。", "Go back to the previous page.", ["Spotify"]);
        a("Alt+Right", "Spotify", "Spotify", "前进到下一个页面。", "Go forward to the next page.", ["Spotify"]);
        a("Ctrl+L", "Spotify", "Spotify", "聚焦搜索框。", "Focus the search box.", ["Spotify"]);

        // MPC-HC / MPC-BE (empty, shares Ctrl+Shift+Esc with Task Manager)
    }

    private static void AddGamingHotkeys(Dictionary<string, List<KnownHotkeyInfo>> d)
    {
        var cat = new LocalizedText("游戏", "Gaming");
        void a(string k, string oz, string oe, string uz, string ue, string[]? p = null)
            => Add(d, k, new(new LocalizedText(oz, oe), new LocalizedText(uz, ue), p, cat));

        // Steam
        a("Shift+Tab", "Steam", "Steam", "在游戏中打开 Steam 覆盖层。", "Open the Steam overlay in-game.", ["Steam"]);
        a("Ctrl+Shift+F", "Steam", "Steam", "在游戏内显示 FPS 计数器（如启用）。", "Show the in-game FPS counter (if enabled).", ["Steam"]);
        a("Ctrl+Tab", "Steam", "Steam", "在 Steam 界面中切换视图。", "Switch views in the Steam interface.", ["Steam"]);

        // Xbox Game Bar (already added in system section)

        // SteelSeries GG / Sonar
        a("Alt+Shift+E", "SteelSeries GG", "SteelSeries GG", "打开或关闭 Sonar 均衡器。", "Toggle the Sonar equalizer.", ["SteelSeriesGG"]);
    }

    private static void AddThirdPartyHotkeys(Dictionary<string, List<KnownHotkeyInfo>> d)
    {
        var cat = new LocalizedText("外设驱动", "Peripheral Drivers");
        void a(string k, string oz, string oe, string uz, string ue, string[]? p = null)
            => Add(d, k, new(new LocalizedText(oz, oe), new LocalizedText(uz, ue), p, cat));

        // Logitech G HUB
        a("Ctrl+Shift+F1", "Logitech G HUB", "Logitech G HUB", "切换 G HUB 配置（鼠标灵敏度/DPI 切换）。", "Cycle G HUB profiles (mouse sensitivity / DPI switch).", ["lghub"]);
        a("Ctrl+Shift+F2", "Logitech G HUB", "Logitech G HUB", "切换 G HUB 配置 2。", "Switch to G HUB profile 2.", ["lghub"]);
        a("Ctrl+Shift+F3", "Logitech G HUB", "Logitech G HUB", "切换 G HUB 配置 3。", "Switch to G HUB profile 3.", ["lghub"]);
        a("Ctrl+Shift+F4", "Logitech G HUB", "Logitech G HUB", "切换 G HUB 配置 4。", "Switch to G HUB profile 4.", ["lghub"]);

        // Razer Synapse
        a("Ctrl+Shift+F10", "Razer Synapse", "Razer Synapse", "切换 Razer 设备配置文件。", "Switch the Razer device profile.", ["RazerSynapse", "RazerSynapse3"]);
        a("Ctrl+Shift+F11", "Razer Synapse", "Razer Synapse", "切换 Razer 灯光效果。", "Switch the Razer lighting effect.", ["RazerSynapse", "RazerSynapse3"]);

        // Corsair iCUE
        a("Ctrl+Shift+F12", "Corsair iCUE", "Corsair iCUE", "切换 iCUE 灯光配置文件。", "Switch the iCUE lighting profile.", ["iCUE"]);
        a("Ctrl+Shift+F10", "Corsair iCUE", "Corsair iCUE", "切换 iCUE 硬件配置文件。", "Switch the iCUE hardware profile.", ["iCUE"]);

        // TranslucentTB (empty, shares Ctrl+Shift+Esc with Task Manager)
    }
}
