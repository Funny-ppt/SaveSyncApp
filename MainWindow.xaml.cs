using System;
using System.Windows;
using System.Windows.Controls;
using ContextMenuStrip = System.Windows.Forms.ContextMenuStrip;
using ToolStripMenuItem = System.Windows.Forms.ToolStripMenuItem;

namespace SaveSyncApp;

/// <summary>
/// MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow : Window
{
    public bool CancelCloseWindow { get; set; } = true;

    ContextMenuStrip _runningMenu;
    ContextMenuStrip _idleMenu;
    StartupControl _startupControl;

    internal StartupControl StartupControl => _startupControl ??= new();

    internal Control LastPage = null;
    internal Control ContentPage
    {
        get
        {
            return PageContainer.Content as Control;
        }
        set
        {
            if (value != ContentPage)
            {
                LastPage = ContentPage;
                PageContainer.Content = value;
                PageChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    internal event EventHandler PageChanged;

    public MainWindow()
    {
        InitializeComponent();

        void ShowWindow(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }
        void Exit(object sender, EventArgs e)
        {
            CancelCloseWindow = false;
            Close();
        }

        // 创建运行时托盘菜单
        var showWindowMeunItem = new ToolStripMenuItem("SaveSync: 运行中");
        showWindowMeunItem.Click += ShowWindow;
        var stopMenuItem = new ToolStripMenuItem("停止");
        stopMenuItem.Click += (sender, e) => {
            App.Context.SaveSync = null;
        };
        var exitMenuItem = new ToolStripMenuItem("退出");
        exitMenuItem.Click += Exit;

        _runningMenu = new() { Items = { showWindowMeunItem, "-", stopMenuItem, exitMenuItem } };

        // 创建未运行时托盘菜单
        var showWindowMeunItem2 = new ToolStripMenuItem("SaveSync: 未运行");
        showWindowMeunItem2.Click += ShowWindow;
        var startMenuItem = new ToolStripMenuItem("运行");
        startMenuItem.Click += (sender, e) => {
            App.Context.SaveSync = new(App.Context.ServiceProvider);
        };
        var exitMenuItem2 = new ToolStripMenuItem("退出");
        exitMenuItem2.Click += Exit;

        _idleMenu = new() { Items = { showWindowMeunItem2, "-", startMenuItem, exitMenuItem2 } };

        App.Context.NotifyIcon.MouseClick += (sender, e) =>
        {
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    Show();
                    WindowState = WindowState.Normal;
                    return;
                default:
                    return;
            }
        };

        App.Context.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "SaveSync")
            {
                App.Context.NotifyIcon.ContextMenuStrip = App.Context.SaveSync == null ? _idleMenu : _runningMenu;
            }
        };

        //App.Context.SaveSync = new SaveSync(App.Context.ServiceProvider);
        GotoStartupPage(null, null);
    }

    private void GotoStartupPage(object sender, RoutedEventArgs e)
    {
        ContentPage = StartupControl;
    }

    private void GotoConfigPage(object sender, RoutedEventArgs e)
    {

    }

    private void GotoNetworkPage(object sender, RoutedEventArgs e)
    {

    }

    private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (CancelCloseWindow)
        {
            e.Cancel = true;
            Hide();
            return;
        }
    }
}
