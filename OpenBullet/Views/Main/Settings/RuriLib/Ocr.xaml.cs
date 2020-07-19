using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RuriLib.ViewModels;

namespace OpenBullet.Views.Main.Settings.RL
{
    /// <summary>
    /// Interaction logic for Ocr.xaml
    /// </summary>
    public partial class Ocr : Page
    {
        SettingsOcr vm;
        public Ocr()
        {
            InitializeComponent();
            DataContext = vm = SB.Settings.RLSettings.Ocr;

            Enum.GetNames(typeof(VariableValueType)).ToList()
                .ForEach(vt => varValueType.Items.Add(vt));

        }

        private void btnVariableAdd_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            vm.VariableList.Add(new TesseractVariable()
            {
                Name = varName.Text,
                Value = varValue.Text,
                ValueType = (VariableValueType)Enum.Parse(typeof(VariableValueType),
                varValueType.SelectedItem.ToString(), true)
            });
        }

        private void btnVariableUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var selectedIndex = variableLB.SelectedIndex;

            if (selectedIndex > 0)
            {
                var itemToMoveUp = variableLB.Items[selectedIndex];
                vm.VariableList.RemoveAt(selectedIndex);
                vm.VariableList.Insert(selectedIndex - 1, (TesseractVariable)itemToMoveUp);
                variableLB.SelectedIndex = selectedIndex - 1;
            }
        }

        private void btnVariableDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var selectedIndex = variableLB.SelectedIndex;

            if (selectedIndex + 1 < variableLB.Items.Count)
            {
                var itemToMoveUp = variableLB.Items[selectedIndex];
                vm.VariableList.RemoveAt(selectedIndex);
                vm.VariableList.Insert(selectedIndex + 1, (TesseractVariable)itemToMoveUp);
                variableLB.SelectedIndex = selectedIndex + 1;
            }
        }

        private void btnVariableRemove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (variableLB.SelectedIndex == -1) return;

            vm.VariableList.RemoveAt(variableLB.SelectedIndex);
        }

        private void btnVariableClear_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            vm.VariableList.Clear();
        }

        //copy var
        private void MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var varItem = vm.VariableList[variableLB.SelectedIndex];
                Clipboard.SetText(varItem.Name + ":" + varItem.Value + ":" + varItem.ValueType.ToString());
            }
            catch { }
        }
    }
}
