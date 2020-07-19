using System.Collections.Generic;
using System.Drawing;
using ImageProcessor.Imaging;
using SilverBullet.ImageProcessor.Imaging.Helpers;

namespace ImageProcessor.Processors
{
    public class Mean : IGraphicsProcessor
    {
        public dynamic DynamicParameter { get; set; }
        public Dictionary<string, string> Settings { get; set; }

        public Image ProcessImage(ImageFactory factory)
        {
            var nWeight = (int)DynamicParameter;
            var m = new ConvMatrix();
            m.SetAll(-1);
            m.Pixel = nWeight;
            m.Factor = nWeight - 8;
            return ImageHelper.Conv3x3(factory.Bitmap, m);
        }
    }
}
