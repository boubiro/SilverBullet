using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AngleSharp.Text;
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.MetaData;
using Microsoft.Scripting.Utils;
using Microsoft.Win32;
using OpenBullet.Models;
using OpenBullet.Views.UserControls.Filters;
using RuriLib;
using Tesseract;

namespace OpenBullet.Views.Main.Configs
{
    /// <summary>
    /// Interaction logic for ConfigOcrSettings.xaml
    /// </summary>
    public partial class ConfigOcrSettings : System.Windows.Controls.Page
    {
        BlockOcr blockOcr = new BlockOcr();
        bool imageFromFile;
        string path;
        int lastSelectedIndex = -1;
        bool clicked;

        public ConfigOcrSettings(bool sendFilter = false)
        {
            InitializeComponent();
            DataContext = SB.MainWindow.ConfigsPage.CurrentConfig.Config.Settings;
            if (sendFilter) return;
            blockOcr.Processors.ForEach(p => filterBox.Items.Add(p.Item1));
            OrigImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            OrigImage.WaitOnLoad = true;
            ProcImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            ProcImage.WaitOnLoad = true;
            LoadTessData();
            InitFilterControls();
        }

        private void LoadTessData()
        {
            try
            {
                if (!Directory.Exists(".\\tessdata"))
                    Directory.CreateDirectory(".\\tessdata");
                foreach (FileInfo file in new DirectoryInfo(".\\tessdata").GetFiles("."))
                {
                    if (file.Name.Contains(".") && !langBox.Items.Contains(file.Name.Split('.')[0]))
                    {
                        langBox.Items.Add(file.Name.Split(new char[]
                        {
                            '.'
                        })[0]);
                    }
                }
                try
                {
                    langBox.SelectedIndex = langBox.Items.IndexOf(blockOcr.OcrLang);
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

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(OcrUrl.Text)) return;
                var bmp = blockOcr.GetOcrImage(false);
                OrigImage.Image = bmp;
                ProcImage.Image = bmp.Clone() as Bitmap;
                imageFromFile = false;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "NOTICE",
                  System.Windows.Forms.MessageBoxButtons.OK,
                  System.Windows.Forms.MessageBoxIcon.Error);
            }
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
                GetSettings().FilterList.Clear();
                scrollFilterTabControl.Visibility = Visibility.Collapsed;
                ProcImage.Image = (Bitmap)OrigImage.Image.Clone();
                SetFilters();
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
                    GetSettings().FilterList.RemoveAt(selectedIndex);
                    if (filterLB.Items.Count == 0)
                    {
                        ProcImage.Image = (Bitmap)OrigImage.Image.Clone();
                    }
                    scrollFilterTabControl.Visibility = Visibility.Collapsed;
                    SetFilters();
                }
            }
            catch
            {
                scrollFilterTabControl.Visibility = Visibility.Collapsed;
                SetFilters();
            }
        }

        private void chbAutoLoad_Click(object sender, RoutedEventArgs e)
        {

        }

        private void chbisBase64_Click(object sender, RoutedEventArgs e)
        {
            blockOcr.Base64 = chbisBase64.IsChecked.GetValueOrDefault();
        }

        //Test OCR
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (OrigImage.Image == null) return;

                var bmp = LoadBmp();
                var output = blockOcr.GetOcr(bmp,
                    engineComboBox.SelectedItem.ToString()
                    .ToEnum(EngineMode.Default),
                    pageSegComboBox.SelectedItem.ToString()
                    .ToEnum(PageSegMode.SingleLine),
                    GetSettings().EvaluateMathOCR);
                ProcImage.Image = blockOcr.ProcessedImage;
                resultOcrTextbox.Text = string.Empty;
                output.ToList().ForEach(o => resultOcrTextbox.Text += o + "\n");
                resultOcrTextbox.Text = resultOcrTextbox.Text.TrimEnd('\n');
                ocrRateTextblock.Text = "OCR Rate: " + blockOcr.OcrRate + "%";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR");
            }
        }

        private Bitmap LoadBmp()
        {
            return (Bitmap)OrigImage.Image.Clone();
        }

        private void OcrUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            blockOcr.Url = OcrUrl.Text;
            if (chbAutoLoad.IsChecked.GetValueOrDefault())
            {
                //if ()
                btnLoad_Click(null, null);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png | All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                var img = System.Drawing.Image.FromFile(dialog.FileName);
                OrigImage.Image = img;
                ProcImage.Image = img;
                imageFromFile = true;
                path = dialog.FileName;
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Bitmap bmp;
                if (!imageFromFile)
                {
                    bmp = blockOcr.GetOcrImage(false);
                }
                else
                {
                    bmp = (Bitmap)System.Drawing.Image.FromFile(path);
                }
                OrigImage.Image = bmp;

                //apply filter
                var proc = blockOcr.ApplyFilters(bmp.Clone() as Bitmap);
                ProcImage.Image = proc;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "NOTICE",
                  System.Windows.Forms.MessageBoxButtons.OK,
                  System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        //add filter
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                GetSettings().FilterList.Add(filterBox.SelectedItem.ToString());
                filterLB.SelectedIndex = filterLB.Items.IndexOf(filterBox.SelectedItem.ToString()); SetFilters();
            }
            catch (Exception ex) { }
        }

        private void SetFilters()
        {
            try
            {
                blockOcr.SetFilters(filterLB.Items.OfType<string>().ToArray());
            }
            catch { }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SaveFileDialog()
                {
                    Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png"
                };
                if (dialog.ShowDialog() == true)
                {
                    ProcImage.Image.Save(dialog.FileName);
                }
            }
            catch { }
        }


        private void InitFilterControls()
        {
            try
            {
                Enum.GetNames(typeof(EngineMode)).ToList()
                   .ForEach(e => engineComboBox.Items.Add(e));
            }
            catch { }
            try
            {
                Enum.GetNames(typeof(PageSegMode)).ToList()
                   .ForEach(p => pageSegComboBox.Items.Add(p));
            }
            catch { }
        }

        private void filterLB_LostFocus(object sender, RoutedEventArgs e)
        {
            if (filterLB.Items.Count == 0) return;

            blockOcr.SetFilters(filterLB.Items.OfType<string>().ToArray());
        }

        private void filterLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var filterLB = sender as ListBox;
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
                    scrollFilterTabControl.Visibility = Visibility.Visible;
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
                        SetInInput(--seletedIndex, new[] { "0" });
                        break;

                    case "blur":
                        SetSize(--seletedIndex);
                        break;

                    case "constrain":
                        SetSize(--seletedIndex);
                        break;

                    case "backgroundcolor":
                    case "tint":
                    case "vignette":
                        SetInInput(--seletedIndex, new string[] { "0,0,0" }, "Color(R,G,B)", true);
                        break;

                    case "gaussianblur":
                    case "gaussiansharpen":
                        SetInInput(--seletedIndex, new string[] { "0" }, "Size");
                        break;

                    case "rotate":
                        SetInInput(--seletedIndex, new string[] { "0" }, "Degrees");
                        break;

                    case "halftone":
                        SetInInputBoolean(--seletedIndex, "False", "Comic Mode");
                        break;

                    case "roundedcorners":
                    case "edge":
                        SetInInput(--seletedIndex, new string[] { "0" }, "Radius");
                        break;

                    case "median":
                        SetInInput(--seletedIndex, new[] { "0" }, "ksize");
                        break;

                    case "crop":
                        SetCropLayer(--seletedIndex, new[] { controlCropLayer.LeftTextBox.Text,
                  controlCropLayer.TopTextBox.Text,
                  controlCropLayer.RightTextBox.Text,
                  controlCropLayer.BottomTextBox.Text, "Percentage" });
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

                    case "alignment":
                        SetInInput(--seletedIndex, new[] { "4" }, "Alignment size(must be a power of two)");
                        break;

                    case "fastnlmeansdenoisingcolored":
                        SetFastNlMeansDenoisingColored(--seletedIndex, new[] { "3", "3" });
                        break;

                    case "resolution":
                        SetResolution(--seletedIndex, new[] { "0", "0", "Inch" });
                        break;

                    default:
                        filterTabControl.SelectedIndex = -1;
                        scrollFilterTabControl.Visibility = Visibility.Collapsed;
                        break;
                }
            }
            catch (IndexOutOfRangeException) { }
            catch (ArgumentOutOfRangeException) { }
            catch (ArgumentException) { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR");
            }
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

        private void SetFastNlMeansDenoisingColored(int index, string[] defValues)
        {
            filterTabControl.SelectIndexByHeaderName(UserControlFastNlMeansDenoisingColored.ControlName);
            var strength = GetFilterValues(index, defValues)[0];
            var colorStrength = GetFilterValues(index, defValues)[1];
            SetTextInTextBox(controlFastNlMeansDenoisingColored.StrengthTextBox, strength);
            SetTextInTextBox(controlFastNlMeansDenoisingColored.ColorStrengthTextBox, colorStrength);
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

        private void SetCropLayer(int index, string[] defValues)
        {
            filterTabControl.SelectIndexByHeaderName(UserControlCropLayer.ControlName);

            var value = filterLB.SelectedItem.ToString();

            var cropMode = CropMode.Percentage;
            try
            {
                cropMode = GetFilterValues(index, value.Split(','))[4].ToEnum(CropMode.Percentage);
            }
            catch { }
            var values = GetFilterValues(index, defValues);
            var left = values[0];
            var top = values[1];
            var right = values[2];
            var bottom = values[3];
            controlCropLayer.CropModeBox.SelectedItem = cropMode;
            SetTextInTextBox(controlCropLayer.LeftTextBox, left);
            SetTextInTextBox(controlCropLayer.TopTextBox, top);
            SetTextInTextBox(controlCropLayer.RightTextBox, right);
            SetTextInTextBox(controlCropLayer.BottomTextBox, bottom);
            controlCropLayer.CropModeBox.SelectedItem = cropMode;
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

                GetSettings().FilterList[index] = split[0] + ": " + value;
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
            var value = filterLB.Items[index].ToString();

            if (!value.Contains(":")) { value += ": "; }

            var val = value.Split(new char[] { ':' }, 2)[1].Trim()
                .Split(',');

            return new[] { val[0],val[1],val[2], "|",
                    val[3],val[4],val[5],
                    val[6]};
        }

        private void SetCaretIndexAndSelect(TextBox textBox, int index = 1)
        {
            try
            {
                if (textBox.Text == "0")
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

        private void btnfilterUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedIndex = filterLB.SelectedIndex;

                if (selectedIndex > 0)
                {
                    var itemToMoveUp = filterLB.Items[selectedIndex].ToString();
                    var settings = GetSettings();
                    settings.FilterList.RemoveAt(selectedIndex);
                    settings.FilterList.Insert(selectedIndex - 1, itemToMoveUp);
                    filterLB.SelectedIndex = selectedIndex - 1;
                    SetFilters();
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
                    var itemToMoveDown = filterLB.Items[selectedIndex].ToString();
                    var settings = GetSettings();
                    settings.FilterList.RemoveAt(selectedIndex);
                    settings.FilterList.Insert(selectedIndex + 1, itemToMoveDown);
                    filterLB.SelectedIndex = selectedIndex + 1;
                    filterLB_LostFocus(null, null);
                }
            }
            catch { }
        }

        private void btnfilterClone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (filterLB.Items.Count == 0 || lastSelectedIndex == -1) return;
                GetSettings().FilterList.Add(filterLB.SelectedItem.ToString());

            }
            catch { }
        }

        private void btnApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (OrigImage.Image == null) return;
                var proc = blockOcr.ApplyFilters(OrigImage.Image.Clone() as Bitmap);
                ProcImage.Image = proc;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + $"\nInnerException: {ex.InnerException?.Message}\n{ex.InnerException?.InnerException?.Message}", "ERROR",
                                  System.Windows.Forms.MessageBoxButtons.OK,
                                  System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        //Save filters in text file
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (filterLB.Items.Count == 0) return;
                var dialog = new SaveFileDialog()
                {
                    Filter = "Text|*.txt"
                };
                if (dialog.ShowDialog() == true)
                {
                    using (var SaveFile = new StreamWriter(dialog.FileName))
                    {
                        foreach (var item in filterLB.Items)
                        {
                            SaveFile.WriteLine(item.ToString());
                        }
                    }
                }
            }
            catch { }
        }

        //copy selected item from listbox filter
        private void MenuItem1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (filterLB.Items.Count == 0) return;
                Clipboard.SetText(filterLB.SelectedItem.ToString());
            }
            catch { }
        }

        //load filters
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
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
                    GetSettings().FilterList.AddRange(filters);
                    if (filters.Length > 0)
                    {
                        filterLB_LostFocus(null, null);
                    }
                }
            }
            catch { }
        }

        private void sizeModeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ProcImage != null)
                    ProcImage.SizeMode = (System.Windows.Forms.PictureBoxSizeMode)Enum.Parse(typeof(System.Windows.Forms.PictureBoxSizeMode), (sizeModeBox.SelectedItem as ComboBoxItem).Content.ToString(), true);
            }
            catch { }
        }

        private ConfigSettings GetSettings()
        {
            return DataContext as ConfigSettings;
        }

        //disable filter in list
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                var settings = GetSettings();
                var selectedIndex = filterLB.SelectedIndex;
                var item = settings.FilterList[selectedIndex];
                if (item.StartsWith("!")) return;
                item = $"!{item}";
                settings.FilterList[selectedIndex] = item;
                filterTabControl.SelectedIndex = -1;
                scrollFilterTabControl.Visibility = Visibility.Collapsed;
                filterLB_LostFocus(null, null);
            }
            catch { }
        }

        //enable filter in list
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                var settings = GetSettings();
                var selectedIndex = filterLB.SelectedIndex;
                var item = settings.FilterList[selectedIndex];
                if (item.StartsWith("!"))
                {
                    settings.FilterList[selectedIndex] = item.Remove(0, 1);
                    filterLB_LostFocus(null, null);
                }
            }
            catch { }
        }

        private void langBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                blockOcr.OcrLang = langBox.SelectedItem.ToString();
            }
            catch { }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GetSettings().FilterList.Count > 0 && blockOcr.Filters.Count == 0)
                {
                    filterLB_LostFocus(null, null);
                }
            }
            catch { }
            try { chbisBase64_Click(null, null); } catch { }
        }

        //paste
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    var item = Clipboard.GetText();
                    var item2 = item;
                    if (item.Contains(":")) item2 = item.Split(':')[0].Trim();
                    if (filterBox.Items.Contains(item2))
                    {
                        GetSettings().FilterList.Add(item2);
                    }
                }
                catch { }
            }
            catch { }
        }

        private void ProcImage_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                if (ProcImage.Image == null) return;
                var color = GetPixelInfo(e.X, e.Y);
                Clipboard.SetText(color.R + "," + color.G + "," + color.B);
            }
            catch { }
        }

        private void ProcImage_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                if (ProcImage.Image == null ||
                    clicked) return;

                var color = GetPixelInfo(e.X, e.Y);
                pixelInfo.Text = e.X + "," + e.Y + ": RGBA(" + color.R + "-" + color.G + "-" + color.B + "-" + color.A + "),Saturation(" +
                    color.GetSaturation() + "),Brightness(" +
                    color.GetBrightness() + ")";
            }
            catch { }
        }

        private Color GetPixelInfo(int x, int y)
        {
            try
            {
                var img = (Bitmap)ProcImage.Image;

                float factorX = (float)ProcImage.Width / img.Width;
                float factorY = (float)ProcImage.Height / img.Height;
                return img.GetPixel((int)(x / factorX), (int)(y / factorY));
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Parameter must be positive and < Height"))
                    SB.Logger.Log(ex.Message, LogLevel.Error, true);
                return Color.Transparent;
            }
        }

        private void pixelInfo_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount != 2) return;
                Clipboard.SetText(pixelInfo.Text);
            }
            catch { }
        }

        private void ProcImage_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                clicked = true;
            }
            catch { }
        }

        private void ProcImage_MouseLeave(object sender, EventArgs e)
        {
            try
            {
                clicked = false;
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
