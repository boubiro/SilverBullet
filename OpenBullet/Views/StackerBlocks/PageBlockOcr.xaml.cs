using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AngleSharp.Text;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.MetaData;
using Microsoft.Win32;
using OpenBullet.Models;
using OpenBullet.Views.UserControls.Filters;
using RuriLib;
using Tesseract;

namespace OpenBullet.Views.StackerBlocks
{
    /// <summary>
    /// Interaction logic for PageBlockOcr.xaml
    /// </summary>
    public partial class PageBlockOcr : System.Windows.Controls.Page
    {
        BlockOcr vm;
        int lastSelectedIndex = -1;
        public PageBlockOcr(BlockOcr block)
        {
            InitializeComponent();
            vm = block;
            DataContext = vm;

            customHeadersRTB.AppendText(vm.GetCustomHeaders());

            try
            {
                foreach (var filter in vm.GetFilters().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    filterLB.Items.Add(filter.Trim());
                }
            }
            catch { }

            vm.PixelFormats.Add("Default");
            vm.PixelFormats.AddRange(Enum.GetNames(typeof(System.Drawing.Imaging.PixelFormat)));
            Enum.GetNames(typeof(EngineMode)).ToList().ForEach(e => EngineModeComboBox.Items.Add(e));
            Enum.GetNames(typeof(PageSegMode)).ToList().ForEach(p => PageSegModeComboBox.Items.Add(p));
            InitFilterControls();
            SetItemToComboBox();
        }

        private void InitFilterControls()
        {

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTessData();
            LoadFilters();
            SetItemToComboBox();
        }

        private void Page_Initialized(object sender, EventArgs e)
        {
        }

        private void customHeadersRTB_LostFocus(object sender, RoutedEventArgs e)
        {
            vm.SetCustomHeaders(customHeadersRTB.Lines());
        }

        private void filterLB_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (filterLB.Items.Count == 0) return;
            try
            {
                vm.SetFilters(filterLB.Items.OfType<string>().ToArray());
            }
            catch { }
        }

        private void btnAddFilter_Click(object sender, RoutedEventArgs e)
        {
            filterLB.Items.Add(filterComboBox.Text + ": ");
            filterLB.SelectedIndex = filterLB.Items.Count - 1;
            filterLB_LostFocus(null, null);
        }

        private void LoadTessData()
        {
            try
            {
                if (!Directory.Exists(".\\tessdata"))
                    Directory.CreateDirectory(".\\tessdata");
                foreach (FileInfo file in new DirectoryInfo(".\\tessdata").GetFiles("."))
                {
                    if (file.Name.Contains(".") && !vm.Languages.Contains(file.Name.Split('.')[0]))
                    {
                        vm.Languages.Add(file.Name.Split(new char[]
                        {
                            '.'
                        })[0]);
                    }
                }
                try
                {
                    vm.Languages = new ObservableCollection<string>(vm.Languages.Distinct());
                }
                catch { }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Missing folder \"tessdata\"! Please go make one and put your language files in it!", "NOTICE",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private void LoadFilters()
        {
            try
            {
                foreach (var processor in vm.Processors)
                {
                    if (!filterComboBox.Items.Contains(processor.Item1))
                    {
                        filterComboBox.Items.Add(processor.Item1);
                    }
                }
            }
            catch (Exception ex) { }
        }

        private void SetItemToComboBox()
        {
            try
            {
                LangComboBox.SelectedIndex = LangComboBox.Items.IndexOf(vm.OcrLang);
                EngineModeComboBox.SelectedIndex = EngineModeComboBox.Items.IndexOf(vm.Engine);
                PageSegModeComboBox.SelectedIndex = PageSegModeComboBox.Items.IndexOf(vm.PageSeg);
            }
            catch { }
        }

        private void btnfilterUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedIndex = filterLB.SelectedIndex;

                if (selectedIndex > 0)
                {
                    var itemToMoveUp = filterLB.Items[selectedIndex];
                    filterLB.Items.RemoveAt(selectedIndex);
                    filterLB.Items.Insert(selectedIndex - 1, itemToMoveUp);
                    filterLB.SelectedIndex = selectedIndex - 1;
                    vm.SetFilters(filterLB.Items.OfType<string>().ToArray());
                }
            }
            catch { }
        }

        private void btnfilterDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedIndex = filterLB.SelectedIndex;

                if (selectedIndex + 1 < filterLB.Items.Count)
                {
                    var itemToMoveDown = filterLB.Items[selectedIndex];
                    filterLB.Items.RemoveAt(selectedIndex);
                    filterLB.Items.Insert(selectedIndex + 1, itemToMoveDown);
                    filterLB.SelectedIndex = selectedIndex + 1;
                    filterLB_LostFocus(null, null);
                }
            }
            catch { }
        }

        private void btnfilterRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedIndex = filterLB.SelectedIndex;

                if (selectedIndex > -1)
                {
                    filterLB.Items.RemoveAt(selectedIndex);
                    filterTabControl.SelectedIndex = -1;
                    filterLB_LostFocus(null, null);
                }
            }
            catch { }
        }

        private void btnfilterClone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (filterLB.Items.Count == 0) return;
                filterLB.Items.Add(
                   filterLB.SelectedItem.ToString());
            }
            catch { }
        }

        private void btnfilterClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (System.Windows.Forms.MessageBox.Show("Do you want to clear the list of filters?", "Warning",
                    System.Windows.Forms.MessageBoxButtons.YesNo,
                    System.Windows.Forms.MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.No)
                {
                    return;
                }
                filterLB.Items.Clear();
                vm.SetFilters(filterLB.Items.OfType<string>().ToArray());
            }
            catch { }
        }
        private void filterLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var seletedIndex = filterLB.SelectedIndex;
                lastSelectedIndex = seletedIndex;

                if (seletedIndex == -1)
                {
                    return;
                }

                var selectedFilter = filterLB.Items[seletedIndex++].ToString();
                if (!selectedFilter.Contains(":")) { selectedFilter += ": "; }

                //Exam -> Contrast: 1 -> split ':' ->  [0] -> Contrast
                var filterType = selectedFilter.Split(':')[0].Trim();

                if (seletedIndex > -1)
                {
                    filterGroupBox.Visibility = Visibility.Visible;
                    filterGroupBox.Header = filterType;
                }

                switch (filterType.ToLower())
                {
                    case "entropycrop":
                    case "binarization":
                        SetInInput(--seletedIndex, new string[] { "0" }, "Threshold");
                        break;

                    case "contrastex":
                    case "contrast":
                    case "brightness":
                    case "saturation":
                    case "scale":
                    case "alpha":
                        SetInInput(--seletedIndex, new string[] { "0" }, "Percentage");
                        break;

                    case "pixelate":
                    case "resize":
                        SetSize(--seletedIndex);
                        break;

                    case "mean":
                    case "gamma":
                    case "smooth":
                    case "colorthreshold":
                    case "sharpenex":
                    case "sharpen":
                        SetInInput(--seletedIndex, new string[] { "0" });
                        break;

                    case "median":
                        SetInInput(--seletedIndex, new[] { "0" }, "ksize");
                        break;

                    case "backgroundcolor":
                    case "tint":
                    case "vignette":
                    case "coloralpha":
                        SetInInput(--seletedIndex, new string[] { "0,0,0" }, "Color(R,G,B)", true);
                        break;

                    case "gaussianblur":
                    case "gaussiansharpen":
                        SetInInput(--seletedIndex, new string[] { "0" }, "Size");
                        break;

                    case "alignment":
                        SetInInput(--seletedIndex, new[] { "4" }, "Alignment size(must be a power of two)");
                        break;

                    case "rotate":
                        SetInInput(--seletedIndex, new string[] { "0" }, "Degrees");
                        break;

                    case "constrain":
                        SetSize(--seletedIndex);
                        break;

                    case "halftone":
                        SetInInputBoolean(--seletedIndex, "False", "Comic Mode");
                        break;

                    case "blur":
                        SetSize(--seletedIndex);
                        break;

                    case "roundedcorners":
                    case "edge":
                        SetInInput(--seletedIndex, new string[] { "0" }, "Radius");
                        break;

                    case "crop":
                        SetCropLayer(--seletedIndex, new[] { "0", "0", "0", "0", "Percentage" });
                        break;

                    case "morphology":
                        SetMorphology(--seletedIndex);
                        break;

                    case "zoom":
                        SetInInputTextAndBoolean(--seletedIndex, "0", false,
                            "Zoom Factor", "NearestNeighbor");
                        break;
                    case "hue":
                        SetInInputTextAndBoolean(--seletedIndex, "0", false,
                          "Degrees", "Rotate (Any integer between 0 and 360)");
                        break;

                    case "adaptivethreshold":
                        SetAdaptiveThreshold(--seletedIndex, new string[] { "1", "MeanC", "Binary", "1", "1" });
                        break;

                    case "threshold":
                        SetThreshold(--seletedIndex, new[] { "0", "255", "Binary" });
                        break;

                    case "replacecolor":
                        SetReplaceColor(--seletedIndex, new[] { "0,0,0", "|", "0,0,0", "0" });
                        break;

                    case "cvtcolor":
                        SetControl(--seletedIndex, UserControlCvtColor.ControlName, new[] {
                           new ControlText<TextBox>(controlCvtColor.dstCnTextBox,controlCvtColor.dstCnTextBox.Text)
                        });
                        break;

                    case "fastnlmeansdenoisingcolored":
                        SetFastNlMeansDenoisingColored(--seletedIndex, new[] { "3", "3" });
                        break;

                    case "resolution":
                        SetResolution(--seletedIndex, new[] { "0", "0", "Inch" });
                        break;

                    default:
                        filterTabControl.SelectedIndex = -1;
                        filterGroupBox.Visibility = Visibility.Collapsed;
                        break;
                }
            }
            catch (Exception ex) { }
        }

        private void SetInInput(int index, string[] defValues, string label = "Value", bool color = false)
        {
            inputControl.SetInputType(UserControlInput.InputType.Text);
            inputControl.label.Content = label + ":";
            filterTabControl.SelectIndexByHeaderName(UserControlInput.ControlName);
            var input = string.Empty;
            if (color)
            {
                var colors = GetFilterColors(index, defValues);
                input = colors[0] + "," + colors[1] + "," + colors[2];
            }
            else
                input = GetFilterValue(index, defValues);
            if (inputControl.InputTextBox.Text != input)
            {
                inputControl.InputTextBox.Text = input;
                SetCaretIndexAndSelect(inputControl.InputTextBox);
            }
        }
        private void SetEnum<TEnum>(int index, string defValue, string label = "Select")
        {
            if (controlEnumBox.EnumComboBox.Items.Count == 0 ||
                controlEnumBox.TEnumName != typeof(TEnum).Name)
            {
                controlEnumBox.AddEnum<TEnum>();
            }
            filterTabControl.SelectIndexByHeaderName(UserControlEnumBox.ControlName);
            controlEnumBox.label.Content = label + ":";
            var @enum = GetFilterValue(index, new string[] { defValue });
            controlEnumBox.EnumComboBox.SelectedItem = @enum;
        }

        private void SetInputTextAndEnum<TEnum>(int index, string[] defValue,
            string labelInput = "Input",
            string labelSelect = "Select", bool reverse = false)
        {
            if (controlInputTextAndEnum.EnumComboBox.Items.Count == 0 ||
              controlInputTextAndEnum.TEnumName != typeof(TEnum).Name)
            {
                controlInputTextAndEnum.AddEnum<TEnum>();
            }
            filterTabControl.SelectIndexByHeaderName(UserControlInputTextAndEnum.ControlName);
            controlInputTextAndEnum.Reverse = reverse;
            controlInputTextAndEnum.labelInput.Content = labelInput + ":";
            controlInputTextAndEnum.labelSelect.Content = labelSelect + ":";
            var input = GetFilterValues(index, defValue)[reverse ? 1 : 0];
            var @enum = GetFilterValues(index, defValue)[!reverse ? 1 : 0];
            controlInputTextAndEnum.EnumComboBox.SelectedItem = @enum;
            SetTextInTextBox(controlInputTextAndEnum.InputTextBox, input);
        }

        private void SetInInputBoolean(int index, string defValue, string label = "Value")
        {
            inputControl.SetInputType(UserControlInput.InputType.Boolean);
            inputControl.label.Content = label + ":";
            filterTabControl.SelectIndexByHeaderName(UserControlInput.ControlName);
            inputControl.InputComboBox.SelectedIndex = GetFilterValue(index, new string[] { defValue })
                .ToBoolean() ? 1 : 0;
        }

        private void SetSize(int index)
        {
            filterTabControl.SelectIndexByHeaderName(UserControlResize.ControlName);
            var width = GetFilterValues(index, new string[] { "0", "0" })[0];
            var height = GetFilterValues(index, new string[] { "0", "0" })[1];
            if (!resizeControl.WidthTextBox.Text.Equals(width)) resizeControl.WidthTextBox.Text = width;
            if (!resizeControl.HeightTextBox.Text.Equals(height)) resizeControl.HeightTextBox.Text = height;
            SetCaretIndexAndSelect(resizeControl.WidthTextBox);
            SetCaretIndexAndSelect(resizeControl.HeightTextBox);
        }

        private void SetReplaceColor(int index, string[] defValues)
        {
            filterTabControl.SelectIndexByHeaderName(UserControlReplaceColor.ControlName);
            if (!string.IsNullOrWhiteSpace(controlReplaceColor.TargetTextBox.Text) &&
                !string.IsNullOrWhiteSpace(controlReplaceColor.ReplacementTextBox.Text))
            {
                try
                {
                    var value = filterLB.Items[index].ToString();

                    if (!value.Contains(":")) { value += ": "; }

                    var val = value.Split(new char[] { ':' }, 2)[1].Trim()
                        .Split(',');

                    defValues = new[] { val[0],val[1],val[2], "|",
                    val[3],val[4],val[5],
                    val[6]};
                }
                catch { }
            }
            var colorTarget = defValues[0] + "," + defValues[1] + "," + defValues[2];
            var colorFill = defValues[4] + "," + defValues[5] + "," + defValues[6];
            SetTextInTextBox(controlReplaceColor.TargetTextBox, colorTarget);
            SetTextInTextBox(controlReplaceColor.ReplacementTextBox, colorFill);
            SetTextInTextBox(controlReplaceColor.FuzzinessTextBox, defValues[7]);
        }


        private void SetInInputTextAndBoolean(int index,
            string defValue, bool defBoolean, string labelVal = "Value", string labelBool = "Grayscale")
        {
            controlInputTextAndBool.label.Content = labelVal + ":";
            controlInputTextAndBool.CheckBox.Content = labelBool;
            filterTabControl.SelectIndexByHeaderName(UserControlInputTextAndBoolean.ControlName);
            var inputText = GetFilterValues(index, new string[] { defValue, defBoolean.ToString() })[0];
            SetTextInTextBox(controlInputTextAndBool.InputTextBox, inputText);
            controlInputTextAndBool.CheckBox.IsChecked =
                GetFilterValues(index, new string[] { defValue, defBoolean.ToString() })[1]
                .ToBoolean();
        }

        private void SetFastNlMeansDenoisingColored(int index, string[] defValues)
        {
            filterTabControl.SelectIndexByHeaderName(UserControlFastNlMeansDenoisingColored.ControlName);
            var strength = GetFilterValues(index, defValues)[0];
            var colorStrength = GetFilterValues(index, defValues)[1];
            SetTextInTextBox(controlFastNlMeansDenoisingColored.StrengthTextBox, strength);
            SetTextInTextBox(controlFastNlMeansDenoisingColored.ColorStrengthTextBox, colorStrength);
        }

        private void SetBlur(int index, string[] defValues)
        {
            filterTabControl.SelectIndexByHeaderName(UserControlBlur.ControlName);
            var radius = GetFilterValues(index, defValues)[0];
            var sigma = GetFilterValues(index, defValues)[1];
            var channels = GetFilterValues(index, defValues)[2];
            SetTextInTextBox(blurControl.RadiusTextBox, radius);
            SetTextInTextBox(blurControl.SigmaTextBox, sigma);
            blurControl.ChannelsComboBox.SelectedItem = channels;
            SetCaretIndexAndSelect(blurControl.RadiusTextBox);
            SetCaretIndexAndSelect(blurControl.SigmaTextBox);
        }

        private void SetThreshold(int index, string[] defValues)
        {
            filterTabControl.SelectIndexByHeaderName(UserControlThreshold.ControlName);
            var thresold = GetFilterValues(index, defValues)[0];
            var maxVal = GetFilterValues(index, defValues)[1];
            var thresholdType = GetFilterValues(index, defValues)[2];
            SetTextInTextBox(controlThreshold.ThreshTextBox, thresold);
            SetTextInTextBox(controlThreshold.MaxValueTextBox, maxVal);
            controlThreshold.ThresholdTypeComboBox.SelectedItem = thresholdType;
        }

        private void SetAdaptiveThreshold(int index, string[] defValues)
        {
            filterTabControl.SelectIndexByHeaderName(UserControlAdaptiveThreshold.ControlName);
            var maxValue = GetFilterValues(index, defValues)[0];
            var adaptiveMethod = GetFilterValues(index, defValues)[1];
            var thresholdType = GetFilterValues(index, defValues)[2];
            var blockSize = GetFilterValues(index, defValues)[3];
            var constant = GetFilterValues(index, defValues)[4];
            SetTextInTextBox(controlAdaptiveThreshold.MaxValueTextBox, maxValue);
            try { controlAdaptiveThreshold.AdaptiveMethodComboBox.SelectedItem = adaptiveMethod; } catch { }
            try { controlAdaptiveThreshold.ThresholdTypeComboBox.SelectedItem = thresholdType; } catch { }
            SetTextInTextBox(controlAdaptiveThreshold.BlockSizeTextBox, blockSize);
            SetTextInTextBox(controlAdaptiveThreshold.ConstantTextBox, constant);
        }

        private void SetCropLayer(int index, string[] defValues)
        {
            filterTabControl.SelectIndexByHeaderName(UserControlCropLayer.ControlName);
            var cropMode = CropMode.Percentage;
            try
            {
                cropMode = GetFilterValues(index,
              new string[] { "0", "0", "0", "0", "Percentage" })[4].ToEnum(CropMode.Percentage);
            }
            catch { }
            controlCropLayer.CropModeBox.SelectedIndex = (int)cropMode;
            var left = GetFilterValues(index, new string[] { "0", "0", "0", "0", cropMode.ToString() })[0];
            var top = GetFilterValues(index, new string[] { "0", "0", "0", "0", cropMode.ToString() })[1];
            var right = GetFilterValues(index, new string[] { "0", "0", "0", "0", cropMode.ToString() })[2];
            var bottom = GetFilterValues(index, new string[] { "0", "0", "0", "0", cropMode.ToString() })[3];
            SetTextInTextBox(controlCropLayer.LeftTextBox, left);
            SetTextInTextBox(controlCropLayer.TopTextBox, top);
            SetTextInTextBox(controlCropLayer.RightTextBox, right);
            SetTextInTextBox(controlCropLayer.BottomTextBox, bottom);
        }


        private void SetMorphology(int index)
        {
            filterTabControl.SelectIndexByHeaderName(UserControlMorphology.ControlName);
            var method = GetFilterValues(index, new string[] { "Erode" })[0];
            var iterations = GetFilterValues(index, new string[] { "Erode", "1", "Constant" })[1];
            var borderType = GetFilterValues(index, new string[] { "Erode", "1", "Constant" })[2];
            var mrophShapes = GetFilterValues(index, new[] { "Erode", "1", "Constant", "Null" })[3];
            var sizeWidthKernel = GetFilterValues(index, new[] { "Erode", "1", "Constant", "Null", "Null" })[4];
            var sizeHeightKernel = GetFilterValues(index, new[] { "Erode", "1", "Constant", "Null", "Null", "Null" })[5];
            try { controlMorphology.MorphMethodComboBox.SelectedItem = method; } catch { }
            try { controlMorphology.BorderTypeComboBox.SelectedItem = borderType; } catch { }
            try { controlMorphology.MorphShapesComboBox.SelectedItem = mrophShapes; } catch { }
            SetTextInTextBox(controlMorphology.IterationsTextBox, iterations);
            SetTextInTextBox(controlMorphology.SizeWidthTextBox, sizeWidthKernel);
            SetTextInTextBox(controlMorphology.SizeHeightTextBox, sizeHeightKernel);
            SetCaretIndexAndSelect(controlMorphology.IterationsTextBox);
        }

        private void SetResolution(int index, string[] defValues)
        {
            filterTabControl.SelectIndexByHeaderName(UserControlResolution.ControlName);
            var hori = GetFilterValues(index, defValues)[0];
            var verti = GetFilterValues(index, defValues)[1];
            var unit = GetFilterValues(index, defValues)[2];
            controlResolution.HorizontalNumeric.Value = int.Parse(hori);
            controlResolution.VerticalNumeric.Value = int.Parse(verti);
            controlResolution.UnitComboBox.SelectedItem = unit.ToEnum(PropertyTagResolutionUnit.Inch);
        }

        private void SetControl(int index, string controlName,
            ControlText<TextBox>[] controls = null)
        {
            filterTabControl.SelectIndexByHeaderName(controlName);
            for (var c = 0; c < controls?.Length; c++)
            {
                SetTextInTextBox(controls[c].Control, controls[c].Text);
            }
        }

        private void SetTextInTextBox(TextBox textBox, string text)
        {
            if (textBox.Text != text) textBox.Text = text;
        }

        private void SetFilter(int index, string[] values)
        {
            try
            {
                var selectedFilter = filterLB.Items[index].ToString();
                if (!selectedFilter.Contains(":")) { selectedFilter += ":"; }
                var split = selectedFilter.Split(new char[] { ':' }, 2);
                var value = split[1];
                if (string.IsNullOrWhiteSpace(split[1]) && values.Length > 0)
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        value += values[i] + ",";
                    }
                    value = value.Trim().TrimEnd(',');
                }
                else if (values.Length > 0)
                {
                    value = string.Empty;
                    for (var i = 0; i < values.Length; i++)
                    {
                        value += values[i] + ",";
                    }
                    value = value.Trim().TrimEnd(',');
                }

                filterLB.Items[index] = split[0] + ": " + value;
                filterLB.SelectedIndex = index;
            }
            catch { }
        }

        private string GetFilterValue(int index, string[] defaultValues, int parameterCount = 0)
        {
            var value = filterLB.Items[index].ToString();
            if (!value.Contains(":")) { value += ": "; }
            var val = value.Split(new char[] { ':' }, 2)[1].Trim();
            if (parameterCount > 0)
            {
                var paramSplit = val.Split(new char[] { ',' }, parameterCount,
                    StringSplitOptions.RemoveEmptyEntries);
                if (paramSplit.Length < parameterCount)
                {
                    foreach (var defVal in defaultValues)
                    {
                        if (!val.EndsWith(",")) { val += ","; }
                        val += defVal + ",";
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(val))
            {
                return val.Trim().TrimEnd(',');
            }
            foreach (var defValue in defaultValues)
            {
                val += defValue + ",";
            }
            return val.Trim().TrimEnd(',');
        }

        private string[] GetFilterValues(int index,
            string[] defaultValues,
            char split = ',')
        {
            var value = filterLB.Items[index].ToString();

            if (!value.Contains(":")) { value += ": "; }

            var val = value.Split(new char[] { ':' }, 2)[1].Trim();

            if (!string.IsNullOrWhiteSpace(val))
            {
                var vSplit = val.Split(split);
                var val2 = string.Empty;
                //Set the value to keep the textbox not empty
                for (var i = 0; i < defaultValues.Length; i++)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(vSplit[i]))
                        {
                            val2 += defaultValues[i] + split;
                        }
                        else { val2 += vSplit[i] + split; }
                    }
                    catch
                    {
                        val2 += defaultValues[i] + split;
                    }
                }
                return (val2 == "" ? val : val2).Trim().TrimEnd(split).Split(split);
            }

            defaultValues.ToList().ForEach(dv =>
            {
                val += dv + split;
            });

            return val.Trim().TrimEnd(split).Split(split);
        }

        private string[] GetFilterColors(int index,
            string[] defaultValues)
        {
            var val = string.Empty;

            defaultValues.ToList().ForEach(dv =>
            {
                if (!dv.EndsWith("|"))
                    val += dv + '|';
                else if (dv != "|")
                {
                    val += dv;
                }
            });

            return val.Trim().TrimEnd('|').Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

        }

        private void SetCaretIndexAndSelect(TextBox textBox, string defVal = "0", int index = 1)
        {
            try
            {
                if (textBox.Text == defVal)
                {
                    textBox.CaretIndex = index;
                    textBox.SelectAll();
                }
            }
            catch { }
        }

        private void inputControl_SetFilter(object sender, EventArgs e)
        {
            if (filterTabControl.SelectedIndex != filterTabControl
                .GetIndexByItemName((e as TextChangedEventArgs).Source.ToString()))
                return;

            SetFilter(lastSelectedIndex, sender as string[]);

            filterLB_LostFocus(null, null);
        }

        //load filters
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog()
                {
                    Filter = "Text|*.txt"
                };
                if (dialog.ShowDialog() == true)
                {
                    var filters = File.ReadAllLines(dialog.FileName);
                    filters.ToList().ForEach(f => filterLB.Items.Add(f));
                    filterLB_LostFocus(null, null);
                }
            }
            catch { }
        }

        //copy filter
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(vm.Filters[filterLB.SelectedIndex].ToString());
            }
            catch { }
        }

        //paste filter
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = Clipboard.GetText();
                var item2 = item;
                if (item.Contains(":")) item2 = item.Split(':')[0].Trim();
                if (filterComboBox.Items.Contains(item2))
                {
                    filterLB.Items.Add(item);
                }
            }
            catch { }
        }

        private void filterLB_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == System.Windows.Input.MouseButton.Left &&
                  filterLB.Items.Count > 0 && filterLB.SelectedIndex > -1)
                {
                    filterLB_SelectionChanged(filterLB, null);
                }
            }
            catch { }
        }
    }
}
