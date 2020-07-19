using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls.Filters
{
    /// <summary>
    /// Interaction logic for UserControlInputTextAndBoolean.xaml
    /// </summary>
    public partial class UserControlInputTextAndBoolean : UserControl
    {
        public UserControlInputTextAndBoolean()
        {
            InitializeComponent();
        }

        public const string ControlName = "InputTextAndBoolean";
        public event EventHandler SetFilter;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Source = ControlName;
            SetFilter?.Invoke(new[] { InputTextBox.Text,
            CheckBox.IsChecked.GetValueOrDefault().ToString()}, e);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetFilter?.Invoke(new[] { InputTextBox.Text,
            CheckBox.IsChecked.GetValueOrDefault().ToString()},
            new TextChangedEventArgs(e.RoutedEvent, UndoAction.None)
            {
                Source = ControlName
            });
        }
    }
}
