using OpenBullet.Views.Main.Runner;
using RuriLib;
using RuriLib.Functions.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Animation;

namespace OpenBullet.Views.StackerBlocks
{
    /// <summary>
    /// Logica di interazione per PageBlockRequest.xaml
    /// </summary>
    public partial class PageBlockRequest : Page
    {
        BlockRequest vm;
        Task analyzeTask;

        public PageBlockRequest(BlockRequest block)
        {
            InitializeComponent();
            vm = block;
            DataContext = vm;

            foreach (string i in Enum.GetNames(typeof(Extreme.Net.HttpMethod)))
                methodCombobox.Items.Add(i);

            methodCombobox.SelectedIndex = (int)vm.Method;

            foreach (string t in Enum.GetNames(typeof(RequestType)))
                requestTypeCombobox.Items.Add(t);

            requestTypeCombobox.SelectedIndex = (int)vm.RequestType;

            foreach (string t in Enum.GetNames(typeof(ResponseType)))
                responseTypeCombobox.Items.Add(t);

            responseTypeCombobox.SelectedIndex = (int)vm.ResponseType;

            customCookiesRTB.AppendText(vm.GetCustomCookies());
            customHeadersRTB.AppendText(vm.GetCustomHeaders());
            multipartContentsRTB.AppendText(vm.GetMultipartContents());

            List<string> commonContentTypes = new List<string>()
            {
                "application/x-www-form-urlencoded",
                "application/json",
                "text/plain"
            };

            foreach (var c in commonContentTypes)
                contentTypeCombobox.Items.Add(c);

            foreach (var s in Enum.GetNames(typeof(SecurityProtocol)))
                securityProtocolCombobox.Items.Add(s);

            securityProtocolCombobox.SelectedIndex = (int)vm.SecurityProtocol;
        }

        private void methodCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.Method = (Extreme.Net.HttpMethod)methodCombobox.SelectedIndex;
        }

        private void requestTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.RequestType = (RequestType)requestTypeCombobox.SelectedIndex;

            switch (vm.RequestType)
            {
                default:
                    requestTypeTabControl.SelectedIndex = 1;
                    break;

                case RequestType.Standard:
                    requestTypeTabControl.SelectedIndex = 2;
                    break;

                case RequestType.Multipart:
                    requestTypeTabControl.SelectedIndex = 3;
                    break;

                case RequestType.Raw:
                    requestTypeTabControl.SelectedIndex = 4;
                    break;
            }
        }

        private void responseTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.ResponseType = (ResponseType)responseTypeCombobox.SelectedIndex;

            switch (vm.ResponseType)
            {
                default:
                    responseTypeTabControl.SelectedIndex = 0;
                    break;

                case ResponseType.File:
                    responseTypeTabControl.SelectedIndex = 1;
                    break;

                case ResponseType.Base64String:
                    responseTypeTabControl.SelectedIndex = 2;
                    break;
            }
        }

        private void customCookiesRTB_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            vm.SetCustomCookies(customCookiesRTB.Lines());
        }

        private void customHeadersRTB_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            vm.SetCustomHeaders(customHeadersRTB.Lines());
        }

        private void multipartContentsRTB_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            vm.SetMultipartContents(multipartContentsRTB.Lines());
        }

        private void securityProtocolCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.SecurityProtocol = (SecurityProtocol)((ComboBox)e.OriginalSource).SelectedIndex;
        }

        private void AnalyzeLoginPage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var analyzeRenderTransform = analyzeIcon.RenderTransform;
            var waitForAnalyze = (Storyboard)FindResource("WaitForAnalyze");
            try
            {
                waitForAnalyze.Begin();
                Tuple<string, string, string> tuple = null;
                try { analyzeTask?.Dispose(); } catch { }
                analyzeTask = Task.Run(() => tuple = vm.Analyze())
                    .ContinueWith(_ =>
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(tuple.Item1) ||
                            string.IsNullOrWhiteSpace(tuple.Item2))
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    waitForAnalyze.Stop();
                                    analyzeIcon.RenderTransform = analyzeRenderTransform;
                                });
                                SB.Logger.Log("URL or POSTDATA not found!", LogLevel.Error, true);
                                return;
                            }
                            vm.Url = tuple.Item1;
                            vm.PostData = tuple.Item2;
                            /*if (!string.IsNullOrEmpty(tuple.Item3))
                            {
                                try
                                {
                                    vm.CustomHeaders.Remove(vm.CustomHeaders.Keys.FirstOrDefault(k => k == "Cookie"));
                                }
                                catch { }
                                try
                                {
                                    vm.CustomHeaders.Add("Cookie", tuple.Item3);
                                    Dispatcher.Invoke(() =>
                                    {
                                        var block = customHeadersRTB.Document.Blocks.FirstOrDefault(b => new TextRange(b.ContentStart, b.ContentEnd).Text.Split(':')[0].Trim().ToLower() == "cookie");
                                        if (block != null) { customHeadersRTB.Document.Blocks.Remove(block); }
                                        customHeadersRTB.AppendText("Cookie: " + tuple.Item3 + "\n");
                                    });
                                }
                                catch { }
                            }*/
                            Dispatcher.Invoke(() =>
                            {
                                waitForAnalyze.Stop();
                                analyzeIcon.RenderTransform = analyzeRenderTransform;
                            });
                        }
                        catch (Exception ex)
                        {
                            waitForAnalyze.Stop();
                            Dispatcher.Invoke(() => analyzeIcon.RenderTransform = analyzeRenderTransform);
                            Dispatcher.Invoke(() => SB.Logger.Log(ex.Message, LogLevel.Error, true));
                        }
                    });
            }
            catch (Exception ex)
            {
                waitForAnalyze.Stop();
                analyzeIcon.RenderTransform = analyzeRenderTransform;
                SB.Logger.Log(ex.Message, LogLevel.Error, true);
            }
        }
    }
}
