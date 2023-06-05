using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoEditor.Helpers
{
    /*
    public class ImageContainer
    {
        public ImageContainer(Image source, bool compressed)
        {
            this.Image = (Bitmap)source;
            this.Width = Image.Width;
            this.Height = Image.Height;
            (new System.Threading.Thread(new System.Threading.ThreadStart(Load))).Start();
            this.Compressed = compressed;
        }

        public bool Compressed { get; set; }
        public Bitmap Image { get; set; }

        public int Width{ get; set; }
        public int Height { get; set; }
        private bool Cancel { get; set; }
        //public Neightborhud[,] Neightborhuds { get; set; }
        public Histogram Histogram { get; set; }

        public void Load()
        {  
            FastPixel fastPixel = new FastPixel((Bitmap)Image.Clone());
            fastPixel.Lock();
            Histogram = GetHistogram(fastPixel);
            //Neightborhuds = GetNeightborhuds(fastPixel);
            fastPixel.Unlock();
        }

        public void Dispose() {
            this.Cancel = true;
        }

        private Histogram GetHistogram(FastPixel fastPixel)
        {
            //double brightness = 0;
            double avgPixel = 0;
            double minAvg = 255;
            double maxAvg = 0;

            for (int y = 0; y < fastPixel.Height; y++)
            {
                if (Cancel) 
                { break; }
                for (int x = 0; x < fastPixel.Width; x++)
                {
                    if (Cancel) 
                    { break; }
                    Color color = fastPixel.GetPixel(x, y);

                    int avg = ColorHelper.GrayAvg(color);
                    avgPixel += avg;

                    minAvg=Math.Min(minAvg, avg);
                    maxAvg = Math.Max(maxAvg, avg);

                    //brightness += color.GetBrightness();
                }
            }

            avgPixel /= (Width * Height);

            return new Histogram()
            {
                //Brightness = brightness,
                Avg = avgPixel,
                Min = minAvg,
                Max = maxAvg
            };
        }

       

        
    }
    */
}
