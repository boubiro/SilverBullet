using System;
using System.Linq;
using System.Windows.Controls;
using ImageMagick;
using OpenCvSharp;

namespace OpenBullet.Views.UserControls.Filters
{
    /// <summary>
    /// Interaction logic for UserControlAdaptiveThreshold.xaml
    /// </summary>
    public partial class UserControlAdaptiveThreshold : UserControl
    {
        public UserControlAdaptiveThreshold()
        {
            InitializeComponent();
        }

        public const string ControlName = "AdaptiveThreshold";
        public event EventHandler SetFilter;


        private void UserControl_Initialized(object sender, EventArgs e)
        {
            try { Enum.GetNames(typeof(AdaptiveThresholdTypes)).ToList().ForEach(a => AdaptiveMethodComboBox.Items.Add(a)); } catch { }
            try { Enum.GetNames(typeof(ThresholdTypes)).ToList().ForEach(t => ThresholdTypeComboBox.Items.Add(t)); } catch { }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Source = ControlName;
            SetFilter?.Invoke(new[] {MaxValueTextBox.Text,AdaptiveMethodComboBox.SelectedItem.ToString(),
                ThresholdTypeComboBox.SelectedItem.ToString(), BlockSizeTextBox.Text,ConstantTextBox.Text}, e);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetFilter?.Invoke(new[] { MaxValueTextBox.Text,AdaptiveMethodComboBox.SelectedItem.ToString(),
                ThresholdTypeComboBox.SelectedItem.ToString(), BlockSizeTextBox.Text,ConstantTextBox.Text },
            new TextChangedEventArgs(e.RoutedEvent, UndoAction.None)
            {
                Source = ControlName
            });
        }
    }
}
