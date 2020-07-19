using System;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls.Filters
{
    /// <summary>
    /// Interaction logic for UserControlHoughLine.xaml
    /// </summary>
    public partial class UserControlHoughLine : UserControl
    {
        public UserControlHoughLine()
        {
            InitializeComponent();
        }


        public const string ControlName = "HoughLine";
        public event EventHandler SetFilter;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Source = ControlName;
            SetFilter?.Invoke(new[] { WidthTextBox.Text,
            HeightTextBox.Text,
            ThresholdTextBox.Text }, e);
        }
    }
}
