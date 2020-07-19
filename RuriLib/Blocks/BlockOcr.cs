using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using AngleSharp.Text;
using Extreme.Net;
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Processors;
using OpenBullet.ImageProcessor;
using OpenBullet.ImageProcessor.Layers;
using RuriLib.Functions.EvalString;
using RuriLib.Functions.Requests;
using RuriLib.LS;
using RuriLib.Models;
using RuriLib.ViewModels;
using Tesseract;

namespace RuriLib
{
    /// <summary>
    /// A block that can perform Image recognition.
    /// </summary>
    public class BlockOcr : BlockBase
    {
        #region Variables

        private string variableName = "";

        /// <summary>The name of the output variable where the OCR response will be stored.</summary>
        public string VariableName { get { return variableName; } set { variableName = value; OnPropertyChanged(); } }

        private bool isCapture = false;

        /// <summary>Whether the output variable should be marked for Capture.</summary>
        public bool IsCapture { get { return isCapture; } set { isCapture = value; OnPropertyChanged(); } }

        private string url = "";

        /// <summary>The URL of the image.</summary>
        public string Url
        {
            get { return url; }
            set
            {
                url = value.Replace("\n", "");
                OnPropertyChanged();
            }
        }


        private int width;
        /// <summary>
        /// image width
        /// </summary>
        public int Width
        {
            get { return width; }
            set
            {
                width = value;
                OnPropertyChanged();
            }
        }

        private int height;
        /// <summary>
        /// image height
        /// </summary>
        public int Height
        {
            get { return height; }
            set
            {
                height = value;
                OnPropertyChanged();
            }
        }

        private string userAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";

        /// <summary>The User-Agent header to use when grabbing the image.</summary>
        public string UserAgent { get { return userAgent; } set { userAgent = value; OnPropertyChanged(); } }

        /// <summary>
        /// Processors image
        /// </summary>
        public List<(string, string, Type)> Processors { get; set; } = new List<(string, string, Type)>
        {
            { (nameof(EntropyCrop), nameof(EntropyCrop), typeof(byte)) },
            { (nameof(Contrast),nameof(Contrast) , typeof(int)) },
            { (nameof(ContrastEx), nameof(ContrastEx) , typeof(sbyte)) },
            { ("Constrain", "Constrain", typeof(System.Drawing.Size)) },
            { ("Morphology", nameof(MorphologyEx), typeof(MorphologyLayer)) },
            { ("Resize", nameof(ResizeEx), typeof(System.Drawing.Size)) },
            { (nameof(Crop), nameof(Crop) , typeof(CropLayer)) },
            { (nameof(Brightness), nameof(Brightness), typeof(int)) },
            { (nameof(Grayscale),nameof(Grayscale), null) },
            { (nameof(SepiaTone), nameof(SepiaTone), null) },
            { (nameof(Invert),nameof(Invert), null) },
            { (nameof(ReplaceColor), nameof(ReplaceColor), typeof(System.Drawing.Color)) },
            { (nameof(BackgroundColor), nameof(BackgroundColor), typeof(System.Drawing.Color)) },
            { (nameof(ReduceNoise), nameof(ReduceNoise), null) },
            { (nameof(FastNlMeansDenoisingColored),nameof(FastNlMeansDenoisingColored), typeof(FastNlMeansDenoisingColoredLayer)) },
            { ("Threshold",nameof(ThresholdEx), typeof(ThresholdLayer)) },
            { (nameof(AdaptiveThreshold),nameof(AdaptiveThreshold), typeof(AdaptiveThresholdLayer)) },
            { (nameof(ColorThreshold),"Threshold", typeof(int)) },
            { (nameof(Transparency),nameof(Transparency), null) },
            { (nameof(Expend),nameof(Expend), null) },
            { (nameof(Soften),nameof(Soften), null) },
            { (nameof(Atomization),nameof(Atomization), null) },
            { (nameof(Embossment),nameof(Embossment), null) },
            { (nameof(Blur), nameof(Blur), typeof(Size)) },
            { (nameof(GaussianBlur), nameof(GaussianBlur), typeof(int)) },
            { (nameof(Gamma), nameof(Gamma), typeof(float)) },
            { (nameof(Smooth), nameof(Smooth), typeof(int)) },
            { (nameof(Median), nameof(Median), typeof(int)) },
            { (nameof(Mean), nameof(Mean), typeof(int)) },
            { (nameof(Sharpen), nameof(Sharpen), typeof(int)) },
            { (nameof(GaussianSharpen), nameof(GaussianSharpen), typeof(int)) },
            { (nameof(Halftone), nameof(Halftone), typeof(bool)) },
            { (nameof(Hue), nameof(Hue), typeof(HueLayer)) },
            { (nameof(Pixelate), nameof(Pixelate), typeof(Size)) },
            { (nameof(Resolution), nameof(Resolution), typeof(ResolutionLayer)) },
            { (nameof(Rotate), nameof(Rotate), typeof(int)) },
            { (nameof(AutoRotate), nameof(AutoRotate), null) },
            { (nameof(Tint),nameof(Tint), typeof(System.Drawing.Color)) },
            { (nameof(Vignette), nameof(Vignette), typeof(System.Drawing.Color)) },
            { (nameof(Saturation), nameof(Saturation), typeof(int)) },
            { ("RemoveBackground", "RemoveBackground", null) },
            { (nameof(FaceWhiten), nameof(FaceWhiten), null) },
            { (nameof(Zoom), nameof(Zoom), typeof(ZoomLayer)) },
            { (nameof(Alpha), nameof(Alpha), typeof(int)) },
            { (nameof(Alignment), nameof(Alignment), typeof(int)) },
        };

        /// <summary>The filters that are set for image</summary>
        public List<ImageFilter> Filters { get; set; } =
           new List<ImageFilter>();

        /// <summary>The custom headers that are sent in the HTTP request.</summary>
        public Dictionary<string, string> CustomHeaders { get; set; } = new Dictionary<string, string>() {
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko" },
            { "Pragma", "no-cache" },
            { "Accept", "*/*" }
        };

        /// <summary>
        /// pixel format bitmap
        /// </summary>
        public List<string> PixelFormats { get; set; } = new List<string>();

        private string selectedPixelFormat = "Default";
        public string SelectedPixelFormat
        {
            get { return selectedPixelFormat; }
            set { selectedPixelFormat = value; OnPropertyChanged(); }
        }

        private string engine = "Default";
        public string Engine { get { return engine; } set { engine = value; OnPropertyChanged(); } }

        private string pageSeg = "SingleLine";
        public string PageSeg { get { return pageSeg; } set { pageSeg = value; OnPropertyChanged(); } }


        private string ocrString = "";

        /// <summary>The URL of the image.</summary>
        public string OcrString { get { return ocrString; } set { ocrString = value; OnPropertyChanged(); } }

        private bool isBase64 = false;

        /// <summary>If the image needs converted from Base64.</summary>
        public bool Base64 { get { return isBase64; } set { isBase64 = value; OnPropertyChanged(); } }

        private string ocrLang = "eng";

        /// <summary>Language the Tesseract uses to read the Image.</summary>
        public string OcrLang { get { return ocrLang; } set { ocrLang = value; OnPropertyChanged(); } }

        /// <summary>
        /// 
        /// </summary>
        public float OcrRate { get; set; }

        ObservableCollection<string> languages = new ObservableCollection<string>();
        /// <summary>
        /// ocr language
        /// </summary>
        public ObservableCollection<string> Languages
        {
            get { return languages; }
            set { languages = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// original captcha
        /// </summary>
        public Image OriginalImage { get; set; }
        /// <summary>
        /// applied filters in captcha
        /// </summary>
        public Image ProcessedImage { get; set; }

        #endregion Variables

        /// <summary>
        /// Creates a OCR block.
        /// </summary>
        public BlockOcr()
        {
            Label = "OCR";
        }

        /// <inheritdoc />
        public override void Process(BotData data)
        {
            base.Process(data);
            InsertVariable(data, IsCapture,
                false, GetOcr(data),
                VariableName);
            if (data.IsDebug)
                data.Log(new LogEntry($"OCR Rate: {data.OcrRate}%\n", Colors.LightGreen));
        }

        /// <summary>
        /// Get text from image
        /// </summary>
        /// <param name="data">bot data</param>
        /// <returns>return text from captcha</returns>
        public string[] GetOcr(BotData data)
        {
            var output = string.Empty;

            var pageSeg = PageSeg.ToEnum(PageSegMode.SingleLine);
            TesseractEngine engine = null;

            try { engine = data.OcrEngine.GetOrCreateEngine(data, OcrLang, Engine.ToEnum(EngineMode.Default)); }
            catch (ArgumentOutOfRangeException e)
            {
                data.Log(new LogEntry(e.Message, Colors.Red));
                data.Status = BotStatus.ERROR;
                return null;
            }

            Pix pix = null;
            Page process = null;
            try
            {
                using (pix = GetOcrImage(data))
                {
                    if (pix == null) { data.Status = BotStatus.ERROR; throw new NullReferenceException("image not found"); }

                    using (process = engine.Process(pix, pageSeg))
                    {
                        try
                        {
                            output = process.GetText();
                        }
                        catch (AccessViolationException aex)
                        {
                            data.Log(new LogEntry(aex.Message, Colors.Red));
                            data.Status = BotStatus.ERROR;
                            return null;
                        }
                        catch (InvalidOperationException iex)
                        {
                            data.Log(new LogEntry(iex.Message, Colors.Red));
                            data.Status = BotStatus.ERROR;
                            return null;
                        }

                        if (data != null)
                        {
                            data.OcrRate = process.GetMeanConfidence();
                            OcrRate = data.OcrRate;
                        }
                        else { OcrRate = process.GetMeanConfidence(); }
                    }
                    try
                    {
                        if (data.GlobalSettings.Ocr.SaveImageToCaptchasFolder)
                        {
                            SaveBitmap(output, PixConverter.ToBitmap(pix));
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                data.Log(new LogEntry(ex.Message, Colors.Red));
            }
            finally
            {
                if (process != null && !process.IsDisposed)
                {
                    process.Dispose();
                }
                if (pix != null && !pix.IsDisposed)
                {
                    pix.Dispose();
                }
            }

            if (output.Contains("\n"))
                return output.Split('\n').ToArray();
            else return new[] { output
    };
        }

        /// <summary>
        /// Get text from image
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public string[] GetOcr(Bitmap bitmap,
            EngineMode engineMode,
            PageSegMode pageSegMode,
            bool evaluateMath = false)
        {
            Pix ocr = null;
            var output = new List<string>();

            if (!Directory.Exists(@".\tessdata"))
            {
                throw new DirectoryNotFoundException("tessdata not found!");
            }

            if (!File.Exists($@".\tessdata\{OcrLang}.traineddata"))
            {
                throw new FileNotFoundException($"Language '{OcrLang}' not found!");
            }

            using (var tesseract = new TesseractEngine(@".\tessdata", OcrLang, engineMode))
            {
                tesseract.SetVariable("tessedit_char_whitelist", "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ+-*/=");
                tesseract.SetVariable("tessedit_unrej_any_wd", true);

                if (bitmap == null) bitmap = (Bitmap)OriginalImage;

                bitmap = ApplyFilters(bitmap);

pixConverter:
                try
                {
                    ocr = PixConverter.ToPix(bitmap);
                }
                catch (InvalidOperationException iEx)
                {
                    if (iEx.Message.Contains("Format32bppPArgb") ||
                        iEx.Message.Contains("Format32bppArgb"))
                    {
                        bitmap = bitmap.ConvertPixelFormat(System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        goto pixConverter;
                    }
                    else if (iEx.Message.Contains("Format24bppRgb"))
                    {
                        bitmap = bitmap.ConvertPixelFormat(System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                        goto pixConverter;
                    }
                }

                Graphics g = Graphics.FromImage(bitmap);
                using (var process = tesseract.Process(ocr, pageSegMode))
                {
                    output.Add(process.GetText());

                    try
                    {
                        if (evaluateMath)
                        {
                            var eval = output.FirstOrDefault();
                            eval = GetNumericAndMathOp(eval.Trim());
                            if (eval.Contains("="))
                            {
                                var result = new CodeDomCalculator(eval
                                             .Split('=')[0])
                                             .Calculate().ToString();
                                output.Add(eval + result);
                            }
                            else
                            {
                                var result = new CodeDomCalculator(eval)
                                   .Calculate().ToString();
                                output.Add(eval + "=" + result);
                            }
                        }
                    }
                    catch { }

                    OcrRate = process.GetMeanConfidence();

                    using (var iter = process.GetIterator())
                    {
                        //Note that the general result hierarchy is as follows:
                        //Block -> Para -> TextLine -> Word -> Symbol
                        do
                        {
                            do
                            {
                                do
                                {
                                    do
                                    {
                                        if (iter.IsAtBeginningOf(PageIteratorLevel.Word))
                                        {
                                            // do whatever you need to do when a word is encountered is encountered.
                                            Tesseract.Rect rect;
                                            iter.TryGetBoundingBox(PageIteratorLevel.Word, out rect);

                                            g.SmoothingMode = SmoothingMode.AntiAlias;
                                            g.DrawRectangle(new System.Drawing.Pen(System.Drawing.Brushes.Blue), rect.X1, rect.Y1, rect.Width,
                                                rect.Height);
                                        }

                                        // get bounding box for symbol
                                        if (iter.IsAtBeginningOf(PageIteratorLevel.Symbol))
                                        {
                                            // do whatever you want with bounding box for the symbol
                                            Tesseract.Rect symbolBounds;
                                            if (iter.TryGetBoundingBox(PageIteratorLevel.Symbol, out symbolBounds))
                                            {
                                                Tesseract.Rect rect;
                                                iter.TryGetBoundingBox(PageIteratorLevel.Symbol, out rect);

                                                g.SmoothingMode = SmoothingMode.AntiAlias;
                                                g.DrawRectangle(new System.Drawing.Pen(System.Drawing.Brushes.Red), rect.X1, rect.Y1, rect.Width,
                                                    rect.Height);
                                            }
                                        }
                                    } while (iter.Next(PageIteratorLevel.Word, PageIteratorLevel.Symbol));
                                } while (iter.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));
                            } while (iter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));
                        } while (iter.Next(PageIteratorLevel.Block, PageIteratorLevel.Para));
                    }
                }

                ProcessedImage = bitmap.Clone() as Bitmap;
                g.Dispose();

                if (evaluateMath)
                {
                    return new string[] { output.LastOrDefault()
                    };
                }
                try { ocr.Dispose(); } catch { }
            }
            return output.ToArray();
        }

        private string GetNumericAndMathOp(string input)
        {
            input = new string(input.Where(c => !char.IsLetter(c)).ToArray());
            var match = new Regex(@"([-+]?[0-9]*\.?[0-9]+[\/\+\-\*])+([-+]?[0-9]*\.?[0-9]+)")
                .Match(input);
            if (match.Success)
            {
                return match.Value;
            }
            return input;
        }

        /// <summary>
        /// Graphics on indexed image
        /// </summary>
        /// <param name="src">source</param>
        /// <returns></returns>
        public Bitmap CreateNonIndexedImage(Bitmap src)
        {
            Bitmap newImage = new Bitmap(src.Width, src.Height);
            using (Graphics graphics = Graphics.FromImage(newImage))
            {
                graphics.DrawImage(src, 0, 0);
            }
            return newImage;
        }

        /// <summary>
        /// Get ocr image from url
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Pix GetOcrImage(BotData data)
        {
            Pix pix = null;
            Bitmap captcha = null;
            Bitmap appliedCaptcha = null;

            var inputs = ReplaceValues(Url, data);

            if (Base64)
            {
                byte[] imageBytes = Convert.FromBase64String(FixBase64ForImage(inputs));
                using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                {
                    captcha = (Bitmap)Bitmap.FromStream(ms);
                    OriginalImage = captcha;
                    appliedCaptcha = CreateNonIndexedImage(captcha);

                    appliedCaptcha = ApplyFilters(appliedCaptcha, data);

                    appliedCaptcha = ChangePixelFormat(appliedCaptcha);

                    pix = PixConverter.ToPix(appliedCaptcha);
                }
            }
            else
            {
                var request = new Request();

                // Setup
                request.Setup(data.GlobalSettings, maxRedirects: data.ConfigSettings.MaxRedirects);

                data.Log(new LogEntry($"Calling URL: {inputs}", Colors.MediumTurquoise));

                request.SetStandardContent(string.Empty, "application/x-www-form-urlencoded", HttpMethod.GET, false, GetLogBuffer(data));

                // Set proxy
                if (data.UseProxies)
                {
                    request.SetProxy(data.Proxy);
                }

                // Set headers
                data.Log(new LogEntry("Sent Headers:", Colors.DarkTurquoise));
                var headers = CustomHeaders.Select(h =>
                       new KeyValuePair<string, string>(ReplaceValues(h.Key, data), ReplaceValues(h.Value, data))
                    ).ToDictionary(h => h.Key, h => h.Value);
                request.SetHeaders(headers, log: GetLogBuffer(data));

                // Set cookies
                data.Log(new LogEntry("Sent Cookies:", Colors.MediumTurquoise));
                request.SetCookies(data.Cookies, GetLogBuffer(data));

                // End the request part
                data.LogNewLine();

                // Perform the request
                try
                {
                    (data.Address, data.ResponseCode, data.ResponseHeaders, data.Cookies) = request
                        .Perform(inputs, HttpMethod.GET, GetLogBuffer(data), true);
                }
                catch (Exception ex)
                {
                    if (data.ConfigSettings.IgnoreResponseErrors)
                    {
                        data.Log(new LogEntry(ex.Message, Colors.Tomato));
                        data.ResponseSource = ex.Message;
                        return null;
                    }
                    throw;
                }

                data.ResponseSource = request.SaveString(true, data.ResponseHeaders, GetLogBuffer(data));
                try
                {
                    data.ResponseStream = request.GetMemoryStream();
                }
                catch { }
                data.IsImage = true;
                try
                {
                    captcha = (Bitmap)Image.FromStream(request.GetMemoryStream());
                }
                catch (Exception ex)
                {
                    data.Status = BotStatus.ERROR;
                    throw new Exception("[Set Captcha] " + ex.Message);
                }
                OriginalImage = captcha;

                appliedCaptcha = CreateNonIndexedImage(captcha);

                appliedCaptcha = ApplyFilters(appliedCaptcha, data);

                appliedCaptcha = ChangePixelFormat(appliedCaptcha);

                var pixConverter = false;
                do
                {
                    try
                    {
                        pix = PixConverter.ToPix(appliedCaptcha);
                        try { appliedCaptcha.Dispose(); } catch { }
                        break;
                    }
                    catch (InvalidOperationException iEx)
                    {
                        if (iEx.Message.Contains("Format32bppPArgb") ||
                            iEx.Message.Contains("Format32bppArgb"))
                        {
                            appliedCaptcha = appliedCaptcha.ConvertPixelFormat(System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        }
                        else if (iEx.Message.Contains("Format24bppRgb"))
                        {
                            appliedCaptcha = appliedCaptcha.ConvertPixelFormat(System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                        }
                        pixConverter = true;
                    }
                } while (pixConverter);
            }
            return pix;
        }

        private Bitmap ChangePixelFormat(Bitmap bitmap)
        {
            if (SelectedPixelFormat == "Default")
            {
                return bitmap;
            }
            return bitmap.ConvertPixelFormat(SelectedPixelFormat.ToEnum(System.Drawing.Imaging.PixelFormat.Format32bppArgb));
        }

        /// <summary>
        /// Get ocr image from url
        /// </summary>
        /// <returns></returns>
        public Bitmap GetOcrImage(bool applyFilter = true)
        {
            Bitmap captcha;
            Bitmap appliedCaptcha;

            var inputs = Url;

            if (Base64)
            {
                byte[] imageBytes = Convert.FromBase64String(FixBase64ForImage(inputs));
                using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                {
                    captcha = (Bitmap)Bitmap.FromStream(ms);
                    OriginalImage = captcha;
                    appliedCaptcha = CreateNonIndexedImage(captcha);

                    if (applyFilter)
                        appliedCaptcha = ApplyFilters(appliedCaptcha, null);

                    //Ocr = PixConverter.ToPix(appliedCaptcha);
                }
            }
            else
            {
                var request = new Request();

                // Perform the request
                try
                {
                    request.Perform(inputs, HttpMethod.GET, resToMemoryStream: true);
                }
                catch (Exception ex)
                {
                    throw;
                }

                captcha = (Bitmap)Image.FromStream(request.GetMemoryStream());
                OriginalImage = captcha;
                appliedCaptcha = CreateNonIndexedImage(captcha);

                if (applyFilter)
                    appliedCaptcha = ApplyFilters(appliedCaptcha);

            }
            return appliedCaptcha;
        }


        private List<LogEntry> GetLogBuffer(BotData data) =>
            data.GlobalSettings.General.EnableBotLog || data.IsDebug ? data.LogBuffer : null;

        /// <summary>
        /// Parses values from a string.
        /// </summary>
        /// <param name="input">The string to parse</param>
        /// <param name="separator">The character that separates the elements</param>
        /// <param name="count">The number of elements to return</param>
        /// <returns>The array of the parsed elements.</returns>
        public static string[] ParseString(string input, char separator, int count)
        {
            return input.Split(new[] { separator }, count).Select(s => s.Trim()).ToArray();
        }

        /// <summary>
        /// Builds a string containing custom headers.
        /// </summary>
        /// <returns>One header per line, with name and value separated by a colon</returns>
        public string GetCustomHeaders()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var pair in CustomHeaders)
            {
                sb.Append($"{pair.Key}: {pair.Value}");
                if (!pair.Equals(CustomHeaders.Last())) sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Builds a string containing filters.
        /// </summary>
        /// <returns>One filter per line, with name and value separated by a colon</returns>
        public string GetFilters()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var pair in Filters)
            {
                sb.Append($"{pair.Name}: {pair.Value}");
                if (!pair.Equals(Filters.Last())) sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Sets custom headers from an array of lines.
        /// </summary>
        /// <param name="lines">The lines containing the colon-separated name and value of the headers</param>
        public void SetCustomHeaders(string[] lines)
        {
            CustomHeaders.Clear();
            foreach (var line in lines)
            {
                if (line.Contains(':'))
                {
                    var split = line.Split(new[] { ':' }, 2);
                    CustomHeaders[split[0].Trim()] = split[1].Trim();
                }
            }
        }

        /// <summary>
        /// Sets filters from an array of lines.
        /// </summary>
        /// <param name="lines">The lines containing the colon-separated name and value of the filters</param>
        public void SetFilters(string[] lines)
        {
            Filters.Clear();
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var newLine = line;
                    if (!newLine.Contains(":")) newLine += ":";
                    var split = newLine.Split(new[] { ':' }, 2);
                    Filters.Add(new ImageFilter()
                    {
                        Name = split[0].Trim(),
                        Value = split[1].Trim()
                    });
                }
            }
        }

        /// <inheritdoc/>
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer.Label(Label)
                .Token("OCR")
                .Token(OcrLang)
                .Literal(Url)
                .Boolean(Base64, nameof(Base64));

            if (!writer.CheckDefault(VariableName, nameof(VariableName)))
            {
                writer.Arrow()
                    .Token(IsCapture ? "CAP" : "VAR")
                    .Literal(VariableName)
                    .Indent();
            }

            if (!string.IsNullOrWhiteSpace(Engine) &&
                Engine != "Default")
            {
                writer.Indent()
                    .Token(nameof(Engine).ToUpper())
                    .Token(Engine, nameof(Engine));
            }

            if (!string.IsNullOrWhiteSpace(PageSeg) &&
                PageSeg != "SingleLine")
            {
                writer.Indent()
                    .Token("PAGESEGMENT")
                    .Token(PageSeg, nameof(PageSeg));
            }

            if (!string.IsNullOrWhiteSpace(SelectedPixelFormat) &&
                SelectedPixelFormat != "Default")
            {
                writer.Indent()
                 .Token("PIXELFORMAT")
                 .Token(SelectedPixelFormat, nameof(SelectedPixelFormat));
            }

            writer.Indent();

            CustomHeaders.ToList().ForEach(header =>
            writer.Token(nameof(header).ToUpper())
                    .Literal($"{header.Key}: {header.Value}")
                    .Indent());

            Filters.ToList().ForEach(filter =>
            writer.Token(nameof(filter).ToUpper())
                 .Literal($"{filter.Name}: {filter.Value}")
                 .Indent());

            return writer.ToString();
        }

        /// <inheritdoc/>
        public override BlockBase FromLS(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Parse the label
            if (input.StartsWith("#"))
                Label = LineParser.ParseLabel(ref input);

            OcrLang = LineParser.ParseToken(ref input, TokenType.Parameter, false);
            Url = LineParser.ParseLiteral(ref input, nameof(Url));

            while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                LineParser.SetBool(ref input, this);

            if (LineParser.ParseToken(ref input, TokenType.Arrow, false) == "")
                return this;

            //Parse the VAR / CAP
            try
            {
                var varType = LineParser.ParseToken(ref input, TokenType.Parameter, true);
                if (varType.ToUpper() == "VAR" || varType.ToUpper() == "CAP")
                    IsCapture = varType.ToUpper() == "CAP";
            }
            catch { throw new ArgumentException("Invalid or missing variable type"); }

            // Parse the variable/capture name
            try { VariableName = LineParser.ParseToken(ref input, TokenType.Literal, true); }
            catch { throw new ArgumentException("Variable name not specified"); }

            while (input != "" && LineParser.Lookahead(ref input) == TokenType.Parameter)
            {
                var parsed = LineParser.ParseToken(ref input, TokenType.Parameter, true).ToUpper();
                switch (parsed)
                {
                    case "HEADER":
                        var headerPair = ParsePair(LineParser.ParseLiteral(ref input, "HEADER VALUE"));
                        CustomHeaders[headerPair.Key] = headerPair.Value;
                        break;
                    case "FILTER":
                        var filterPair = ParsePair(LineParser.ParseLiteral(ref input, "FILTER VALUE"), true);
                        Filters.Add(new ImageFilter() { Name = filterPair.Key, Value = filterPair.Value });
                        break;
                    case "PIXELFORMAT":
                        {
                            SelectedPixelFormat = LineParser.ParseToken(ref input, TokenType.Parameter, false);
                        }
                        break;
                    case "ENGINE":
                        {
                            Engine = LineParser.ParseToken(ref input, TokenType.Parameter, false);
                        }
                        break;
                    case "PAGESEGMENT":
                        {
                            PageSeg = LineParser.ParseToken(ref input, TokenType.Parameter, false);
                        }
                        break;
                    default:
                        break;
                }
            }
            return this;
        }

        /// <summary>
        /// Parse pair
        /// </summary>
        /// <param name="pair"></param>
        /// <param name="nullValue"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> ParsePair(string pair, bool nullValue = false)
        {
            if (nullValue)
            {
                if (!pair.Contains(":")) pair += ":";
            }
            var split = pair.Split(new[] { ':' }, 2);
            return new KeyValuePair<string, string>(split[0].Trim(), split[1].Trim());
        }

        /// <summary>
        /// Fix base64 image
        /// </summary>
        /// <param name="Image"></param>
        /// <returns></returns>
        public string FixBase64ForImage(string Image)
        {
            StringBuilder sbText = new StringBuilder(Image, Image.Length);
            sbText.Replace("\r\n", String.Empty); sbText.Replace(" ", String.Empty);
            sbText.Replace(@"\/", "/");
            if (sbText.ToString().Contains("base64,"))
            {
                return sbText.ToString().Split(',')[1];
            }
            return sbText.ToString();
        }

        private void SaveBitmap(string text, Bitmap proc)
        {
            try
            {
                text = text.RemoveIllegalCharacters();

                if (!Directory.Exists("Captchas"))
                {
                    Directory.CreateDirectory("Captchas");
                }

                var format = OriginalImage.GetImageFormat();
                if (format != System.Drawing.Imaging.ImageFormat.Png)
                {
                    OriginalImage.Save($"Captchas\\Original ({text}).Png");
                    proc.Save($"Captchas\\Processed ({text}).Png");
                }
                OriginalImage.Save($"Captchas\\Original ({text}).{format}");
                proc.Save($"Captchas\\Processed ({text}).{format}");
            }
            catch (Exception ex) { }
        }

        /// <summary>
        /// Apply filter 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Bitmap ApplyFilters(Bitmap bitmap, BotData data = null)
        {
            if (Filters.Count <= 0)
            {
                return bitmap;
            }

            Bitmap newBmp = null;

            var taskApplyFilters = Task.Run(() =>
              {
                  using (var imageFactory = new ImageFactory())
                  {
                      imageFactory.Load(bitmap.Clone() as Bitmap);
                      var filterList = GetFilters(imageFactory);
                      for (var i = 0; i < filterList.Count; i++)
                      {
                          if (filterList[i] == null) continue;
                          object objResult;
                          if ((objResult = InvokeFilter(filterList[i], imageFactory, data)) == null)
                          { i--; continue; }
                          newBmp = imageFactory.Bitmap.Clone() as Bitmap;
                      }
                  }
              });
            taskApplyFilters.Wait();
            taskApplyFilters.Dispose();

            return newBmp ?? bitmap;
        }

        private object InvokeFilter(CaptchaFilter filter,
          object obj = null, BotData data = null)
        {
            var input = (obj as ImageFactory).Image as Bitmap;
            //Item3 -> type
            if (filter.ParameterType == null)
            {
                //Item1 -> method filter
                //has extension
                if (filter.Method.GetParameters()
                    .FirstOrDefault()?.ParameterType == typeof(Bitmap))
                {
                    return filter.Method.Invoke(null, new[] { input });
                }
                else
                {
                    return filter.Method.Invoke(obj, null);
                }
            }
            var parametersForMethod = new List<object>();

            object[] myParameters;
            if (filter.Parameter is object[] myPars) { myParameters = myPars; }
            else { myParameters = new[] { filter.Parameter }; }

            var parameters = filter.Method.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameterType = parameters[i].ParameterType;

                if (parameterType == typeof(Bitmap))
                {
                    parametersForMethod.Add(input);
                    continue;
                }

                if (parameters.Length > 0 && myParameters.Length > 0)
                {
                    parametersForMethod.Add(
                    FilterParser.ParseObject(ref myParameters, parameterType)
                    );
                }
            }
            try
            {
                return filter.Method.Invoke(obj, parametersForMethod.ToArray());
            }
            catch (TargetInvocationException tEx)
            {
                if (data != null)
                {
                    //item4 -> filter name
                    data.Log(new LogEntry($"[{filter.Name}]: {tEx.InnerException.Message}", Colors.Red));
                }
                return filter.Method.Invoke(null, parametersForMethod.ToArray());
            }
        }

        private List<CaptchaFilter> GetFilters(ImageFactory imageFactory)
        {
            var filterList = new List<CaptchaFilter>();

            Filters = Filters.Where(f => !f.Name.StartsWith("!"))
                .ToList();

            var fCount = Filters.Count;
            for (var i = 0; i < fCount; i++)
            {
                var filter = Filters.ToList()[i];
                var obj = GetObjectType(filter.Value);
                (string, string, Type) process;
                try
                {
                    process = Processors.First(p => p.Item1 == filter.Name);
                }
                catch (InvalidOperationException) { continue; }

                //process item1 -> filter name
                //process item2 -> method name

                //extentions
                //method
                //method with type

                if (process.Item1 != null &&
                process.Item2 != null)
                {
                    if (process.Item3 != null)
                    {
                        var method = imageFactory.GetType()
                               .GetMethod(process.Item2, new[] { process.Item3
                          });
                        if (method == null)
                        {
                            try
                            {
                                method = imageFactory.GetType()
                                .GetMethod(process.Item2);
                            }
                            catch { }
                        }
                        if (method != null)
                        {
                            filterList.Add(new CaptchaFilter()
                            {
                                Method = method,
                                Parameter = obj,
                                ParameterType = process.Item3,
                                Name = process.Item1
                            });
                        }
                    }
                    else
                    {
                        var method = imageFactory.GetType()
                                .GetMethod(process.Item2);
                        if (method != null)
                        {
                            filterList.Add(new CaptchaFilter()
                            {
                                Method = method,
                                Parameter = obj,
                                ParameterType = process.Item3,
                                Name = process.Item1
                            });
                        }
                    }
                }
            }
            return filterList;
        }

        /// <summary>
        /// Get object type  array OR not array
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private object GetObjectType(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.ToLower() == "null")
            {
                return null;
            }
            if (value.Contains(","))
            {
                return value.Split(new char[] { ',' },
                   StringSplitOptions.RemoveEmptyEntries).ToArray<object>();
            }
            return value;
        }
    }
}
