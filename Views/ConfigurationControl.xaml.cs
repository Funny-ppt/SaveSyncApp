using Microsoft.Extensions.Logging;
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

        App.Context.PropertyChanged += (sender, e) =>
        {
            if (App.Current.MainWindow.ContentPage == this && e.PropertyName == "Profile")
            {
                TrackPathsListBox.ItemsSource = App.Context.Profile.TrackPaths;
                IgnorePathsListBox.ItemsSource = App.Context.Profile.IgnorePaths;
            }
        };
        App.Current.MainWindow.PageChanged += (sender, e) =>
        {
            var mainWindows = sender as MainWindow;
            if (mainWindows.LastPage == this)
            {
                App.Context.RefreshProfileCache();
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
            if (elem.Visibility == Visibility.Collapsed)
            {
                continue;
            }
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
        if (sender is Button button && button.Tag is TextBox textbox)
        {
            using var folderPicker = new System.Windows.Forms.FolderBrowserDialog();
            var result = folderPicker.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                textbox.SetCurrentValue(TextBox.TextProperty, folderPicker.SelectedPath);
                var binding = textbox.GetBindingExpression(TextBox.TextProperty);
                binding?.UpdateSource();
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
            if (!App.Context.Profile.TrackPaths.Contains(path))
            {
                App.Context.Profile.TrackPaths.Add(path);
            }
        }
    }

    private void AddIgnorePathButton_Click(object sender, RoutedEventArgs e)
    {
        var path = NewIgnorePathTextBox.Text;
        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
        {
            if (!App.Context.Profile.IgnorePaths.Contains(path))
            {
                App.Context.Profile.IgnorePaths.Add(path);
            }
        }
    }

    private void LogLevelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.SelectedItem is LogLevel level)
        {
            App.Context.LogLevel = level;
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
