using Extreme.Net;
using Microsoft.Win32;
using OpenBullet.Views;
using OpenBullet.Views.Main;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per DialogAddProxies.xaml
    /// </summary>
    public partial class DialogAddProxies : Page
    {
        public object Caller { get; set; }

        public DialogAddProxies(object caller)
        {
            InitializeComponent();
            Caller = caller;
            foreach (string i in Enum.GetNames(typeof(ProxyType)))
                if (i != "Chain") proxyTypeCombobox.Items.Add(i);
            proxyTypeCombobox.SelectedIndex = 0;
        }

        private void loadProxiesButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Proxy files | *.txt",
                FilterIndex = 1,
                Multiselect = true,
            };
            if (ofd.ShowDialog() == true)
            {
                for (var i = 0; i < ofd.FileNames.Length; i++)
                {
                    locationListBox.Items.Add(ofd.FileNames[i]);
                }
            }
        }

        private void acceptButton_Click(object sender, RoutedEventArgs e)
        {

            string[] fileNames;
            if (locationListBox.SelectedItems == null)
                fileNames = locationListBox.Items.OfType<string>().ToArray();
            else fileNames = locationListBox.Items.OfType<string>().ToArray();
            List<string> lines = new List<string>();

            try
            {
                switch (modeTabControl.SelectedIndex)
                {
                    // File
                    case 0:
                        if (fileNames.Length == 1)
                        {
                            SB.Logger.LogInfo(Components.ProxyManager, $"Trying to load from file {fileNames[0]}");
                            lines.AddRange(File.ReadAllLines(fileNames[0]));
                        }
                        else if (fileNames.Length > 1)
                        {
                            for (var i = 0; i < fileNames.Length; i++)
                            {
                                lines.AddRange(File.ReadAllLines(fileNames[i]));
                            }
                        }
                        else
                        {
                            SB.Logger.LogError(Components.ProxyManager, "No file specified!", true);
                            return;
                        }
                        break;

                    case 1:
                        if (proxiesBox.Text != string.Empty)
                        {
                            lines.AddRange(proxiesBox.Text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None));
                        }
                        else
                        {
                            SB.Logger.LogError(Components.ProxyManager, "The box is empty!", true);
                            return;
                        }
                        break;

                    case 2:
                        if (urlTextbox.Text != string.Empty)
                        {
                            HttpRequest request = new HttpRequest();
                            var response = request.Get(urlTextbox.Text).ToString();
                            lines.AddRange(response.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None));
                        }
                        else
                        {
                            SB.Logger.LogError(Components.ProxyManager, "No URL specified!", true);
                            return;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                SB.Logger.LogError(Components.ProxyManager, $"There was an error: {ex.Message}");
                return;
            }

            if (Caller.GetType() == typeof(ProxyManager))
            {
                ((ProxyManager)Caller).AddProxies(lines, (ProxyType)Enum.Parse(typeof(ProxyType), proxyTypeCombobox.Text), usernameTextbox.Text, passwordTextbox.Text);
            }
            ((MainDialog)Parent).Close();
        }

        private void FileMode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            fileMode.Foreground = Utils.GetBrush("ForegroundMenuSelected");
            pasteMode.Foreground = Utils.GetBrush("ForegroundMain");
            apiMode.Foreground = Utils.GetBrush("ForegroundMain");
            modeTabControl.SelectedIndex = 0;
        }

        private void PasteMode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            fileMode.Foreground = Utils.GetBrush("ForegroundMain");
            pasteMode.Foreground = Utils.GetBrush("ForegroundMenuSelected");
            apiMode.Foreground = Utils.GetBrush("ForegroundMain");
            modeTabControl.SelectedIndex = 1;
        }

        private void ApiMode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            fileMode.Foreground = Utils.GetBrush("ForegroundMain");
            pasteMode.Foreground = Utils.GetBrush("ForegroundMain");
            apiMode.Foreground = Utils.GetBrush("ForegroundMenuSelected");
            modeTabControl.SelectedIndex = 2;
        }

        //remove seleted path proxy
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try { locationListBox.Items.RemoveAt(locationListBox.SelectedIndex); } catch { }
        }

        private void Grid_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            try { locationListBox.Items.Clear(); } catch { }
        }

        private void locationListBox_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                    e.Effects = System.Windows.DragDropEffects.Copy;
            }
            catch { }
        }

        private void locationListBox_Drop(object sender, DragEventArgs e)
        {
            try
            {
                var locations = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                Task.Run(() =>
                {
                    for (var i = 0; i < locations.Length; i++)
                    {
                        try
                        {
                            var loc = locations[i];
                            if (loc.EndsWith(".txt") && File.Exists(loc))
                            {
                                Dispatcher.Invoke(() => locationListBox.Items.Add(loc));
                            }
                        }
                        catch { }
                    }
                });
            }
            catch { }
        }
    }
}
