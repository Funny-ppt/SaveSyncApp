using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SaveSyncApp.Properties;
using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace SaveSyncApp
{
    internal class AppDataContext : INotifyPropertyChanged, IDisposable
    {
        bool _disposed = false;
        TaskbarIcon _notifyIcon;
        SaveSync? _saveSync;
        LogLevel _logLevel;


        public ILoggerFactory LoggerFactory => Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            builder.AddFilter("Microsoft", LogLevel.Warning)
                   .AddFilter("System", LogLevel.Warning)
                   .AddFilter("SaveSync", LogLevel)
                   .AddCustom());

        ServiceCollection? _services = null;
        ServiceProvider? _serviceProvider = null;
        public ServiceProvider ServiceProvider => _serviceProvider ??= RegisterServices();

        ServiceProvider RegisterServices()
        {
            _services = new ServiceCollection();
            _services.AddTransient<ITrackPathProvider>((services) => new TrackPathProvider());
            var profilePath = Settings.Default.ProfilePath.Trim();
            if (string.IsNullOrEmpty(profilePath))
            {
                profilePath = Settings.DefaultProfileFile;
            }
            _services.AddSingleton<IProfileProvider>(new FileProfileProvider(profilePath));
            _services.AddSingleton<IProfileVersionManagement>(new ProfileVersionManagement());
            _services.AddSingleton<ILoggerFactory>(LoggerFactory);
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
                _saveSync?.Dispose();
                _saveSync = value;
                PropertyChanged?.Invoke(this, new(nameof(SaveSync)));
            }
        }

        public AppDataContext()
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
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
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
}
