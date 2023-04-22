﻿#nullable disable
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

    static readonly string DefaultWorkingDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SaveSync");
    const string DefaultPlaceholder = "<Default>";

    public Settings() {
        PropertyChanged += PropertyChangedHandler;
        SettingChanging += SettingChangingHandler;
        SettingsLoaded += SettingsLoadedHandler;
        SettingsSaving += SettingsSavingHandler;
    }

    public string WorkingDirectory
    {
        get
        {
            if (InternalWorkingDirectory == DefaultPlaceholder || string.IsNullOrEmpty(InternalWorkingDirectory))
            {
                return DefaultWorkingDirectory;
            }
            return InternalWorkingDirectory;
        }
        set
        {
            InternalWorkingDirectory = value?.Trim() ?? "";
        }
    }

    public NotificationLevel NotificationLevel
    {
        get => Enum.Parse<NotificationLevel>(InternalNotificationLevel);
        set => InternalNotificationLevel = value.ToString();
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
        if (string.IsNullOrEmpty(WorkingDirectory) || WorkingDirectory == DefaultPlaceholder)
        {
            WorkingDirectory = DefaultWorkingDirectory;
        }
        if (InternalNotificationLevel != "Normal" || InternalNotificationLevel != "Minimum")
        {
            InternalNotificationLevel = "Normal";
        }
    }
    private void SettingsSavingHandler(object sender, CancelEventArgs e)
    {
    }
}