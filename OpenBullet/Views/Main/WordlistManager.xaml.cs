using OpenBullet.ViewModels;
using OpenBullet.Views.Dialogs;
using RuriLib.Models;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace OpenBullet.Views.Main
{
    /// <summary>
    /// Logica di interazione per WordlistManager.xaml
    /// </summary>
    public partial class WordlistManager : Page
    {
        private WordlistManagerViewModel vm = null;
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        public WordlistManager()
        {
            vm = SB.WordlistManager;
            DataContext = vm;

            InitializeComponent();
        }

        public void AddWordlist(Wordlist wordlist)
        {
            try
            {
                vm.Add(wordlist);
            }
            catch (Exception e)
            {
                SB.Logger.LogError(Components.WordlistManager, e.Message);
            }
        }

        #region Buttons
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            (new MainDialog(new DialogAddWordlist(this), "Add Wordlist")).ShowDialog();
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            SB.Logger.LogInfo(Components.WordlistManager, $"Deleting {wordlistListView.SelectedItems.Count} references from the DB");
            foreach (var wordlist in wordlistListView.SelectedItems.Cast<Wordlist>().ToList())
            {
                vm.Remove(wordlist);
            }
            SB.Logger.LogInfo(Components.WordlistManager, "Successfully deleted the wordlist references from the DB");
        }

        private void deleteAllButton_Click(object sender, RoutedEventArgs e)
        {
            SB.Logger.LogWarning(Components.WordlistManager, "Purge selected, prompting warning");

            if (MessageBox.Show("This will purge the WHOLE Wordlists DB, are you sure you want to continue?", "WARNING", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                SB.Logger.LogInfo(Components.WordlistManager, "Purge initiated");
                vm.RemoveAll();
                SB.Logger.LogInfo(Components.WordlistManager, "Purge finished");
            }
            else { SB.Logger.LogInfo(Components.WordlistManager, "Purge dismissed"); }
        }

        private void deleteNotFoundWordlistsButton_Click(object sender, RoutedEventArgs e)
        {
            SB.Logger.LogWarning(Components.WordlistManager, "Deleting wordlists with missing files.");
            vm.DeleteNotFound();
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            vm.SearchString = filterTextbox.Text;
        }

        private void filterTextbox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                searchButton_Click(this, null);
        }
        #endregion

        #region ListView
        private void listViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                wordlistListView.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            wordlistListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void wordlistListViewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var file in files.Where(x => x.EndsWith(".txt")).ToArray())
                {
                    try
                    {
                        // Build the wordlist object
                        var path = file;
                        var cwd = Directory.GetCurrentDirectory();
                        if (path.StartsWith(cwd)) path = path.Substring(cwd.Length + 1);
                        var wordlist = new Wordlist(Path.GetFileNameWithoutExtension(file), path, SB.Settings.Environment.WordlistTypes.First().Name, "");

                        // Get the first line
                        var first = File.ReadLines(wordlist.Path).First(l => !string.IsNullOrWhiteSpace(l));

                        // Set the correct wordlist type
                        wordlist.Type = SB.Settings.Environment.RecognizeWordlistType(first);

                        // Add the wordlist to the manager
                        AddWordlist(wordlist);
                    }
                    catch { }
                }
            }
        }
        #endregion

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (wordlistListView.SelectedIndex == -1 ||
                    wordlistListView.SelectedItem == null) return;

                var dialogEditWordlist = new DialogEditWordlist((Wordlist)wordlistListView.SelectedItem);
                new MainDialog(dialogEditWordlist, "Edit Wordlist").ShowDialog();
                if (dialogEditWordlist.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    vm.Update(dialogEditWordlist.WordList);
                    vm.RefreshList();
                }
            }
            catch (Exception ex)
            {
                SB.Logger.Log(ex.Message, RuriLib.LogLevel.Error, true);
            }
        }
    }
}
