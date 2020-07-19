using System;
using System.Collections.Generic;
using System.Drawing;

namespace ImageProcessor.Processors
{
    public class FaceWhiten : IGraphicsProcessor
    {
        public dynamic DynamicParameter { get; set; }
        public Dictionary<string, string> Settings { get; set; }

        public Image ProcessImage(ImageFactory factory)
        {
            var bitmap = factory.Bitmap;
            int x, y;
            for (x = 1; x < bitmap.Width - 1; x++)
            {
                for (y = 1; y < bitmap.Height - 1; y++)
                {

                    Color pixelColor = bitmap.GetPixel(x, y);
                    int oldR = pixelColor.R;
                    int oldG = pixelColor.G;
                    int oldB = pixelColor.B;
                    if (oldR > oldG && oldG > oldB && Math.Abs(oldR - oldG) > 30)
                    {
                        int brighteness = 30;

                        int newR = Convert.ToInt32((((pixelColor.R / 255.0 - 0.5) * 1.2 + 0.5)) * 255) + brighteness;
                        int newG = Convert.ToInt32((((pixelColor.G / 255.0 - 0.5) * 1.2 + 0.5)) * 255) + brighteness;
                        int newB = Convert.ToInt32((((pixelColor.B / 255.0 - 0.5) * 1.1 + 0.5)) * 255) + brighteness;



                        if (newR < 0)
                            newR = 0;
                        else if (newR > 255)
                            newR = 255;

                        if (newB < 0)
                            newB = 0;
                        else if (newB > 255)
                            newB = 255;

                        if (newG < 0)
                            newG = 0;
                        else if (newG > 255)
                            newG = 255;

                        Color newColor = Color.FromArgb(newR, newG, newB);
                        bitmap.SetPixel(x, y, newColor);
                    }
                }
            }
            return bitmap;
        }
    }
}
