using OpenCvSharp;

namespace OpenBullet.ImageProcessor.Layers
{
    public class AdaptiveThresholdLayer
    {
        public AdaptiveThresholdLayer(double maxValue, AdaptiveThresholdTypes adaptiveMethod,
              ThresholdTypes thresholdType, int blockSize, double c)
        {
            MaxValue = maxValue;
            AdaptiveMethod = adaptiveMethod;
            ThresholdType = thresholdType;
            BlockSize = blockSize;
            C = c;
        }
        
        public double MaxValue { get; private set; }
        public AdaptiveThresholdTypes AdaptiveMethod { get; private set; }
        public ThresholdTypes ThresholdType { get; private set; }
        public int BlockSize { get; private set; }
        public double C { get; private set; }
    }
}
