using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SaveSyncApp.Properties;
using System;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace SaveSyncApp;

internal class AppContext : INotifyPropertyChanged, IDisposable
{
    bool _disposed = false;
    TaskbarIcon _notifyIcon;
    SaveSync? _saveSync;
    LogLevel _logLevel;
    IProfileProvider? _cachedProfileProvider;
    Profile? _cachedProfile;
    IProfileProvider CachedProfileProvider => _cachedProfileProvider ??= DefaultFileProvider();
    Profile? CachedProfile
    {
        get
        {
            if (_cachedProfile == null)
            {
                if (!File.Exists(Path.Combine(Settings.Default.WorkingDirectory, "profile.json")))
                {
                    var tryCreateDefault = CachedProfileProvider.TrySaveProfile(new ProfileVersionManagement().GetDefaultProfile());
                    App.Current.LogMessage($"尝试在 {ProfilePath} 下创建默认配置{(tryCreateDefault ? "成功" : "失败")}");
                    if (!tryCreateDefault) throw new InvalidOperationException($"创建默认配置文件 {ProfilePath} 失败");
                }
                if (!CachedProfileProvider.TryGetProfile(out _cachedProfile))
                {
                    throw new InvalidOperationException($"打开配置文件 {ProfilePath} 失败");
                }
                PropertyChanged?.Invoke(this, new(nameof(Profile)));
            }
            return _cachedProfile;
        }
    }


    public ILoggerFactory LoggerFactory => Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        builder.AddFilter("Microsoft", LogLevel.Warning)
               .AddFilter("System", LogLevel.Warning)
               .AddFilter("SaveSync", LogLevel)
               .AddCustom());

    ServiceCollection? _services = null;
    ServiceProvider? _serviceProvider = null;
    public ServiceProvider ServiceProvider => _serviceProvider ??= RegisterServices();

    string ProfilePath => Path.Combine(Settings.Default.WorkingDirectory, "profile.json");
    FileProfileProvider DefaultFileProvider() => new(ProfilePath);
    ServiceProvider RegisterServices()
    {
        _services = new ServiceCollection();
        _services.AddTransient<ITrackPathProvider>((services) => new TrackPathProvider());
        _services.AddSingleton<IProfileProvider>(DefaultFileProvider());
        _services.AddSingleton<IProfileVersionManagement>(new ProfileVersionManagement());
        _services.AddSingleton<ILoggerFactory>(LoggerFactory);
        _services.AddSingleton<IUserRequestProvider>(new UserRequestProvider());
        return _services.BuildServiceProvider();
    }

    public TaskbarIcon NotifyIcon => _notifyIcon;
    public LogLevel LogLevel
    {
        get => _logLevel;
        set
        {
            if (_logLevel != value)
            {
                _logLevel = value;
                if (_services != null)
                {
                    _services.Replace(new(typeof(ILoggerFactory), LoggerFactory));
                    _serviceProvider.Dispose();
                    _serviceProvider = _services.BuildServiceProvider();
                }
                PropertyChanged?.Invoke(this, new(nameof(LogLevel)));
            }
        }
    }
    public SaveSync? SaveSync
    {
        get => _saveSync;
        set
        {
            if (value == _saveSync) return;
            RefreshProfileCache();
            _saveSync?.Dispose();
            _saveSync = value;
            PropertyChanged?.Invoke(this, new(nameof(SaveSync)));
            PropertyChanged?.Invoke(this, new(nameof(Profile)));
        }
    }

    public Profile Profile => _saveSync?.Profile ?? CachedProfile;

    public AppContext()
    {
#if DEBUG
        _logLevel = LogLevel.Debug;
#else
        _logLevel = LogLevel.Information;
#endif

        _notifyIcon = new()
        {
            IconSource = new BitmapImage(new Uri("pack://application:,,,/app.ico", UriKind.Absolute)),
            Visibility = System.Windows.Visibility.Visible,
        };

        Settings.Default.PropertyChanged += (sender, e) =>
        {
            switch (e.PropertyName)
            {
                case "WorkingDirectory":
                    if (_services != null)
                    {
                        _services.Replace(new(typeof(IProfileProvider), DefaultFileProvider()));
                        _serviceProvider.Dispose();
                        _serviceProvider = _services.BuildServiceProvider();
                    }
                    RefreshProfileCache();
                    break;
            }
        };
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    public void RefreshProfileCache(bool notify = true)
    {
        if (_cachedProfile != null)
        {
            _cachedProfileProvider.TrySaveProfile(_cachedProfile);
            _cachedProfile = null;
            _cachedProfileProvider = null;
        }
        if (notify)
        {
            PropertyChanged?.Invoke(this, new(nameof(Profile)));
        }
    }

    static readonly Regex JapaneseRegex = new(@"[\u0800-\u4e00]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public void StartNewSaveSync()
    {
        SaveSync = new SaveSync(ServiceProvider, Settings.Default.WorkingDirectory);

        if (Settings.Default.SaveFileMatchEnabled)
        {
            void Match(object? sender, FileChangeMatchEventArgs e)
            {
                if (Regex.IsMatch(e.ChangedFile, Settings.Default.SaveFilePattern, RegexOptions.IgnoreCase))
                {
                    e.MatchType = MatchType.FuzzyMatch;
                }
            }
            SaveSync.MatchRules += Match;
        }
        if (Settings.Default.JapaneseMatchRuleEnabled)
        {
            void Match(object? sender, FileChangeMatchEventArgs e)
            {
                var fileContainsJP = JapaneseRegex.IsMatch(e.ChangedFile);
                var processFileContainsJP = JapaneseRegex.IsMatch(e.ForegroundProcessFile);
                var windowTitleContainsJP = JapaneseRegex.IsMatch(e.ForegroundWindowTitle);
                if (fileContainsJP && (processFileContainsJP || windowTitleContainsJP))
                {
                    e.MatchType = MatchType.FuzzyMatch;
                }
            }
            SaveSync.MatchRules += Match;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                RefreshProfileCache(false);
                NotifyIcon.Dispose();
                SaveSync?.Dispose();
            }

            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null
            _disposed = true;
        }
    }

    // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
    // ~AppDataContext()
    // {
    //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
