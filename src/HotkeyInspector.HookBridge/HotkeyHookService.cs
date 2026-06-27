using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace HotkeyInspector.HookBridge;

[SupportedOSPlatform("windows")]
public sealed class HotkeyHookService : IDisposable
{
    private const string ShmName = "Global\\HotkeyInspectorSHM";
    private const int ShmSize = 1024 * 64;
    private const int RecordSize = 32;
    private const int MaxRecords = ShmSize / RecordSize;

    private IntPtr _shmHandle;
    private IntPtr _shmBuffer;
    private bool _disposed;

    public HotkeyHookService()
    {
        OpenSharedMemory();
    }

    private void OpenSharedMemory()
    {
        _shmHandle = OpenFileMapping(FileMapAccess.AllAccess, false, ShmName);
        if (_shmHandle == IntPtr.Zero)
        {
            _shmBuffer = IntPtr.Zero;
            return;
        }

        _shmBuffer = MapViewOfFile(_shmHandle, FileMapAccess.AllAccess, 0, 0, (UIntPtr)ShmSize);
    }

    public bool IsHookActive => _shmBuffer != IntPtr.Zero;

    public List<HotkeyRecord> ReadRecords()
    {
        var records = new List<HotkeyRecord>();
        if (_shmBuffer == IntPtr.Zero) return records;

        unsafe
        {
            var buffer = (byte*)_shmBuffer;
            for (var i = 0; i < MaxRecords; i++)
            {
                var offset = i * RecordSize;
                var processId = *(uint*)(buffer + offset);
                if (processId == 0) continue; // empty slot

                records.Add(new HotkeyRecord
                {
                    ProcessId = *(int*)(buffer + offset),
                    ThreadId = *(int*)(buffer + offset + 4),
                    HotkeyId = *(int*)(buffer + offset + 8),
                    Modifiers = *(uint*)(buffer + offset + 12),
                    VirtualKey = *(uint*)(buffer + offset + 16),
                    Timestamp = *(uint*)(buffer + offset + 20),
                    Action = (char)*(buffer + offset + 24)
                });
            }
        }

        return records;
    }

    public IEnumerable<(int ProcessId, uint Modifiers, uint VirtualKey)> GetActiveRegistrations()
    {
        var seen = new HashSet<(int, uint, uint)>();
        foreach (var record in ReadRecords())
        {
            if (record.Action == 'R')
            {
                seen.Add((record.ProcessId, record.Modifiers, record.VirtualKey));
            }
        }
        return seen;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_shmBuffer != IntPtr.Zero)
        {
            UnmapViewOfFile(_shmBuffer);
            _shmBuffer = IntPtr.Zero;
        }
        if (_shmHandle != IntPtr.Zero)
        {
            CloseHandle(_shmHandle);
            _shmHandle = IntPtr.Zero;
        }
    }

    // --- P/Invoke ---
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr OpenFileMapping(uint dwDesiredAccess, bool bInheritHandle, string lpName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess,
        uint dwFileOffsetHigh, uint dwFileOffsetLow, UIntPtr dwNumberOfBytesToMap);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    private static class FileMapAccess
    {
        public const uint AllAccess = 0xF001F;
    }
}

public sealed class HotkeyRecord
{
    public int ProcessId { get; init; }
    public int ThreadId { get; init; }
    public int HotkeyId { get; init; }
    public uint Modifiers { get; init; }
    public uint VirtualKey { get; init; }
    public uint Timestamp { get; init; }
    public char Action { get; init; }
}
