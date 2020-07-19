using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AngleSharp.Text;

namespace OpenBullet
{
    /// <summary>
    /// Logica di interazione per LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            InitializeComponent();
            ((Storyboard)FindResource("WaitStoryboard")).Begin();
        }

        bool? checkUpdate = null;
        MainWindow mainWindow;
        NotesWindow notesWindow;
        private bool _canClose;
        private bool showMainWindow = true;
        private const string discoard = "https://discord.gg/8jFtRs";
        CancellationTokenSource cancellationToken;
        Task task;

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckBox_Click(null, null);
            try
            {
                task = Task.Run(() =>
                 {
                     CheckForUpdate();
                 }).ContinueWith(_ =>
                 {
                 }, (cancellationToken = new CancellationTokenSource()).Token);
            }
            catch
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                Close();
            }
        }

        private void CheckForUpdate()
        {
            Task.Delay(200);
            if (!checkUpdate.GetValueOrDefault())
            {
                Task.Delay(2000);
                showMainWindow = true;
                Window_Closing(null, null);
                return;
            }
            cancellationToken.Token.ThrowIfCancellationRequested();
            var result = CheckUpdate.Run(SB.updateUrl);
            showMainWindow = !result.Update;
            if (result.Update)
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate
                {
                    var notesWindow = new NotesWindow()
                    {
                        MainWindow = mainWindow,
                        SBUrl = result.Url,
                    };
                    notesWindow.titleLabel.Content += " " + result.Version;
                    for (var i = 0; i < result.Notes.Count; i++)
                    {
                        notesWindow.richTextBox.AppendText(result.Notes[i].Note + "\n");
                    }
                    notesWindow.Show();
                    Close();
                });
            }
            else
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate
                {
                    if (notesWindow == null)
                    {
                        Hide();
                        mainWindow = new MainWindow();
                        mainWindow.Show();
                        Close();
                    }
                });
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (checkUpdate == null)
                {
                    if (!File.Exists("Settings\\Update.txt")) { checkUpdate = true; }
                    else
                    {
                        checkUpdate = File.ReadAllText("Settings\\Update.txt").ToBoolean();
                        checkBoxUpdate.IsChecked = checkUpdate;
                        return;
                    }
                }
                try
                {
                    checkUpdate = checkBoxUpdate.IsChecked.GetValueOrDefault();
                }
                catch (NullReferenceException) { checkUpdate = true; }
                if (File.Exists("Settings\\Update.txt")) using (File.CreateText("Settings\\Update.txt")) ;
                File.WriteAllText("Settings\\Update.txt", checkUpdate.ToString());
            }
            catch { }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (mainWindow == null && notesWindow == null && showMainWindow)
                {
                    Hide();
                    mainWindow = new MainWindow();
                    mainWindow.Show();
                    showMainWindow = false;
                    if (e == null) Close();
                }
            }
            catch (InvalidOperationException)
            {
                try
                {
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate
                    {
                        mainWindow = new MainWindow();
                        mainWindow.Show();
                        showMainWindow = false;
                        if (e == null) Close();
                    });
                }
                catch { }
            }
            catch { }
        }

        private void dragPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void titleLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void quitPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_canClose)
            {
                Environment.Exit(0);
            }
        }

        private void quitPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            quitPanel.Background = new SolidColorBrush(Colors.Transparent);
            _canClose = false;
        }

        private void quitPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _canClose = true;
        }

        private void quitPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            quitPanel.Background = new SolidColorBrush(Colors.DarkRed);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                try { task.Dispose(); } catch { }
                try { cancellationToken.Cancel(true); } catch { }
                try { cancellationToken.Dispose(); } catch { }

                mainWindow = new MainWindow();

                if (notesWindow != null)
                    notesWindow.MainWindow = mainWindow;

                mainWindow.Show();
                Close();
            }
            catch { }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(discoard);
            }
            catch { }
        }
    }
}
