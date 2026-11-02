using System.Runtime.InteropServices;
using HotkeyInspector.Core;

namespace HotkeyInspector.Infrastructure;

/// <summary>
/// 用 <c>RegisterHotKey</c> 检测组合是否可用：成功即立即注销，不留下任何系统状态。
/// 这是"安全模式"的唯一实现，不注入、不常驻钩子。
/// </summary>
public sealed class Win32HotkeyProbe : IHotkeyProbe
{
    public bool TryAcquire(HotkeyDefinition hotkey, int id, out int errorCode)
    {
        var modifiers = (uint)(hotkey.Modifiers | HotkeyModifier.NoRepeat);

        Marshal.SetLastPInvokeError(0);
        if (NativeMethods.RegisterHotKey(IntPtr.Zero, id, modifiers, hotkey.VirtualKey))
        {
            NativeMethods.UnregisterHotKey(IntPtr.Zero, id);
            errorCode = 0;
            return true;
        }

        errorCode = Marshal.GetLastPInvokeError();
        return false;
    }
}
