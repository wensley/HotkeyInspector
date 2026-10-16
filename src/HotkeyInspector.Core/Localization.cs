namespace HotkeyInspector.Core;

public enum Language
{
    Zh,
    En
}

public sealed record LocalizedText(string Zh, string En)
{
    public string Get(Language language) => language == Language.En
        ? string.IsNullOrEmpty(En) ? Zh : En
        : string.IsNullOrEmpty(Zh) ? En : Zh;

    public static LocalizedText Of(string zh, string en) => new(zh, en);
}
