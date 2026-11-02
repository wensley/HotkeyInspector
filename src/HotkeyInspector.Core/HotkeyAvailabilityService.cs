namespace HotkeyInspector.Core;

public sealed class HotkeyAvailabilityService
{
    // RegisterHotKey 的应用程序 id 合法范围是 0x0000–0xBFFF（0x4000–0x7FFF 等区段被系统保留）。
    private const int MinHotkeyId = 0x0001;
    private const int MaxHotkeyId = 0xBFFF;

    private readonly IHotkeyProbe _probe;
    private readonly IRunningProcessSource _processes;
    private int _nextId = 0x4810;

    public HotkeyAvailabilityService(IHotkeyProbe probe, IRunningProcessSource processes)
    {
        _probe = probe ?? throw new ArgumentNullException(nameof(probe));
        _processes = processes ?? throw new ArgumentNullException(nameof(processes));
    }

    public HotkeyCheckResult Check(string text) => Check(HotkeyParser.Parse(text));

    public HotkeyCheckResult Check(HotkeyDefinition hotkey)
    {
        var knowledge = HotkeyKnowledge.FindMatches(hotkey.DisplayText).ToList();

        if (_probe.TryAcquire(hotkey, NextHotkeyId(), out var error))
        {
            return new HotkeyCheckResult(
                hotkey,
                true,
                0,
                KnownHotkeyCatalog.Available,
                null,
                [],
                knowledge);
        }

        IReadOnlyList<ProcessCandidate> candidates = [];
        string? matchedProcess = null;
        var knownInfo = KnownHotkeyCatalog.UnknownOccupied;

        if (error == HotkeyErrorCodes.AlreadyRegistered)
        {
            // 同一组合可能有多个归属（如 Ctrl+Shift+Esc：任务管理器 + Teams），
            // 目录取优先级最高者作为默认答案，但进程匹配要覆盖所有归属。
            var owners = KnownHotkeyCatalog.LookupAll(hotkey.DisplayText);
            knownInfo = owners.Count > 0 ? owners[0] : KnownHotkeyCatalog.UnknownOccupied;

            var processNames = owners
                .SelectMany(owner => owner.ProcessNames ?? [])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            candidates = _processes.GetRunningProcesses();
            matchedProcess = _processes.FindMatch(processNames);

            if (matchedProcess is not null)
            {
                knownInfo = PickMatchedOwner(owners, matchedProcess) ?? knownInfo;
            }
        }

        return new HotkeyCheckResult(
            hotkey,
            false,
            error,
            knownInfo,
            matchedProcess,
            candidates,
            knowledge);
    }

    /// <summary>命中的进程名可能来自任一归属，回填该归属以便说明文字与占用方一致。</summary>
    private static KnownHotkeyInfo? PickMatchedOwner(IReadOnlyList<KnownHotkeyInfo> owners, string matchedProcess)
    {
        var names = matchedProcess.Split(", ", StringSplitOptions.RemoveEmptyEntries);
        return owners.FirstOrDefault(owner => owner.ProcessNames?.Any(
            name => names.Contains(name, StringComparer.OrdinalIgnoreCase)) == true);
    }

    /// <summary>在合法范围内循环分配热键 id，避免长期运行后越界。</summary>
    private int NextHotkeyId()
    {
        while (true)
        {
            var current = Volatile.Read(ref _nextId);
            var next = current >= MaxHotkeyId ? MinHotkeyId : current + 1;
            if (Interlocked.CompareExchange(ref _nextId, next, current) == current)
            {
                return next;
            }
        }
    }
}
