using System;
using System.Linq;
using System.Windows.Controls;
using OpenCvSharp;

namespace OpenBullet.Views.UserControls.Filters
{
    /// <summary>
    /// Interaction logic for UserControlThreshold.xaml
    /// </summary>
    public partial class UserControlThreshold : UserControl
    {
        public UserControlThreshold()
        {
            InitializeComponent();
        }

        public const string ControlName = "Threshold";
        public event EventHandler SetFilter;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Source = ControlName;
            SetFilter?.Invoke(new[] { ThreshTextBox.Text,MaxValueTextBox.Text
                ,ThresholdTypeComboBox.SelectedItem.ToString()}, e);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetFilter?.Invoke(new[] { ThreshTextBox.Text,MaxValueTextBox.Text
                ,ThresholdTypeComboBox.SelectedItem.ToString()},
            new TextChangedEventArgs(e.RoutedEvent, UndoAction.None)
            {
                Source = ControlName
            });
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            try
            {
                Enum.GetNames(typeof(ThresholdTypes)).ToList().ForEach(t => ThresholdTypeComboBox.Items.Add(t));
            }
            catch { }
        }
    }
}
