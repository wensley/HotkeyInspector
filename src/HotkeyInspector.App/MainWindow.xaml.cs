using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using HotkeyInspector.Core;

namespace HotkeyInspector.App;

public partial class MainWindow : Window
{
    private readonly HotkeyAvailabilityService _checker = new();
    private readonly KeyboardHook _hook = new();
    private readonly List<HotkeyRow> _rows = [];

    private bool _isScanning;
    private bool _captureActive;
    private bool _awaitingCapture;
    private string? _lastCaptured;
    private string? _liveCombo;
    private Func<Language, string>? _statusBuilder;

    private sealed record ReservedHotkey(string Combo, LocalizedText Note, bool Uncapturable);

    private static readonly ReservedHotkey[] ReservedHotkeys =
    [
        new("Win", new("打开或关闭“开始”菜单", "Open or close the Start menu"), false),
        new("Alt+Tab", new("切换打开的窗口", "Switch between open windows"), false),
        new("Ctrl+Esc", new("打开“开始”菜单", "Open the Start menu"), false),
        new("Win+Tab", new("打开任务视图", "Open Task View"), false),
        new("Alt+F4", new("关闭活动窗口", "Close the active window"), false),
        new("Win+D", new("显示或隐藏桌面", "Show or hide the desktop"), false),
        new("Win+L", new("锁定电脑", "Lock the computer"), false),
        new("Win+Shift+S", new("区域截图到剪贴板", "Screenshot a region to clipboard"), false),
        new("PrintScreen", new("全屏截图到剪贴板", "Screenshot the screen to clipboard"), false),
        new("Ctrl+Shift+Esc", new("打开任务管理器", "Open Task Manager"), false),
        new("Ctrl+Alt+Delete", new("打开安全屏幕（安全注意序列）", "Open the security screen (secure attention sequence)"), true),
    ];

    private WindowStyle _prevStyle;
    private ResizeMode _prevResize;
    private WindowState _prevState;
    private bool _prevTopmost;

    public ObservableCollection<HotkeyResultViewModel> Results { get; } = [];

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        _hook.ComboCaptured += OnComboCaptured;
        _hook.ModifiersChanged += OnModifiersChanged;
        _hook.CancelRequested += ExitLockCapture;
        Localization.Instance.LanguageChanged += OnLanguageChanged;
        RebuildReserved();
    }

    private void CheckButton_Click(object sender, RoutedEventArgs e)
    {
        CheckAndAdd(HotkeyTextBox.Text, interactive: true);
        ShowLiveMatches(HotkeyTextBox.Text);
    }

    private void DownloadDotnetButton_Click(object sender, RoutedEventArgs e) => Links.Open(Links.DotnetDownload);

    private void HotkeyTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        _awaitingCapture = true;
        HotkeyTextBox.SelectAll();
    }

    private void HotkeyTextBox_LostFocus(object sender, RoutedEventArgs e) => _awaitingCapture = false;

    private void HotkeyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (!_awaitingCapture)
        {
            return;
        }

        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key == Key.ImeProcessed)
        {
            key = e.ImeProcessedKey;
        }

        if (IsModifierKey(key))
        {
            e.Handled = true;
            return;
        }

        if (key == Key.Escape)
        {
            _awaitingCapture = false;
            e.Handled = true;
            return;
        }

        var combo = BuildComboFromModifiers(key);
        string text;
        try
        {
            text = HotkeyParser.Parse(combo).DisplayText;
        }
        catch (HotkeyParseException)
        {
            text = combo;
        }

        _awaitingCapture = false;
        e.Handled = true;
        HotkeyTextBox.Text = text;
        HotkeyTextBox.CaretIndex = text.Length;
        CheckAndAdd(text, interactive: true);
        ShowLiveMatches(combo);
    }

    private async void ScanButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isScanning)
        {
            SetStatus(_ => Localization.T("StatusScanningWait"));
            return;
        }

        _isScanning = true;
        Results.Clear();
        _rows.Clear();
        SetStatus(_ => Localization.T("StatusScanningStart"));

        var count = 0;
        foreach (var hotkey in CommonHotkeyGenerator.Generate())
        {
            CheckAndAdd(hotkey);
            count++;

            if (count % 12 == 0)
            {
                var progress = count;
                SetStatus(_ => Localization.F("StatusScanningProgress", progress));
                await Task.Delay(1);
            }
        }

        SetStatus(_ => Localization.F("StatusScanDone", count));
        _isScanning = false;
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (Results.Count == 0)
        {
            SetStatus(_ => Localization.T("StatusNothingToCopy"));
            return;
        }

        System.Windows.Clipboard.SetText(BuildTabSeparatedResults());
        SetStatus(_ => Localization.F("StatusCopied", Results.Count));
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (Results.Count == 0)
        {
            SetStatus(_ => Localization.T("StatusNothingToExport"));
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = Localization.T("ExportDialogTitle"),
            Filter = Localization.T("ExportCsvFilter"),
            FileName = $"hotkey-results-{DateTime.Now:yyyyMMdd-HHmmss}.csv"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        File.WriteAllText(dialog.FileName, BuildCsvResults());
        SetStatus(_ => Localization.F("StatusExported", Results.Count));
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        Results.Clear();
        _rows.Clear();
        _liveCombo = null;
        LiveMatchesPanel.Visibility = Visibility.Collapsed;
        CapturePrompt.Visibility = Visibility.Collapsed;
        SetStatus(_ => Localization.T("StatusReady"));
    }

    private void LanguageButton_Click(object sender, RoutedEventArgs e)
    {
        Localization.Instance.Language = Localization.Instance.Language == HotkeyInspector.Core.Language.Zh
            ? HotkeyInspector.Core.Language.En
            : HotkeyInspector.Core.Language.Zh;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        RebuildResults();
        RebuildReserved();
        RefreshLiveMatches();
        if (_statusBuilder is not null)
        {
            StatusText.Text = _statusBuilder(Localization.Instance.Language);
        }
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (_captureActive || CaptureToggle.IsChecked != true)
        {
            return;
        }

        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (IsModifierKey(key))
        {
            return;
        }

        var text = BuildComboFromModifiers(key);

        try
        {
            HotkeyTextBox.Text = HotkeyParser.Parse(text).DisplayText;
            e.Handled = true;
        }
        catch (HotkeyParseException)
        {
        }
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_captureActive)
        {
            ExitLockCapture();
        }

        if (System.Windows.Application.Current is App { IsExplicitShutdown: true })
        {
            return;
        }

        e.Cancel = true;
        Hide();
        SetStatus(_ => Localization.T("StatusHiddenToTray"));
    }

    // ---- 全屏锁定捕获 ----

    private void LockCaptureButton_Click(object sender, RoutedEventArgs e)
    {
        if (_captureActive)
        {
            return;
        }

        _prevStyle = WindowStyle;
        _prevResize = ResizeMode;
        _prevState = WindowState;
        _prevTopmost = Topmost;

        CaptureToggle.IsChecked = false;
        _lastCaptured = null;
        CaptureComboText.Text = Localization.T("CaptureHint");
        CaptureModsText.Text = string.Empty;
        CaptureMatchesPanel.Visibility = Visibility.Collapsed;

        try
        {
            _hook.Start();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(this, ex.Message, Localization.T("CaptureTitle"),
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        _captureActive = true;
        CaptureOverlay.Visibility = Visibility.Visible;

        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        WindowState = WindowState.Maximized;
        Topmost = true;

        Activate();
        Focus();
    }

    private void ExitCaptureButton_Click(object sender, RoutedEventArgs e) => ExitLockCapture();

    private void ExitLockCapture()
    {
        if (!_captureActive)
        {
            return;
        }

        _captureActive = false;
        _hook.Stop();
        CaptureOverlay.Visibility = Visibility.Collapsed;

        WindowStyle = _prevStyle;
        ResizeMode = _prevResize;
        WindowState = _prevState;
        Topmost = _prevTopmost;

        if (_lastCaptured is { Length: > 0 } combo)
        {
            HotkeyTextBox.Text = combo;
            CheckAndAdd(combo);
            _lastCaptured = null;
        }
    }

    private void OnModifiersChanged(string modifiers)
    {
        if (!_captureActive)
        {
            return;
        }

        CaptureModsText.Text = string.IsNullOrEmpty(modifiers)
            ? Localization.T("CaptureWaitingMods")
            : $"{modifiers}+…";
    }

    private void OnComboCaptured(string combo)
    {
        if (!_captureActive)
        {
            return;
        }

        _lastCaptured = combo;
        CaptureComboText.Text = combo;

        var matches = HotkeyKnowledge.FindMatches(combo)
            .Select(m =>
            {
                var app = m.App.Get(Localization.Instance.Language);
                var action = m.Action.Get(Localization.Instance.Language);
                return $"{app}: {action}";
            })
            .Distinct(StringComparer.Ordinal)
            .Take(8)
            .ToList();

        CaptureMatchesText.Text = matches.Count == 0
            ? Localization.T("CaptureNoMatch")
            : string.Join(Environment.NewLine, matches.Select(m => "• " + m));
        CaptureMatchesPanel.Visibility = Visibility.Visible;
    }

    // ---- 检测结果 ----

    private void CheckAndAdd(string text, bool interactive = false)
    {
        HotkeyRow row;
        try
        {
            var result = _checker.Check(text);
            row = new HotkeyRow { Hotkey = result.Hotkey.DisplayText, Result = result };
        }
        catch (HotkeyParseException ex)
        {
            row = new HotkeyRow
            {
                Hotkey = string.IsNullOrWhiteSpace(text) ? Localization.T("EmptyHotkey") : text,
                RawText = text,
                ParseError = ex.Detail
            };
        }

        _rows.Add(row);
        Results.Add(HotkeyResultViewModel.Create(row, Localization.Instance.Language));

        if (interactive)
        {
            UpdateCapturePrompt(row);
        }

        SetStatus(language =>
        {
            var vm = HotkeyResultViewModel.Create(row, language);
            return Localization.F("StatusChecked", vm.Hotkey, vm.Detail);
        });
    }

    private void UpdateCapturePrompt(HotkeyRow row)
    {
        var unknownOwner = row.Result is { IsAvailable: false, MatchedProcess: null } result
            && result.Known == KnownHotkeyCatalog.UnknownOccupied;

        CapturePrompt.Visibility = unknownOwner ? Visibility.Visible : Visibility.Collapsed;
    }

    // ---- 实时识别器（非锁定） ----

    private void ShowLiveMatches(string combo)
    {
        _liveCombo = string.IsNullOrWhiteSpace(combo) ? null : combo;
        if (_liveCombo is null)
        {
            LiveMatchesPanel.Visibility = Visibility.Collapsed;
            return;
        }

        var lang = Localization.Instance.Language;
        var matches = HotkeyKnowledge.FindMatches(_liveCombo)
            .Select(m => $"{m.Category.Get(lang)} · {m.App.Get(lang)}: {m.Action.Get(lang)}")
            .Distinct(StringComparer.Ordinal)
            .Take(8)
            .ToList();

        LiveMatchesPanel.Visibility = Visibility.Visible;
        LiveMatchesText.Text = matches.Count == 0
            ? Localization.T("LiveNoMatch")
            : string.Join(Environment.NewLine, matches.Select(m => "• " + m));
    }

    private void RefreshLiveMatches()
    {
        if (_liveCombo is not null)
        {
            ShowLiveMatches(_liveCombo);
        }
    }

    // ---- 系统保留快捷键 ----

    private void ReservedToggle_Changed(object sender, RoutedEventArgs e)
    {
        if (ReservedPanel is null)
        {
            return;
        }

        ReservedPanel.Visibility = ReservedToggle.IsChecked == true
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void RebuildReserved()
    {
        var lang = Localization.Instance.Language;
        ReservedCapturableLabel.Text = Localization.T("ReservedCapturable");
        ReservedUncapturableLabel.Text = Localization.T("ReservedUncapturable");
        ReservedCapturableText.Text = string.Join(Environment.NewLine,
            ReservedHotkeys.Where(r => !r.Uncapturable).Select(r => $"{r.Combo}  —  {r.Note.Get(lang)}"));
        ReservedUncapturableText.Text = string.Join(Environment.NewLine,
            ReservedHotkeys.Where(r => r.Uncapturable).Select(r => $"{r.Combo}  —  {r.Note.Get(lang)}"));
    }

    private void RebuildResults()
    {
        Results.Clear();
        foreach (var row in _rows)
        {
            Results.Add(HotkeyResultViewModel.Create(row, Localization.Instance.Language));
        }
    }

    private void SetStatus(Func<Language, string> builder)
    {
        _statusBuilder = builder;
        StatusText.Text = builder(Localization.Instance.Language);
    }

    private string BuildTabSeparatedResults()
    {
        return string.Join(Environment.NewLine, Results.Select(r => string.Join('\t',
            r.Hotkey, r.Status, r.OwnerApplication, r.Detail, r.Knowledge ?? string.Empty, r.ErrorCode)));
    }

    private string BuildCsvResults()
    {
        static string Escape(string value) => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";

        var lines = new List<string> { Localization.T("CsvHeader") };
        lines.AddRange(Results.Select(r => string.Join(',',
            Escape(r.Hotkey), Escape(r.Status), Escape(r.OwnerApplication), Escape(r.Detail), Escape(r.Knowledge ?? string.Empty), r.ErrorCode)));
        return string.Join(Environment.NewLine, lines);
    }

    private static string KeyToText(Key key)
    {
        return key switch
        {
            Key.Escape => "Esc",
            Key.Return => "Enter",
            Key.Space => "Space",
            Key.Delete => "Delete",
            Key.Insert => "Insert",
            Key.PageUp => "PageUp",
            Key.PageDown => "PageDown",
            Key.PrintScreen => "PrintScreen",
            Key.OemPeriod => "Period",
            _ => key.ToString()
        };
    }

    private static bool IsModifierKey(Key key) => key is
        Key.LeftCtrl or Key.RightCtrl or
        Key.LeftAlt or Key.RightAlt or
        Key.LeftShift or Key.RightShift or
        Key.LWin or Key.RWin;

    private static string BuildComboFromModifiers(Key key)
    {
        var parts = new List<string>();
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Windows))
        {
            parts.Add("Win");
        }

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            parts.Add("Ctrl");
        }

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
        {
            parts.Add("Alt");
        }

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            parts.Add("Shift");
        }

        parts.Add(KeyToText(key));
        return string.Join('+', parts);
    }
}
