using System.Collections.ObjectModel;
using HotkeyInspector.Core;

namespace HotkeyInspector.App;

/// <summary>
/// 结果表的单一数据源：保存与语言无关的原始行（<see cref="HotkeyRow"/>），
/// 再按当前语言投影成可绑定的视图模型；语言切换时只重建投影，不重新检测。
/// </summary>
public sealed class HotkeyResultsModel
{
    private readonly List<HotkeyRow> _rows = [];

    public ObservableCollection<HotkeyResultViewModel> Items { get; } = [];

    public int Count => _rows.Count;

    public void Add(HotkeyRow row, Language language)
    {
        _rows.Add(row);
        Items.Add(HotkeyResultViewModel.Create(row, language));
    }

    public void Rebuild(Language language)
    {
        Items.Clear();
        foreach (var row in _rows)
        {
            Items.Add(HotkeyResultViewModel.Create(row, language));
        }
    }

    public void Clear()
    {
        _rows.Clear();
        Items.Clear();
    }

    public string ToTabSeparated()
        => string.Join(Environment.NewLine, Items.Select(r => string.Join('\t',
            r.Hotkey, r.Status, r.OwnerApplication, r.Detail, r.Knowledge ?? string.Empty, r.ErrorCode)));

    public string ToCsv()
    {
        static string Escape(string value) => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";

        var lines = new List<string> { Localization.T("CsvHeader") };
        lines.AddRange(Items.Select(r => string.Join(',',
            Escape(r.Hotkey), Escape(r.Status), Escape(r.OwnerApplication), Escape(r.Detail),
            Escape(r.Knowledge ?? string.Empty), r.ErrorCode)));
        return string.Join(Environment.NewLine, lines);
    }
}
