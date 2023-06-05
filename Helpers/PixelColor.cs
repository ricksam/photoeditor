using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoEditor.Helpers
{
    public class PixelColor
    {
        public PixelColor() { 
            this.Valid = true;
        }

        public PixelColor(Color c)
        {
            this.Valid = Color.Transparent!=c;
            this.R = c.R;
            this.G = c.G;
            this.B = c.B;
        }
        
        public static PixelColor FromRGB(double R, double G, double B)
        {
            return new PixelColor() { R = R, G = G, B = B };
        }

        public Color ToColor()
        {
            return Color.FromArgb(ColorHelper.LimitPixel(R), ColorHelper.LimitPixel(G), ColorHelper.LimitPixel(B));
        }

        public double R { get; set; }
        public double G { get; set; }
        public double B { get; set; }

        public bool Valid { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", (int)R, (int)G, (int)B);
        }
    }
}
