using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using OpenBullet.ViewModels;
using RuriLib;
using RuriLib.ViewModels;

namespace OpenBullet.Views.Main.Configs
{
    /// <summary>
    /// Logica di interazione per ConfigManager.xaml
    /// </summary>

    public partial class ConfigManager : Page
    {
        private ConfigManagerViewModel vm = null;
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        private IEnumerable<ConfigViewModel> Selected => configsListView.SelectedItems.Cast<ConfigViewModel>();

        public void OnSaveConfig(object sender, EventArgs e)
        {
            saveConfigButton_Click(this, new RoutedEventArgs());
        }

        public ConfigManager()
        {
            vm = SB.ConfigManager;
            DataContext = vm;

            InitializeComponent();
        }

        #region State
        // Checks if the config's hash is the same as the saved one
        public bool CheckSaved()
        {
            var stacker = SB.MainWindow.ConfigsPage.StackerPage;

            // If we don't have a config selected or we suppressed the warning or we don't have a Stacker open
            if (vm.CurrentConfig == null || SB.OBSettings.General.DisableNotSavedWarning || stacker == null)
            {
                return true;
            }

            // Blocks to LS conversion because we are going to hash the LS
            stacker.SetScript();
            var cvm = SB.Stacker.Config;

            // If we don't have a config loaded in Stacker
            if (cvm == null)
            {
                return true;
            }

            return vm.SavedHash == cvm.Config.Script.GetHashCode();
        }

        // Saves a hash of the LS of the config in Stacker
        public void SaveState()
        {
            var cvm = SB.Stacker.Config;

            if (cvm != null)
            {
                vm.SavedHash = cvm.Config.Script.GetHashCode();
            }
        }
        #endregion

        #region Buttons
        private void loadConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckSaved())
            {
                SB.Logger.LogWarning(Components.Stacker, "Config not saved, prompting quit confirmation");
                if (MessageBox.Show("The Config in Stacker wasn't saved.\nAre you sure you want to load another config?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    return;
            }

            // Load the config
            LoadConfig(configsListView.SelectedItem as ConfigViewModel);
        }

        private void saveConfigButton_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
        }

        private void deleteConfigsButton_Click(object sender, RoutedEventArgs e)
        {
            SB.Logger.LogWarning(Components.ConfigManager, "Deletion initiated, prompting warning");
            if (MessageBox.Show("This will delete the physical files from your disk! Are you sure you want to continue?", "WARNING", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                vm.Remove(Selected);

                SB.Logger.LogInfo(Components.ConfigManager, "Deletion completed");
            }
            else
            {
                SB.Logger.LogInfo(Components.ConfigManager, "Deletion cancelled");
            }
        }

        private void rescanConfigsButton_Click(object sender, RoutedEventArgs e)
        {
            vm.Rescan();
        }

        private void newConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckSaved())
            {
                SB.Logger.LogWarning(Components.Stacker, "Config not saved, prompting quit confirmation");
                if (MessageBox.Show("The Config in Stacker wasn't saved.\r\nAre you sure you want to create a new config?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    return;
            }

            (new MainDialog(new DialogNewConfig(this), "New Config")).ShowDialog();
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            vm.SearchString = filterTextbox.Text;
        }

        private void filterTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                searchButton_Click(this, null);
        }

        private void openConfigFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(System.IO.Path.Combine(Directory.GetCurrentDirectory(), SB.configFolder));
            }
            catch { SB.Logger.LogError(Components.ConfigManager, "No config folder found!", true); }
        }
        #endregion

        #region Saving, Loading and Creating
        public void SaveConfig()
        {
            if (vm.CurrentConfig == null ||
                SB.MainWindow.ConfigsPage.StackerPage == null ||
                SB.MainWindow.ConfigsPage.OtherOptionsPage == null)
            {
                SB.Logger.LogError(Components.ConfigManager, "No config eligible for saving!", true);
                return;
            }

            if (vm.CurrentConfig.Remote)
            {
                SB.Logger.LogError(Components.ConfigManager, "The config was pulled from a remote source and cannot be saved!", true);
                return;
            }

            if (vm.CurrentConfigName == string.Empty)
            {
                SB.Logger.LogError(Components.ConfigManager, "Empty config name, cannot save", true);
                return;
            }

            var stacker = SB.Stacker;
            stacker.ConvertKeychains();
            stacker.ConvertPlugins();

            if (stacker.View == StackerView.Blocks)
                stacker.LS.FromBlocks(stacker.GetList());

            vm.CurrentConfig.Config.Script = stacker.LS.Script;

            SB.Logger.LogInfo(Components.ConfigManager, $"Saving config {vm.CurrentConfigName}");

            vm.CurrentConfig.Config.Settings.LastModified = DateTime.Now;
            vm.CurrentConfig.Config.Settings.Version = SB.Version + " [SB]";
            vm.CurrentConfig.Config.Settings.RequiredPlugins = vm.GetRequiredPlugins(vm.CurrentConfig).ToArray();
            SB.Logger.LogInfo(Components.ConfigManager, "Converted the unbinded observables and set the Last Modified date");

            // Save to file
            try
            {
                vm.SaveCurrent();

                // Save the last state of the config
                SaveState();
            }
            catch (Exception ex)
            {
                SB.Logger.LogError(Components.ConfigManager, $"Failed to save the config. Reason: {ex.Message}", true);
            }
        }

        public void LoadConfig(ConfigViewModel config)
        {
            if (config == null)
            {
                SB.Logger.LogError(Components.ConfigManager, "The config to load cannot be null", true);
                return;
            }

            try
            {
                OBIOManager.CheckRequiredPlugins(SB.BlockPlugins.Select(b => b.Name), config.Config);
            }
            catch (Exception ex)
            {
                SB.Logger.LogError(Components.ConfigManager, ex.Message, true);
                return;
            }

            // Set the config as current
            vm.CurrentConfig = config;

            if (vm.CurrentConfig.Remote)
            {
                SB.Logger.LogError(Components.ConfigManager, "The config was pulled from a remote source and cannot be edited!", true);
                vm.CurrentConfig = null;
                return;
            }

            SB.Logger.LogInfo(Components.ConfigManager, "Loading config: " + vm.CurrentConfig.Name);

            SB.MainWindow.ConfigsPage.menuOptionStacker.IsEnabled = true;
            SB.MainWindow.ConfigsPage.menuOptionOtherOptions.IsEnabled = true;

            var newStackerVM = new StackerViewModel(vm.CurrentConfig);

            // Preserve the old stacker test data and proxy
            if (SB.MainWindow.ConfigsPage.StackerPage != null)
            {
                newStackerVM.TestData = SB.Stacker.TestData;
                newStackerVM.TestProxy = SB.Stacker.TestProxy;
                newStackerVM.ProxyType = SB.Stacker.ProxyType;
            }

            SB.Stacker = newStackerVM;
            SB.MainWindow.ConfigsPage.StackerPage = new Stacker(); // Create a Stacker instance
            SB.Logger.LogInfo(Components.ConfigManager, "Created and assigned a new Stacker instance");
            SB.MainWindow.ConfigsPage.OtherOptionsPage = new ConfigOtherOptions(); // Create an Other Options instance
            SB.Logger.LogInfo(Components.ConfigManager, "Created and assigned a new Other Options instance");
            SB.MainWindow.ConfigsPage.menuOptionStacker_MouseDown(this, null); // Switch to Stacker

            // Save the last state of the config
            SB.MainWindow.ConfigsPage.StackerPage.SetScript();
            SaveState();
        }

        public void CreateConfig(string name, string category, string author)
        {
            // Build the base config structure
            var settings = new ConfigSettings();
            settings.Name = name;
            settings.Author = author;

            var newConfig = new ConfigViewModel(name, category, new Config(settings, string.Empty));

            // Add it to the collection and persistent storage
            vm.Add(newConfig);

            vm.CurrentConfig = newConfig;
            var newStackerVM = new StackerViewModel(vm.CurrentConfig);
            if (SB.MainWindow.ConfigsPage.StackerPage != null) // Maintain the previous stacker settings
            {
                newStackerVM.TestData = SB.Stacker.TestData;
                newStackerVM.TestProxy = SB.Stacker.TestProxy;
                newStackerVM.ProxyType = SB.Stacker.ProxyType;
            }
            SB.Stacker = newStackerVM;
            SB.MainWindow.ConfigsPage.StackerPage = new Stacker(); // Create a Stacker instance
            SB.Logger.LogInfo(Components.ConfigManager, "Created and assigned a new Stacker instance");
            SB.MainWindow.ConfigsPage.OtherOptionsPage = new ConfigOtherOptions(); // Create an Other Options instance
            SB.Logger.LogInfo(Components.ConfigManager, "Created and assigned a new Other Options instance");
            SB.MainWindow.ConfigsPage.menuOptionStacker_MouseDown(this, null); // Switch to Stacker
        }
        #endregion

        #region ListView
        private void configsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try { vm.HoveredConfig = ((ConfigViewModel)configsListView.SelectedItem).Config; } catch { }
        }

        private void listViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                configsListView.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            configsListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            loadConfigButton_Click(this, new RoutedEventArgs());
        }
        #endregion

        private void pasteConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Paste.

                // Get the DataObject.
                var dataObject = Clipboard.GetDataObject();

                // Look for a file drop.
                if (dataObject.GetDataPresent(DataFormats.FileDrop))
                {
                    var pathConfig = (dataObject.GetData(DataFormats.FileDrop)
                        as string[]).FirstOrDefault();
                    var confExtension = Path.GetExtension(pathConfig);
                    if (pathConfig != null && confExtension == ".svb" ||
                        confExtension == ".loli" || confExtension == ".loliX" ||
                        confExtension == ".anom")
                    {
                        if (!File.Exists(pathConfig)) return;

                        var configName = Path.GetFileName(pathConfig);
                        var dst = Environment.CurrentDirectory + "\\Configs\\" + configName;
                        if (File.Exists($"Configs\\{configName}"))
                        {
                            if (MessageBox.Show
                               ("The File you want to copy already exists. Do you want to replace it?", $"File exists [{Path.GetFileNameWithoutExtension(pathConfig)}]", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                var b = pathConfig == dst;
                                if (!b)
                                {
                                    File.Delete(dst);
                                }
                                File.Copy(pathConfig, $"Configs\\{configName}", b);
                                rescanConfigsButton_Click(null, null);
                                return;
                            }
                            else
                            {
                                var fullPath = $"Configs\\{ Path.GetFileName(pathConfig)}"
                                    .Rename();
                                File.Copy(pathConfig, fullPath, false);
                                rescanConfigsButton_Click(null, null);
                                return;
                            }
                        }
                        File.Copy(pathConfig, $"Configs\\{Path.GetFileName(pathConfig)}", false);
                        rescanConfigsButton_Click(null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR");
            }
        }

        private void ListViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.V)
                {
                    pasteConfig_Click(sender, e);
                }
            }
            catch { }
        }

        private void filterTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(filterTextbox.Text))
                {
                    searchButton_Click(sender, null);
                }
            }
            catch { }
        }
    }
}
