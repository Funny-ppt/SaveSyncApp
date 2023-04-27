using System;
using System.IO;

namespace SaveSyncApp.IO;

public class OverwritePreview
{
    public CommonFileSystemInfo? SourceInfo;
    public CommonFileSystemInfo DestinationInfo;
    public FileSystemAction Action = FileSystemAction.Overwrite;

    public OverwritePreview(CommonFileSystemInfo? sourceInfo, CommonFileSystemInfo destinationInfo)
    {
        SourceInfo = sourceInfo;
        DestinationInfo = destinationInfo ?? throw new ArgumentException(null, nameof(destinationInfo));
    }
}
