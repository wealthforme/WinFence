using System;
using System.Windows;
using H.NotifyIcon;
using H.NotifyIcon.Core;
using WinFence.Core.Platform;

namespace WinFence.App;

public partial class App : Application
{
    private TaskbarIcon? _trayIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 是否启动时直接隐藏窗口(走 --tray 参数 = 开机自启的入口)
        bool startMinimizedToTray = Array.Exists(
            e.Args,
            a => string.Equals(a, "--tray", StringComparison.OrdinalIgnoreCase));

        _trayIcon = new TaskbarIcon
        {
            IconSource = (System.Windows.Media.ImageSource)FindResource("TrayIconImage"),
            ToolTipText = "WinFence — 桌面围栏",
            Visibility = Visibility.Visible,
        };

        // 托盘右键菜单 — 代码里构造,避免引入 ContextMenu XAML 资源
        var menu = new System.Windows.Controls.ContextMenu();

        var showItem = new System.Windows.Controls.MenuItem { Header = "显示主窗口" };
        showItem.Click += (_, _) => ShowMainWindow();
        menu.Items.Add(showItem);

        var hideItem = new System.Windows.Controls.MenuItem { Header = "隐藏主窗口" };
        hideItem.Click += (_, _) => HideMainWindow();
        menu.Items.Add(hideItem);

        menu.Items.Add(new System.Windows.Controls.Separator());

        var autoStartItem = new System.Windows.Controls.MenuItem
        {
            Header = "开机自启",
            IsCheckable = true,
            IsChecked = AutoStartService.IsEnabled(),
        };
        autoStartItem.Click += (_, _) =>
        {
            if (autoStartItem.IsChecked) AutoStartService.Enable();
            else AutoStartService.Disable();
        };
        menu.Items.Add(autoStartItem);

        menu.Items.Add(new System.Windows.Controls.Separator());

        var exitItem = new System.Windows.Controls.MenuItem { Header = "退出" };
        exitItem.Click += (_, _) => ExitApp();
        menu.Items.Add(exitItem);

        _trayIcon.ContextMenu = menu;

        // 左键单击托盘 = 显示主窗口
        _trayIcon.TrayLeftMouseUp += (_, _) => ShowMainWindow();

        // 启动主窗口
        var win = new MainWindow();
        MainWindow = win;
        win.Closing += MainWindow_Closing;

        if (startMinimizedToTray) HideMainWindow();
        else win.Show();
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // 关闭按钮 = 隐藏到托盘,不退出
        e.Cancel = true;
        HideMainWindow();
    }

    private void ShowMainWindow()
    {
        if (MainWindow == null) return;
        MainWindow.Show();
        MainWindow.WindowState = WindowState.Normal;
        MainWindow.Activate();
    }

    private void HideMainWindow()
    {
        if (MainWindow == null) return;
        MainWindow.Hide();
    }

    private void ExitApp()
    {
        _trayIcon?.Dispose();
        _trayIcon = null;
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        base.OnExit(e);
    }
}
