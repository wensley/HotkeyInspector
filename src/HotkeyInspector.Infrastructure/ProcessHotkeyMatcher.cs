using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HotkeyInspector.Infrastructure;

public static class ProcessHotkeyMatcher
{
    private static List<ProcessInfo>? _cachedProcesses;
    private static DateTime _lastCacheTime;

    private static readonly Dictionary<string, string[]> ProcessAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Code"] = ["Code"],
        ["devenv"] = ["devenv"],
        ["ShareX"] = ["ShareX"],
        ["Snipaste"] = ["Snipaste"],
        ["Greenshot"] = ["Greenshot"],
        ["obs64"] = ["obs64"],
        ["obs32"] = ["obs32"],
        ["PowerToys"] = ["PowerToys"],
        ["AutoHotkey64"] = ["AutoHotkey64", "AutoHotkey32", "AutoHotkeyUX", "autohotkey"],
        ["Everything"] = ["Everything"],
        ["flow.launcher"] = ["flow.launcher", "flow"],
        ["Listary"] = ["Listary"],
        ["lghub"] = ["lghub"],
        ["RazerSynapse"] = ["RazerSynapse", "RazerSynapse3"],
        ["RazerSynapse3"] = ["RazerSynapse3", "RazerSynapse"],
        ["iCUE"] = ["iCUE"],
    };

    private static readonly HashSet<string> SystemProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "explorer", "SearchHost", "SearchApp", "Widgets", "ShellExperienceHost",
        "GameBar", "Taskmgr", "SnippingTool", "FeedbackHub", "Copilot",
        "SystemSettings", "LogonUI", "winlogon", "ctfmon",
    };

    public static IReadOnlyList<ProcessInfo> GetRunningGuiProcesses()
    {
        const int refreshIntervalMs = 5000;
        if (_cachedProcesses != null && (DateTime.UtcNow - _lastCacheTime).TotalMilliseconds < refreshIntervalMs)
            return _cachedProcesses;

        var list = new List<ProcessInfo>();
        var seenIds = new HashSet<int>();

        try
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.HasExited) continue;

                    var hMain = process.MainWindowHandle;
                    if (hMain == IntPtr.Zero) continue;

                    if (!seenIds.Add(process.Id)) continue;

                    var title = process.MainWindowTitle;
                    var name = process.ProcessName;

                    list.Add(new ProcessInfo(
                        process.Id,
                        name,
                        string.IsNullOrWhiteSpace(title) ? null : title,
                        GetWindowClass(hMain)));
                }
                catch
                {
                }
            }
        }
        catch
        {
        }

        _cachedProcesses = list;
        _lastCacheTime = DateTime.UtcNow;
        return list;
    }

    public static string? FindMatchingProcess(IReadOnlyList<string>? catalogProcessNames)
    {
        if (catalogProcessNames == null || catalogProcessNames.Count == 0)
            return null;

        var running = GetRunningGuiProcesses();
        var runningNames = new HashSet<string>(running.Select(p => p.ProcessName), StringComparer.OrdinalIgnoreCase);

        var matched = new List<string>();
        foreach (var name in catalogProcessNames)
        {
            if (ProcessAliases.TryGetValue(name, out var aliases))
            {
                if (aliases.Any(a => runningNames.Contains(a)))
                    matched.Add(name);
            }
            else if (runningNames.Contains(name))
            {
                matched.Add(name);
            }
        }

        return matched.Count > 0 ? string.Join(", ", matched) : null;
    }

    public static void InvalidateCache()
    {
        _cachedProcesses = null;
    }

    private static string GetWindowClass(IntPtr hWnd)
    {
        try
        {
            const int maxLength = 256;
            var sb = new System.Text.StringBuilder(maxLength);
            return GetClassNameW(hWnd, sb, maxLength) > 0 ? sb.ToString() : "";
        }
        catch
        {
            return "";
        }
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int GetClassNameW(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);
}

public sealed record ProcessInfo(int ProcessId, string ProcessName, string? WindowTitle, string WindowClass);
