using System;
using System.IO;

namespace SaveSyncApp;

public class FileSystemOperationPreview : EventArgs
{
    public FileSystemInfo? SourceInfo;
    public FileSystemInfo DestinationInfo;
    public FileSystemAction Action = FileSystemAction.Overwrite;

    public FileSystemOperationPreview(FileSystemInfo? sourceInfo, FileSystemInfo destinationInfo)
    {
        SourceInfo = sourceInfo;
        DestinationInfo = destinationInfo ?? throw new ArgumentException(null, nameof(destinationInfo));
    }
}
