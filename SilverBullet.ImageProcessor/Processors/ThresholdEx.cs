using System.Collections.Generic;
using System.Drawing;
using OpenBullet.ImageProcessor.Layers;
using SilverBullet.ImageProcessor.Imaging.Helpers;

namespace ImageProcessor.Processors
{
    public class ThresholdEx : IGraphicsProcessor
    {
        public dynamic DynamicParameter { get; set; }
        public Dictionary<string, string> Settings { get; set; }

        public Image ProcessImage(ImageFactory factory)
        {
            var layer = (ThresholdLayer)DynamicParameter;
            return ImageHelper.OpenCvProcessor(factory.Bitmap,
                 (src) => src.Threshold(layer.Thresh, layer.MaxValue, layer.ThresholdType));
        }
    }
}
