using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace ImageProcessor.Imaging.Helpers.Converters
{
    public static class ImageConverter
    {
        static private object bufferLock = new object();

        public static byte[] ToByteArray(this Image image)
        {
            lock (bufferLock)
                using (MemoryStream ms = new MemoryStream())
                {
                    image.Save(ms, ImageFormat.Bmp);
                    return ms.ToArray();
                }
        }

        public static byte[] ToByteArray(this Image image, ImageFormat format)
        {
            lock (bufferLock)
                using (MemoryStream ms = new MemoryStream())
                {
                    image.Save(ms, format);
                    return ms.ToArray();
                }
        }

        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {

            BitmapData bmpdata = null;

            try
            {
                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int numbytes = bmpdata.Stride * bitmap.Height;
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                return bytedata;
            }
            finally
            {
                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
            }
        }

        public static Image ToImage(this Bitmap bitmap)
        {
            return (Image)bitmap;
        }

        public static Bitmap ToBitmap(this Image image)
        {
            return (Bitmap)image;
        }
    }
}
