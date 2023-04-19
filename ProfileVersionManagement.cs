using SaveSyncApp.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveSyncApp;

internal class ProfileVersionManagement : IProfileVersionManagement
{
    readonly static string _roamingFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    readonly static string _documentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    public static readonly IEnumerable<string> DefaultTrackPaths = new[] { _roamingFolder, _documentFolder };
    public int CurrentVersion => 1;

    public Profile GetDefaultProfile() => new()
    {
        Version = CurrentVersion,
        Items = new(),
        SyncPath = Settings.DefaultSyncFolder,
        TrackPaths = new(DefaultTrackPaths),
        IgnorePaths = new(),
    };

    void UpdateToV1(Profile profile)
    {
        if (profile.Version >= 1)
        {
            return;
        }
        profile.Version = 1;
    }

    // 更新模板
    // void UpdateToVx(Profile profile)
    // {
    //     if (profile.Version >= x)
    //     {
    //         return;
    //     }
    //     UpdateToVx-1(profile);
    //     // do updates
    //     profile.Version = x;
    // }

    public void UpdateToCurrentVersion(Profile profile)
    {
        // 每当更新新版本时，修改该函数到最新版
        UpdateToV1(profile);
    }
}
