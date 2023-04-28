using System;
using System.Collections.Generic;
using System.IO;

namespace SaveSyncApp.IO;

public interface IDirectoryAccess : IHasCommonFileSystemInfo
{
    string Name { get; }
    DateTime LastWriteTimeUtc { get; }
    bool Exists(string path);
    IEnumerable<IFileAccess> GetFiles();
    IEnumerable<IDirectoryAccess> GetDirectories();
    IDirectoryAccess? GetDirectory(string name);
    IFileAccess? GetFile(string name);
    IDirectoryAccess CreateSubdirectory(string name);
    Stream CreateFile(string name);
    void Delete(bool recursive);
}
