using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using HotkeyInspector.Core;
using HotkeyInspector.Infrastructure;

namespace HotkeyInspector.App;

public partial class MainWindow : Window
{
    private readonly HotkeyAvailabilityService _checker = new();
    private bool _isScanning;

    public ObservableCollection<HotkeyResultViewModel> Results { get; } = [];

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void CheckButton_Click(object sender, RoutedEventArgs e)
    {
        CheckAndAdd(HotkeyTextBox.Text);
    }

    private async void ScanButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isScanning)
        {
            StatusText.Text = "正在扫描，请稍候。";
            return;
        }

        _isScanning = true;
        Results.Clear();
        StatusText.Text = "正在扫描常见快捷键...";

        var count = 0;
        foreach (var hotkey in CommonHotkeyGenerator.Generate())
        {
            CheckAndAdd(hotkey);
            count++;

            if (count % 12 == 0)
            {
                StatusText.Text = $"正在扫描... 已检测 {count} 个";
                await Task.Delay(1);
            }
        }

        StatusText.Text = $"扫描完成，共检测 {count} 个快捷键。";
        _isScanning = false;
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (Results.Count == 0)
        {
            StatusText.Text = "没有可复制的结果。";
            return;
        }

        System.Windows.Clipboard.SetText(BuildTabSeparatedResults());
        StatusText.Text = $"已复制 {Results.Count} 行结果。";
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (Results.Count == 0)
        {
            StatusText.Text = "没有可导出的结果。";
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "导出快捷键检测结果",
            Filter = "CSV 文件 (*.csv)|*.csv|文本文件 (*.txt)|*.txt",
            FileName = $"hotkey-results-{DateTime.Now:yyyyMMdd-HHmmss}.csv"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        File.WriteAllText(dialog.FileName, BuildCsvResults());
        StatusText.Text = $"已导出 {Results.Count} 行结果。";
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        Results.Clear();
        StatusText.Text = "就绪";
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (CaptureToggle.IsChecked != true)
        {
            return;
        }

        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin)
        {
            return;
        }

        var parts = new List<string>();
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

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Windows))
        {
            parts.Add("Win");
        }

        parts.Add(KeyToText(key));
        var text = string.Join("+", parts);

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
        if (System.Windows.Application.Current is App { IsExplicitShutdown: true })
        {
            return;
        }

        e.Cancel = true;
        Hide();
        StatusText.Text = "已隐藏到托盘。";
    }

    private void CheckAndAdd(string text)
    {
        try
        {
            var result = _checker.Check(text);
            var candidatesText = result.CandidateProcesses != null && result.CandidateProcesses.Count > 0
                ? string.Join("; ", result.CandidateProcesses
                    .Where(p => result.CandidateProcessNames == null ||
                        !result.CandidateProcessNames.Any(n =>
                            string.Equals(n, p.ProcessName, StringComparison.OrdinalIgnoreCase)))
                    .Select(p => p.ProcessName)
                    .Distinct()
                    .Take(5))
                : null;

            Results.Add(new HotkeyResultViewModel
            {
                Hotkey = result.Hotkey.DisplayText,
                Status = result.Status,
                OwnerApplication = result.OwnerApplication,
                Detail = result.Detail,
                ErrorCode = result.ErrorCode,
                CandidateProcesses = candidatesText
            });
            StatusText.Text = $"{result.Hotkey.DisplayText}: {result.Detail}";
        }
        catch (HotkeyParseException ex)
        {
            Results.Add(new HotkeyResultViewModel
            {
                Hotkey = string.IsNullOrWhiteSpace(text) ? "(空)" : text,
                Status = "无效",
                OwnerApplication = KnownHotkeyCatalog.Invalid.OwnerApplication,
                Detail = ex.Message,
                ErrorCode = 0
            });
            StatusText.Text = ex.Message;
        }
    }

    private string BuildTabSeparatedResults()
    {
        var lines = Results.Select(result => string.Join('\t', result.Hotkey, result.Status, result.OwnerApplication, result.Detail, result.ErrorCode));
        return string.Join(Environment.NewLine, lines);
    }

    private string BuildCsvResults()
    {
        static string Escape(string value) => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";

        var lines = new List<string> { "快捷键,状态,占用应用,用途说明,错误码" };
        lines.AddRange(Results.Select(result => string.Join(',', Escape(result.Hotkey), Escape(result.Status), Escape(result.OwnerApplication), Escape(result.Detail), result.ErrorCode)));
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
}
