﻿using Microsoft.Extensions.DependencyInjection;
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
using System.Text.RegularExpressions;
using System.Windows;

namespace SaveSyncApp;

internal class SaveSync : IDisposable
{
    static readonly ISet<string> IgnoreProcessNames = new HashSet<string> { "explorer" };
    static readonly ISet<string> IgnoreWords = new HashSet<string> { "app", "code" };

    readonly ILogger<SaveSync>? _logger;
    readonly INotificationProvider? _notificationProvider;
    readonly ITrackPathProvider _trackPathProvider;
    readonly IProfileProvider _profileProvider;

    bool _disposed = false;
    readonly string _savesDirectory;
    readonly Profile _profile;
    readonly CancellationTokenSource _cts;
    readonly ConcurrentDictionary<string, FileSystemWatcher> _watchers = new();
    readonly ConcurrentDictionary<int, Process> _trackedProcesses = new();

    public Profile Profile => _profile;

    public SaveSync(IServiceProvider services, string workingDirectory)
    {
        _savesDirectory = Path.Combine(workingDirectory, "Saves");
        var cacheDirectory = Path.Combine(_savesDirectory, "SaveSyncCache");
        if (!Directory.Exists(cacheDirectory))
        {
            Directory.CreateDirectory(cacheDirectory);
        }

        _logger = services.GetService<ILoggerFactory>()?.CreateLogger<SaveSync>();
        _logger?.LogDebug("SaveSync正在输出调试日志");
        _notificationProvider = services.GetService<INotificationProvider>();
        _trackPathProvider = services.GetRequiredService<ITrackPathProvider>();
        _profileProvider = services.GetRequiredService<IProfileProvider>();
        _profile = ProfileHelper.LoadProfile(services);

        _cts = new CancellationTokenSource();

        _trackPathProvider.AddPaths(_profile.TrackPaths);
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
            _logger?.LogTrace("ThreadId {ThreadId} 检测到写入文件{FullPath}, {ChangeType}", Environment.CurrentManagedThreadId, e.FullPath, e.ChangeType);
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
                _logger?.LogInformation("正在跟踪进程 {processName}, 将在进程退出时备份存档", processName);
                _notificationProvider?.ShowNotification(100000 + processId, $"正在跟踪进程 {processName}, 将在进程退出时备份存档");
                process.WaitForExit();
                _logger?.LogInformation("进程 {processName} 结束, 正在备份存档中", processName);
                FolderHelper.CopyOrOverwriteFolder(saveFolder, _savesDirectory);
                _profile[processName].RecentChangeDate = DateTime.UtcNow;
                _logger?.LogInformation("进程{processName}的存档已经备份完成", processName);
                _notificationProvider?.ShowNotification(100000 + processId, $"进程{processName}的存档已经备份完成");
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
                _logger?.LogTrace("ThreadId {ThreadId} 前台进程 {ProcessName}[{ProcessId}]", Environment.CurrentManagedThreadId, processName, processId);

                var executablePath = process.MainModule.FileName;
                //var executablePath = ProcessHelper.GetProcessFilePath(process);

                void UpdateProfile(string? friendlyName = null)
                {
                    if (_trackedProcesses.TryAdd(processId, process))
                    {
                        var saveFolder = GetSubfolderName(path, e.FullPath);

                        if (!_profile.Items.ContainsKey(processName)) // 如果对应的进程没有配置信息，则添加相应信息
                        {
                            var iconPath = Path.Combine(_savesDirectory, "SaveSyncCache", $"{processName}.ico");
                            ImageExtractor.SaveProgramIcon(executablePath, iconPath);
                            _profile[processName] = new ProfileItem()
                            {
                                ProcessName = processName,
                                UserFriendlyName = friendlyName ?? Path.GetFileName(saveFolder),
                                SavePath = saveFolder,
                                IconPath = iconPath,
                                RecentChangeDate = DateTime.UtcNow,
                            };
                            _logger?.LogInformation("已将由 {ProcessName} 写入的 {SaveFolder} 加入跟踪列表中", processName, saveFolder);

                        }

                        Task.Run(() => ProcessEndTask(saveFolder), _cts.Token);
                    }
                }

                if (ProcessHelper.IsProcessLaunchedBySteam(process))
                {
                    _logger?.LogDebug("ThreadId {ThreadId} 捕获到Steam启动的前台进程 {ProcessName}[{OrocessId}]", Environment.CurrentManagedThreadId, processName, processId);
                    var steamFolderPath = GetSteamFolderPath(executablePath);
                    if (steamFolderPath != null)
                    {
                        var steamFolderName = Path.GetFileName(steamFolderPath);
                        var words = GetWords(steamFolderName);
                        var fullPathLowerCase = e.FullPath.ToLower();
                        if (words.Where(w => w.Length > 2 && fullPathLowerCase.Contains(w.ToLower())).Any())
                        {
                            UpdateProfile(steamFolderName);
                            return;
                        }
                    }
                    else
                    {
                        _logger?.LogWarning("ThreadId {ThreadId} 进程 {ProcessName}[{ProcessId}] 由Steam启动, 但未能获取Steam库文件夹名称", Environment.CurrentManagedThreadId, processName, processId);
                    }
                }
                if (FullScreenDetect.IsFullScreen(hWnd))
                {
                    _logger?.LogDebug("ThreadId {ThreadId} 捕获到全屏前台进程 {ProcessName}[{ProcessId}]", Environment.CurrentManagedThreadId, processName, processId);
                    var executableWords = GetWords(executablePath).Where(w => w.Length > 2).Select(w => w.ToLower()).ToHashSet();
                    var saveWords = GetWords(e.FullPath).Where(w => w.Length > 2).Select(w => w.ToLower()).ToHashSet();
                    var intersectWords = executableWords.Intersect(saveWords).Except(IgnoreWords);
                    if (intersectWords.Any())
                    {
                        UpdateProfile(intersectWords.First());
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

    static readonly Regex WordRegex = new(@"([A-Z]+(?=[A-Z][a-z]+|$|[^a-zA-Z])|[A-Z][a-z]*|[a-z]+)", RegexOptions.Compiled);
    static IEnumerable<string> GetWords(string name) => WordRegex.Matches(name).Select(m => m.ToString());

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

                if (_trackedProcesses.Any())
                {
                    _notificationProvider?.ShowNotification(1, $"仍有游戏正在运行中, 其存档文件未被同步到目标文件夹");
                }
                foreach (var trackedProcess in _trackedProcesses.Values)
                {
                    _logger?.LogWarning("{processName} 正在运行中, 其存档文件未被同步到目标文件夹", trackedProcess.ProcessName);
                }
                _trackedProcesses.Clear();

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