using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static SaveSyncApp.Win32.WinAPI;
using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading;

namespace SaveSyncApp;

internal class SaveSync : IDisposable
{
    static readonly ISet<string> IgnoreProcessNames = new HashSet<string> { "explorer" };

    readonly ILogger<SaveSync>? _logger;
    readonly ITrackPathProvider _trackPathProvider;
    readonly IProfileProvider _profileProvider;

    bool _disposed = false;
    readonly Profile _profile;
    readonly CancellationTokenSource _cts;
    readonly ConcurrentDictionary<string, FileSystemWatcher> _watchers = new();
    readonly ConcurrentDictionary<int, Process> _trackedProcesses = new();

    public SaveSync(IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<SaveSync>();
        _logger?.LogDebug("正在调试模式下运行SaveSync");
        _trackPathProvider = serviceProvider.GetRequiredService<ITrackPathProvider>();
        _profileProvider = serviceProvider.GetRequiredService<IProfileProvider>();
        var pfvm = serviceProvider.GetRequiredService<IProfileVersionManagement>();
        _cts = new CancellationTokenSource();
        if (_profileProvider.TryGetProfile(out _profile))
        {
            pfvm.UpdateToCurrentVersion(_profile);
        }
        else
        {
            _profile = pfvm.GetDefaultProfile();
        }

        foreach (var path in _profile.TrackPaths)
        {
            _trackPathProvider.AddPath(path);
        }
        foreach (var path in _trackPathProvider.GetPaths())
        {
            AddWatcher(path);
        }

        _trackPathProvider.OnChanged += (sender, e) =>
        {
            switch (e.Action)
            {
                case ChangeAction.Add:
                    AddWatcher(e.Path);
                    return;
                case ChangeAction.Remove:
                    RemoveWatcher(e.Path);
                    return;
                default:
                    return;
            }
        };
    }

    void AddWatcher(string path)
    {
        if (_disposed) return;

        var fileWatcher = new FileSystemWatcher()
        {
            Path = path,
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
        };
        fileWatcher.Created += OnChanged;
        fileWatcher.Changed += OnChanged;
        fileWatcher.EnableRaisingEvents = true;
        _watchers[path] = fileWatcher;

        _logger?.LogInformation("正在监控文件夹: {Path}", path);

        void OnChanged(object sender, FileSystemEventArgs e) // 假定每个应用只有一个配置文件夹
        {
            foreach (var path in _trackPathProvider.GetIgnorePaths())
            {
                if (IsSubdirectory(path, e.FullPath))
                {
                    return;
                }
            }

            var hWnd = GetForegroundWindow();
            var hResult = GetWindowThreadProcessId(hWnd, out var processId);

            if (_trackedProcesses.ContainsKey(processId)) // 如果该进程已经被跟踪，则跳过该进程
            {
                return;
            }

            var process = Process.GetProcessById(processId);
            var processName = process.ProcessName;

            if (IgnoreProcessNames.Contains(processName))
            {
                return;
            }

            // 等待进程退出，退出后复制文件并更新修改日期
            void ProcessEndTask(string saveFolder)
            {
                process.WaitForExit();
                FolderHelper.CopyOrOverwriteFolder(saveFolder, _profile.SyncPath);
                _profile[processName].RecentChangeDate = DateTime.UtcNow;
                _trackedProcesses.TryRemove(processId, out _);
            }

            if (_profile.Items.ContainsKey(processName)) // 如果配置文件已经记录次进程，则跳过目录识别
            {
                if (_trackedProcesses.TryAdd(processId, process))
                {
                    Task.Run(() => ProcessEndTask(_profile[processName].SavePath), _cts.Token);
                }
                return;
            }

            try
            {
                _logger?.LogDebug("前台进程 {ProcessName}[{ProcessId}], 正在写入文件为 {FullPath}", processName, processId, e.FullPath);

                var executablePath = ProcessHelper.GetProcessFilePath(process);

                void UpdateProfile(string? friendlyName = null)
                {
                    if (_trackedProcesses.TryAdd(processId, process))
                    {
                        var saveFolder = GetSubfolderName(path, e.FullPath);

                        if (!_profile.Items.ContainsKey(processName)) // 如果对应的进程没有配置信息，则添加相应信息
                        {
                            _profile[processName] = new ProfileItem()
                            {
                                ProcessName = process.ProcessName,
                                UserFriendlyName = friendlyName ?? Path.GetFileName(saveFolder),
                                SavePath = saveFolder,
                                RecentChangeDate = DateTime.UtcNow,
                            };
                            _logger?.LogInformation("已将由 {ProcessName} 写入的 {SaveFolder} 加入跟踪列表中", processName, saveFolder);

                        }

                        Task.Run(() => ProcessEndTask(saveFolder), _cts.Token);
                    }
                }

                if (ProcessHelper.IsProcessLaunchedBySteam(process))
                {
                    _logger?.LogDebug("捕获到Steam启动的前台进程 {ProcessName}[{OrocessId}] 尝试写入监控目录", processName, processId);
                    var steamFolderPath = GetSteamFolderPath(executablePath);
                    if (steamFolderPath != null)
                    {
                        var steamFolderName = Path.GetFileName(steamFolderPath);
                        var words = GetWords(steamFolderName);
                        if (words.Where(w => w.Length > 2).Where(w => e.FullPath.ToLower().Contains(w.ToLower())).Any())
                        {
                            UpdateProfile(steamFolderName);
                            return;
                        }
                    }
                    else
                    {
                        _logger?.LogWarning("进程 {ProcessName}[{ProcessId}] 由Steam启动, 但未能获取Steam库文件夹名称", processName, processId);
                    }
                }
                if (FullScreenDetect.IsFullScreen(hWnd))
                {
                    _logger?.LogDebug("捕获到全屏前台进程 {ProcessName}[{ProcessId}] 尝试写入监控目录", processName, processId);
                    var executableWords = GetWords(executablePath).Where(w => w.Length > 2).Select(w => w.ToLower()).ToHashSet();
                    var saveWords = GetWords(e.FullPath).Where(w => w.Length > 2).Select(w => w.ToLower()).ToHashSet();
                    if (executableWords.Intersect(saveWords).Any())
                    {
                        UpdateProfile();
                    }
                }
            }
            catch (Win32Exception ex)
            {
                _logger?.LogDebug(ex, "未能获取进程对应文件所在的位置");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "{Message}", ex.Message);
            }
        }
    }

    void RemoveWatcher(string path)
    {
        if (_disposed) return;

        if (_watchers.ContainsKey(path))
        {
            if (_watchers.TryRemove(path, out var fileWatcher))
            {
                fileWatcher.Dispose();
            }
        }
    }

    static bool IsSubdirectory(string dir, string subDir)
    {
        string fullPath = Path.GetFullPath(dir).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string subDirFullPath = Path.GetFullPath(subDir).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return subDirFullPath.StartsWith(fullPath, StringComparison.OrdinalIgnoreCase);
    }

    static string? GetSteamFolderPath(string path)
    {
        var directoryInfo = new DirectoryInfo(path);

        // 逐级向上遍历父级目录
        while (directoryInfo != null && directoryInfo.Parent?.Name != "common")
        {
            directoryInfo = directoryInfo.Parent;
        }

        // 如果找到common目录，返回common目录所在的父级目录
        if (directoryInfo != null)
        {
            return directoryInfo.FullName;
        }

        // 如果未找到common目录，返回null
        return null;
    }

    static string GetSubfolderName(string rootPath, string path)
    {
        var rootDI = new DirectoryInfo(rootPath);
        var currDI = new DirectoryInfo(path);
        while (currDI.Parent.FullName != rootDI.FullName)
        {
            currDI = currDI.Parent;
        }

        return currDI.FullName;
    }

    static string[] GetWords(string name) => name.Trim().ToLower().Split(' ', '_', '\\', '/');

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _cts.Cancel();
                _cts.Dispose();

                foreach (var watcher in _watchers.Values)
                {
                    watcher.Dispose();
                }
                _watchers.Clear();

                _profile.TrackPaths = _trackPathProvider.GetPaths().ToList();
                _profile.IgnorePaths = _trackPathProvider.GetIgnorePaths().ToList();
                _profileProvider.TrySaveProfile(_profile);
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            _disposed = true;
        }
    }

    // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
    ~SaveSync()
    {
        Dispose(disposing: false);
    }

    // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}