using System;
using System.Configuration;
using System.ComponentModel;
using System.IO;

namespace SaveSyncApp.Properties;

// 通过此类可以处理设置类的特定事件: 
//  在更改某个设置的值之前将引发 SettingChanging 事件。
//  在更改某个设置的值之后将引发 PropertyChanged 事件。
//  在加载设置值之后将引发 SettingsLoaded 事件。
//  在保存设置值之前将引发 SettingsSaving 事件。
internal sealed partial class Settings {

    public static readonly string AppDataFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SaveSync");
    public static readonly string DefaultProfileFile = Path.Combine(AppDataFolder, "profile.json");
    public static readonly string DefaultSyncFolder = Path.Combine(AppDataFolder, "Saves");

    public Settings() {
        PropertyChanged += PropertyChangedHandler;
        SettingChanging += SettingChangingHandler;
        SettingsLoaded += SettingsLoadedHandler;
        SettingsSaving += SettingsSavingHandler;
    }


    bool _instant = false;
    internal void InstantSetValue(string key, object value)
    {
        _instant = true;
        this[key] = value;
        _instant = false;
    }


    private void SettingChangingHandler(object sender, SettingChangingEventArgs e)
    {
        if (_instant) return;

        switch (e.SettingKey)
        {
            case "Startup":
                if (e.NewValue is bool b && b != Startup)
                {
                    if (!AutorunHelper.TrySetAutorun("SaveSync", b))
                    {
                        e.Cancel = true;
                    }
                }
                break;
            default:
                break;
        }
    }

    private void PropertyChangedHandler(object? sender, PropertyChangedEventArgs e)
    {
        if (_instant) return;
    }

    private void SettingsLoadedHandler(object sender, SettingsLoadedEventArgs e)
    {
        if (string.IsNullOrEmpty(ProfilePath))
        {
            ProfilePath = DefaultProfileFile;
        }
    }
    private void SettingsSavingHandler(object sender, CancelEventArgs e)
    {
    }
}
