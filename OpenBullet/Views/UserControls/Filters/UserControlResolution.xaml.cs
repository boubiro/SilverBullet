using System;
using System.Linq;
using System.Windows.Controls;
using ImageProcessor.Imaging.MetaData;

namespace OpenBullet.Views.UserControls.Filters
{
    /// <summary>
    /// Interaction logic for UserControlResolution.xaml
    /// </summary>
    public partial class UserControlResolution : UserControl
    {
        public UserControlResolution()
        {
            InitializeComponent();
        }

        public const string ControlName = "Resolution";
        public event EventHandler SetFilter;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Source = ControlName;
            SetFilter?.Invoke(new[] { HorizontalNumeric.Value.GetValueOrDefault(0).ToString(),
            VerticalNumeric.Value.GetValueOrDefault(0).ToString(),UnitComboBox.SelectedItem.ToString()}, e);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetFilter?.Invoke(new[] { HorizontalNumeric.Value.GetValueOrDefault(0).ToString(),
            VerticalNumeric.Value.GetValueOrDefault(0).ToString(),UnitComboBox.SelectedItem.ToString()},
            new TextChangedEventArgs(e.RoutedEvent, UndoAction.None)
            {
                Source = ControlName
            });
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            try
            {
                Enum.GetNames(typeof(PropertyTagResolutionUnit)).ToList().ForEach(p => UnitComboBox.Items.Add(p));
            }
            catch { }
        }

    }
}
