namespace OpenBullet.ImageProcessor
{
    public class ZoomLayer
    {
        public ZoomLayer(int zoomFactor, bool nearestNeighbor)
        {
            ZoomFactor = zoomFactor;
            NearestNeighbor = nearestNeighbor;
        }
        public int ZoomFactor { get; private set; }

        public bool NearestNeighbor { get; private set; }
    }
}
