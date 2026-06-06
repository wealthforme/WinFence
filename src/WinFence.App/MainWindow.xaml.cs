using System.Windows;
using System.Windows.Input;
using WinFence.Core;

namespace WinFence.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        MouseRightButtonDown += MainWindow_MouseRightButtonDown;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 验证 Core 类库引用正常 — 调一个简单的纯逻辑方法
        StatusText.Text = $"WinFence.Core says: {Greeter.Greet()}";
    }

    private void RootBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            // WPF 内置拖动窗口 API,等价于发 WM_NCLBUTTONDOWN + HTCAPTION
            try { DragMove(); }
            catch (InvalidOperationException) { /* 拖动结束时偶发,忽略 */ }
        }
    }

    private void MainWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        // M0 简易退出:右键窗口关闭
        Close();
    }
}
