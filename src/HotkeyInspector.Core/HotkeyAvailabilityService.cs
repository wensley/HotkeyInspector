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
                KnownHotkeyCatalog.Available,
                null,
                [],
                HotkeyKnowledge.FindMatches(hotkey.DisplayText).ToList());
        }

        var knownInfo = error == NativeMethods.ErrorHotkeyAlreadyRegistered
            ? KnownHotkeyCatalog.Lookup(hotkey)
            : KnownHotkeyCatalog.UnknownOccupied;

        var candidates = ProcessHotkeyMatcher.GetRunningGuiProcesses().ToList();
        var matchedProcess = error == NativeMethods.ErrorHotkeyAlreadyRegistered
            ? ProcessHotkeyMatcher.FindMatchingProcess(knownInfo.ProcessNames)
            : null;

        return new HotkeyCheckResult(
            hotkey,
            false,
            error,
            knownInfo,
            matchedProcess,
            candidates,
            HotkeyKnowledge.FindMatches(hotkey.DisplayText).ToList());
    }
}
