using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using ImageProcessor.Common.Exceptions;

namespace ImageProcessor.Processors
{
    public class ColorThreshold : IGraphicsProcessor
    {
        public dynamic DynamicParameter { get; set; }
        public Dictionary<string, string> Settings { get; set; }

        public Image ProcessImage(ImageFactory factory)
        {
            var image = factory.Image;
            Bitmap newBmp = null;
            try
            {
                // Make the result bitmap.
                Bitmap bm = new Bitmap(image.Width, image.Height);

                // Make the ImageAttributes object and set the threshold.
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetThreshold((float)DynamicParameter);

                // Draw the image onto the new bitmap
                // while applying the new ColorMatrix.
                Point[] points =
                {
            new Point(0, 0),
            new Point(image.Width, 0),
            new Point(0, image.Height),
             };
                Rectangle rect =
                    new Rectangle(0, 0, image.Width, image.Height);
                using (Graphics gr = Graphics.FromImage(bm))
                {
                    gr.DrawImage(image, points, rect,
                        GraphicsUnit.Pixel, attributes);
                }

                // Return the result.
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
