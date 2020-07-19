using OpenCvSharp;

namespace OpenBullet.ImageProcessor.Layers
{
    public class CvtColorLayer
    {
        public CvtColorLayer(ColorConversionCodes code, int dstCn)
        {
            Code = code;
            DstCn = dstCn;
        }
        public ColorConversionCodes Code { get; set; }
        public int DstCn { get; set; }
    }
}
