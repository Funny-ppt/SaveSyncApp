using SaveSyncApp.IO;
using System;
using System.IO;

namespace SaveSyncApp;

public interface IFileAccess : IHasCommonFileSystemInfo
{
    string Name { get; }
    DateTime LastWriteTimeUtc { get; }
    Stream OpenRead();
    Stream OpenWrite();
    void Delete();
}
