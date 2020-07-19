using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using IronPython.Runtime;
using Microsoft.Win32;
using RuriLib.Models;

namespace OpenBullet.Views.Main.Tools
{
    /// <summary>
    /// Interaction logic for WordlistTools.xaml
    /// </summary>
    public partial class WordlistTools : Page
    {
        public WordlistTools()
        {
            InitializeComponent();
        }

        private string recognizeWordlistType, wordlistName;
        private List<string> wordList = new List<string>();
        private List<string> remDupWordlist = new List<string>();

        //load
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Wordlist files | *.txt";
                ofd.FilterIndex = 1;

                if (ofd.ShowDialog() == false) return;

                locationTextBox.Text = ofd.FileName;
                wordlistName = System.IO.Path.GetFileNameWithoutExtension(ofd.FileName);

                // Set the recognized wordlist type
                try
                {
                    wordList.AddRange(File.ReadLines(ofd.FileName));
                    loadedForDup.Text = $"Loaded: {wordList.Count}";
                    var first = wordList.First();
                    recognizeWordlistType = SB.Settings.Environment.RecognizeWordlistType(first);
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR");
            }
        }

        //remove dup
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                remDupWordlist = wordList.Distinct().ToList();
                removedDupTextBlock.Text = $"Removed: {wordList.Count - remDupWordlist.Count}";
            }
            catch (Exception ex) { }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                wordList.Clear();
                remDupWordlist.Clear();
                loadedForDup.Text = $"Loaded: {wordList.Count}";
                removedDupTextBlock.Text = "Removed: 0";
            }
            catch { }
        }

        //save
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SaveFileDialog()
                {
                    Filter = "Wordlist file | *.txt"
                };
                if (dialog.ShowDialog() == true)
                {
                    File.WriteAllLines(dialog.FileName, remDupWordlist.ToArray());
                }
            }
            catch { }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {

        }

        //send into wordlist
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (remDupWordlist.Count == 0 || wordList.Count == 0) return;

                if (!Directory.Exists("Wordlists")) Directory.CreateDirectory("Wordlists");
                var path = $"Wordlists\\{wordlistName}.txt";

                if (File.Exists(path)) MessageBox.Show($"Wordlist is exists!\nSaved to {path}", "NOTICE");

                int count = 1;
                while (File.Exists(path))
                {
                    var tempFileName = string.Format("{0}({1})", wordlistName, count++);
                    path = System.IO.Path.Combine(path, tempFileName + ".txt");
                }

                File.WriteAllLines(path, remDupWordlist.ToArray());
                var cwd = Directory.GetCurrentDirectory();
                if (path.StartsWith(cwd)) path = path.Substring(cwd.Length + 1);
                SB.WordlistManager.Add(
                new Wordlist(wordlistName, path, recognizeWordlistType, string.Empty)
                );

                MessageBox.Show("Sended into wordlist successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR");
            }
        }
    }
}
