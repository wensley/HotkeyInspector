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
                KnownHotkeyCatalog.Available.Usage,
                KnownHotkeyCatalog.Available.OwnerApplication);
        }

        var knownInfo = error == NativeMethods.ErrorHotkeyAlreadyRegistered
            ? KnownHotkeyCatalog.Lookup(hotkey)
            : KnownHotkeyCatalog.UnknownOccupied;

        var detail = error switch
        {
            NativeMethods.ErrorHotkeyAlreadyRegistered => knownInfo.Usage,
            0 => "该快捷键不可用。",
            _ => new Win32Exception(error).Message
        };

        return new HotkeyCheckResult(hotkey, false, error, "已占用", detail, knownInfo.OwnerApplication);
    }
}
