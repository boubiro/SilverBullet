using IronPython.Runtime;
using Microsoft.Win32;
using OpenBullet.Views.Main;
using RuriLib.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per DialogAddWordlist.xaml
    /// </summary>
    public partial class DialogAddWordlist : Page
    {
        public object Caller { get; set; }

        private List<SubWordlist> SubWordlists = new List<SubWordlist>();

        public DialogAddWordlist(object caller)
        {
            InitializeComponent();

            Caller = caller;

            foreach (string i in SB.Settings.Environment.GetWordlistTypeNames())
                typeCombobox.Items.Add(i);

            SB.Settings.Environment.GetWordlistTypeNames()
                .ForEach(wt => subTypeComboBox.Items.Add(wt));

            typeCombobox.SelectedIndex = 0;
        }

        private void acceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (Caller.GetType() == typeof(WordlistManager))
            {
                if (nameTextbox.Text.Trim() == string.Empty) { MessageBox.Show("The name cannot be blank"); return; }

                var path = locationTextbox.Text;
                var cwd = Directory.GetCurrentDirectory();
                if (path.StartsWith(cwd)) path = path.Substring(cwd.Length + 1);
                ((WordlistManager)Caller).AddWordlist(new Wordlist(nameTextbox.Text, path, typeCombobox.Text, purposeTextbox.Text, subwordlists: SubWordlists.ToArray()));
            }
            ((MainDialog)Parent).Close();
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Wordlist files | *.txt";
            ofd.FilterIndex = 1;

            if (ofd.ShowDialog() == false) return;

            locationTextbox.Text = ofd.FileName;
            nameTextbox.Text = Path.GetFileNameWithoutExtension(ofd.FileName);

            // Set the recognized wordlist type
            try
            {
                var first = File.ReadLines(ofd.FileName).First();
                typeCombobox.Text = SB.Settings.Environment.RecognizeWordlistType(first);
            }
            catch { }
        }

        private void Image_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(locationTextbox.Text))
                {
                    MessageBox.Show("Please select main wordlist!", "NOTICE", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Sub Wordlist files | *.txt";
                ofd.FilterIndex = 1;
                if (ofd.ShowDialog() == false) return;

                locationsSubWordlistsTextbox.Text = ofd.FileName;

                // Set the recognized wordlist type
                try
                {
                    var first = File.ReadLines(ofd.FileName).First();
                    subTypeComboBox.Text = SB.Settings.Environment.RecognizeWordlistType(first);
                }
                catch { }
            }
            catch { }
        }

        private void selectSubButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(locationTextbox.Text))
                {
                    MessageBox.Show("Please select main wordlist!", "NOTICE", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (string.IsNullOrWhiteSpace(locationsSubWordlistsTextbox.Text))
                {
                    MessageBox.Show("Please select sub wordlist!", "NOTICE", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var path = locationsSubWordlistsTextbox.Text;
                var cwd = Directory.GetCurrentDirectory();
                if (path.StartsWith(cwd)) path = path.Substring(cwd.Length + 1);

                var sub = new SubWordlist(nameTextbox.Text, path, subTypeComboBox.Text, purposeTextbox.Text);
                SubWordlists.Add(sub);
                MessageBox.Show($"Added!\nTotal: {sub.Total}\nSubWordlist count: {SubWordlists.Count}");
            }
            catch { }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                selectSubButton.IsEnabled = subTypeLabel.IsEnabled = subTypeComboBox.IsEnabled = locationsSubWordlistsTextbox.IsEnabled =
 loadSubWordlistIco.IsEnabled = (sender as CheckBox).IsChecked.GetValueOrDefault();
            }
            catch { }
        }
    }
}
