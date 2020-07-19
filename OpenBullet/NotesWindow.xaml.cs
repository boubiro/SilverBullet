using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenBullet
{
    /// <summary>
    /// Interaction logic for NotesWindow.xaml
    /// </summary>
    public partial class NotesWindow : Window
    {
        public NotesWindow()
        {
            InitializeComponent();
        }

        private bool _canClose;

        public string SBUrl { get; set; }

        public bool DontShowMainWindow { get; set; }

        public MainWindow MainWindow;

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
                _canClose = false;
                if (MainWindow == null)
                {
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                }
                Close();
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
                Clipboard.SetText(SBUrl);
            }
            catch { }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MainWindow == null && !DontShowMainWindow)
                {
                    MainWindow = new MainWindow();
                }
                MainWindow.Show();
                Close();
            }
            catch { }
        }
    }
}
