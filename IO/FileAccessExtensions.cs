using System;
using System.IO;

namespace SaveSyncApp.IO;

internal static class FileAccessExtensions
{
    public static void CopyTo(this IFileAccess source, IFileAccess destination)
    {
        using var sourceStream = source.OpenRead();
        using var destStream = destination.OpenWrite();
        sourceStream.CopyTo(destStream);
    }
}
