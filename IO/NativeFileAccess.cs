using System;
using System.IO;

namespace SaveSyncApp.IO;

public class NativeFileAccess : IFileAccess
{
    private FileInfo _fileInfo;

    public NativeFileAccess(string path)
    {
        _fileInfo = new FileInfo(path);
    }

    public string Name => _fileInfo.Name;

    public DateTime LastWriteTimeUtc => _fileInfo.LastWriteTimeUtc;
    public CommonFileSystemInfo GetCommonFileSystemInfo() => new(Name, LastWriteTimeUtc);

    public Stream OpenRead() => _fileInfo.OpenRead();

    public Stream OpenWrite() => _fileInfo.OpenWrite();

    public void Delete() => _fileInfo.Delete();
}
