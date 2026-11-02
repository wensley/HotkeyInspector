using System.Diagnostics;
using HotkeyInspector.Core;

namespace HotkeyInspector.Infrastructure;

/// <summary>
/// 从 <see cref="Process.GetProcesses()"/> 读取带主窗口的进程，并缓存 5 秒。
/// 目录里的进程名与实际可执行文件名常有差异（如 VS Code、AutoHotkey、Razer），
/// 这里用别名表把两者对上。
/// </summary>
public sealed class RunningProcessSource : IRunningProcessSource
{
    private const int RefreshIntervalMs = 5000;

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

    private readonly object _cacheLock = new();
    private List<ProcessCandidate>? _cached;
    private DateTime _lastRefreshUtc;

    public IReadOnlyList<ProcessCandidate> GetRunningProcesses()
    {
        lock (_cacheLock)
        {
            if (_cached is not null && (DateTime.UtcNow - _lastRefreshUtc).TotalMilliseconds < RefreshIntervalMs)
            {
                return _cached;
            }

            var list = new List<ProcessCandidate>();
            try
            {
                foreach (var process in Process.GetProcesses())
                {
                    try
                    {
                        if (process.HasExited || process.MainWindowHandle == IntPtr.Zero)
                        {
                            continue;
                        }

                        list.Add(new ProcessCandidate(process.Id, process.ProcessName));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[RunningProcessSource] 读取进程 {process.Id} 失败：{ex.Message}");
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RunningProcessSource] 枚举进程失败：{ex.Message}");
                return _cached ?? list;
            }

            _cached = list;
            _lastRefreshUtc = DateTime.UtcNow;
            return list;
        }
    }

    public string? FindMatch(IReadOnlyList<string>? processNames)
    {
        if (processNames is null || processNames.Count == 0)
        {
            return null;
        }

        var runningNames = new HashSet<string>(
            GetRunningProcesses().Select(p => p.ProcessName),
            StringComparer.OrdinalIgnoreCase);

        var matched = new List<string>();
        foreach (var name in processNames)
        {
            if (ProcessAliases.TryGetValue(name, out var aliases))
            {
                if (aliases.Any(runningNames.Contains))
                {
                    matched.Add(name);
                }
            }
            else if (runningNames.Contains(name))
            {
                matched.Add(name);
            }
        }

        return matched.Count > 0 ? string.Join(", ", matched) : null;
    }

    public void InvalidateCache()
    {
        lock (_cacheLock)
        {
            _cached = null;
        }
    }
}
