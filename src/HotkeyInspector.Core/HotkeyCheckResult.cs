using HotkeyInspector.Infrastructure;

namespace HotkeyInspector.Core;

public sealed record HotkeyResultText(string Status, string OwnerApplication, string Detail);

public sealed record HotkeyCheckResult(
    HotkeyDefinition Hotkey,
    bool IsAvailable,
    int ErrorCode,
    KnownHotkeyInfo? Known,
    string? MatchedProcess,
    IReadOnlyList<ProcessInfo> Candidates,
    IReadOnlyList<KnowledgeMatch> Knowledge)
{
    private static string T(Language language, string zh, string en)
        => language == Language.En ? en : zh;

    public HotkeyResultText Render(Language language)
    {
        string owner;
        string detail;

        if (MatchedProcess is not null)
        {
            owner = MatchedProcess;
            detail = T(language,
                $"该快捷键已被「{MatchedProcess}」注册。",
                $"This shortcut is registered by \"{MatchedProcess}\".")
                + (Known?.Usage.Get(language) ?? string.Empty);
        }
        else if (Known is not null && Known != KnownHotkeyCatalog.UnknownOccupied && Known != KnownHotkeyCatalog.Available)
        {
            owner = Known.OwnerApplication.Get(language);
            detail = Known.Usage.Get(language) + RunningSuffix(language);
        }
        else
        {
            owner = T(language, "未知应用", "Unknown app");

            var guiProcesses = Candidates
                .Select(c => c.ProcessName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(10)
                .ToList();

            detail = guiProcesses.Count > 0
                ? T(language,
                    $"该快捷键已被注册，但 Windows 未公开占用应用。当前正在运行的可能注册全局热键的进程：{string.Join("、", guiProcesses)}。",
                    $"This shortcut is registered, but Windows did not report the owning app. Processes currently running that may register global hotkeys: {string.Join(", ", guiProcesses)}.")
                : T(language,
                    "该快捷键已被注册，但 Windows 未公开占用应用信息。",
                    "This shortcut is registered, but Windows did not report the owning app.");
        }

        detail = ErrorCode switch
        {
            NativeMethods.ErrorHotkeyAlreadyRegistered => detail,
            0 => T(language, "该快捷键不可用。", "This shortcut is unavailable."),
            _ => detail + T(language,
                $" 错误信息：{new System.ComponentModel.Win32Exception(ErrorCode).Message}",
                $" Error: {new System.ComponentModel.Win32Exception(ErrorCode).Message}")
        };

        var status = IsAvailable
            ? T(language, "可用", "Available")
            : T(language, "已占用", "In use");

        return new HotkeyResultText(status, owner, detail);
    }

    private string RunningSuffix(Language language)
    {
        if (Known?.ProcessNames is not { Length: > 0 } names)
        {
            return string.Empty;
        }

        var found = Candidates
            .Where(c => names.Any(p => string.Equals(p, c.ProcessName, StringComparison.OrdinalIgnoreCase)))
            .Select(c => c.ProcessName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (found.Count == 0)
        {
            return string.Empty;
        }

        return language == Language.En
            ? $" (currently running: {string.Join(", ", found)})"
            : $"（当前正在运行：{string.Join("、", found)}）";
    }
}
