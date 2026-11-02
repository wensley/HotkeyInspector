using HotkeyInspector.Core;

namespace HotkeyInspector.App;

internal static class Strings
{
    private static readonly Dictionary<string, LocalizedText> Map = new(StringComparer.Ordinal)
    {
        // Window / header
        ["AppTitle"] = L("快捷键检测", "Hotkey Inspector"),
        ["Subtitle"] = L(
            "检测 Windows 全局快捷键是否已被系统或其他应用占用。通过热键目录、运行时进程分析与内置快捷键知识库推断占用方。",
            "Check whether a Windows global hotkey is already taken by the system or another app. Uses a known-hotkey catalog, runtime process analysis, and the built-in knowledge base to infer the owner."),

        // Input row
        ["LabelHotkey"] = L("快捷键", "Shortcut"),
        ["BtnCheck"] = L("检测", "Check"),
        ["BtnCapture"] = L("捕获", "Capture"),
        ["BtnScan"] = L("扫描常见组合", "Scan common combos"),
        ["BtnExport"] = L("导出", "Export"),
        ["BtnDotnet"] = L("下载 .NET 运行时", "Download .NET Runtime"),
        ["InputHint"] = L("点击输入框即可直接按键捕获；也可手动输入后点击「检测」。", "Click the box and press a combo to capture it, or type one and click Check."),
        ["CapturePrompt"] = L(
            "该组合已被占用，但无法确定占用应用。若它涉及 Win、Alt+Tab、Ctrl+Esc 等由系统优先处理的按键，请输入框会被系统抢先处理而无法捕获，请点击「全屏并锁定以捕获快捷键」。",
            "This combo is in use, but the owning app could not be determined. If it involves system-reserved keys such as Win, Alt+Tab, or Ctrl+Esc, the input box cannot capture it — use “Go fullscreen & lock to capture shortcuts” instead."),
        ["Hint"] = L(
            "提示：占用应用列优先显示已知系统/第三方热键目录中的归属信息；知识库用途列显示该组合在其他应用中的常见含义；候选进程列显示当前正在运行的、可能与热键相关的进程。",
            "Tip: the Owner column shows attribution from the known-hotkey catalog first; the KB usage column shows what the combo commonly means in other apps; the Candidates column lists running processes that may relate to the hotkey."),

        // Live detector (non-locked, works in the input box)
        ["LiveMatchesTitle"] = L("知识库匹配", "Knowledge base matches"),
        ["LiveNoMatch"] = L("知识库中未找到该组合", "No matching shortcut in the knowledge base"),

        // System-reserved shortcuts (desktop analogue of the web page's reserved list)
        ["ReservedTitle"] = L("系统保留快捷键", "System-reserved shortcuts"),
        ["ReservedCapturable"] = L("可被本应用捕获（全屏锁定后）", "Capturable by this app (after fullscreen lock)"),
        ["ReservedUncapturable"] = L("任何程序都无法捕获（安全注意序列）", "Cannot be captured by any program (secure attention sequence)"),

        // Grid columns
        ["ColHotkey"] = L("快捷键", "Shortcut"),
        ["ColStatus"] = L("状态", "Status"),
        ["ColOwner"] = L("占用应用", "Owner"),
        ["ColDetail"] = L("用途说明", "Usage"),
        ["ColKnowledge"] = L("知识库用途", "KB usage"),
        ["ColCandidates"] = L("候选进程", "Candidates"),
        ["ColError"] = L("错误码", "Code"),

        // Footer
        ["BtnCopy"] = L("复制", "Copy"),
        ["BtnClear"] = L("清空", "Clear"),
        ["StatusReady"] = L("就绪", "Ready"),

        // Language toggle shows the *other* language
        ["BtnLanguage"] = L("English", "中文"),

        // Fullscreen lock capture
        ["BtnLockCapture"] = L("全屏并锁定以捕获快捷键", "Go fullscreen & lock to capture shortcuts"),
        ["CaptureTitle"] = L("全屏锁定捕获", "Fullscreen lock capture"),
        ["CaptureHint"] = L("请按下任意组合键…", "Press any key combo…"),
        ["CaptureExitHint"] = L("按 Esc 退出", "Press Esc to exit"),
        ["CaptureUseHint"] = L("捕获到的组合会填入上方输入框并自动检测。", "A captured combo is filled into the input box and checked automatically."),
        ["CaptureNote"] = L(
            "锁定后，Win、Alt+Tab、Ctrl+Esc 等被系统抢占的组合也会被本应用捕获并拦截。Esc 用于退出锁定，因此无法捕获单独的 Esc。Ctrl+Alt+Del 属于安全注意序列，无法被任何程序捕获。",
            "While locked, combos reserved by the system such as Win, Alt+Tab, and Ctrl+Esc are captured and suppressed by this app. Esc exits the lock, so a bare Esc cannot be captured. Ctrl+Alt+Del is a secure attention sequence and cannot be captured by any program."),
        ["CaptureNoMatch"] = L("知识库中未找到该组合", "No matching shortcut in the knowledge base"),
        ["CaptureMatches"] = L("知识库匹配", "Knowledge base matches"),
        ["CaptureWaitingMods"] = L("（等待非修饰键）", "(waiting for a non-modifier key)"),

        // Dynamic status messages
        ["StatusScanningWait"] = L("正在扫描，请稍候。", "Scanning, please wait."),
        ["StatusScanningStart"] = L("正在扫描常见快捷键...", "Scanning common shortcuts..."),
        ["StatusScanningProgress"] = L("正在扫描... 已检测 {0} 个", "Scanning... {0} checked"),
        ["StatusScanDone"] = L("扫描完成，共检测 {0} 个快捷键。", "Scan complete: {0} shortcuts checked."),
        ["StatusScanFailed"] = L("扫描失败：{0}", "Scan failed: {0}"),
        ["StatusNothingToCopy"] = L("没有可复制的结果。", "No results to copy."),
        ["StatusCopied"] = L("已复制 {0} 行结果。", "Copied {0} rows."),
        ["StatusCopyFailed"] = L("复制失败：{0}", "Copy failed: {0}"),
        ["StatusNothingToExport"] = L("没有可导出的结果。", "No results to export."),
        ["StatusExported"] = L("已导出 {0} 行结果。", "Exported {0} rows."),
        ["StatusExportFailed"] = L("导出失败：{0}", "Export failed: {0}"),
        ["StatusHiddenToTray"] = L("已隐藏到托盘。", "Minimized to tray."),
        ["StatusChecked"] = L("{0}: {1}", "{0}: {1}"),

        // Export dialog
        ["ExportDialogTitle"] = L("导出快捷键检测结果", "Export hotkey results"),
        ["ExportCsvFilter"] = L("CSV 文件 (*.csv)|*.csv|文本文件 (*.txt)|*.txt", "CSV file (*.csv)|*.csv|Text file (*.txt)|*.txt"),

        // CSV header
        ["CsvHeader"] = L("快捷键,状态,占用应用,用途说明,知识库用途,错误码", "Shortcut,Status,Owner,Usage,KB usage,Error code"),

        // Invalid row
        ["StatusInvalid"] = L("无效", "Invalid"),
        ["EmptyHotkey"] = L("(空)", "(empty)"),

        // Tray
        ["TrayTooltip"] = L("快捷键检测", "Hotkey Inspector"),
        ["TrayOpen"] = L("打开", "Open"),
        ["TrayExit"] = L("退出", "Exit"),

        // .NET runtime prompt
        ["RuntimeMissingTitle"] = L("缺少 .NET 运行时", "Missing .NET Runtime"),
        ["RuntimeMissingBody"] = L(
            "未检测到所需的 .NET 桌面运行时（.NET 10）。本程序为框架依赖版本，需要先安装运行时。是否立即打开官方下载页面？",
            "The required .NET Desktop Runtime (.NET 10) was not found. This app is framework-dependent and needs the runtime installed. Open the official download page now?"),
    };

    private static LocalizedText L(string zh, string en) => new(zh, en);

    public static string Get(Language language, string key)
        => Map.TryGetValue(key, out var text) ? text.Get(language) : key;

    public static string Format(Language language, string key, params object[] args)
        => string.Format(Get(language, key), args);
}
