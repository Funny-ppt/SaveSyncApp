using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SaveSyncApp
{
    /// <summary>
    /// CustomDialog.xaml 的交互逻辑
    /// </summary>
    public partial class CustomDialog : Window
    {
        public CustomDialogViewModel ViewModel => (CustomDialogViewModel)DataContext;

        public CustomDialog()
        {
            InitializeComponent();
        }

        private void Initialize(string title, string content, (string buttonText, string buttonValue)[] buttons, (string key, string value)[] args)
        {
            ViewModel.Title = title;
            ViewModel.Content = content;

            foreach (var (key, value) in args)
            {
                ViewModel.Add(key, value);
            }

            foreach (var (buttonText, buttonValue) in buttons)
            {
                var button = new Button { Content = buttonText, Margin = new Thickness(0, 0, 10, 0), Width = 60 };
                button.Click += (sender, e) => {
                    ViewModel["action"] = buttonValue;
                    DialogResult = true;
                    Close();
                };
                ButtonPanel.Children.Add(button);
            }

            //ContentTextBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            //var desiredSize = ContentTextBlock.DesiredSize;
            //Width = desiredSize.Width;
        }

        public static IReadOnlyDictionary<string, string>? ShowDialog(string title, string content, (string buttonText, string buttonValue)[] buttons, (string key, string value)[]? args = null)
        {
            var customDialog = new CustomDialog();
            customDialog.Initialize(title, content, buttons, args ?? Array.Empty<(string, string)>());

            // 如果当前上下文是窗口，请使用以下代码将对话框的Owner设置为当前窗口
            // customDialog.Owner = Application.Current.MainWindow;

            bool? dialogResult = customDialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            {
                return customDialog.ViewModel.Arguments;
            }

            return null;
        }
    }
}
