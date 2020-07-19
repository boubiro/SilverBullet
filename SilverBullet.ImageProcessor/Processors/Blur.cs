using System;
using System.Collections.Generic;
using System.Drawing;
using ImageProcessor.Common.Exceptions;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Helpers.Converters;
using SilverBullet.ImageProcessor.Imaging.Helpers;

namespace ImageProcessor.Processors
{
    public class Blur : IGraphicsProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Blur"/> class.
        /// </summary>
        public Blur() => this.Settings = new Dictionary<string, string>();

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
            Bitmap proc = null;
            try
            {
                ConvMatrix m = new ConvMatrix();
                m.SetAll(1);
                var nWeight = (int)DynamicParameter;
                m.Pixel = nWeight;
                m.TopMid = m.MidLeft = m.MidRight = m.BottomMid = 2;
                m.Factor = nWeight + 12;
                proc = ImageHelper.Conv3x3(factory.Image.ToBitmap(), m);
            }
            catch (Exception ex)
            {
                proc?.Dispose();

                throw new ImageProcessingException("Error processing image with " + GetType().Name, ex);
            }
            return proc;
        }
    }
}
