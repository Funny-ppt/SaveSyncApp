﻿using SaveSyncApp.Properties;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SaveSyncApp;

/// <summary>
/// SavesControl.xaml 的交互逻辑
/// </summary>
public partial class SavesControl : UserControl
{
    public SavesViewModel Model { get; }
    bool _requireSave = false;

    public SavesControl()
    {
        InitializeComponent();

        DataContext = Model = new();

        App.Current.MainWindow.PageChanged += (sender, e) =>
        {
            var mainWindows = sender as MainWindow;
            if (mainWindows.ContentPage == this)
            {
                Model.RefreshProfile();
            }
            else if (mainWindows.LastPage  == this)
            {
                Model.RefreshProfileCache(_requireSave);
            }
        };
    }

    private void Label_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Label label)
        {
            Clipboard.SetText(label.Content.ToString());
        }
    }

    private void SyncAllButton_Click(object sender, RoutedEventArgs e)
    {
        Task.Run(() => {
            var tasks = new List<Task>();
            foreach (var item in App.Context.Profile.Items.Values)
            {
                tasks.Add(LoadSaveAsync(item, false));
            }
            try
            {
                Task.WaitAll(tasks.ToArray());
                App.Current.ShowNotification(AppNotificationId.NotifyLoadAllSuccess, $"全部游戏的存档已经成功加载", true);
            }
            catch { }
        });
    }

    private void SyncButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ProfileItem item)
        {
            LoadSaveAsync(item, true);
        }
    }

    private static async Task LoadSaveAsync(ProfileItem item, bool notify)
    {
        if (App.Context.Profile.Items.Values.Contains(item))
        {
            try
            {
                await Task.Run(() => FolderHelper.CopyOrOverwriteFolder(
                    item.GetSyncSaveFolder(), Path.GetDirectoryName(item.ReplacedSavePath))
                );
                if (!notify)
                {
                    App.Current.LogMessage($"游戏 {item.UserFriendlyName} 的存档已经成功加载");
                }
                else
                {
                    App.Current.ShowNotification(AppNotificationId.NotifyLoadSuccess, $"游戏 {item.UserFriendlyName} 的存档已经成功加载", true);
                }
            }
            catch
            {
                App.Current.ShowNotification(AppNotificationId.ErrorFailToSave, $"游戏 {item.UserFriendlyName} 的存档加载失败", true);
                throw;
            }
        }
    }

    private void CheckLocalSaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ProfileItem item)
        {
            Process.Start("explorer.exe", item.ReplacedSavePath);
        }
    }

    private void CheckSyncSaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ProfileItem item)
        {
            Process.Start("explorer.exe", item.GetSyncSaveFolder());
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (Model.IsSaveSyncActive)
        {
            MessageBox.Show("SaveSync在运行时, 禁止删除存档", "SaveSync", MessageBoxButton.OK);
        }
        if (sender is Button button && button.Tag is ProfileItem item)
        {
            var result = MessageBox.Show($"确认要删除 {item.UserFriendlyName} 的存档吗?", "SaveSync", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                _requireSave = true;
                Directory.Delete(item.GetSyncSaveFolder(), true);
                App.Context.Profile.Items.Remove(item.ProcessName, out _);
                Model.RefreshProfileCache(true);
            }
        }
    }
}
