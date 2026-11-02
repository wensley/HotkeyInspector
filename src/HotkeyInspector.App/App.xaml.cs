using System.IO;
using System.Windows;
using Forms = System.Windows.Forms;

namespace HotkeyInspector.App;

public partial class App : System.Windows.Application
{
    private Forms.NotifyIcon? _trayIcon;
    private Forms.ToolStripMenuItem? _openItem;
    private Forms.ToolStripMenuItem? _exitItem;

    public bool IsExplicitShutdown { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        CheckDotnetRuntime();
        SetupTrayIcon();
        PrewarmKnowledge();
        Localization.Instance.LanguageChanged += (_, _) => UpdateTrayText();
    }

    /// <summary>
    /// 预先加载知识库：避免首次按键（可能发生在全屏捕获的低级键盘钩子回调里）时才解析
    /// 3000+ 条 JSON，阻塞钩子线程而被系统摘钩。
    /// </summary>
    private static void PrewarmKnowledge() => _ = HotkeyInspector.Core.HotkeyKnowledge.Base;

    /// <summary>
    /// 框架依赖版本需要 .NET 10 桌面运行时。若本机未检测到（或版本过低），提示用户前往官网下载。
    /// 说明：运行时完全缺失时进程无法启动，此检查主要用于「运行时存在但版本不符」等边界情况。
    /// </summary>
    private void CheckDotnetRuntime()
    {
        if (IsDesktopRuntimeInstalled())
        {
            return;
        }

        var result = System.Windows.MessageBox.Show(
            Localization.T("RuntimeMissingBody"),
            Localization.T("RuntimeMissingTitle"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            Links.Open(Links.DotnetDownload);
        }
    }

    private static bool IsDesktopRuntimeInstalled()
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var root = Path.Combine(programFiles, "dotnet", "shared", "Microsoft.WindowsDesktop.App");
        if (!Directory.Exists(root))
        {
            return false;
        }

        foreach (var dir in Directory.GetDirectories(root))
        {
            var name = Path.GetFileName(dir);
            var dash = name.IndexOf('-');
            var versionPart = dash >= 0 ? name[..dash] : name;
            if (Version.TryParse(versionPart, out var version) && version.Major >= 10)
            {
                return true;
            }
        }

        return false;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        base.OnExit(e);
    }

    private void SetupTrayIcon()
    {
        var menu = new Forms.ContextMenuStrip();
        _openItem = new Forms.ToolStripMenuItem(Localization.T("TrayOpen"), null, (_, _) => ShowMainWindow());
        _exitItem = new Forms.ToolStripMenuItem(Localization.T("TrayExit"), null, (_, _) => ShutdownFromTray());
        menu.Items.Add(_openItem);
        menu.Items.Add(_exitItem);

        _trayIcon = new Forms.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Visible = true,
            ContextMenuStrip = menu
        };
        _trayIcon.DoubleClick += (_, _) => ShowMainWindow();
        UpdateTrayText();
    }

    private void UpdateTrayText()
    {
        if (_trayIcon is null || _openItem is null || _exitItem is null)
        {
            return;
        }

        _trayIcon.Text = Localization.T("TrayTooltip");
        _openItem.Text = Localization.T("TrayOpen");
        _exitItem.Text = Localization.T("TrayExit");
    }

    private void ShowMainWindow()
    {
        if (MainWindow is null)
        {
            MainWindow = new MainWindow();
        }

        MainWindow.Show();
        MainWindow.Activate();
    }

    private void ShutdownFromTray()
    {
        IsExplicitShutdown = true;
        Shutdown();
    }
}
