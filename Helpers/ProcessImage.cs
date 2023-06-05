using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PhotoEditor.Helpers
{
    public class ProcessImage
    {
        #region public static string ImageToString(string path)
        public static string ImageToString(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            Image image = Image.FromFile(path);
            return ImageToString(image);
        }
        #endregion

        #region public static string ImageToString(Image image)
        public static string ImageToString(Image image)
        {
            return Convert.ToBase64String(ImageToByteArray(image));
        }
        #endregion

        #region public static string ImageToString(Image image)
        public static byte[] ImageToByteArray(Image image)
        {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }
        #endregion

        #region public static Image StringToImage(string imageString)
        public static Image StringToImage(string imageString)
        {
            try
            {
                if (imageString == null)
                    throw new ArgumentNullException("imageString");
                byte[] array = Convert.FromBase64String(imageString);
                return ByteArrayToImage(array);//Image.FromStream(new MemoryStream(array));
            }
            catch { return null; }
        }
        #endregion

        #region public static Image StringToImage(string imageString)
        public static Image ByteArrayToImage(byte[] array)
        {
            try
            { return Image.FromStream(new MemoryStream(array)); }
            catch { return null; }
        }
        #endregion

        #region public static Image ResizeImage(...)
        public static Image ResizeImage(string FileName, int Percent)
        {
            return ResizeImage(Image.FromFile(FileName), Percent);
        }

        public static Image RotateImage(Image originalBitmap) {
            // Assuming you have a Bitmap object called originalBitmap and an angle in degrees for rotation
            float angle = 90.0f; // Example angle of 90 degrees

            // Create a new Bitmap object with the rotated dimensions
            Bitmap rotatedBitmap = new Bitmap(originalBitmap.Height, originalBitmap.Width);

            // Create a Graphics object from the rotatedBitmap
            using (Graphics g = Graphics.FromImage(rotatedBitmap))
            {
                // Set the rotation angle and center point for rotation
                g.TranslateTransform(rotatedBitmap.Width / 2, rotatedBitmap.Height / 2);
                g.RotateTransform(angle);

                // Draw the originalBitmap onto the rotatedBitmap with the rotation applied
                g.DrawImage(originalBitmap, new Point(-originalBitmap.Width / 2, -originalBitmap.Height / 2));
            }

            // The rotatedBitmap now contains the rotated image
            return rotatedBitmap;
        }

        public static Image ResizeImage(Image Source, int Percent, System.Drawing.Imaging.PixelFormat PixelFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb)
        {
            int w = (int)((decimal)Source.Width * (decimal)Percent / 100);
            int h = (int)((decimal)Source.Height * (decimal)Percent / 100);
            return ResizeImage(Source, w, h, PixelFormat);
        }

        public static Image ResizeImage(string FileName, int Width, int Height)
        {
            return ResizeImage(Image.FromFile(FileName), Width, Height);
        }

        public static Image ResizeImage(Image Source, int Width, int Height, PixelFormat PixelFormat = PixelFormat.Format24bppRgb)
        {
            if (Width == 0 || Height == 0) {
                return Source;
            }

            Bitmap bmp = new Bitmap(Width, Height, PixelFormat);

            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(Source, new Rectangle(0, 0, Width, Height));

            Image Resp = bmp.GetThumbnailImage(Width, Height, null, IntPtr.Zero);

            Source.Dispose();
            Source = null;
            g.Dispose();
            g = null;
            bmp.Dispose();
            bmp = null;

            return Resp;
        }
        #endregion

        public static Image SetFormat(Image Source, PixelFormat PixelFormat = PixelFormat.Format24bppRgb) {
            Bitmap clone = new Bitmap(Source.Width, Source.Height,PixelFormat);
            Graphics gr = Graphics.FromImage(clone);
            gr.DrawImage(Source, new Rectangle(0, 0, clone.Width, clone.Height));
            return clone;
        }

        public static Image CutImage(Image Source, int x, int y, int Width, int Height, System.Drawing.Imaging.PixelFormat PixelFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb) {
            
            Rectangle cropRect = new Rectangle(x, y, Width, Height);
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            Graphics g = Graphics.FromImage(target);
            g.DrawImage(Source, new Rectangle(0, 0, target.Width, target.Height),
                            cropRect,
                            GraphicsUnit.Pixel);
                
            return target;
        }

        #region public static bool IsPortrait(string FileName)
        public static bool IsPortrait(string FileName)
        {
            return IsPortrait(Image.FromFile(FileName));
        }
        #endregion

        #region public static bool IsPortrait(Image Source)
        public static bool IsPortrait(Image Source)
        {
            int Resp = Source.Height / Source.Width;
            return Resp > 1;
        }
        #endregion

        #region public static bool IsLandscape(string FileName)
        public static bool IsLandscape(string FileName)
        {
            return IsLandscape(Image.FromFile(FileName));
        }
        #endregion

        #region public static bool IsLandscape(Image Source)
        public static bool IsLandscape(Image Source)
        {
            int Resp = Source.Height / Source.Width;
            return Resp < 1;
        }
        #endregion

        #region public static Image SquareImage(Image Source)
        public static Image SquareImage(Image Source)
        {
            System.Drawing.Imaging.PixelFormat PixelFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
            if (Source.Height > Source.Width)
            {
                //Retrato
                System.Drawing.Image Image = new System.Drawing.Bitmap(Source.Width, Source.Width, PixelFormat);
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(Image);
                int dif = (Source.Height - Source.Width) / 2;

                System.Drawing.Rectangle srcRect = new System.Drawing.Rectangle(0, dif, Source.Width, Source.Width);
                System.Drawing.Rectangle desRect = new System.Drawing.Rectangle(0, 0, Source.Width, Source.Width);

                g.DrawImage(Source, desRect, srcRect, System.Drawing.GraphicsUnit.Pixel);
                return Image;
            }
            else
            {
                //Paisagem
                System.Drawing.Image Image = new System.Drawing.Bitmap(Source.Height, Source.Height, PixelFormat);
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(Image);
                int dif = (Source.Width - Source.Height) / 2;

                System.Drawing.Rectangle srcRect = new System.Drawing.Rectangle(dif, 0, Source.Height, Source.Height);
                System.Drawing.Rectangle desRect = new System.Drawing.Rectangle(0, 0, Source.Height, Source.Height);

                g.DrawImage(Source, desRect, srcRect, System.Drawing.GraphicsUnit.Pixel);
                return Image;
            }
        }
        #endregion

        public static byte[] Sharpen(
            BitmapData pbits,
            byte[] rgbValues,
            int width,
            int height,
            double strength) {
            //var rgbValues = new byte[bytes];

            var filter = new double[,]
                        {
                    {-1, -1, -1, -1, -1},
                    {-1,  2,  2,  2, -1},
                    {-1,  2, 16,  2, -1},
                    {-1,  2,  2,  2, -1},
                    {-1, -1, -1, -1, -1}
                        };

            double bias = 1.0 - strength;

            double factor = strength / 16.0;

            int bytes = pbits.Stride * height;

            const int filterWidth = 5;
            const int filterHeight = 5;

            var result = new Color[width, height];

            // Copy the RGB values into the array.
            //Marshal.Copy(pbits.Scan0, rgbValues, 0, bytes);

            int rgb;
            // Fill the color array with the new sharpened color values.
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    double red = 0.0, green = 0.0, blue = 0.0;

                    for (int filterX = 0; filterX < filterWidth; filterX++)
                    {
                        for (int filterY = 0; filterY < filterHeight; filterY++)
                        {
                            int imageX = (x - filterWidth / 2 + filterX + width) % width;
                            int imageY = (y - filterHeight / 2 + filterY + height) % height;

                            rgb = imageY * pbits.Stride + 3 * imageX;

                            red += rgbValues[rgb + 2] * filter[filterX, filterY];
                            green += rgbValues[rgb + 1] * filter[filterX, filterY];
                            blue += rgbValues[rgb + 0] * filter[filterX, filterY];
                        }

                        rgb = y * pbits.Stride + 3 * x;

                        int r = Math.Min(Math.Max((int)(factor * red + (bias * rgbValues[rgb + 2])), 0), 255);
                        int g = Math.Min(Math.Max((int)(factor * green + (bias * rgbValues[rgb + 1])), 0), 255);
                        int b = Math.Min(Math.Max((int)(factor * blue + (bias * rgbValues[rgb + 0])), 0), 255);

                        result[x, y] = Color.FromArgb(r, g, b);
                    }
                }
            }

            // Update the image with the sharpened pixels.
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    rgb = y * pbits.Stride + 3 * x;

                    rgbValues[rgb + 2] = result[x, y].R;
                    rgbValues[rgb + 1] = result[x, y].G;
                    rgbValues[rgb + 0] = result[x, y].B;
                }
            }

            // Copy the RGB values back to the bitmap.
            //Marshal.Copy(rgbValues, 0, pbits.Scan0, bytes);
            return rgbValues;
        }

        public static Bitmap Sharpen(Bitmap bitmap, double strength)
        {
            var sharpenImage = bitmap.Clone() as Bitmap;

            int width = bitmap.Width;
            int height = bitmap.Height;

            // Lock image bits for read/write.
            if (sharpenImage != null)
            {
                BitmapData pbits = sharpenImage.LockBits(new Rectangle(0, 0, width, height),
                                                            ImageLockMode.ReadWrite,
                                                            PixelFormat.Format24bppRgb);

                // Declare an array to hold the bytes of the bitmap.
                int bytes = pbits.Stride * height;
                var rgbValues = new byte[bytes];

                // Copy the RGB values into the array.
                Marshal.Copy(pbits.Scan0, rgbValues, 0, bytes);

                rgbValues = Sharpen(pbits, rgbValues, width, height, strength);

                // Copy the RGB values back to the bitmap.
                Marshal.Copy(rgbValues, 0, pbits.Scan0, bytes);
                // Release image bits.
                sharpenImage.UnlockBits(pbits);
            }

            return sharpenImage;
        }
    }
}
