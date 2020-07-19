using System;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls.Filters
{
    /// <summary>
    /// Interaction logic for UserControlInput.xaml
    /// </summary>
    public partial class UserControlInput : UserControl
    {
        public UserControlInput()
        {
            InitializeComponent();
        }

        public const string ControlName = "Input";
        public event EventHandler SetFilter;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Source = ControlName;
            SetFilter?.Invoke(new[] { (sender as TextBox).Text }, e);
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            SetInputType(InputType.Text);
        }

        public enum InputType
        {
            Text,
            Boolean
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var inputBox = sender as ComboBox;
            if (inputBox.Visibility != System.Windows.Visibility.Visible) return;
            SetFilter?.Invoke(new[] { (inputBox.SelectedItem as ComboBoxItem).Content.ToString() },
            new TextChangedEventArgs(e.RoutedEvent, UndoAction.None)
            {
                Source = ControlName
            });
        }

        internal void SetInputType(InputType inputType)
        {
            switch (inputType)
            {
                case InputType.Text:
                    InputComboBox.Visibility = System.Windows.Visibility.Collapsed;
                    InputTextBox.Visibility = System.Windows.Visibility.Visible;
                    break;
                case InputType.Boolean:
                    InputTextBox.Visibility = System.Windows.Visibility.Collapsed;
                    InputComboBox.Visibility = System.Windows.Visibility.Visible;
                    break;

                default:
                    InputTextBox.Visibility = System.Windows.Visibility.Visible;
                    InputComboBox.Visibility = System.Windows.Visibility.Collapsed;
                    break;
            }
        }
    }
}
