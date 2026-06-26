using System.Diagnostics;

namespace HotkeyInspector.Infrastructure;

public static class ProcessHotkeyMatcher
{
    private static readonly Dictionary<string, string> KnownProcessApps = new(StringComparer.OrdinalIgnoreCase)
    {
        ["discord"] = "Discord",
        ["spotify"] = "Spotify",
        ["obs64"] = "OBS Studio",
        ["obs32"] = "OBS Studio",
        ["sharex"] = "ShareX",
        ["greenshot"] = "Greenshot",
        ["snipaste"] = "Snipaste",
        ["powertoys"] = "Microsoft PowerToys",
        ["autohotkey64"] = "AutoHotkey",
        ["autohotkey32"] = "AutoHotkey",
        ["autohotkey"] = "AutoHotkey",
        ["listary"] = "Listary",
        ["flow"] = "Flow Launcher",
        ["flow.launcher"] = "Flow Launcher",
        ["ditto"] = "Ditto Clipboard Manager",
        ["nvidiashare"] = "NVIDIA GeForce Experience",
        ["nvidia share"] = "NVIDIA GeForce Experience",
        ["amdsoftware"] = "AMD Software: Adrenalin",
        ["icue"] = "Corsair iCUE",
        ["razersynapse"] = "Razer Synapse",
        ["razersynapse3"] = "Razer Synapse",
        ["lghub"] = "Logitech G HUB",
        ["logioptions"] = "Logitech Options",
        ["displayfusion"] = "DisplayFusion",
        ["eartrumpet"] = "EarTrumpet",
        ["everything"] = "Everything",
        ["translucenttb"] = "TranslucentTB",
        ["keyboad"] = "Keyboad",
        ["wox"] = "Wox",
    };

    private static readonly Dictionary<string, string> KnownHotkeyOverrides = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Win+Shift+S"] = "截图工具",
        ["Win+Alt+R"] = "Xbox Game Bar",
        ["Win+Alt+G"] = "Xbox Game Bar",
    };

    private static List<string>? _cachedRunning;
    private static DateTime _lastCacheTime;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(3);

    public static IReadOnlyList<string> GetRunningHotkeyApplications()
    {
        if (_cachedRunning != null && DateTime.UtcNow - _lastCacheTime < CacheDuration)
            return _cachedRunning;

        var running = new List<string>();
        try
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    var name = process.ProcessName;
                    if (KnownProcessApps.TryGetValue(name, out var appName) && !running.Contains(appName))
                        running.Add(appName);
                }
                catch
                {
                }
            }
        }
        catch
        {
        }

        _cachedRunning = running;
        _lastCacheTime = DateTime.UtcNow;
        return running;
    }

    public static string? GetOwnerFromRunningProcesses(string displayText)
    {
        if (KnownHotkeyOverrides.TryGetValue(displayText, out var overrideOwner))
            return overrideOwner;

        var running = GetRunningHotkeyApplications();
        return running.Count > 0 ? string.Join(", ", running) : null;
    }

    public static void InvalidateCache()
    {
        _cachedRunning = null;
    }
}
