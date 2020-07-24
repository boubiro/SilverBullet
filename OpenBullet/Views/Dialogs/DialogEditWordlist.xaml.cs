using System.Windows.Controls;
using System.Windows.Forms;
using RuriLib.Models;

namespace OpenBullet.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for DialogEditWordlist.xaml
    /// </summary>
    public partial class DialogEditWordlist : Page
    {
        public Wordlist WordList { get; private set; }
        public DialogResult DialogResult { get; private set; }

        public DialogEditWordlist(Wordlist wordlist)
        {
            WordList = wordlist;
            InitializeComponent();

            foreach (string i in SB.Settings.Environment.GetWordlistTypeNames())
                wordlistType.Items.Add(i);
        }

        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                wordlistName.Text = WordList.Name;
                wordlistPath.Text = WordList.Path;
                wordlistPurpose.Text = WordList.Purpose;
                wordlistType.SelectedItem = WordList.Type;
            }
            catch { }
        }

        //cancel
        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                DialogResult = DialogResult.Cancel;

                ((MainDialog)Parent).Close();
            }
            catch { }
        }

        //edit
        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                DialogResult = DialogResult.OK;

                WordList.Name = wordlistName.Text;
                WordList.Path = wordlistPath.Text;
                WordList.Purpose = wordlistPurpose.Text;
                WordList.Type = wordlistType.SelectedItem.ToString();

                ((MainDialog)Parent).Close();
            }
            catch { }
        }

        private void Page_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    Button_Click_1(null, null);
                }
            }
            catch { }
        }
    }
}
