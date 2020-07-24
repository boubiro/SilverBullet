using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Extreme.Net;
using MahApps.Metro.IconPacks;
using RuriLib;
using RuriLib.Models;
using RuriLib.Runner;

namespace OpenBullet.Views.Main.Runner
{
    /// <summary>
    /// Logica di interazione per Runner.xaml
    /// </summary>
    public partial class Runner : Page
    {
        private RunnerViewModel vm;
        private SoundPlayer hitPlayer;
        private SoundPlayer reloadPlayer;

        private bool soundLock = false;

        public Runner(RunnerViewModel vm)
        {
            this.vm = vm;
            DataContext = vm;

            InitializeComponent();

            vm.MessageArrived += LogRunnerData;
            vm.WorkerStatusChanged += LogWorkerStatus;
            vm.WorkerStatusChanged += ProcessStatusChange;
            vm.FoundHit += PlayHitSound;
            vm.FoundHit += RegisterHit;
            vm.ReloadProxies += PlayReloadSound;
            vm.ReloadProxies += LoadProxiesFromManager;
            vm.DispatchAction += ExecuteAction;
            vm.SaveProgress += SaveProgressToDB;
            vm.AskCustomInputs += InitCustomInputs;

            if (SB.OBSettings.General.ChangeRunnerInterface)
            {
                SB.Logger.LogInfo(Components.About, "Changed the Runner interface");
                Grid.SetColumn(rightGrid, 0);
                Grid.SetRow(rightGrid, 2);
                Grid.SetColumn(bottomLeftGrid, 2);
                Grid.SetRow(bottomLeftGrid, 0);
            }

            logBox.AppendText("", Colors.White);
            logBox.AppendText("Runner initialized succesfully!" + Environment.NewLine, Utils.GetColor("ForegroundMain"));
        }

        #region Events
        private void LogRunnerData(IRunnerMessaging sender, LogLevel level, string message, bool prompt, int timeout)
        {
            SB.Logger.Log(Components.Runner, level, message, prompt, timeout);
        }

        private void LogWorkerStatus(IRunnerMessaging sender)
        {
            var runner = sender as RunnerViewModel;
            switch (runner.Master.Status)
            {
                case WorkerStatus.Running:
                    logBox.AppendText($"Started Running Config {runner.ConfigName} with Wordlist {runner.WordlistName} at {DateTime.Now}.{Environment.NewLine}",
                        Utils.GetColor("ForegroundGood"));
                    break;

                case WorkerStatus.Stopping:
                    logBox.AppendText($"Sent Abort Request at {DateTime.Now}.{Environment.NewLine}",
                        Utils.GetColor("ForegroundCustom"));
                    break;

                case WorkerStatus.Idle:
                    logBox.AppendText($"Aborted Runner at {DateTime.Now}.{Environment.NewLine}",
                        Utils.GetColor("ForegroundBad"));
                    break;
            }
        }

        private void ExecuteAction(IRunnerMessaging sender, Action action)
        {
            App.Current.Dispatcher.Invoke(action);
        }

        private void RegisterHit(IRunnerMessaging sender, Hit hit)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (vm.Config != null && vm.Config.Settings.SaveHitsToTextFile)
                {
                    try
                    {
                        SB.Logger.LogInfo(Components.Runner, $"Adding {hit.Type} hit " + hit.Data + " to the text file");
                        var folderName = Path.Combine("Hits", RuriLib.Functions.Files.Files.MakeValidFileName(vm.Config.Settings.Name));

                        if (!Directory.Exists(folderName))
                            Directory.CreateDirectory(folderName);

                        var fileName = Path.Combine(folderName, $"{hit.Type}.txt");

                        lock (FileLocker.GetLock(fileName))
                        {
                            File.AppendAllText(fileName, $"{hit.Data} | {hit.CapturedString}{Environment.NewLine}");
                        }
                    }
                    catch (Exception ex) { SB.Logger.LogError(Components.Runner, $"Failed to add {hit.Type} hit " + hit.Data + $" to the text file - {ex.Message}"); }
                }
                else
                {
                    try
                    {
                        SB.Logger.LogInfo(Components.Runner, $"Adding {hit.Type} hit " + hit.Data + " to the DB");
                        SB.HitsDB.Add(hit);
                        SB.MainWindow.HitsDBPage.AddConfigToFilter(vm.ConfigName);
                    }
                    catch (Exception ex) { SB.Logger.LogError(Components.Runner, $"Failed to add {hit.Type} hit " + hit.Data + $" to the DB - {ex.Message}"); }
                }
            }));
        }

        private void PlayHitSound(IRunnerMessaging sender, Hit hit)
        {
            if (SB.OBSettings.Sounds.EnableSounds && hit.Type == "SUCCESS")
            {
                try
                {
                    while (soundLock) { Thread.Sleep(10); }
                    soundLock = true;
                    hitPlayer.Play();
                    soundLock = false;
                }
                catch { }
                finally { soundLock = false; }
            }
        }

        private void PlayReloadSound(IRunnerMessaging sender)
        {
            if (SB.OBSettings.Sounds.EnableSounds)
            {
                try
                {
                    while (soundLock) { Thread.Sleep(10); }
                    soundLock = true;
                    reloadPlayer.Play();
                    soundLock = false;
                }
                catch { }
                finally { soundLock = false; }
            }
        }

        private void LoadProxiesFromManager(IRunnerMessaging sender)
        {
            var proxies = SB.ProxyManager.ProxiesCollection.ToList();
            List<CProxy> toAdd;
            if (vm.Config.Settings.OnlySocks) toAdd = proxies.Where(x => x.Type != ProxyType.Http).ToList();
            else if (vm.Config.Settings.OnlySsl) toAdd = proxies.Where(x => x.Type == ProxyType.Http).ToList();
            else toAdd = proxies;

            vm.ProxyPool = new ProxyPool(toAdd, SB.Settings.RLSettings.Proxies.ShuffleOnStart);
        }

        private void ProcessStatusChange(IRunnerMessaging sender)
        {
            switch (vm.Master.Status)
            {
                case WorkerStatus.Idle:
                    SaveRecord();
                    startButtonLabel.Text = "START"; startButtonIcon.Kind = PackIconMaterialKind.PlayCircleOutline; ;
                    break;
            }
        }

        private void SaveProgressToDB(IRunnerMessaging sender)
        {
            SaveRecord();
        }

        private void InitCustomInputs(IRunnerMessaging sender)
        {
            // Ask for Custom User Input
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                vm.CustomInputs = new List<KeyValuePair<string, string>>();
                foreach (var input in vm.Config.Settings.CustomInputs)
                {
                    SB.Logger.LogInfo(Components.Runner, $"Asking for input {input.Description}");
                    (new MainDialog(new DialogCustomInput(vm, input.VariableName, input.Description), "Custom Input")).ShowDialog();
                }
                vm.CustomInputsInitialized = true;
            }));
        }
        #endregion

        #region Start
        public void OnStartRunner(object sender, EventArgs e)
        {
            startButton_Click(this, new RoutedEventArgs());
        }

        public void startButton_Click(object sender, RoutedEventArgs e)
        {
            switch (vm.Master.Status)
            {
                case WorkerStatus.Idle:
                    // Check if the required plugins are present
                    try
                    {
                        OBIOManager.CheckRequiredPlugins(SB.BlockPlugins.Select(b => b.Name), vm.Config);
                    }
                    catch (Exception ex)
                    {
                        SB.Logger.LogError(Components.Runner, ex.Message, true);
                        return;
                    }

                    SetupSoundPlayers();
                    ThreadPool.SetMinThreads(vm.BotsAmount * 2 + 1, vm.BotsAmount * 2 + 1);
                    ServicePointManager.DefaultConnectionLimit = 10000; // This sets the default connection limit for requests on the whole application
                    startButtonLabel.Text = "STOP";
                    startButtonIcon.Kind = PackIconMaterialKind.StopCircleOutline;
                    vm.Start();
                    break;

                case WorkerStatus.Running:
                    vm.Stop();
                    startButtonLabel.Text = "HARD ABORT"; startButtonIcon.Kind = PackIconMaterialKind.StopCircleOutline;
                    break;

                case WorkerStatus.Stopping:
                    vm.ForceStop();
                    startButtonLabel.Text = "START"; startButtonIcon.Kind = PackIconMaterialKind.PlayCircleOutline;
                    SaveRecord();
                    break;
            }
        }
        #endregion

        #region Private Setup Methods
        private void SetupSoundPlayers()
        {
            var hitSound = $"Sounds/{SB.OBSettings.Sounds.OnHitSound}";
            var reloadSound = $"Sounds/{SB.OBSettings.Sounds.OnReloadSound}";
            if (File.Exists(hitSound)) hitPlayer = new SoundPlayer(hitSound);
            if (File.Exists(reloadSound)) reloadPlayer = new SoundPlayer(reloadSound);
            SB.Logger.LogInfo(Components.Runner, "Set up sound players");
        }
        #endregion

        #region External Set Methods
        public void SetConfig(Config config)
        {
            vm.SetConfig(config, SB.OBSettings.General.RecommendedBots);
            RetrieveRecord();
        }

        public void SetWordlist(Wordlist wordlist)
        {
            vm.SetWordlist(wordlist);
            RetrieveRecord();
        }


        #endregion

        #region UI Elements
        private void selectConfigButton_Click(object sender, RoutedEventArgs e)
        {
            (new MainDialog(new DialogSelectConfig(this), "Select Config")).ShowDialog();
        }

        private void selectWordlistButton_Click(object sender, RoutedEventArgs e)
        {
            (new MainDialog(new DialogSelectWordlist(this), "Select Wordlist")).ShowDialog();
        }

        private void selectpProxylistButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //
            }
            catch { }
        }

        private void hitsFilterButton_Click(object sender, RoutedEventArgs e)
        {
            vm.ResultsFilter = BotStatus.SUCCESS;
            SB.Logger.LogInfo(Components.Runner, $"Changed valid filter to {vm.ResultsFilter}");
            RefreshListView();
        }

        private void customFilterButton_Click(object sender, RoutedEventArgs e)
        {
            vm.ResultsFilter = BotStatus.CUSTOM;
            SB.Logger.LogInfo(Components.Runner, $"Changed valid filter to {vm.ResultsFilter}");
            RefreshListView();
        }

        private void toCheckFilterButton_Click(object sender, RoutedEventArgs e)
        {
            vm.ResultsFilter = BotStatus.NONE;
            SB.Logger.LogInfo(Components.Runner, $"Changed valid filter to {vm.ResultsFilter}");
            RefreshListView();
        }

        private void RefreshListView()
        {
            validListView.ItemsSource = vm.EmptyList;
            switch (vm.ResultsFilter)
            {
                case BotStatus.SUCCESS:
                    validListView.ItemsSource = vm.HitsList;
                    break;

                case BotStatus.CUSTOM:
                    validListView.ItemsSource = vm.CustomList;
                    break;

                case BotStatus.NONE:
                    validListView.ItemsSource = vm.ToCheckList;
                    break;
            }
        }

        private ListView GetCurrentListView()
        {
            return validListView;
        }

        private void showManagerButton_Click(object sender, RoutedEventArgs e)
        {
            SB.MainWindow.ShowRunnerManager();
        }
        #endregion

        #region Rightclick options
        private void ListViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void showHTML_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                File.WriteAllText("source.html", ((ValidData)GetCurrentListView().SelectedItem).Source);
                System.Diagnostics.Process.Start("source.html");
                SB.Logger.LogInfo(Components.Runner, "Saved the html to source.html and opened it with the default viewer");
            }
            catch (Exception ex) { SB.Logger.LogError(Components.Runner, $"Couldn't show the HTML - {ex.Message}", true); }
        }

        private void showLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                (new MainDialog(new DialogShowLog(((ValidData)GetCurrentListView().SelectedItem).Log), "Complete Log")).Show();
                SB.Logger.LogInfo(Components.Runner, "Opened the log for the hit " + ((ValidData)GetCurrentListView().SelectedItem).Data);
            }
            catch { MessageBox.Show("FAILED"); }
        }

        private void copySelectedData_Click(object sender, RoutedEventArgs e)
        {
            var clipboardText = "";
            try
            {
                foreach (ValidData selected in GetCurrentListView().SelectedItems)
                    clipboardText += selected.Data + Environment.NewLine;

                SB.Logger.LogInfo(Components.Runner, $"Copied {GetCurrentListView().SelectedItems.Count} data");
                Clipboard.SetText(clipboardText);
            }
            catch (Exception ex) { SB.Logger.LogError(Components.Runner, $"Exception while copying data - {ex.Message}"); }
        }

        private void copySelectedCapture_Click(object sender, RoutedEventArgs e)
        {
            var clipboardText = "";
            try
            {
                foreach (ValidData selected in GetCurrentListView().SelectedItems)
                    clipboardText += selected.Data + " | " + selected.CapturedData + Environment.NewLine;

                SB.Logger.LogInfo(Components.Runner, $"Copied {GetCurrentListView().SelectedItems.Count} data");
                Clipboard.SetText(clipboardText);
            }
            catch (Exception ex) { SB.Logger.LogError(Components.Runner, $"Exception while copying data - {ex.Message}"); }
        }

        private void selectAll_Click(object sender, RoutedEventArgs e)
        {
            GetCurrentListView().SelectAll();
        }

        private void copySelectedProxy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(((ValidData)GetCurrentListView().SelectedItem).Proxy);
                SB.Logger.LogInfo(Components.Runner, "Copied the proxy " + ((ValidData)GetCurrentListView().SelectedItem).Proxy);
            }
            catch (Exception ex) { SB.Logger.LogError(Components.Runner, $"Couldn't copy the proxy for the selected hit - {ex.Message}"); }
        }

        private void sendToDebugger_Click(object sender, RoutedEventArgs e)
        {
            try // Try because StackerPage can be null if not initialized yet
            {
                var stacker = SB.Stacker;
                var current = GetCurrentListView().SelectedItem as ValidData;

                stacker.TestData = current.Data;

                stacker.TestProxy = current.Proxy;
                stacker.ProxyType = current.ProxyType;

                SB.Logger.LogInfo(Components.Runner, $"Sent to the debugger");
            }
            catch (Exception ex) { SB.Logger.LogError(Components.Runner, $"Could not send data and proxy to the debugger - {ex.Message}"); }
        }
        #endregion

        #region Records
        private void SaveRecord()
        {
            SB.RunnerManager.SaveRecord(vm.Config, vm.Wordlist, vm.TestedCount + vm.StartingPoint);
        }

        private void RetrieveRecord()
        {
            vm.StartingPoint = SB.RunnerManager.RetrieveRecord(vm.Config, vm.Wordlist);
        }
        #endregion

        public void LabelCustom_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (vm.CustomCount == 0)
                    return;

                var label = e.OriginalSource as Label;

                var toolTip = string.Empty;

                var customItems =
                    from cType in vm.CustomList.Select(v => v.Type)
                    group cType by cType into g
                    let customCount = g.Count()
                    orderby customCount descending
                    select new
                    {
                        Count = customCount,
                        Name = g.Key
                    };

                if (customItems?.Count() > 0)
                {
                    customItems.ToList()
                    .ForEach(ct => toolTip += $"{ct.Name}:{ct.Count}\n");
                    toolTip = toolTip.TrimEnd('\n');
                    label.ToolTip = new ToolTip() { Content = toolTip };
                }
            }
            catch { }
        }

        public void LabelCustom_MouseLeave(object sender, MouseEventArgs e)
        {
            try { (e.OriginalSource as Label).ToolTip = null; } catch { }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
                if (!e.Handled)
                {
                    var textBox = (TextBox)sender;
                    var value = textBox.Text;
                    if (textBox.SelectedText != string.Empty)
                    {
                        value = textBox.Text.Remove(textBox.SelectionStart,
                           textBox.SelectedText.Length);
                    }
                    var botsAmount = int.Parse(value + e.Text);
                    e.Handled = !(botsAmount <= botsSlider.Maximum && botsAmount > botsSlider.Minimum - 1);
                }
            }
            catch { }
        }
    }
}
