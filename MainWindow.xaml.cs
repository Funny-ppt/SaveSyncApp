using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
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
    NotifyIcon _notifyIcon;

    internal SaveSync SaveSync
    {
        get => App.Current.SaveSync;
        set
        {
            _notifyIcon.ContextMenuStrip = value == null ? _idleMenu : _runningMenu;
            App.Current.SaveSync = value;
        }
    }
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
            SaveSync.Dispose();
            SaveSync = null;
        };
        var exitMenuItem = new ToolStripMenuItem("退出");
        exitMenuItem.Click += Exit;

        _runningMenu = new() { Items = { showWindowMeunItem, "-", stopMenuItem, exitMenuItem } };

        // 创建未运行时托盘菜单
        var showWindowMeunItem2 = new ToolStripMenuItem("SaveSync: 未运行");
        showWindowMeunItem2.Click += ShowWindow;
        var startMenuItem = new ToolStripMenuItem("运行");
        startMenuItem.Click += (sender, e) => {
            SaveSync = new(App.ServiceProvider);
        };
        var exitMenuItem2 = new ToolStripMenuItem("退出");
        exitMenuItem2.Click += Exit;

        _idleMenu = new() { Items = { showWindowMeunItem2, "-", startMenuItem, exitMenuItem2 } };

        _notifyIcon = new()
        {
            Icon = new System.Drawing.Icon(
               Application.GetResourceStream(
                 new Uri("app.ico", UriKind.Relative)
               ).Stream
            ),
            ContextMenuStrip = _idleMenu,
            Visible = true
        };
        _notifyIcon.MouseClick += (sender, e) =>
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

        SaveSync = new SaveSync(App.ServiceProvider);

        GotoStartupPage(null, null);
    }

    private void GotoStartupPage(object sender, RoutedEventArgs e)
    {

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
        }
        else
        {
            _notifyIcon?.Dispose();
        }
    }
}
