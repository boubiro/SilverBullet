using System;
using System.Collections.Generic;
using System.Drawing;
using ImageProcessor;
using ImageProcessor.Common.Exceptions;
using ImageProcessor.Processors;
using OpenBullet.ImageProcessor.Layers;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using SilverBullet.ImageProcessor.Imaging.Helpers;

namespace ImageProcessor.Processors
{
    public class AdaptiveThreshold : IGraphicsProcessor
    {
        public dynamic DynamicParameter { get; set; }
        public Dictionary<string, string> Settings { get; set; }

        public Image ProcessImage(ImageFactory factory)
        {
            var layer = (AdaptiveThresholdLayer)DynamicParameter;
            Bitmap newBmp = null;
            try
            {
                newBmp = ImageHelper.OpenCvProcessor(factory.Bitmap,
                    (src) => src.AdaptiveThreshold(layer.MaxValue, layer.AdaptiveMethod,
                    layer.ThresholdType, layer.BlockSize, layer.C));
            }
            catch (OpenCVException ex)
            {
                if (ex.Message == "src.type() == CV_8UC1")
                {
                    newBmp = ImageHelper.OpenCvProcessor(factory.Bitmap, delegate (Mat src)
                    {
                        Cv2.CvtColor(src, src, ColorConversionCodes.BGR2GRAY);
                        newBmp = src.ToBitmap();
                        return src.AdaptiveThreshold(layer.MaxValue, layer.AdaptiveMethod, layer.ThresholdType, layer.BlockSize, layer.C);
                    });
                }
            }
            catch (Exception ex2)
            {
                newBmp?.Dispose();
                throw new ImageProcessingException("Error processing image with " + GetType().Name, ex2);
            }
            return newBmp;
        }
    }
}
