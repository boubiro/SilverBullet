using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using ImageProcessor.Common.Exceptions;
using ImageProcessor.Imaging.Helpers;

namespace ImageProcessor.Processors
{
    /// <summary>
    /// FilterColorMatrix
    /// </summary>
    public class CMatrix : IGraphicsProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CMatrix"/> class.
        /// </summary>
        public CMatrix() => this.Settings = new Dictionary<string, string>();

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
            return Apply(factory, (ColorMatrix)DynamicParameter);
        }

        public static Image Apply(ImageFactory factory, ColorMatrix colorMatrix)
        {
            Bitmap bmp32BppSource = null,
                bmp32BppDest = null;
            try
            {
                bmp32BppSource = ImageMaths.GetArgbCopy(factory.Image);
                bmp32BppDest = new Bitmap(bmp32BppSource.Width, bmp32BppSource.Height, PixelFormat.Format32bppArgb);

                using (Graphics graphics = Graphics.FromImage(bmp32BppDest))
                {
                    ImageAttributes bmpAttributes = new ImageAttributes();
                    bmpAttributes.SetColorMatrix(colorMatrix);

                    graphics.DrawImage(bmp32BppSource, new Rectangle(0, 0, bmp32BppSource.Width, bmp32BppSource.Height),
                                        0, 0, bmp32BppSource.Width, bmp32BppSource.Height, GraphicsUnit.Pixel, bmpAttributes);

                }
            }
            catch (Exception ex)
            {
                bmp32BppSource?.Dispose();
                throw new ImageProcessingException("Error processing image with " + nameof(CMatrix), ex);
            }

            bmp32BppSource.Dispose();

            return bmp32BppDest;
        }
    }
}
