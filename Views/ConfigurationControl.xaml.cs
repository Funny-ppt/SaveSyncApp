using SaveSyncApp.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Windows.Storage.Pickers;

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
    }

    private void SelectDirectoryButton_Click(object sender, RoutedEventArgs e)
    {
        //var fp = new FolderPicker();
        using var folderPicker = new System.Windows.Forms.FolderBrowserDialog();
        var result = folderPicker.ShowDialog();

        if (result == System.Windows.Forms.DialogResult.OK)
        {
            Settings.Default.WorkingDirectory = folderPicker.SelectedPath;
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
