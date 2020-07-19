using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using OpenBullet.Views.UserControls;
using RuriLib;
using RuriLib.Models;

namespace OpenBullet.Views.Main
{
    /// <summary>
    /// Interaction logic for Support.xaml
    /// </summary>
    public partial class Support : Page
    {
        public Support()
        {
            InitializeComponent();
        }

        Supporters[] supporters;

        BrushConverter brushConverter = new BrushConverter();

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (wrapPanel.Children.Count <= 0) { waitingLabel.Visibility = Visibility.Visible; }
                else { waitingLabel.Visibility = Visibility.Collapsed; }

                var data = string.Empty;
                using (Task.Run(() =>
                {
                    using (var wc = new WebClient())
                    {
                        data = wc.DownloadString("https://c-cracking.org/ApplicationVeri/SilverBullet/support.json");
                    }
                }).ContinueWith(_ =>
               {
                   Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate
                    {
                        waitingLabel.Visibility = Visibility.Collapsed;
                    });
                   supporters = IOManager.DeserializeObject<Supporters[]>(data);
                   SetSupporters();
               })) ;
            }
            catch (InvalidOperationException) { }
            catch (NullReferenceException) { }
            catch (Exception ex)
            {
                waitingLabel.Content = "ERROR";
            }
        }

        private async void SetSupporters()
        {
            for (var i = 0; i < supporters.Length; i++)
            {
                try
                {
                    await Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate
                      {
                          var uc = new UserControlSupport()
                          {
                              Width = 200,
                              Height = 200,
                              SupportName = supporters[i].Name,
                              Margin = new Thickness(0, 0, 8, 8),
                              BackgroundButton = (SolidColorBrush)brushConverter.ConvertFrom(supporters[i].Color),
                              Url = supporters[i].Address
                          };
                          uc.SetImage(new Uri(supporters[i].Logo));
                          if (!wrapPanel.Children.OfType<UserControlSupport>().Any(u => u.Url == uc.Url))
                          {
                              wrapPanel.Children.Add(uc);
                          }
                      });
                }
                catch { }
            }
        }
    }
}
