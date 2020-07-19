using System;
using System.Windows.Controls;

namespace OpenBullet.Views.UserControls.Filters
{
    /// <summary>
    /// Interaction logic for UserControlReplaceColor.xaml
    /// </summary>
    public partial class UserControlReplaceColor : UserControl
    {
        public UserControlReplaceColor()
        {
            InitializeComponent();
        }

        public const string ControlName = "ReplaceColor";
        public event EventHandler SetFilter;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Source = ControlName;
            SetFilter?.Invoke(new[] { TargetTextBox.Text,
            ReplacementTextBox.Text , FuzzinessTextBox.Text}, e);
        }
    }
}
