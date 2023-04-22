using SaveSyncApp.Properties;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SaveSyncApp
{
    /// <summary>
    /// SavesControl.xaml 的交互逻辑
    /// </summary>
    public partial class SavesControl : UserControl
    {
        public SavesViewModel Model { get; }

        public SavesControl()
        {
            InitializeComponent();

            DataContext = Model = new();
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
                Task.WaitAll(tasks.ToArray());
                App.Current.LogMessage($"全部游戏存档已经成功加载", null);
                NotificationHelper.ShowNotification(1001, $"全部游戏的存档已经成功加载");
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
                await Task.Run(() => FolderHelper.CopyOrOverwriteFolder(
                    Path.Combine(Settings.Default.WorkingDirectory, "Saves", Path.GetFileName(item.SavePath)),
                    Path.GetDirectoryName(item.SavePath))
                );
                App.Current.LogMessage($"游戏 {item.UserFriendlyName} 的存档已经成功加载", null);
                if (notify)
                {
                    NotificationHelper.ShowNotification(1001, $"游戏 {item.UserFriendlyName} 的存档已经成功加载");
                }
            }
        }
    }
}
