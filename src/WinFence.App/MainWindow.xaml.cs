using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using WinFence.Core;

namespace WinFence.App;

public partial class MainWindow : Window
{
    // Win32 常量
    private const int WS_EX_TRANSPARENT = 0x00000020; // 鼠标穿透:hit-test 返回 HTTRANSPARENT
    private const int WS_EX_LAYERED = 0x00080000;     // 分层窗口(配合 AllowsTransparency 已隐式启用)

    // Hit-test 返回值(WM_NCHITTEST)
    private const int HTCLIENT = 1;       // 客户区 — 接收鼠标事件
    private const int HTLEFT = 10;
    private const int HTRIGHT = 11;
    private const int HTTOP = 12;
    private const int HTTOPLEFT = 13;
    private const int HTTOPRIGHT = 14;
    private const int HTBOTTOM = 15;
    private const int HTBOTTOMLEFT = 16;
    private const int HTBOTTOMRIGHT = 17;
    private const int HTCAPTION = 2;      // 标题栏区 — 系统会启动拖动

    private const int WM_NCHITTEST = 0x0084;
    private const int WM_NCLBUTTONDOWN = 0x00A1;

    // 边缘热区宽度(屏幕像素,DPI 无关 — 后面用 DwmGetWindowAttribute 修正)
    private const int ResizeBorderPx = 8;
    // 拖动热区"标题栏"高度(顶部 N px 可以点住拖动整窗口)
    private const int DragBarPx = 28;

    private HwndSource? _hwndSource;
    private bool _altMode;  // Alt 键按住 → 整窗口不再穿透,露出边缘 resize + 顶部拖动条

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        MouseRightButtonDown += MainWindow_MouseRightButtonDown;
        PreviewKeyDown += MainWindow_PreviewKeyDown;
        PreviewKeyUp += MainWindow_PreviewKeyUp;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        StatusText.Text = $"WinFence.Core says: {Greeter.Greet()}";
        HintText.Text = "默认:鼠标穿透整个窗口 · 按住 Alt 露出边缘(可 resize)+ 顶部条(可拖动) · 右键关闭";
        UpdateModeDisplay();

        // 挂 HwndSource,接管 WM_NCHITTEST
        _hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
        _hwndSource.AddHook(WndProc);

        // 初始:整窗口鼠标穿透
        SetClickThrough(true);
    }

    private void MainWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        Close();
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
        {
            if (!_altMode) { _altMode = true; SetClickThrough(false); UpdateModeDisplay(); }
        }
    }

    private void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
        {
            if (_altMode) { _altMode = false; SetClickThrough(true); UpdateModeDisplay(); }
        }
    }

    private void UpdateModeDisplay()
    {
        ModeBadge.Text = _altMode ? "Alt 按下 · 可交互" : "鼠标穿透中";
        ModeBadge.Foreground = _altMode
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0xCC, 0x66))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0xCC, 0x66));
    }

    /// <summary>
    /// 切换窗口的"鼠标穿透"特性。
    /// true  = 鼠标点击穿透到下面的应用(窗口不接收输入)
    /// false = 窗口正常接收鼠标事件
    /// </summary>
    private void SetClickThrough(bool clickThrough)
    {
        if (_hwndSource == null) return;
        var hwnd = _hwndSource.Handle;
        var exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        if (clickThrough)
            exStyle |= WS_EX_TRANSPARENT;
        else
            exStyle &= ~WS_EX_TRANSPARENT;
        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_NCHITTEST && _altMode)
        {
            // lParam: loword = x, hiword = y (screen coords)
            int screenX = (int)(short)(lParam.ToInt64() & 0xFFFF);
            int screenY = (int)(short)((lParam.ToInt64() >> 16) & 0xFFFF);

            // 转窗口客户区坐标
            var rect = new RECT();
            GetWindowRect(hwnd, out rect);
            int clientX = screenX - rect.Left;
            int clientY = screenY - rect.Top;
            int w = (int)ActualWidth;
            int h = (int)ActualHeight;

            // 8 向 resize 热区
            bool onLeft = clientX < ResizeBorderPx;
            bool onRight = clientX >= w - ResizeBorderPx;
            bool onTop = clientY < ResizeBorderPx;
            bool onBottom = clientY >= h - ResizeBorderPx;

            if (onTop && onLeft) { handled = true; return (IntPtr)HTTOPLEFT; }
            if (onTop && onRight) { handled = true; return (IntPtr)HTTOPRIGHT; }
            if (onBottom && onLeft) { handled = true; return (IntPtr)HTBOTTOMLEFT; }
            if (onBottom && onRight) { handled = true; return (IntPtr)HTBOTTOMRIGHT; }
            if (onLeft) { handled = true; return (IntPtr)HTLEFT; }
            if (onRight) { handled = true; return (IntPtr)HTRIGHT; }
            if (onTop) { handled = true; return (IntPtr)HTTOP; }
            if (onBottom) { handled = true; return (IntPtr)HTBOTTOM; }

            // 顶部 N px 当作标题栏 — 系统自动启动拖动
            if (clientY < DragBarPx)
            {
                handled = true;
                return (IntPtr)HTCAPTION;
            }
        }
        return IntPtr.Zero;
    }

    // ===== Win32 互操作 =====

    private const int GWL_EXSTYLE = -20;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
}
