using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace SilverBullet.ImageProcessor
{
    public class LockBitmap : IDisposable
    {
        private readonly Bitmap _bitmap;
        private readonly int _width;
        private readonly int _height;
        private readonly Rectangle _rect;
        private readonly PixelFormat _pixelFormat;
        private BitmapData _bitmapData;
        private IntPtr _ptr;
        private int _length;
        private byte[] _values;
        private readonly int _depth;
        private readonly int _colorLength;

        public byte[] Pixels => new byte[_width * _height * _colorLength];
        public int Depth => _depth;
        public int Width => _width;
        public int Height => _height;
        public Bitmap Source { get; private set; }

        public bool IsModify { get; private set; }

        public LockBitmap(Bitmap bitmap)
        {
            Source = bitmap;
            _bitmap = bitmap;
            _width = _bitmap.Width;
            _height = _bitmap.Height;
            _rect = new Rectangle(0, 0, _width, _height);
            _pixelFormat = _bitmap.PixelFormat;
            _depth = Bitmap.GetPixelFormatSize(_bitmap.PixelFormat);
            _colorLength = _depth / 8;
            IsModify = false;
        }

        public void LockBits()
        {
            // Check if bpp (Bits Per Pixel) is 8, 24, or 32
            if (_depth != 8 && _depth != 24 && _depth != 32)
            {
                throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
            }

            _bitmapData = _bitmap.LockBits(_rect, ImageLockMode.ReadWrite, _pixelFormat);
            _ptr = _bitmapData.Scan0;
            _length = _bitmapData.Stride * _height;
            _values = new byte[_length];
            System.Runtime.InteropServices.Marshal.Copy(_ptr, _values, 0, _length);
        }

        /// <summary>
        /// Unlock bitmap data
        /// </summary>
        public void UnlockBits()
        {
            if (IsModify)
            {
                // Copy data from byte array to pointer
                System.Runtime.InteropServices.Marshal.Copy(_values, 0, _ptr, _length);
            }
            _bitmap.UnlockBits(_bitmapData);
        }

        /// <summary>
        /// Get the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Color GetPixel(int x, int y)
        {
            int k = _colorLength * x;
            int pos = y * _bitmapData.Stride + k;
            Color color = Color.Empty;

            if (pos > _values.Length - _colorLength)
                throw new IndexOutOfRangeException();
            byte b, g, r, a;
            switch (_depth)
            {
                case 32:
                    // For 32 bpp get Red, Green, Blue and Alpha
                    b = _values[pos];
                    g = _values[pos + 1];
                    r = _values[pos + 2];
                    a = _values[pos + 3];
                    color = Color.FromArgb(a, r, g, b);
                    break;
                case 24:
                    // For 24 bpp get Red, Green and Blue
                    b = _values[pos];
                    g = _values[pos + 1];
                    r = _values[pos + 2];
                    color = Color.FromArgb(r, g, b);
                    break;
                case 8:
                    // For 8 bpp get color value (Red, Green and Blue values are the same)
                    b = _values[pos];
                    color = Color.FromArgb(b, b, b);
                    break;
            }

            return color;
        }

        /// <summary>
        /// Set the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void SetPixel(int x, int y, Color color)
        {
            int k = _colorLength * x;
            int pos = y * _bitmapData.Stride + k;
            switch (_depth)
            {
                case 32:
                    // For 32 bpp set Red, Green, Blue and Alpha
                    _values[pos] = color.B;
                    _values[pos + 1] = color.G;
                    _values[pos + 2] = color.R;
                    _values[pos + 3] = color.A;
                    break;
                case 24:
                    // For 24 bpp set Red, Green and Blue
                    _values[pos] = color.B;
                    _values[pos + 1] = color.G;
                    _values[pos + 2] = color.R;
                    break;
                case 8:
                    // For 8 bpp set color value (Red, Green and Blue values are the same)
                    _values[pos] = color.B;
                    break;
            }

            IsModify = true;
        }

        public void Dispose()
        {
            _bitmap?.Dispose();
            UnlockBits();
        }
    }
}
