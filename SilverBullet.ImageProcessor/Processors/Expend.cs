using System;
using System.Collections.Generic;
using System.Drawing;
using ImageProcessor.Common.Exceptions;
using ImageProcessor.Imaging.Helpers;
using ImageProcessor.Imaging.Helpers.Converters;

namespace ImageProcessor.Processors
{
    public class Expend : IGraphicsProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Expend"/> class.
        /// </summary>
        public Expend() => this.Settings = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets DynamicParameter.
        /// </summary>
        public dynamic DynamicParameter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets any additional settings required by the processor.
        /// </summary>
        public Dictionary<string, string> Settings
        {
            get;
            set;
        }

        public Image ProcessImage(ImageFactory factory)
        {
            var b = factory.Image.ToBitmap();
            Bitmap newBmp = null;

            try
            {
                int height = b.Height;
                int width = b.Width;
                newBmp = new Bitmap(width, height);

                bool[] pixels;
                for (int i = 1; i < width - 1; i++)
                {
                    for (int j = 1; j < height - 1; j++)
                    {

                        if (b.GetPixel(i, j).R != 0)
                        {
                            pixels = ImageMaths.GetRoundPixel(b, i, j);
                            for (int k = 0; k < pixels.Length; k++)
                            {
                                if (pixels[k] == true)
                                {
                                    //set this piexl's color to black
                                    newBmp.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                newBmp?.Dispose();
                throw new ImageProcessingException("Error processing image with " + GetType().Name, ex);
            }

            return newBmp;
        }
    }
}
