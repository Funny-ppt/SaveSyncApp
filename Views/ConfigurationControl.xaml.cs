using SaveSyncApp.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SaveSyncApp;

/// <summary>
/// ConfigurationControl.xaml 的交互逻辑
/// </summary>
public partial class ConfigurationControl : UserControl
{
    public ConfigurationControl()
    {
        InitializeComponent();

        AppSettingGroup.DataContext = Settings.Default;
        ProfileSettingGroup.DataContext = App.Context.Profile;
        SetComponentRows(AppSettingGroup.Content as Grid);
        SetComponentRows(ProfileSettingGroup.Content as Grid);

        App.Current.MainWindow.PageChanged += (sender, e) =>
        {
            var mainWindows = sender as MainWindow;
            if (mainWindows.ContentPage == this)
            {
                ProfileSettingGroup.DataContext = App.Context.Profile;
            }
            else if (mainWindows.LastPage == this)
            {
                App.Context.RefreshProfileCache(true);
            }
            SetComponentRows(ProfileSettingGroup.Content as Grid);
        };
    }

    private void SetComponentRows(Grid grid)
    {
        grid.RowDefinitions.Clear();
        var row = -1;
        var lastColumn = 100;
        foreach (UIElement elem in grid.Children)
        {
            var col = (int)elem.GetValue(Grid.ColumnProperty);
            var colSpan = (int)elem.GetValue(Grid.ColumnSpanProperty);
            if (col < lastColumn)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                ++row;
            }
            lastColumn = col + colSpan;
            elem.SetValue(Grid.RowProperty, row);
        }
    }

    private void SelectDirectoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is TextBox box)
        {
            using var folderPicker = new System.Windows.Forms.FolderBrowserDialog();
            var result = folderPicker.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                box.Text = folderPicker.SelectedPath;
            }
        }
    }

    private void RestartSaveSyncButton_Click(object sender, RoutedEventArgs e)
    {
        App.Context.StartNewSaveSync();
    }

    private void AddTrackPathButton_Click(object sender, RoutedEventArgs e)
    {
        var path = NewTrackPathTextBox.Text;
        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
        {
            App.Context.Profile.TrackPaths.Add(path);
            App.Context.RefreshProfileCache(true);
        }
    }

    private void AddIgnorePathButton_Click(object sender, RoutedEventArgs e)
    {
        var path = NewIgnorePathTextBox.Text;
        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
        {
            App.Context.Profile.IgnorePaths.Add(path);
            App.Context.RefreshProfileCache(true);
        }
    }

    ////preserverd
    //private Grid CreatePropertyGrid(string key, string value, string type = nameof(TextBox), object[]? candidateValues = null)
    //{
    //    var grid = new Grid()
    //    {
    //        ColumnDefinitions = {
    //                        new ColumnDefinition() { Width = new GridLength(200) },
    //                        new ColumnDefinition(),
    //                    }
    //    };

    //    var label = new Label
    //    {
    //        Content = key,
    //        Style = (Style)Resources["LabelStyle"],
    //    };
    //    grid.Children.Add(label);

    //    UIElement elem;

    //    switch (type)
    //    {
    //        default:
    //        case nameof(TextBox):
    //            var textbox = new TextBox
    //            {
    //                Text = value,
    //                Style = (Style)Resources["TextBoxStyle"]
    //            };
    //            textbox.TextChanged += (sender, e) =>
    //            {
    //                // todo
    //            };
    //            elem = textbox;
    //            break;
    //    }

    //    grid.Children.Add(elem);
    //    return grid;
    //}
}
