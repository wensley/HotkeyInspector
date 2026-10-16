using HotkeyInspector.Core;

namespace HotkeyInspector.App;

/// <summary>一条检测行的原始数据（与语言无关），语言切换时据此重建视图模型。</summary>
public sealed class HotkeyRow
{
    public required string Hotkey { get; init; }

    public HotkeyCheckResult? Result { get; init; }

    public string RawText { get; init; } = string.Empty;

    public LocalizedText? ParseError { get; init; }
}

public sealed class HotkeyResultViewModel
{
    private const int MaxKnowledge = 8;

    public required string Hotkey { get; init; }

    public required string Status { get; init; }

    public required string OwnerApplication { get; init; }

    public required string Detail { get; init; }

    public string? Knowledge { get; init; }

    public string? CandidateProcesses { get; init; }

    public int ErrorCode { get; init; }

    public bool HasCandidates => !string.IsNullOrWhiteSpace(CandidateProcesses);

    public bool HasKnowledge => !string.IsNullOrWhiteSpace(Knowledge);

    public static HotkeyResultViewModel Create(HotkeyRow row, Language language)
    {
        if (row.Result is null)
        {
            return new HotkeyResultViewModel
            {
                Hotkey = string.IsNullOrWhiteSpace(row.RawText) ? Localization.T("EmptyHotkey") : row.RawText,
                Status = Localization.T("StatusInvalid"),
                OwnerApplication = KnownHotkeyCatalog.Invalid.OwnerApplication.Get(language),
                Detail = row.ParseError?.Get(language) ?? string.Empty,
                ErrorCode = 0
            };
        }

        var text = row.Result.Render(language);
        return new HotkeyResultViewModel
        {
            Hotkey = row.Result.Hotkey.DisplayText,
            Status = text.Status,
            OwnerApplication = text.OwnerApplication,
            Detail = text.Detail,
            Knowledge = BuildKnowledge(row.Result, language),
            CandidateProcesses = BuildCandidates(row.Result),
            ErrorCode = row.Result.ErrorCode
        };
    }

    private static string? BuildKnowledge(HotkeyCheckResult result, Language language)
    {
        if (result.Knowledge.Count == 0)
        {
            return null;
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var parts = new List<string>();
        foreach (var match in result.Knowledge)
        {
            var app = match.App.Get(language);
            var action = match.Action.Get(language);
            var entry = $"{app}: {action}";
            if (seen.Add(entry))
            {
                parts.Add(entry);
            }

            if (parts.Count >= MaxKnowledge)
            {
                break;
            }
        }

        return string.Join(language == Language.En ? "; " : "；", parts);
    }

    private static string? BuildCandidates(HotkeyCheckResult result)
    {
        if (result.Candidates.Count == 0)
        {
            return null;
        }

        var knownNames = result.Known?.ProcessNames;
        var names = result.Candidates
            .Where(p => knownNames is null || !knownNames.Any(n => string.Equals(n, p.ProcessName, StringComparison.OrdinalIgnoreCase)))
            .Select(p => p.ProcessName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();

        return names.Count > 0 ? string.Join("; ", names) : null;
    }
}
