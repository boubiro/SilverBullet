using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using ImageProcessor;
using ImageProcessor.Common.Exceptions;
using ImageProcessor.Processors;

namespace ImageProcessor.Processors
{
    public class Invert : IGraphicsProcessor
    {
        public dynamic DynamicParameter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Dictionary<string, string> Settings { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Image ProcessImage(ImageFactory factory)
        {
            Bitmap b = factory.Bitmap;
            try
            {
                // GDI+ still lies to us - the return format is BGR, NOT RGB.
                BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                int stride = bmData.Stride;
                System.IntPtr Scan0 = bmData.Scan0;

                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;

                    int nOffset = stride - b.Width * 3;
                    int nWidth = b.Width * 3;

                    for (int y = 0; y < b.Height; ++y)
                    {
                        for (int x = 0; x < nWidth; ++x)
                        {
                            p[0] = (byte)(255 - p[0]);
                            ++p;
                        }
                        p += nOffset;
                    }
                }

                b.UnlockBits(bmData);
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error processing image with " + GetType().Name, ex);
            }
            return b;
        }
    }
}
