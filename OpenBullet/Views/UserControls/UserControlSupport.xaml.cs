using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenBullet.Views.UserControls
{
    /// <summary>
    /// Interaction logic for UserControlSupport.xaml
    /// </summary>
    public partial class UserControlSupport : UserControl, INotifyPropertyChanged
    {
        public UserControlSupport()
        {
            InitializeComponent();
            DataContext = this;
        }

        private bool _mouseDown;

        public event RoutedEventHandler Click;

        public event PropertyChangedEventHandler PropertyChanged;

        private Stretch _stretch = Stretch.Fill;
        public Stretch Stretch
        {
            get => _stretch;
            set
            {
                if (Equals(_stretch, value))
                    return;
                _stretch = value;
                RaisePropertyChanged();
            }
        }

        private string supportName;
        public string SupportName { get { return supportName; } set { supportName = value; RaisePropertyChanged(); } }

        private SolidColorBrush backgroundButton;
        public SolidColorBrush BackgroundButton { get { return backgroundButton; } set { backgroundButton = value; RaisePropertyChanged(); } }

        public string Url { get; set; }

        public void SetImage(Uri imgSource)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = imgSource;
            bitmap.EndInit();
            imageBrush.ImageSource = bitmap;
        }

        private void Border_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_mouseDown)
            {
                Click?.Invoke(this, e);
            }
            _mouseDown = false;
        }

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _mouseDown = true;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            _mouseDown = false;
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(Url);
            }
            catch { }
        }
    }
}
