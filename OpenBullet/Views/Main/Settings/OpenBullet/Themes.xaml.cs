using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xceed.Wpf.Toolkit;

namespace OpenBullet.Views.Main.Settings.OpenBullet
{

    /// <summary>
    /// Logica di interazione per Themes.xaml
    /// </summary>
    public partial class Themes : Page
    {
        public Themes()
        {
            InitializeComponent();
            DataContext = SB.OBSettings.Themes;
            
            // Load all the saved colors
            SetColors();
            SetColorPreviews();
            SetImagePreviews();
            SB.MainWindow.AllowsTransparency = SB.OBSettings.Themes.AllowTransparency;
        }

        public void SetColors()
        {
            SetAppColor("BackgroundMain", SB.OBSettings.Themes.BackgroundMain);
            SetAppColor("BackgroundSecondary", SB.OBSettings.Themes.BackgroundSecondary);
            SetAppColor("ForegroundMain", SB.OBSettings.Themes.ForegroundMain);
            SetAppColor("ForegroundGood", SB.OBSettings.Themes.ForegroundGood);
            SetAppColor("ForegroundBad", SB.OBSettings.Themes.ForegroundBad);
            SetAppColor("ForegroundCustom", SB.OBSettings.Themes.ForegroundCustom);
            SetAppColor("ForegroundRetry", SB.OBSettings.Themes.ForegroundRetry);
            SetAppColor("ForegroundToCheck", SB.OBSettings.Themes.ForegroundToCheck);
            SetAppColor("ForegroundMenuSelected", SB.OBSettings.Themes.ForegroundMenuSelected);
            SetAppColor(nameof(SB.OBSettings.Themes.ForegroundOcrRate), SB.OBSettings.Themes.ForegroundOcrRate);

            // This sets the background for the mainwindow (alternatively solid or image)
            SB.MainWindow.SetStyle();
        }

        private void SetColorPreviews()
        {
            BackgroundMain.SelectedColor = GetAppColor("BackgroundMain");
            BackgroundSecondary.SelectedColor = GetAppColor("BackgroundSecondary");
            ForegroundMain.SelectedColor = GetAppColor("ForegroundMain");
            ForegroundGood.SelectedColor = GetAppColor("ForegroundGood");
            ForegroundBad.SelectedColor = GetAppColor("ForegroundBad");
            ForegroundCustom.SelectedColor = GetAppColor("ForegroundCustom");
            ForegroundRetry.SelectedColor = GetAppColor("ForegroundRetry");
            ForegroundToCheck.SelectedColor = GetAppColor("ForegroundToCheck");
            ForegroundOcrRate.SelectedColor = GetAppColor("ForegroundOcrRate");
            ForegroundMenuSelected.SelectedColor = GetAppColor("ForegroundMenuSelected");
        }

        public void SetAppColor(string resourceName, string color)
        {
            App.Current.Resources[resourceName] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }

        public Color GetAppColor(string resourceName)
        {
            return ((SolidColorBrush)App.Current.Resources[resourceName]).Color;
        }

        private void SetImagePreviews()
        {
            try
            {
                backgroundImagePreview.Source = GetImageBrush(SB.OBSettings.Themes.BackgroundImage);
                backgroundLogoPreview.Source = GetImageBrush(SB.OBSettings.Themes.BackgroundLogo);
            }
            catch { }
        }

        private BitmapImage GetImageBrush(string file)
        {
            try
            {
                if (File.Exists(file))
                    return new BitmapImage(new Uri(file));
                else
                    return new BitmapImage(new Uri(@"pack://application:,,,/"
                        + Assembly.GetExecutingAssembly().GetName().Name
                        + ";component/"
                        + "Images/Themes/empty.png", UriKind.Absolute));
            }
            catch { return null; }
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            SB.OBSettings.Themes.BackgroundMain = "#222";
            SB.OBSettings.Themes.BackgroundSecondary = "#111";
            SB.OBSettings.Themes.ForegroundMain = "#dcdcdc";
            SB.OBSettings.Themes.ForegroundGood = "#adff2f";
            SB.OBSettings.Themes.ForegroundBad = "#ff6347";
            SB.OBSettings.Themes.ForegroundCustom = "#ff8c00";
            SB.OBSettings.Themes.ForegroundRetry = "#ffff00";
            SB.OBSettings.Themes.ForegroundToCheck = "#7fffd4";
            SB.OBSettings.Themes.ForegroundMenuSelected = "#1e90ff";
            SB.OBSettings.Themes.ForegroundOcrRate = "#ff8cc6ff";

            SetColors();
            SetColorPreviews();
            SetImagePreviews();
        }

        private void loadBackgroundImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "BMP|*.bmp|GIF|*.gif|JPG|*.jpg;*.jpeg|PNG|*.png|TIFF|*.tif;*.tiff|"
       + "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff";
            ofd.FilterIndex = 4;
            ofd.ShowDialog();
            SB.OBSettings.Themes.BackgroundImage = ofd.FileName;

            SetColors();
            SetImagePreviews();
        }

        private void loadBackgroundLogo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "BMP|*.bmp|GIF|*.gif|JPG|*.jpg;*.jpeg|PNG|*.png|TIFF|*.tif;*.tiff|"
       + "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff";
            ofd.FilterIndex = 4;
            ofd.ShowDialog();
            SB.OBSettings.Themes.BackgroundLogo = ofd.FileName;

            SetColors();
            SetImagePreviews();
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
                SB.OBSettings.Themes.GetType().GetProperty(((ColorPicker)sender).Name.ToString()).SetValue(SB.OBSettings.Themes, ColorToHtml(e.NewValue.Value), null);

            SetColors();
        }

        private string ColorToHtml(Color color)
        {
            return $"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}";
        }

        private void useImagesCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            SetColors();
        }

        private void useImagesCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            SetColors();
        }

        private void backgroundImageOpacityUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SB.MainWindow.SetStyle();
        }
    }
}
