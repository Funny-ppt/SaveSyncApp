using SaveSyncApp.Properties;
using System;
using System.Collections.Generic;
using System.IO;

namespace SaveSyncApp;

internal class ProfileVersionManagement : IProfileVersionManagement
{
    readonly static string _userProfileFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    readonly static string _applicationDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    readonly static string _applicationDataFolder2 = Path.Combine(_userProfileFolder, "AppData", "LocalLow");
    readonly static string _documentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    public static readonly IEnumerable<string> DefaultTrackPaths = new[] { _applicationDataFolder, _applicationDataFolder2, _documentFolder };
    readonly static string _qqDataFolder = Path.Combine(_documentFolder, "Tencent Files");
    public static readonly IEnumerable<string> DefaultIgnorePaths = new[] { _qqDataFolder };
    public int CurrentVersion => 2;

    public Profile GetDefaultProfile() => new()
    {
        Version = CurrentVersion,
        Items = new(),
        TrackPaths = new(DefaultTrackPaths),
        IgnorePaths = new(DefaultIgnorePaths),
    };

    void UpdateToV1(Profile profile)
    {
        if (profile.Version >= 1)
        {
            return;
        }
        profile.Version = 1;
    }

    void UpdateToV2(Profile profile)
    {
        if (profile.Version >= 2)
        {
            return;
        }
        UpdateToV1(profile);
        profile.Version = 2;

        profile.IgnorePaths = new(DefaultIgnorePaths);
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
        UpdateToV2(profile);
    }
}
