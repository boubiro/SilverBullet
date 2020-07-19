using System;
using System.Linq;
using System.Windows.Controls;
using ImageMagick;

namespace OpenBullet.Views.UserControls.Filters
{
    /// <summary>
    /// Interaction logic for UserControlBlur.xaml
    /// </summary>
    public partial class UserControlBlur : UserControl
    {
        public UserControlBlur()
        {
            InitializeComponent();
        }

        public const string ControlName = "Blur";
        public event EventHandler SetFilter;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Source = ControlName;
            SetFilter?.Invoke(new[] { RadiusTextBox.Text,
            SigmaTextBox.Text,ChannelsComboBox.SelectedItem.ToString()}, e);
        }

        private void ChannelsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChannelsComboBox.Visibility != System.Windows.Visibility.Visible) return;
            SetFilter?.Invoke(new[] { RadiusTextBox.Text,
            SigmaTextBox.Text,ChannelsComboBox.SelectedItem.ToString() },
            new TextChangedEventArgs(e.RoutedEvent, UndoAction.None)
            {
                Source = ControlName
            });
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            try
            {
                Enum.GetNames(typeof(Channels)).ToList().ForEach(c => ChannelsComboBox.Items.Add(c));
            }
            catch { }
        }
    }
}
