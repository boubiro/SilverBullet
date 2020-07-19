using System;
using System.Linq;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls.Filters
{
    /// <summary>
    /// Interaction logic for UserControlEnumBox.xaml
    /// </summary>
    public partial class UserControlEnumBox : UserControl
    {
        public UserControlEnumBox()
        {
            InitializeComponent();
        }

        public const string ControlName = "Enum";
        public event EventHandler SetFilter;

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

            SetFilter?.Invoke(new[] { EnumComboBox.SelectedItem.ToString() },
           new TextChangedEventArgs(e.RoutedEvent, UndoAction.None)
           {
               Source = ControlName
           });
        }
    }
}
