using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SaveSyncApp.IO;

public class NativeDirectoryAccess : IDirectoryAccess
{
    private DirectoryInfo _directoryInfo;

    public NativeDirectoryAccess(string path)
    {
        if (!Directory.Exists(path)) throw new ArgumentException(null, nameof(path));
        _directoryInfo = new DirectoryInfo(path);
    }

    public string Name => _directoryInfo.Name;

    public DateTime LastWriteTimeUtc => _directoryInfo.LastWriteTimeUtc;

    public CommonFileSystemInfo GetCommonFileSystemInfo() => new(Name, LastWriteTimeUtc);
    public bool Exists(string path)
    {
        return Directory.Exists(Path.Combine(_directoryInfo.FullName, path));
    }

    public IEnumerable<IFileAccess> GetFiles()
    {
        return _directoryInfo.GetFiles().Select(file => new NativeFileAccess(file.FullName));
    }

    public IEnumerable<IDirectoryAccess> GetDirectories()
    {
        return _directoryInfo.GetDirectories().Select(dir => new NativeDirectoryAccess(dir.FullName));
    }

    public IDirectoryAccess? GetDirectory(string name)
    {
        var path = Path.Combine(_directoryInfo.FullName, name);
        return Directory.Exists(path) ? new NativeDirectoryAccess(Path.Combine(_directoryInfo.FullName, name)) : null;
    }

    public IFileAccess? GetFile(string name)
    {
        var path = Path.Combine(_directoryInfo.FullName, name);
        return File.Exists(path) ? new NativeFileAccess(path) : null;
    }

    public IDirectoryAccess CreateSubdirectory(string name)
    {
        return new NativeDirectoryAccess(_directoryInfo.CreateSubdirectory(name).FullName);
    }

    public Stream CreateFile(string name)
    {
        return File.Create(Path.Combine(_directoryInfo.FullName, name));
    }

    public void Delete(bool recursive)
    {
        _directoryInfo.Delete(recursive);
    }

}