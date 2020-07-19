using System;
using System.Collections.Generic;
using System.Drawing;
using ImageProcessor.Common.Exceptions;
using ImageProcessor.Imaging.Helpers.Converters;
using SilverBullet.ImageProcessor;

namespace ImageProcessor.Processors
{
    public class Embossment : IGraphicsProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Embossment"/> class.
        /// </summary>
        public Embossment() => this.Settings = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets DynamicParameter.
        /// </summary>
        public dynamic DynamicParameter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets any additional settings required by the processor.
        /// </summary>
        public Dictionary<string, string> Settings
        {
            get;
            set;
        }

        public Image ProcessImage(ImageFactory factory)
        {
            var bmp = factory.Image.ToBitmap();
            Bitmap newBmp = null;
            try
            {
                int height = bmp.Height;
                int width = bmp.Width;
                Bitmap newbmp = new Bitmap(width, height);

                LockBitmap lbmp = new LockBitmap(bmp);
                LockBitmap newlbmp = new LockBitmap(newbmp);
                lbmp.LockBits();
                newlbmp.LockBits();

                Color pixel1, pixel2;
                for (int x = 0; x < width - 1; x++)
                {
                    for (int y = 0; y < height - 1; y++)
                    {
                        int r = 0, g = 0, b = 0;
                        pixel1 = lbmp.GetPixel(x, y);
                        pixel2 = lbmp.GetPixel(x + 1, y + 1);
                        r = Math.Abs(pixel1.R - pixel2.R + 128);
                        g = Math.Abs(pixel1.G - pixel2.G + 128);
                        b = Math.Abs(pixel1.B - pixel2.B + 128);
                        if (r > 255)
                            r = 255;
                        if (r < 0)
                            r = 0;
                        if (g > 255)
                            g = 255;
                        if (g < 0)
                            g = 0;
                        if (b > 255)
                            b = 255;
                        if (b < 0)
                            b = 0;
                        newlbmp.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }
                lbmp.UnlockBits();
                newlbmp.UnlockBits();
            }
            catch (Exception ex)
            {
                newBmp?.Dispose();

                throw new ImageProcessingException("Error processing image with " + GetType().Name, ex);
            }

            return newBmp;
        }
    }
}
