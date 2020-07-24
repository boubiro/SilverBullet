using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace ImageProcessor.Processors
{
    public class ReduceNoise : IGraphicsProcessor
    {
        public dynamic DynamicParameter { get; set; }
        public Dictionary<string, string> Settings { get; set; }

        static object locker = new object();

        public Image ProcessImage(ImageFactory factory)
        {
            var b = factory.Bitmap;

            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            };

            var width = b.Width;
            var height = b.Height;
            Parallel.For(0, width, options, x =>
            {
                Parallel.For(0, height, options, y =>
                {
                    Color c;
                    lock (locker) c = b.GetPixel(x, y);
                    if ((c.R + c.G + c.B) / 3 > 0x60)
                        lock (locker) b.SetPixel(x, y, Color.White);
                    else
                        lock (locker) b.SetPixel(x, y, Color.Black);
                });
            });

            //for (int x = 0; x < b.Width; x++)
            //{
            //    for (int y = 0; y < b.Height; y++)
            //    {
            //        Color c = b.GetPixel(x, y);
            //        if (c != Color.White)
            //        {
            //            int size = 0;
            //            Bitmap tmp = (Bitmap)b.Clone();
            //            CalcArea(ref tmp, x, y, ref size);
            //            if (size < 60)
            //            {
            //                b.Dispose();
            //                b = tmp;
            //            }
            //            else
            //            {
            //                tmp.Dispose();
            //            }
            //        }
            //}
            return b;
        }

        static private void CalcArea(ref Bitmap bm, int x, int y, ref int size)
        {
            if (x < 0 || x >= bm.Width || y < 0 || y >= bm.Height) return;
            Color c = bm.GetPixel(x, y);
            if (c.R == 0xff && c.G == 0xff && c.B == 0xff)
            {
                return;
            }
            else
            {
                size++;
                bm.SetPixel(x, y, Color.White);
                CalcArea(ref bm, x - 1, y, ref size);
                CalcArea(ref bm, x, y - 1, ref size);
                CalcArea(ref bm, x + 1, y, ref size);
                CalcArea(ref bm, x, y + 1, ref size);
            }
        }
    }
}
