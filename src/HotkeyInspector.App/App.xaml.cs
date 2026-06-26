using System.Windows;
using Forms = System.Windows.Forms;

namespace HotkeyInspector.App;

public partial class App : System.Windows.Application
{
    private Forms.NotifyIcon? _trayIcon;

    public bool IsExplicitShutdown { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        SetupTrayIcon();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        base.OnExit(e);
    }

    private void SetupTrayIcon()
    {
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("打开", null, (_, _) => ShowMainWindow());
        menu.Items.Add("退出", null, (_, _) => ShutdownFromTray());

        _trayIcon = new Forms.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Text = "快捷键检测",
            Visible = true,
            ContextMenuStrip = menu
        };
        _trayIcon.DoubleClick += (_, _) => ShowMainWindow();
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
