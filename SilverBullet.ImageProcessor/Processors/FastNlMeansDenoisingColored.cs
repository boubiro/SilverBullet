using System;
using System.Collections.Generic;
using System.Drawing;
using ImageProcessor.Common.Exceptions;
using ImageProcessor.Imaging.Helpers.Converters;
using OpenBullet.ImageProcessor.Layers;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace ImageProcessor.Processors
{
    public class FastNlMeansDenoisingColored : IGraphicsProcessor
    {
        public dynamic DynamicParameter { get; set; }
        public Dictionary<string, string> Settings { get; set; }

        public Image ProcessImage(ImageFactory factory)
        {
            Bitmap newBmp = null;
            var layer = (FastNlMeansDenoisingColoredLayer)DynamicParameter;
            try
            {
                using (Mat src = Mat.FromImageData(factory.Bitmap.ToByteArray(), ImreadModes.Color))
                {
                    Cv2.FastNlMeansDenoisingColored(src, src, layer.Strength, layer.ColorStrength, 7, 21);
                    newBmp = src.ToBitmap();
                }
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
