using OpenBullet.Views.Main.Runner;
using RuriLib.Runner;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per DialogSelectBots.xaml
    /// </summary>
    public partial class DialogSelectBots : Page
    {
        public object Caller { get; set; }

        const int Maximum = 400;
        const int Minimum = 1;

        public DialogSelectBots(object caller, int initial = 1)
        {
            InitializeComponent();
            Caller = caller;
            botsNumberTextbox.Text = initial.ToString();
        }

        private void selectButton_Click(object sender, RoutedEventArgs e)
        {
            int bots = 1;
            int.TryParse(botsNumberTextbox.Text, out bots);

            if (Caller.GetType() == typeof(RunnerViewModel))
            {
                (Caller as RunnerViewModel).BotsAmount = bots;
            }
            ((MainDialog)Parent).Close();
        }

        private void botsNumberTextbox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try { if (e.Key == System.Windows.Input.Key.Enter) selectButton_Click(null, null); } catch { }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                botsNumberTextbox.CaretIndex = botsNumberTextbox.Text.Length ;
                botsNumberTextbox.Focus();
            }
            catch { }
        }

        private void botsNumberTextbox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            try
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
                if (!e.Handled)
                {
                    var textBox = (TextBox)sender;
                    var value = textBox.Text;
                    if (textBox.SelectedText != string.Empty)
                    {
                        value = textBox.Text.Remove(textBox.SelectionStart,
                           textBox.SelectedText.Length);
                    }
                    var botsAmount = int.Parse(value + e.Text);
                    e.Handled = !(botsAmount <= Maximum && botsAmount > Minimum - 1);
                }
            }
            catch { }
        }
    }
}
