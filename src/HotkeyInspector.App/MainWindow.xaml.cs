using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using HotkeyInspector.Core;
using HotkeyInspector.Infrastructure;

namespace HotkeyInspector.App;

public partial class MainWindow : Window
{
    private readonly HotkeyAvailabilityService _checker = new(new Win32HotkeyProbe(), new RunningProcessSource());
    private readonly HotkeyCaptureSession _capture = new();

    private bool _isScanning;
    private bool _awaitingCapture;
    private string? _lastCaptured;
    private string? _liveCombo;
    private Func<Language, string>? _statusBuilder;

    private WindowStyle _prevStyle;
    private ResizeMode _prevResize;
    private WindowState _prevState;
    private bool _prevTopmost;

    public HotkeyResultsModel Results { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        ClampToWorkArea();

        _capture.ComboCaptured += OnComboCaptured;
        _capture.ModifiersChanged += OnModifiersChanged;
        _capture.CancelRequested += ExitLockCapture;
        Localization.Instance.LanguageChanged += OnLanguageChanged;
        RebuildReserved();
    }

    /// <summary>
    /// 小屏 / 高缩放下初始尺寸可能超出工作区（例如 1366×768 @150% 逻辑宽度仅约 911px），
    /// 这里把窗口收进工作区，避免右侧内容被裁掉。
    /// </summary>
    private void ClampToWorkArea()
    {
        var area = SystemParameters.WorkArea;
        Width = Math.Min(Width, Math.Max(MinWidth, area.Width - 24));
        Height = Math.Min(Height, Math.Max(MinHeight, area.Height - 24));
    }

    protected override void OnClosed(EventArgs e)
    {
        Localization.Instance.LanguageChanged -= OnLanguageChanged;
        _capture.Dispose();
        base.OnClosed(e);
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

        if (WpfKeyMapper.IsModifier(key))
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

        var combo = WpfKeyMapper.BuildCombo(key);
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
        try
        {
            Results.Clear();
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
        }
        catch (Exception ex)
        {
            SetStatus(_ => Localization.F("StatusScanFailed", ex.Message));
        }
        finally
        {
            _isScanning = false;
        }
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (Results.Count == 0)
        {
            SetStatus(_ => Localization.T("StatusNothingToCopy"));
            return;
        }

        try
        {
            System.Windows.Clipboard.SetText(Results.ToTabSeparated());
            SetStatus(_ => Localization.F("StatusCopied", Results.Count));
        }
        catch (Exception ex)
        {
            SetStatus(_ => Localization.F("StatusCopyFailed", ex.Message));
        }
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

        try
        {
            File.WriteAllText(dialog.FileName, Results.ToCsv(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            SetStatus(_ => Localization.F("StatusExported", Results.Count));
        }
        catch (Exception ex)
        {
            SetStatus(_ => Localization.F("StatusExportFailed", ex.Message));
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        Results.Clear();
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
        Results.Rebuild(Localization.Instance.Language);
        RebuildReserved();
        RefreshLiveMatches();
        if (_statusBuilder is not null)
        {
            StatusText.Text = _statusBuilder(Localization.Instance.Language);
        }
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (_capture.IsActive || CaptureToggle.IsChecked != true)
        {
            return;
        }

        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (WpfKeyMapper.IsModifier(key))
        {
            return;
        }

        var text = WpfKeyMapper.BuildCombo(key);

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
        if (_capture.IsActive)
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
        if (_capture.IsActive)
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

        if (!_capture.TryStart(out var error))
        {
            System.Windows.MessageBox.Show(this, error, Localization.T("CaptureTitle"),
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

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
        if (!_capture.IsActive)
        {
            return;
        }

        _capture.Stop();
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
        if (!_capture.IsActive)
        {
            return;
        }

        CaptureModsText.Text = string.IsNullOrEmpty(modifiers)
            ? Localization.T("CaptureWaitingMods")
            : $"{modifiers}+…";
    }

    private void OnComboCaptured(string combo)
    {
        if (!_capture.IsActive)
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

        Results.Add(row, Localization.Instance.Language);

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
            SystemReservedHotkeys.All.Where(r => !r.Uncapturable).Select(r => $"{r.Combo}  —  {r.Note.Get(lang)}"));
        ReservedUncapturableText.Text = string.Join(Environment.NewLine,
            SystemReservedHotkeys.All.Where(r => r.Uncapturable).Select(r => $"{r.Combo}  —  {r.Note.Get(lang)}"));
    }

    private void SetStatus(Func<Language, string> builder)
    {
        _statusBuilder = builder;
        StatusText.Text = builder(Localization.Instance.Language);
    }
}
