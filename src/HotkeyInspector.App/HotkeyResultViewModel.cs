namespace HotkeyInspector.App;

public sealed class HotkeyResultViewModel
{
    public required string Hotkey { get; init; }

    public required string Status { get; init; }

    public required string OwnerApplication { get; init; }

    public required string Detail { get; init; }

    public int ErrorCode { get; init; }

    public string? CandidateProcesses { get; init; }

    public bool HasCandidates => !string.IsNullOrWhiteSpace(CandidateProcesses);
}
