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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageProcessor.Imaging;

namespace OpenBullet.Views.UserControls.Filters
{
    /// <summary>
    /// Interaction logic for UserControlCropLayer.xaml
    /// </summary>
    public partial class UserControlCropLayer : UserControl
    {
        public UserControlCropLayer()
        {
            InitializeComponent();
        }

        public const string ControlName = "CropLayer";
        public event EventHandler SetFilter;

        private void TextBoxCL_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Source = ControlName;
            SetFilter?.Invoke(new[] { LeftTextBox.Text,TopTextBox.Text,
            RightTextBox.Text,BottomTextBox.Text,CropModeBox.SelectedItem.ToString()}, e);
        }

        private void CropModeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetFilter?.Invoke(new[] { LeftTextBox.Text,TopTextBox.Text,
            RightTextBox.Text,BottomTextBox.Text,CropModeBox.SelectedItem.ToString() },
            new TextChangedEventArgs(e.RoutedEvent, UndoAction.None)
            {
                Source = ControlName
            });
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            try
            {
                Enum.GetNames(typeof(CropMode)).ToList()
                    .ForEach(c => CropModeBox.Items.Add(c));
            }
            catch { }
        }
    }
}
