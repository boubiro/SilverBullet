using System;
using System.Collections.Generic;
using System.Drawing;
using ImageProcessor.Common.Exceptions;
using OpenBullet.ImageProcessor.Layers;
using SilverBullet.ImageProcessor.Imaging.Helpers;

namespace ImageProcessor.Processors
{
    public class MorphologyEx : IGraphicsProcessor
    {
        public dynamic DynamicParameter { get; set; }
        public Dictionary<string, string> Settings { get; set; }

        public Image ProcessImage(ImageFactory factory)
        {
            var layer = (MorphologyLayer)DynamicParameter;
            Bitmap newBmp = null;
            try
            {
                if (layer.Kernel != null)
                {
                    newBmp = ImageHelper.OpenCvProcessor(factory.Bitmap,
                         (src) => src.MorphologyEx(layer.MorphTypes, layer.Kernel,
                         iterations: layer.Iterations, borderType: layer.BorderTypes));
                }
                else
                {
                    newBmp = ImageHelper.OpenCvProcessor(factory.Bitmap, (src) =>
                       src.MorphologyEx(layer.MorphTypes, null, iterations: layer.Iterations,
                       borderType: layer.BorderTypes));
                }
            }
            catch (Exception ex)
            {
                layer.Kernel?.Dispose();
                throw new ImageProcessingException("Error processing image with " + this.GetType().Name, ex);
            }

            layer.Kernel?.Dispose();
            return newBmp;
        }
    }
}
