using System.Collections.Generic;
using System.IO;
using static SaveSyncApp.SpecialFolders;

namespace SaveSyncApp;

internal class ProfileVersionManagement : IProfileVersionManagement
{
    public static readonly IEnumerable<string> DefaultTrackPaths = new[] { ApplicationDataPlaceholder, ApplicationDataPlaceholder2, DocumentPlaceholder };
    readonly static string _qqDataFolder = Path.Combine(DocumentFolder, "Tencent Files");
    public static readonly IEnumerable<string> DefaultIgnorePaths = new[] { _qqDataFolder };
    public int CurrentVersion => 3;

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

    void UpdateToV3(Profile profile)
    {
        if (profile.Version >= 3)
        {
            return;
        }
        UpdateToV2(profile);
        profile.Version = 3;

        profile.EnableCompression = false;
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
        UpdateToV3(profile);
    }
}
