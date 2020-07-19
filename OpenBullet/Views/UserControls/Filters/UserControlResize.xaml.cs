using System;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls.Filters
{
    /// <summary>
    /// Interaction logic for UserControlResize.xaml
    /// </summary>
    public partial class UserControlResize : UserControl
    {
        public UserControlResize()
        {
            InitializeComponent();
        }

        public const string ControlName = "Resize";
        public event EventHandler SetFilter;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Source = ControlName;
            SetFilter?.Invoke(new[] { WidthTextBox.Text, HeightTextBox.Text }, e);
        }
    }
}
