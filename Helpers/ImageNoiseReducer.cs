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
        public static void ReduceNoise(FastPixel fastPixel, Histogram histogram, Effect effect, ImageEditorProgress progress, ImageEditorSetPixelColor setPixelColor)
        {
            fastPixelCopy = fastPixel.Clone();

            int size_array = (int)Math.Pow((2 * effect.NoiseReduce.Size) + 1, 2);

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
                        Color[] neig = GetNeighborhoods(x, y, effect.NoiseReduce.Size, size_array);
                        //fastPixel.SetPixel(x, y, ShakeSimilarColors(neig, color, sigma));
                        setPixelColor(fastPixel, x, y, histogram, effect, color, ShakeSimilarColors(neig, color, effect.NoiseReduce.Limit));
                    }
                }
                catch{
                    Debug.WriteLine("Erro:"+y);
                }
            });

            progress(0, 0);

            //return fastPixel.Bitmap;
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

        public static PixelColor ShakeSimilarColors(Color[] colors, Color comp, int sigma)
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
                return new PixelColor(comp);
            }
            else
            {
                return PixelColor.FromRGB((tR / count), (tG / count), (tB / count));
            }
        }
    }
}

