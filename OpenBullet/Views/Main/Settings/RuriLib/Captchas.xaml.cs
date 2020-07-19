using CaptchaSharp.Exceptions;
using RuriLib;
using RuriLib.Enums;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet.Views.Main.Settings.RL
{
    /// <summary>
    /// Logica di interazione per Captchas.xaml
    /// </summary>
    public partial class Captchas : Page
    {
        public Captchas()
        {
            InitializeComponent();
            DataContext = SB.Settings.RLSettings.Captchas;

            foreach (string i in Enum.GetNames(typeof(CaptchaServiceType)))
                currentServiceCombobox.Items.Add(i);

            currentServiceCombobox.SelectedIndex = (int)SB.Settings.RLSettings.Captchas.CurrentService;
        }

        private void currentServiceCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SB.Settings.RLSettings.Captchas.CurrentService = (CaptchaServiceType)currentServiceCombobox.SelectedIndex;

            var dict = new Dictionary<CaptchaServiceType, int>
            {
                { CaptchaServiceType.TwoCaptcha,        0 },
                { CaptchaServiceType.AntiCaptcha,       1 },
                { CaptchaServiceType.CustomTwoCaptcha,  2 },
                { CaptchaServiceType.CapMonster,        2 },
                { CaptchaServiceType.DeathByCaptcha,    3 },
                { CaptchaServiceType.DeCaptcher,        4 },
                { CaptchaServiceType.ImageTyperz,       5 },
                { CaptchaServiceType.AzCaptcha,         6 },
                { CaptchaServiceType.CaptchasIO,        7 },
                { CaptchaServiceType.RuCaptcha,         8 },
                { CaptchaServiceType.SolveCaptcha,      9 },
                { CaptchaServiceType.SolveRecaptcha,    10 },
                { CaptchaServiceType.TrueCaptcha,       11 }
            };

            captchaServiceTabControl.SelectedIndex = dict[SB.Settings.RLSettings.Captchas.CurrentService];
        }

        private async void checkBalanceButton_Click(object sender, RoutedEventArgs e)
        {
            // Save
            IOManager.SaveSettings(SB.rlSettingsFile, SB.Settings.RLSettings);

            try
            {
                var balance = await RuriLib.Functions.Captchas.Captchas.GetService(SB.Settings.RLSettings.Captchas)
                    .GetBalanceAsync();

                balanceLabel.Content = balance;
                balanceLabel.Foreground = balance > 0 ? Utils.GetBrush("ForegroundGood") : Utils.GetBrush("ForegroundBad");
            }
            catch (BadAuthenticationException) { 
                balanceLabel.Content = "WRONG TOKEN / CREDENTIALS";
                balanceLabel.Foreground = Utils.GetBrush("ForegroundBad");
            }
            catch
            {
                balanceLabel.Content = "AN ERROR OCCURRED";
                balanceLabel.Foreground = Utils.GetBrush("ForegroundBad");
            }
        }
    }
}
