using System.Drawing;
using System.Drawing.Imaging;

namespace OpenBullet.ImageProcessor
{
    public abstract class PixelFormatConvert
    {
        public static Bitmap To(Bitmap orig, PixelFormat pixelFormat)
        {
            Bitmap clone = new Bitmap(orig.Width, orig.Height,
             pixelFormat);

            using (Graphics gr = Graphics.FromImage(clone))
            {
                gr.DrawImage(orig, new Rectangle(0, 0, clone.Width, clone.Height));
            }
            return clone;
        }
    }
}
