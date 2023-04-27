using System;
using System.IO.Compression;
using System.IO;

namespace SaveSyncApp.IO;

public class ZipFileAccess : IFileAccess
{
    private ZipArchiveEntry _zipEntry;

    public ZipFileAccess(ZipArchiveEntry zipEntry)
    {
        _zipEntry = zipEntry;
    }

    public string Name => _zipEntry.Name;

    public string RelativePath => _zipEntry.FullName;

    public DateTime LastWriteTimeUtc => _zipEntry.LastWriteTime.UtcDateTime;

    public CommonFileSystemInfo GetCommonFileSystemInfo() => new(Name, LastWriteTimeUtc);

    public Stream OpenRead()
    {
        return _zipEntry.Open();
    }

    // todo: 检查错误和有效性
    public Stream OpenWrite()
    {
        _zipEntry.Delete();
        _zipEntry = _zipEntry.Archive.CreateEntry(_zipEntry.FullName);
        return _zipEntry.Open();
    }

    public void Delete()
    {
        _zipEntry.Delete();
    }
}
