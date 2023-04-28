using SaveSyncApp.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SaveSyncApp;

internal static class ProfileExtensions
{
    public static string GetSyncSaveFolder(this ProfileItem item)
        => Path.Combine(Settings.Default.WorkingDirectory, "Saves", Path.GetFileName(item.ReplacedSavePath));

    public static string GetSyncSaveArchive(this ProfileItem item)
        => Path.Combine(Settings.Default.WorkingDirectory, "Saves", Path.GetFileName(item.ReplacedSavePath) + ".zip");
}
