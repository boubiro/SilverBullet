using System;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls.Filters
{
    /// <summary>
    /// Interaction logic for UserControlFastNlMeansDenoisingColored.xaml
    /// </summary>
    public partial class UserControlFastNlMeansDenoisingColored : UserControl
    {
        public UserControlFastNlMeansDenoisingColored()
        {
            InitializeComponent();
        }

        public const string ControlName = "FastNlMeansDenoisingColored";
        public event EventHandler SetFilter;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Source = ControlName;
            SetFilter?.Invoke(new[] {StrengthTextBox.Text,
            ColorStrengthTextBox.Text}, e);
        }
    }
}
