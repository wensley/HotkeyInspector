using System.ComponentModel;
using System.Globalization;
using HotkeyInspector.Core;

namespace HotkeyInspector.App;

/// <summary>
/// 应用级本地化单例。XAML 通过 <c>{Binding [Key], Source={x:Static local:Localization.Instance}}</c>
/// 绑定到索引器；切换 <see cref="Language"/> 时触发 <c>Item[]</c> 通知，界面即时刷新。
/// </summary>
public sealed class Localization : INotifyPropertyChanged
{
    public static Localization Instance { get; } = new();

    private Language _language = DetectSystemLanguage();

    private static Language DetectSystemLanguage()
        => string.Equals(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, "zh", StringComparison.OrdinalIgnoreCase)
            ? Language.Zh
            : Language.En;

    public Language Language
    {
        get => _language;
        set
        {
            if (_language == value)
            {
                return;
            }

            _language = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string this[string key] => Strings.Get(_language, key);

    public static string T(string key) => Strings.Get(Instance._language, key);

    public static string F(string key, params object[] args) => Strings.Format(Instance._language, key, args);

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? LanguageChanged;
}
