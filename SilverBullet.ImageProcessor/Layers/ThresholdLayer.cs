using OpenCvSharp;

namespace OpenBullet.ImageProcessor.Layers
{
    public class ThresholdLayer
    {
        public ThresholdLayer(double thresh,double maxval,ThresholdTypes thresholdType)
        {
            Thresh = thresh;
            MaxValue = maxval;
            ThresholdType = thresholdType;
        }
        public double Thresh { get; private set; }
        public double MaxValue { get; private set; }
        public ThresholdTypes ThresholdType { get; private set; }

    }
}
