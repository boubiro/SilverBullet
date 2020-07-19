using OpenCvSharp;

namespace OpenBullet.ImageProcessor.Layers
{
    public class MorphologyLayer
    {
        public MorphologyLayer(MorphTypes op,
        int iterations, BorderTypes borderTypes,
        MorphShapes? morphShapes = null, int? width=null, int? height=null)
        {
            MorphTypes = op;
            BorderTypes = borderTypes;
            Iterations = iterations;
            if (morphShapes != null && width != null && height != null)
            {
                Kernel = Cv2.GetStructuringElement(morphShapes.Value, new Size(width.Value,
                    height.Value));
            }
        }
        public MorphTypes MorphTypes { get; private set; }
        public BorderTypes BorderTypes { get; set; }
        public int Iterations { get; private set; }
        public Mat Kernel { get; private set; }
    }
}
