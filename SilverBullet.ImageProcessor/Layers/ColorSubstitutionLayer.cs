using System.Drawing;

namespace OpenBullet.ImageProcessor
{
    public class ColorSubstitutionLayer
    {
        public ColorSubstitutionLayer(int threshold, Color sourceColor, Color newColor)
        {
            Threshold = threshold;
            SourceColor = sourceColor;
            NewColor = newColor;
        }

        //private int thresholdValue = 10;
        public int Threshold { get; private set; }

        //private Color sourceColor = Color.White;
        public Color SourceColor { get; private set; }

        //private Color newColor = Color.White;
        public Color NewColor { get; private set; }
    }
}
