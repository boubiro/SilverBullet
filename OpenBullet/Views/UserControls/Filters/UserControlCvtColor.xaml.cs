using System;
using System.Linq;
using System.Windows.Controls;
using OpenCvSharp;

namespace OpenBullet.Views.UserControls.Filters
{
    /// <summary>
    /// Interaction logic for UserControlCvtColor.xaml
    /// </summary>
    public partial class UserControlCvtColor : UserControl
    {
        public UserControlCvtColor()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            try { Enum.GetNames(typeof(ColorConversionCodes)).ToList().ForEach(c => CodeComboBox.Items.Add(c)); } catch { }
        }

        public const string ControlName = "CvtColor";
        public event EventHandler SetFilter;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Source = ControlName;
            SetFilter?.Invoke(new[] { CodeComboBox.SelectedItem.ToString(),
            dstCnTextBox.Text }, e);
        }

        private void CodeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CodeComboBox.Items.Count == 0) return;

            SetFilter?.Invoke(new[] { CodeComboBox.SelectedItem.ToString() ,
             dstCnTextBox.Text},
           new TextChangedEventArgs(e.RoutedEvent, UndoAction.None)
           {
               Source = ControlName
           });
        }
    }
}
