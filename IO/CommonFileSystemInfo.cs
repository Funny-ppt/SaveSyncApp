using System;

namespace SaveSyncApp.IO;

public class CommonFileSystemInfo
{
    public string Name;
    public DateTime LastWriteTimeUtc;

    public CommonFileSystemInfo(string name, DateTime lastWriteTimeUtc)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        LastWriteTimeUtc = lastWriteTimeUtc;
    }
}
