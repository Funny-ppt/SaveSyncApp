using SaveSyncApp.IO;
using System;
using System.Collections.Generic;

namespace SaveSyncApp;

// 思考：是否允许创建一个不存在的文件、目录的Access？
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
    void Delete(bool recursive);
}
