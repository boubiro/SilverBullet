using System;
using System.Linq;
using ImageProcessor.Imaging.Helpers.Converters;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using PluginFramework;
using PluginFramework.Attributes;
using RuriLib;
using RuriLib.LS;
using Tesseract;

namespace SilverBullet.Plugin
{
    public class McDonaldsCapReader : BlockBase, IBlockPlugin
    {

        public McDonaldsCapReader()
        {
            Label = nameof(McDonaldsCapReader);
        }

       //data:image/png;base64,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

        public string Name => nameof(McDonaldsCapReader);

        public string Color => "#FFFFA500";

        public bool LightForeground => false;

        private string base64Captcha;

        private string variableName;

        [Text("VariableName:")]
        public string InputVariableName
        {
            get { return variableName; }
            set { variableName = value; OnPropertyChanged(); }
        }

        [Text("Base64:")]
        public string Base64Captcha
        {
            get { return base64Captcha; }
            set { base64Captcha = value; OnPropertyChanged(); }
        }

        public override void Process(BotData data)
        {
            base.Process(data);
            var blockOcr = new BlockOcr();
            using (var captcha = Decaptcha(blockOcr, data))
            {
                using (var pix = PixConverter.ToPix(captcha.ConvertPixelFormat(System.Drawing.Imaging.PixelFormat.Format24bppRgb)))
                {
                    InsertVariable(data, false, blockOcr.GetOcr(data, pix).First(),
                        InputVariableName);
                }
            }
        }

        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);

            writer.Label(Label)
                .Token(nameof(McDonaldsCapReader))
                .Literal(Base64Captcha);

            if (!writer.CheckDefault(InputVariableName, nameof(InputVariableName)))
            {
                writer.Arrow()
                    .Token("VAR")
                    .Literal(InputVariableName)
                    .Indent();
            }

            return writer.ToString();
        }

        public override BlockBase FromLS(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Parse the label
            if (input.StartsWith("#"))
                Label = LineParser.ParseLabel(ref input);

            Base64Captcha = LineParser.ParseLiteral(ref input, nameof(Base64Captcha));

            if (LineParser.ParseToken(ref input, TokenType.Arrow, false) == "")
                return this;

            // Parse the variable/capture name
            try { InputVariableName = LineParser.ParseToken(ref input, TokenType.Literal, true); }
            catch { throw new ArgumentException("Variable name not specified"); }

            return this;
        }

        System.Drawing.Bitmap Decaptcha(BlockOcr blockOcr, BotData data)
        {
            var captcha = blockOcr.Base64ImageDecoder(ReplaceValues(Base64Captcha, data));

            using (var src = captcha.ToMat())
            {
                using (var binaryMask = new Mat())
                {
                    var linesColor = Scalar.FromRgb(0x74, 0x74, 0x74);

                    Cv2.InRange(src, linesColor, linesColor, binaryMask);
                    using (var masked = new Mat())
                    {
                        src.CopyTo(masked, binaryMask);
                        int linesDilate = 3;
                        using (var element = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(linesDilate, linesDilate)))
                        {
                            Cv2.Dilate(masked, masked, element);
                        }

                        try
                        {
                            Cv2.CvtColor(masked, masked, ColorConversionCodes.RGB2GRAY);
                        }
                        catch { }

                        using (var dst = src.EmptyClone())
                        {
                            Cv2.Inpaint(src, masked, dst, 3, InpaintMethod.NS);

                            linesDilate = 2;
                            using (var element = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(linesDilate, linesDilate)))
                            {
                                Cv2.Dilate(dst, dst, element);
                            }

                            Cv2.GaussianBlur(dst, dst, new Size(5, 5), 0);
                            using (var dst2 = dst.BilateralFilter(5, 75, 75))
                            {
                                try
                                {
                                    Cv2.CvtColor(dst2, dst2, ColorConversionCodes.RGB2GRAY);
                                }
                                catch { }

                                Cv2.Threshold(dst2, dst2, 255, 255, ThresholdTypes.Otsu);

                                return dst2.ToBitmap().Clone() as System.Drawing.Bitmap;
                            }
                        }
                    }
                }
            }
        }
    }
}
