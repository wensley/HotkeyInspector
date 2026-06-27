using System.ComponentModel;
using System.Runtime.InteropServices;
using HotkeyInspector.Infrastructure;

namespace HotkeyInspector.Core;

public sealed class HotkeyAvailabilityService
{
    private int _nextId = 0x4811;

    public HotkeyCheckResult Check(string text)
    {
        var hotkey = HotkeyParser.Parse(text);
        return Check(hotkey);
    }

    public HotkeyCheckResult Check(HotkeyDefinition hotkey)
    {
        var id = _nextId++;
        var modifiers = (uint)(hotkey.Modifiers | HotkeyModifier.NoRepeat);

        Marshal.SetLastPInvokeError(0);
        var registered = NativeMethods.RegisterHotKey(IntPtr.Zero, id, modifiers, hotkey.VirtualKey);
        var error = Marshal.GetLastPInvokeError();

        if (registered)
        {
            NativeMethods.UnregisterHotKey(IntPtr.Zero, id);
            return new HotkeyCheckResult(
                hotkey,
                true,
                0,
                "可用",
                "该快捷键当前可注册为全局快捷键。",
                "无");
        }

        var knownInfo = error == NativeMethods.ErrorHotkeyAlreadyRegistered
            ? KnownHotkeyCatalog.Lookup(hotkey)
            : KnownHotkeyCatalog.UnknownOccupied;

        var candidates = ProcessHotkeyMatcher.GetRunningGuiProcesses().ToList();
        var matchedProcess = error == NativeMethods.ErrorHotkeyAlreadyRegistered
            ? ProcessHotkeyMatcher.FindMatchingProcess(knownInfo.ProcessNames)
            : null;

        string ownerApp;
        string detail;

        if (matchedProcess != null)
        {
            ownerApp = matchedProcess;
            detail = $"该快捷键已被「{matchedProcess}」注册。{knownInfo.Usage}";
        }
        else if (knownInfo != KnownHotkeyCatalog.UnknownOccupied && knownInfo != KnownHotkeyCatalog.Available)
        {
            ownerApp = knownInfo.OwnerApplication;
            var runningSuffix = "";
            if (knownInfo.ProcessNames?.Length > 0)
            {
                var foundCandidates = candidates
                    .Where(c => knownInfo.ProcessNames.Any(p =>
                        string.Equals(p, c.ProcessName, StringComparison.OrdinalIgnoreCase)))
                    .Select(c => c.ProcessName)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (foundCandidates.Count > 0)
                    runningSuffix = $"（当前正在运行：{string.Join("、", foundCandidates)}）";
            }

            detail = knownInfo.Usage;
            if (!string.IsNullOrEmpty(runningSuffix))
                detail += runningSuffix;
        }
        else
        {
            ownerApp = "未知应用";

            // Show GUI processes as candidates
            var guiProcesses = candidates
                .Select(c => c.ProcessName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(10)
                .ToList();

            detail = guiProcesses.Count > 0
                ? $"该快捷键已被注册，但 Windows 未公开占用应用。当前正在运行的可能注册全局热键的进程：{string.Join("、", guiProcesses)}。"
                : "该快捷键已被注册，但 Windows 未公开占用应用信息。";
        }

        detail = error switch
        {
            NativeMethods.ErrorHotkeyAlreadyRegistered => detail,
            0 => "该快捷键不可用。",
            _ => $"{detail} 错误信息：{new Win32Exception(error).Message}"
        };

        return new HotkeyCheckResult(
            hotkey,
            false,
            error,
            "已占用",
            detail,
            ownerApp,
            candidates,
            knownInfo.ProcessNames);
    }
}
