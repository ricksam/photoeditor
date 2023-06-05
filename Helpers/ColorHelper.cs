using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoEditor.Helpers
{
    public class ColorHelper
    {
        public static int GrayAvg(Color c)
        {
            return (c.R + c.G + c.B) / 3;
        }
        public static int GrayAvg(PixelColor c)
        {
            return (int)((c.R + c.G + c.B) / 3);
        }

        public static int GrayMin(Color c)
        {
            return Math.Min(c.R, Math.Min(c.G, c.B));
        }

        public static int GrayMax(Color c)
        {
            return Math.Max(c.R, Math.Max(c.G, c.B));
        }

        public static int LimitPixel(double px)
        {
            return Math.Max(0, Math.Min(255, (int)px));
        }

        public static PixelColor ShakeColors(PixelColor[] colors)
        {
            double tR = 0;
            double tG = 0;
            double tB = 0;
            //double ln = colors.Length;
            double count = 0;

            foreach (var c in colors)
            {
                if (c.Valid)
                {
                    count++;
                    tR += c.R;
                    tG += c.G;
                    tB += c.B;
                }
                
            }

            return PixelColor.FromRGB((int)Math.Ceiling(tR / count), (int)Math.Ceiling(tG / count), (int)Math.Ceiling(tB / count));
        }

        public static double GetPixelWeight(double minorPixel, double majorPixel, int weight)
        {
            int outerWeight = 100 - weight;
            return ((minorPixel * outerWeight) + (majorPixel * weight)) / 100;
        }

        public static PixelColor GetColorWeight(PixelColor minorColor, PixelColor majorColor, int perc)
        {

            return PixelColor.FromRGB(
                GetPixelWeight(minorColor.R, majorColor.R, perc),
                GetPixelWeight(minorColor.G, majorColor.G, perc),
                GetPixelWeight(minorColor.B, majorColor.B, perc)
            );
        }
    }
}
