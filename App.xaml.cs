using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

    static readonly ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
    {
#if DEBUG
        builder.AddDebug();
#else
        builder
            .AddFilter("Microsoft", LogLevel.Warning)
            .AddFilter("System", LogLevel.Warning)
            .AddFilter("SaveSyncHelper", LogLevel.Information)
            .AddConsole();
#endif
    });

    public static readonly string AppDataFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SaveSync");
    public static readonly string DefaultProfileFile = Path.Combine(AppDataFolder, "profile.json");
    public static readonly string DefaultSyncFolder = Path.Combine(AppDataFolder, "Saves");

    readonly static string _roamingFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    readonly static string _documentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    public static readonly IEnumerable<string> DefaultTrackPaths = new[] { _roamingFolder, _documentFolder };


    static ServiceProvider serviceProvider = null;
    public static ServiceProvider ServiceProvider => serviceProvider ??= RegisterServices();

    static ServiceProvider RegisterServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITrackPathProvider>(new TrackPathProvider());
        services.AddSingleton<IProfileProvider>(new FileProfileProvider(DefaultProfileFile));
        services.AddSingleton<ILoggerFactory>(loggerFactory);
        return services.BuildServiceProvider();
    }


    readonly Mutex _mutex;
    readonly Dictionary<Type, int> _exceptionCount = new ();
    internal SaveSync SaveSync { get; set; }

    internal App()
    {
        _mutex = new Mutex(true, $"_SaveSync_{Environment.CurrentDirectory.Replace('\\', '_')}", out bool createNew);
        if (!createNew)
        {
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

            if (count >= 3)
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

        SaveSync?.Dispose();
    }
}
