using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using ImageProcessor;
using ImageProcessor.Common.Exceptions;
using ImageProcessor.Processors;
using OpenBullet.ImageProcessor;

namespace ImageProcessor.Processors
{
    public class Zoom : IGraphicsProcessor
    {
        public dynamic DynamicParameter { get; set; }
        public Dictionary<string, string> Settings { get; set; }

        public Image ProcessImage(ImageFactory factory)
        {
            Size sz = factory.Bitmap.Size;
            var layer = (ZoomLayer)DynamicParameter;
            Bitmap zoomed = null;
            try
            {
                zoomed = new Bitmap((int)(sz.Width * layer.ZoomFactor), (int)(sz.Height * layer.ZoomFactor));
                using (Graphics g = Graphics.FromImage(zoomed))
                {
                    if (layer.NearestNeighbor) g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    {
                        g.DrawImage(factory.Bitmap, new Rectangle(Point.Empty, zoomed.Size));
                    }
                }
            }
            catch (Exception ex)
            {
                zoomed?.Dispose();
                throw new ImageProcessingException("Error processing image with " + GetType().Name, ex);
            }
            return zoomed;
        }
    }
}
