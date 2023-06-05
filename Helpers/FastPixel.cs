using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace PhotoEditor.Helpers
{
    public class FastPixel
    {
        public byte[] rgbValues = new byte[] { };

        public System.Drawing.Imaging.BitmapData bmpData;

        private IntPtr bmpPtr;

        private bool _isAlpha = false;

        private Bitmap _bitmap;

        private int _width;

        private int _height;

        public int Width
        {
            get
            {
                return this._width;
            }
        }

        public int Height
        {
            get
            {
                return this._height;
            }
        }

        public bool IsAlphaBitmap
        {
            get
            {
                return this._isAlpha;
            }

            set
            {
                this._isAlpha = value;
            }
        }

        public Bitmap Bitmap
        {
            get
            {
                return this._bitmap;
            }
        }

        public byte[] RgbValues
        {
            get
            {
                return rgbValues;
            }

        }

        public FastPixel Clone() {
            FastPixel fastPixel = new FastPixel();
            fastPixel.rgbValues = new byte[this._isAlpha?this._width*this._height*4: this._width * this._height * 3];
            this.rgbValues.CopyTo(fastPixel.rgbValues, 0);
            fastPixel._width = this._width;
            fastPixel._height = this._height;
            fastPixel._isAlpha = this._isAlpha;
            return fastPixel;
        }

        public FastPixel() { 
        
        }

        public FastPixel(Bitmap bitmap)
        {
            if ((bitmap.PixelFormat == (bitmap.PixelFormat | System.Drawing.Imaging.PixelFormat.Indexed)))
            {
                throw new Exception("Cannot lock an Indexed image.");
                //return;
            }
            this._bitmap = bitmap;
            this._isAlpha = (this.Bitmap.PixelFormat == (this.Bitmap.PixelFormat | System.Drawing.Imaging.PixelFormat.Alpha));
            this._width = bitmap.Width;
            this._height = bitmap.Height;
        }

        public void Lock()
        {
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);

            this.bmpData = this.Bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, this.Bitmap.PixelFormat);
            this.bmpPtr = this.bmpData.Scan0;
            int bytes = this.IsAlphaBitmap ? ((this.Width * this.Height) * 4) : ((this.Width * this.Height) * 3);
            this.rgbValues = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(this.bmpPtr, rgbValues, 0, this.rgbValues.Length);
        }

        public void Unlock()
        {
            System.Runtime.InteropServices.Marshal.Copy(this.rgbValues, 0, this.bmpPtr, this.rgbValues.Length);
            //  Unlock the bits.
            this.Bitmap.UnlockBits(bmpData);
        }

        public void Clear(Color colour)
        {
            if (this.IsAlphaBitmap)
            {
                for (int index = 0; (index
                            <= (this.rgbValues.Length - 1)); index = (index + 4))
                {
                    this.rgbValues[index] = colour.B;
                    this.rgbValues[(index + 1)] = colour.G;
                    this.rgbValues[(index + 2)] = colour.R;
                    this.rgbValues[(index + 3)] = colour.A;
                }
            }
            else
            {
                for (int index = 0; (index
                            <= (this.rgbValues.Length - 1)); index = (index + 3))
                {
                    this.rgbValues[index] = colour.B;
                    this.rgbValues[(index + 1)] = colour.G;
                    this.rgbValues[(index + 2)] = colour.R;
                }
            }
        }

        public void SetPixel(Point location, Color colour)
        {
            this.SetPixel(location.X, location.Y, colour);
        }

        public void SetPixel(int x, int y, Color colour)
        {
            if (this.IsAlphaBitmap)
            {
                int index = (((y * this.Width) + x) * 4);
                this.rgbValues[index] = colour.B;
                this.rgbValues[(index + 1)] = colour.G;
                this.rgbValues[(index + 2)] = colour.R;
                this.rgbValues[(index + 3)] = colour.A;
            }
            else
            {
                int index = (((y * this.Width)
                            + x)
                            * 3);
                this.rgbValues[index] = colour.B;
                this.rgbValues[(index + 1)] = colour.G;
                this.rgbValues[(index + 2)] = colour.R;
            }
        }

        public Color GetPixel(Point location)
        {
            return this.GetPixel(location.X, location.Y);
        }

        public Color GetPixel(int x, int y)
        {
            if (this.IsAlphaBitmap)
            {
                int index = (((y * this.Width) + x) * 4);
                if (index < 0 || index > this.rgbValues.Length - 3)
                {
                    return Color.FromArgb(255, 255, 255);
                }
                int b = this.rgbValues[index];
                int g = this.rgbValues[(index + 1)];
                int r = this.rgbValues[(index + 2)];
                int a = this.rgbValues[(index + 3)];
                return Color.FromArgb(a, r, g, b);
            }
            else
            {
                int index = (((y * this.Width) + x) * 3);
                if (index < 0 || index > this.rgbValues.Length - 3)
                {
                    return Color.FromArgb(255, 255, 255);
                }
                int b = this.rgbValues[index];
                int g = this.rgbValues[(index + 1)];
                int r = this.rgbValues[(index + 2)];
                return Color.FromArgb(r, g, b);
            }
        }

        public static Color GetPixel(byte[] rgbValues, bool IsAlphaBitmap, int Width, int x, int y)
        {
            if (IsAlphaBitmap)
            {
                int index = (((y * Width) + x) * 4);
                if (index < 0 || index > rgbValues.Length - 3)
                {
                    return Color.FromArgb(255, 255, 255);
                }
                int b = rgbValues[index];
                int g = rgbValues[(index + 1)];
                int r = rgbValues[(index + 2)];
                int a = rgbValues[(index + 3)];
                return Color.FromArgb(a, r, g, b);
            }
            else
            {
                int index = (((y * Width) + x) * 3);
                if (index < 0 || index > rgbValues.Length - 3)
                {
                    return Color.FromArgb(255, 255, 255);
                }
                int b = rgbValues[index];
                int g = rgbValues[(index + 1)];
                int r = rgbValues[(index + 2)];
                return Color.FromArgb(r, g, b);
            }
        }
    }
}
