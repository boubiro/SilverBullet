using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

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

        //load
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Wordlist files | *.txt";
                ofd.FilterIndex = 1;

                if (ofd.ShowDialog() == false) return;

                //    locationTextBox.Text = ofd.FileName;
                wordlistName = System.IO.Path.GetFileNameWithoutExtension(ofd.FileName);

                // Set the recognized wordlist type
                try
                {
                    locTextBox.Text = ofd.FileName;
                    wordList.AddRange(File.ReadLines(ofd.FileName));
                    loaded.Text = wordList.Count.ToString();
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
                var newList = wordList.Distinct().ToList();
                removedDup.Text = (wordList.Count - newList.Count).ToString();
                SaveFile("Remove Duplicate", newList.ToArray());
            }
            catch (Exception ex) { }
        }

        //split
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                var newList = wordList.Select(w =>
                {
                    try
                    {
                        return w.Split(new[] { splitTextBox.Text }, StringSplitOptions.RemoveEmptyEntries)
                           [splitIndex.Value.GetValueOrDefault() - 1];
                    }
                    catch { return w; }
                }).Where(w => !w.Contains(splitTextBox.Text))
                .ToList();
                splited.Text = newList.Count.ToString();
                SaveFile("Splitter", newList.ToArray());
            }
            catch (Exception ex)
            {
                SB.Logger.Log(ex.Message, RuriLib.LogLevel.Error, true);
            }
        }

        //change separator
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                var reader = new StreamReader(locTextBox.Text);
                var content = reader.ReadToEnd();
                reader.Close();

                content = Regex.Replace(content, currentSepTextBox.Text.Trim(), newSepTextBox.Text.Trim());
                changed.Text = wordList.Count(w => w.Contains(currentSepTextBox.Text)).ToString();

                var saveDialog = new SaveFileDialog()
                {
                    Title = "Change Separator",
                    Filter = "Text File|*.txt"
                };
                if (saveDialog.ShowDialog() == true)
                {
                    StreamWriter writer = new StreamWriter(saveDialog.FileName);
                    writer.Write(content);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                SB.Logger.Log(ex.Message, RuriLib.LogLevel.Error, true);
            }
        }

        private void SaveFile(string title, string[] contents)
        {
            var saveDialog = new SaveFileDialog()
            {
                Title = title,
                Filter = "Text File|*.txt"
            };
            if (saveDialog.ShowDialog() == true)
            {
                File.WriteAllLines(saveDialog.FileName, contents);
            }
        }
    }
}
