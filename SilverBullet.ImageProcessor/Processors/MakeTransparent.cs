using System.Collections.Generic;
using System.Drawing;

namespace ImageProcessor.Processors
{
    public class MakeTransparent : IGraphicsProcessor
    {
        public dynamic DynamicParameter { get; set; }
        public Dictionary<string, string> Settings { get; set; }

        public Image ProcessImage(ImageFactory factory)
        {
            var bmp = factory.Bitmap;
            bmp.MakeTransparent(bmp.GetPixel(1, 1));
            return bmp;
        }
    }
}
