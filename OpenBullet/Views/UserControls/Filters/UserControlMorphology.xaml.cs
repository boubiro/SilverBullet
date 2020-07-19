using System;
using System.Linq;
using System.Windows.Controls;
using OpenCvSharp;

namespace OpenBullet.Views.UserControls.Filters
{
    /// <summary>
    /// Interaction logic for UserControlMorphology.xaml
    /// </summary>
    public partial class UserControlMorphology : UserControl
    {
        public UserControlMorphology()
        {
            InitializeComponent();
        }

        public const string ControlName = "Morphology";
        public event EventHandler SetFilter;

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            try { Enum.GetNames(typeof(MorphTypes)).ToList().ForEach(m => MorphMethodComboBox.Items.Add(m)); } catch { }
            try { Enum.GetNames(typeof(BorderTypes)).ToList().ForEach(c => BorderTypeComboBox.Items.Add(c)); } catch { }
            try { MorphShapesComboBox.Items.Add("Null"); } catch { }
            try { Enum.GetNames(typeof(MorphShapes)).ToList().ForEach(m => MorphShapesComboBox.Items.Add(m)); } catch { }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetFilter?.Invoke(new[] { MorphMethodComboBox.SelectedItem.ToString(),
            IterationsTextBox.Text , BorderTypeComboBox.SelectedItem.ToString(),
            MorphShapesComboBox.SelectedItem.ToString(),
            SizeWidthTextBox.Text,SizeHeightTextBox.Text},
            new TextChangedEventArgs(e.RoutedEvent, UndoAction.None)
            {
                Source = ControlName
            });
        }

        private void IterationsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Source = ControlName;
            SetFilter?.Invoke(new[] { MorphMethodComboBox.SelectedItem.ToString(),
            IterationsTextBox.Text , BorderTypeComboBox.SelectedItem.ToString(),
            MorphShapesComboBox.SelectedItem.ToString(),
            SizeWidthTextBox.Text,SizeHeightTextBox.Text}, e);
        }
    }
}
