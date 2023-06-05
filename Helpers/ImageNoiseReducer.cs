using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoEditor.Helpers
{
    public class ImageNoiseReducer
    {
        static FastPixel fastPixelCopy;
        public static Bitmap ReduceNoise(FastPixel fastPixel, int size, int sigma, ImageEditorProgress progress)
        {
            fastPixelCopy = fastPixel.Clone();

            int size_array = (int)Math.Pow((2 * size) + 1, 2);
            /*if (size == 1) { size_array = 9; }
            else if (size == 2) { size_array = 25; }
            else if (size == 3) { size_array = 49; }
            else if (size == 4) { size_array = 81; }
            else if (size == 5) { size_array = 121; }*/

            int adjusts_count = 0;
            Parallel.For(0, fastPixel.Height, (y, loopState) =>
            {
                try {

                    if (ImageEditor.Cancel) { loopState.Stop(); }
                    progress(adjusts_count, fastPixel.Height);
                    adjusts_count++;
                    for (int x = 0; x < fastPixel.Width; x++)
                    {
                        if (ImageEditor.Cancel) { break; }
                        Color color = fastPixelCopy.GetPixel(x, y);
                        Color[] neig = GetNeighborhoods(x, y, size, size_array);
                        fastPixel.SetPixel(x, y, ShakeSimilarColors(neig, color, sigma));
                    }
                }
                catch{
                    Debug.WriteLine("Erro:"+y);
                }
            });

            progress(0, 0);

            return fastPixel.Bitmap;
        }

        public static Color[] GetNeighborhoods(int x, int y, int dist, int size_array)
        {
            Color[] cursor = new Color[size_array];
            int idx = 0;
            for (int i = -dist; i <= dist; i++)
            {
                for (int j = -dist; j <= dist; j++)
                {
                    int xr = x + i;
                    int yr = y + j;
                    if (xr >= 0 && yr >= 0 && xr < fastPixelCopy.Width && yr < fastPixelCopy.Height)
                    {
                        Color pixel = fastPixelCopy.GetPixel(xr, yr);
                        cursor[idx] = pixel;
                    }
                    idx++;
                }
            }
            return cursor;
        }

        /*
        public static Color[] GetNeighborhoods(FastPixel fastPixel, int x, int y, int size, Color[] cursor)
        {
            int idx = 0;
            for (int i = -size; i <= size; i++)
            {
                for (int j = -size; j <= size; j++)
                {
                    int xr = x + i;
                    int yr = y + j;
                    if (xr >= 0 && yr >= 0 && xr < fastPixel.Width && yr < fastPixel.Height)
                    {
                        Color pixel = fastPixel.GetPixel(xr, yr);
                        cursor[idx] = pixel;
                    }
                    idx++;
                }
            }
            return cursor;
        }

        public static Color[] GetNewNeighborhoodsX(FastPixel fastPixel, int x, int y, Color[] current_neighborhoods, int size, int pw, Color[] cursor)
        {
            //int pw = (int)Math.Sqrt(current_neighborhoods.Length);
            //Color[] cursor = new Color[current_neighborhoods.Length];

            for (int i = 0; i < cursor.Length - pw; i++)
            {
                cursor[i] = current_neighborhoods[i + pw];
            }

            int idx = cursor.Length - pw;
            for (int j = -size; j <= size; j++)
            {
                int xr = x + size;
                int yr = y + j;
                if (xr >= 0 && yr >= 0 && xr < fastPixel.Width && yr < fastPixel.Height)
                {
                    Color pixel = fastPixel.GetPixel(xr, yr);
                    cursor[idx] = pixel;
                }
                idx++;
            }
            return cursor;
        }

        public static Color[] GetNewNeighborhoodsY(FastPixel fastPixel, int x, int y, Color[] current_neighborhoods, int size, int pw , Color[] cursor)
        {
            //int pw = (int)Math.Sqrt(current_neighborhoods.Length);
            //Color[] cursor = new Color[current_neighborhoods.Length];

            for (int i = 0; i < cursor.Length - 1; i++)
            {
                cursor[i] = current_neighborhoods[i + 1];
            }

            int idx = 1;
            for (int i = -size; i <= size; i++)
            {

                int xr = x + i;
                int yr = y + size;
                if (xr >= 0 && yr >= 0 && xr < fastPixel.Width && yr < fastPixel.Height)
                {
                    Color pixel = fastPixel.GetPixel(xr, yr);
                    cursor[(idx * pw) - 1] = pixel;
                }
                idx++;
            }

            return cursor;
        }
        */
        public static Color ShakeSimilarColors(Color[] colors, Color comp, int sigma)
        {

            int tR = 0;
            int tG = 0;
            int tB = 0;
            int count = 0;

            foreach (var c in colors)
            {
                if (c.A == 255)
                {
                    if ((Math.Abs(c.R - comp.R) + Math.Abs(c.G - comp.G) + Math.Abs(c.B - comp.B)) < sigma)
                    {
                        count++;
                        tR += c.R;
                        tG += c.G;
                        tB += c.B;
                    }
                }
            }

            if (count == 0)
            {
                return comp;
            }
            else
            {
                return Color.FromArgb((tR / count), (tG / count), (tB / count));
            }
        }
    }
}

