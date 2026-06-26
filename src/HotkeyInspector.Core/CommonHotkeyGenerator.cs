namespace HotkeyInspector.Core;

public static class CommonHotkeyGenerator
{
    public static IEnumerable<string> Generate()
    {
        string[] modifierSets =
        [
            "Ctrl+Alt",
            "Ctrl+Shift",
            "Alt+Shift",
            "Ctrl+Alt+Shift",
            "Win+Shift",
            "Win+Alt"
        ];

        var keys = Enumerable.Range('A', 26)
            .Select(value => ((char)value).ToString())
            .Concat(Enumerable.Range(0, 10).Select(value => value.ToString()))
            .Concat(Enumerable.Range(1, 12).Select(value => $"F{value}"));

        foreach (var modifiers in modifierSets)
        {
            foreach (var key in keys)
            {
                yield return $"{modifiers}+{key}";
            }
        }
    }
}
