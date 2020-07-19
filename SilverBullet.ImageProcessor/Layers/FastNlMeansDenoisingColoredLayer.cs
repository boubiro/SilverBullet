namespace OpenBullet.ImageProcessor.Layers
{
    public class FastNlMeansDenoisingColoredLayer
    {
        public FastNlMeansDenoisingColoredLayer(float strength,
          float colorStrength)
        {
            Strength = strength;
            ColorStrength = colorStrength;
        }
        public float Strength { get; private set; }
        public float ColorStrength { get; private set; }
    }
}
