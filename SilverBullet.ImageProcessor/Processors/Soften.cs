using System;
using System.Collections.Generic;
using System.Drawing;
using ImageProcessor.Common.Exceptions;
using SilverBullet.ImageProcessor;

namespace ImageProcessor.Processors
{
    public class Soften : IGraphicsProcessor
    {
        public dynamic DynamicParameter { get; set; }
        public Dictionary<string, string> Settings { get; set; }

        public Image ProcessImage(ImageFactory factory)
        {
            var bmp = factory.Bitmap;
            Bitmap newbmp = null;
            try
            {
                int height = bmp.Height;
                int width = bmp.Width;
                newbmp = new Bitmap(width, height);

                LockBitmap lbmp = new LockBitmap(bmp);
                LockBitmap newlbmp = new LockBitmap(newbmp);
                lbmp.LockBits();
                newlbmp.LockBits();

                Color pixel;

                int[] Gauss = { 1, 2, 1, 2, 4, 2, 1, 2, 1 };
                for (int x = 1; x < width - 1; x++)
                {
                    for (int y = 1; y < height - 1; y++)
                    {
                        int r = 0, g = 0, b = 0;
                        int Index = 0;
                        for (int col = -1; col <= 1; col++)
                        {
                            for (int row = -1; row <= 1; row++)
                            {
                                pixel = lbmp.GetPixel(x + row, y + col);
                                r += pixel.R * Gauss[Index];
                                g += pixel.G * Gauss[Index];
                                b += pixel.B * Gauss[Index];
                                Index++;
                            }
                        }
                        r /= 16;
                        g /= 16;
                        b /= 16;

                        r = r > 255 ? 255 : r;
                        r = r < 0 ? 0 : r;
                        g = g > 255 ? 255 : g;
                        g = g < 0 ? 0 : g;
                        b = b > 255 ? 255 : b;
                        b = b < 0 ? 0 : b;
                        newlbmp.SetPixel(x - 1, y - 1, Color.FromArgb(r, g, b));
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
