using System.Drawing;

namespace OpenBullet.ImageProcessor.Layers
{
    public class ReplaceColorLayer
    {
        public ReplaceColorLayer(Color target, Color replacement, int fuzziness)
        {
            Target = target;
            Replacement = replacement;
            Fuzziness = fuzziness;
        }
        public Color Target { get; private set; }
        public Color Replacement { get; private set; }
        public int Fuzziness { get; private set; }
    }
}
