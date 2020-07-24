using System;
using System.Collections.Generic;
using System.Drawing;
using ImageProcessor.Common.Exceptions;

namespace ImageProcessor.Processors
{
    public class MakeTransparent : IGraphicsProcessor
    {
        public dynamic DynamicParameter { get; set; }
        public Dictionary<string, string> Settings { get; set; }

        public Image ProcessImage(ImageFactory factory)
        {
            try
            {
                var bmp = factory.Bitmap;
                bmp.MakeTransparent(bmp.GetPixel(1, 1));
                return bmp;
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error processing image with " + this.GetType().Name, ex);
            }
        }
    }
}
