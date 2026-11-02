using HotkeyInspector.Core;

namespace HotkeyInspector.Tests;

public sealed class HotkeyAvailabilityServiceTests
{
    private sealed class FakeProbe(bool available, int errorCode = 0) : IHotkeyProbe
    {
        public int Calls { get; private set; }

        public int LastId { get; private set; }

        public bool TryAcquire(HotkeyDefinition hotkey, int id, out int errorCodeOut)
        {
            Calls++;
            LastId = id;
            errorCodeOut = errorCode;
            return available;
        }
    }

    private sealed class FakeProcesses(params string[] names) : IRunningProcessSource
    {
        public IReadOnlyList<ProcessCandidate> GetRunningProcesses()
            => names.Select((name, index) => new ProcessCandidate(index + 1, name)).ToList();

        public string? FindMatch(IReadOnlyList<string>? processNames)
            => processNames?.FirstOrDefault(name => names.Contains(name, StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public void Available_DoesNotTouchProcesses()
    {
        var service = new HotkeyAvailabilityService(new FakeProbe(true), new FakeProcesses());

        var result = service.Check("Ctrl+Alt+Shift+F24");

        Assert.True(result.IsAvailable);
        Assert.Equal(0, result.ErrorCode);
        Assert.Same(KnownHotkeyCatalog.Available, result.Known);
        Assert.Empty(result.Candidates);
    }

    [Fact]
    public void Occupied_UsesCatalogAndRunningProcess()
    {
        var service = new HotkeyAvailabilityService(new FakeProbe(false, HotkeyErrorCodes.AlreadyRegistered), new FakeProcesses("Teams"));

        var result = service.Check("Ctrl+Shift+Esc");

        Assert.False(result.IsAvailable);
        Assert.Equal(1409, result.ErrorCode);
        Assert.Equal("Microsoft Teams", result.Known?.OwnerApplication.Zh);
        Assert.Equal("Teams", result.MatchedProcess);
        Assert.Single(result.Candidates);
        Assert.NotEmpty(result.Knowledge);
    }

    [Fact]
    public void Occupied_WithoutRunningOwner_UsesTopPriorityEntry()
    {
        var service = new HotkeyAvailabilityService(new FakeProbe(false, HotkeyErrorCodes.AlreadyRegistered), new FakeProcesses("Notepad"));

        var result = service.Check("Ctrl+Shift+Esc");

        Assert.Equal("任务管理器 / 其他工具", result.Known?.OwnerApplication.Zh);
        Assert.Null(result.MatchedProcess);
        Assert.Single(result.Candidates);
    }

    [Fact]
    public void Occupied_UnknownComboFallsBackToUnknownOccupied()
    {
        var service = new HotkeyAvailabilityService(new FakeProbe(false, HotkeyErrorCodes.AlreadyRegistered), new FakeProcesses());

        var result = service.Check("Ctrl+PageUp");

        Assert.Same(KnownHotkeyCatalog.UnknownOccupied, result.Known);
        Assert.Null(result.MatchedProcess);
        Assert.Empty(result.Candidates);
    }

    [Fact]
    public void OtherErrors_DoNotConsultCatalogOrProcesses()
    {
        var processes = new FakeProcesses("Teams");
        var service = new HotkeyAvailabilityService(new FakeProbe(false, 87), processes);

        var result = service.Check("Ctrl+Alt+K");

        Assert.Equal(87, result.ErrorCode);
        Assert.Same(KnownHotkeyCatalog.UnknownOccupied, result.Known);
        Assert.Empty(result.Candidates);
    }

    [Fact]
    public void ProbeReceivesIdsInValidRange()
    {
        var probe = new FakeProbe(true);
        var service = new HotkeyAvailabilityService(probe, new FakeProcesses());

        for (var i = 0; i < 3; i++)
        {
            service.Check("Ctrl+Alt+K");
        }

        Assert.Equal(3, probe.Calls);
        Assert.InRange(probe.LastId, 1, 0xBFFF);
    }
}
