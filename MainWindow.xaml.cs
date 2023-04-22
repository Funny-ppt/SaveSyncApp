using System;
using System.Windows;
using System.Windows.Controls;

namespace SaveSyncApp;

/// <summary>
/// MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow : Window
{
    public bool CancelCloseWindow { get; set; }
#if DEBUG
    = false;
#else
    = true;
#endif

    ContextMenu _runningMenu;
    ContextMenu _idleMenu;
    StartupControl? _startupControl;
    ConfigurationControl? _configurationControl;
    SavesControl? _savesControl;

    internal StartupControl StartupControl => _startupControl ??= new();
    internal ConfigurationControl ConfigurationControl => _configurationControl ??= new();
    internal SavesControl SavesControl => _savesControl ??= new();

    internal Control? LastPage = null;
    internal Control? ContentPage
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
    internal event EventHandler? PageChanged;

    void ShowWindow(object? _)
    {
        Show();
        WindowState = WindowState.Normal;
    }
    void Exit(object? _)
    {
        CancelCloseWindow = false;
        Close();
    }

    public MainWindow()
    {
        InitializeComponent();

        var showWindowCommand = new DelegateCommand(ShowWindow);
        var exitCommand = new DelegateCommand(Exit);

        // 创建运行时托盘菜单
        _runningMenu = new ContextMenu();
        _runningMenu.Items.Add(new MenuItem { Header = "SaveSync: 运行中", Command = showWindowCommand });
        _runningMenu.Items.Add(new Separator());
        _runningMenu.Items.Add(new MenuItem { Header = "停止", Command = new DelegateCommand((_) => App.Context.SaveSync = null) });
        _runningMenu.Items.Add(new MenuItem { Header = "退出", Command = exitCommand });

        // 创建未运行时托盘菜单
        _idleMenu = new ContextMenu();
        _idleMenu.Items.Add(new MenuItem { Header = "SaveSync: 未运行", Command = showWindowCommand });
        _idleMenu.Items.Add(new Separator());
        _idleMenu.Items.Add(new MenuItem { Header = "运行", Command = new DelegateCommand((_) => App.Context.StartNewSaveSync()) });
        _idleMenu.Items.Add(new MenuItem { Header = "退出", Command = exitCommand });

        App.Context.NotifyIcon.LeftClickCommand = showWindowCommand;

        App.Context.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "SaveSync")
            {
                App.Context.NotifyIcon.ContextMenu = App.Context.SaveSync == null ? _idleMenu : _runningMenu;
            }
        };

        App.Context.NotifyIcon.ContextMenu = App.Context.SaveSync == null ? _idleMenu : _runningMenu;
        //App.Context.SaveSync = new SaveSync(App.Context.ServiceProvider);
        ContentPage = StartupControl;
    }

    private void GotoStartupPage(object sender, RoutedEventArgs e)
    {
        ContentPage = StartupControl;
    }

    private void GotoConfigPage(object sender, RoutedEventArgs e)
    {
        ContentPage = ConfigurationControl;
    }

    private void GotoSavePage(object sender, RoutedEventArgs e)
    {
        ContentPage = SavesControl;
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
