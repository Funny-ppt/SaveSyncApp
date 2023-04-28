using Microsoft.Extensions.Logging;
using SaveSyncApp.Properties;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace SaveSyncApp;

/// <summary>
/// App.xaml 的交互逻辑
/// </summary>
public partial class App : Application
{
    public static new App Current => (App)Application.Current;
    public new MainWindow MainWindow => (MainWindow)base.MainWindow;
    internal static AppContext Context => Current.AppContext;

#if DEBUG
    const int ReportCount = 1;
#else
    const int ReportCount = 3;
#endif
    internal AppContext AppContext { get; } = new();
    readonly Mutex _mutex;
    readonly Dictionary<Type, int> _exceptionCount = new();

    internal App()
    {
        _mutex = new Mutex(true, $"_SaveSync_{Environment.CurrentDirectory.Replace('\\', '_')}", out bool createNew);
        if (!createNew)
        {
            MessageBox.Show("已经有一个SaveSync在运行!", "SaveSync");
            Shutdown();
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += ExceptionHandler;
        void ExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            var exceptionType = e.Exception.GetType();
            _exceptionCount.TryGetValue(exceptionType, out var count);
            _exceptionCount[exceptionType] = ++count;

            if (count >= ReportCount)
            {
                string errorMessage = 
$@"发生异常: {e.Exception.Message}

{e.Exception.StackTrace}

该异常已经第{count}次触发，请关闭应用并告知开发者
单击'是'以关闭应用程序，单击'否'继续使用";

                var result = MessageBox.Show(errorMessage, "SaveSync错误", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (result == MessageBoxResult.Yes)
                {
                    Shutdown();
                }
            }
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        Context.Dispose();
        Settings.Default.Save();
    }


    public delegate void LogMessageHandler(string message, string? name);
    public event LogMessageHandler? LogMessageImpl;
    public void LogMessage(string message, string? name = null)
    {
        LogMessageImpl?.Invoke(message, name);
    }
    public void ShowNotification(int id, string message, bool important = false)
    {
        LogMessage($"(通知) [{id}] {message}");
        NotificationHelper.ShowNotification(id, message, important);
    }
}
