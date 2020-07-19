using System.Collections.Generic;
using System.Drawing;
using SilverBullet.ImageProcessor.Imaging.Helpers;

namespace ImageProcessor.Processors
{
    public class Alignment : IGraphicsProcessor
    {
        public dynamic DynamicParameter { get; set; }
        public Dictionary<string, string> Settings { get; set; }

        public Image ProcessImage(ImageFactory factory)
        {
            return ImageHelper.OpenCvProcessor(factory.Bitmap, (src) => src.Alignment((int)DynamicParameter));
        }
    }
}
