using System;
using System.Linq;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls.Filters
{
    /// <summary>
    /// Interaction logic for UserControlInputTextAndEnum.xaml
    /// </summary>
    public partial class UserControlInputTextAndEnum : UserControl
    {
        public UserControlInputTextAndEnum()
        {
            InitializeComponent();
        }

        public const string ControlName = "InputTextAndEnum";
        public event EventHandler SetFilter;
        public bool Reverse { get; set; }

        public string TEnumName { get; set; }
        public void AddEnum<TEnum>()
        {
            EnumComboBox.Items.Clear();
            var type = typeof(TEnum);
            Enum.GetNames(type).ToList().ForEach(e =>
                EnumComboBox.Items.Add(e));
            TEnumName = type.Name;
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
        }

        private void EnumComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EnumComboBox.Items.Count == 0) return;

            SetFilter?.Invoke(GetInputs(),
           new TextChangedEventArgs(e.RoutedEvent, UndoAction.None)
           {
               Source = ControlName
           });
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Source = ControlName;
            SetFilter?.Invoke(GetInputs(), e);
        }

        private string[] GetInputs()
        {
            if (Reverse)
            {
                return new[] { EnumComboBox.SelectedItem.ToString(),
                InputTextBox.Text };
            }
            return new[] { InputTextBox.Text,
                EnumComboBox.SelectedItem.ToString() };
        }

    }
}
