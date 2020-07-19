using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;

namespace OpenBullet.Views.Main.Tools
{
    /// <summary>
    /// Interaction logic for TessDataDownloads.xaml
    /// </summary>
    public partial class TessDataDownloads : Page
    {

        private WebClient loadSite = new WebClient();
        public string url = @"https://github.com/tesseract-ocr/tessdata/tree/3.04.00";
        public string siteResponse = null;
        public string language = null;
        private Regex lang = new Regex("title=\"(.*).traineddata\" id=\"");
        Task taskDl;

        public TessDataDownloads()
        {
            InitializeComponent();
        }

        private void LoadSite_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                DispatcherInvoke(() => progressBar.Value = e.ProgressPercentage);
            }
            catch { }
        }

        private void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            LogsText.Clear();
            progressBar.Value = 0;

            int i = 0;
            LogsText.Text += "Downloading tessdata files..." + Environment.NewLine;

            var items = DownloadList.Items;
            try { taskDl?.Dispose(); } catch { }
            taskDl = Task.Run(async () =>
            {
                foreach (string item in items)
                {
                    if (!Directory.Exists(@".\tessdata"))
                    {
                        Directory.CreateDirectory(@".\tessdata");
                    }
                    i++;
                    if (File.Exists($@".\tessdata\{item}.traineddata"))
                    {
                        DispatcherInvoke(() => LogsText.Text += $"\n{item}.traineddata is exists!\n" + Environment.NewLine);
                        continue;
                    }
                    DispatcherInvoke(() => LogsText.Text += String.Format("{0}/{1} | Downloading: {2}..", i, DownloadList.Items.Count, item));
                    try
                    {
                        await DownloadLanguage(i, item.ToString());
                    }
                    catch (Exception ex)
                    {
                        //System.Windows.Forms.MessageBox.Show("Please create a folder named 'tessdata' in your Openbullet directory", "FOLDER MISSING", MessageBoxButtons.OK);
                        DispatcherInvoke(() => LogsText.Text += $"\n[{nameof(Exception).ToUpper()}] {ex}");
                    }
                }
            }).ContinueWith(_ =>
            {
                DispatcherInvoke(() => LogsText.Text += "Your chosen languages have been downloaded ");
            });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LogsText.Clear();
            LanguageList.Items.Clear();

            LogsText.Text = "Downloading language list...\n";
            Task.Run(() => siteResponse = loadSite.DownloadString(url))
                .ContinueWith(c =>
                {
                    foreach (Match line in lang.Matches(siteResponse))
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate
                        {
                            LanguageList.Items.Add(line.Groups[1].Value);
                        });
                    }
                    DispatcherInvoke(() => LogsText.Text += "Downloading language list Finished!" + Environment.NewLine);
                });
        }

        private void DispatcherInvoke(Action action)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate
            {
                action?.Invoke();
            });
        }

        public async Task DownloadLanguage(int i, string language)
        {
            language += ".traineddata";
            loadSite.DownloadProgressChanged += LoadSite_DownloadProgressChanged;
            await loadSite.DownloadFileTaskAsync(new Uri("https://github.com/tesseract-ocr/tessdata/raw/3.04.00/" + language), AppDomain.CurrentDomain.BaseDirectory + "/tessdata/" + language);
            DispatcherInvoke(() => LogsText.Text += "\t| Finished!" + Environment.NewLine);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DownloadList.Items.Add(LanguageList.SelectedItem);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DownloadList.Items.Remove(DownloadList.SelectedItem);
        }

        private void Button_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show("This button will add ALL languages to the download list", "ALERT", MessageBoxButtons.OKCancel) == DialogResult.OK)
                foreach (string item in LanguageList.Items)
                    DownloadList.Items.Add(item);
        }

        private void Button_MouseRightButtonDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show("This button will remove ALL languages to the download list", "ALERT", MessageBoxButtons.OKCancel) == DialogResult.OK)
                DownloadList.Items.Clear();
        }
    }
}
