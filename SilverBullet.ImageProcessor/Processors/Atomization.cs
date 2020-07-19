using System;
using System.Collections.Generic;
using System.Drawing;
using ImageProcessor.Common.Exceptions;
using ImageProcessor.Imaging.Helpers.Converters;
using SilverBullet.ImageProcessor;

namespace ImageProcessor.Processors
{
    public class Atomization : IGraphicsProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Atomization"/> class.
        /// </summary>
        public Atomization() => this.Settings = new Dictionary<string, string>();

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
            var b = factory.Image.ToBitmap();
            Bitmap newbmp = null;
            try
            {
                int height = b.Height;
                int width = b.Width;
                newbmp = new Bitmap(width, height);

                LockBitmap lbmp = new LockBitmap(b);
                LockBitmap newlbmp = new LockBitmap(newbmp);
                lbmp.LockBits();
                newlbmp.LockBits();

                Random MyRandom = new Random();
                Color pixel;
                for (int x = 1; x < width - 1; x++)
                {
                    for (int y = 1; y < height - 1; y++)
                    {
                        int k = MyRandom.Next(123456);

                        int dx = x + k % 19;
                        int dy = y + k % 19;
                        if (dx >= width)
                            dx = width - 1;
                        if (dy >= height)
                            dy = height - 1;
                        pixel = lbmp.GetPixel(dx, dy);
                        newlbmp.SetPixel(x, y, pixel);
                    }
                }
                lbmp.UnlockBits();
                newlbmp.UnlockBits();
            }
            catch (Exception ex)
            {
                newbmp?.Dispose();

                throw new ImageProcessingException("Error processing image with " + GetType().Name, ex);
            }

            return newbmp;
        }
    }
}
