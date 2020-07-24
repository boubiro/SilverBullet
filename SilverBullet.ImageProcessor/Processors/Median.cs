using System;
using System.Collections.Generic;
using System.Drawing;
using ImageProcessor.Common.Exceptions;
using SilverBullet.ImageProcessor.Imaging.Helpers;

namespace ImageProcessor.Processors
{
    public class Median : IGraphicsProcessor
    {
        public dynamic DynamicParameter { get; set; }
        public Dictionary<string, string> Settings { get; set; }

        public Image ProcessImage(ImageFactory factory)
        {
            try
            {
                var ksize = (int)DynamicParameter;
                return ImageHelper.OpenCvProcessor(factory.Bitmap,
                    (src) => src.MedianBlur(ksize));
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error processing image with " + this.GetType().Name, ex);
            }
        }
    }
}
