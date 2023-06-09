using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoEditor.Helpers
{
    public delegate void ImageEditorProgress(int step, int count);
    public delegate void ImageEditorSetPixelColor(FastPixel fastPixel, int x, int y, Histogram histogram, Effect effect, Color color, PixelColor pixelColor);
    //public delegate void ImageEditorPlugin(FastPixel fastPixel, int value);
    public class ImageEditor
    {
        public static bool Cancel = false;
        public static Bitmap GetAdjusts(Bitmap bitmap, Histogram histogram, List<Effect> effects, ImageEditorProgress progress)
        {
            Cancel = true; // Cancela outras processos

            if (bitmap != null && histogram != null)
            {
                Effect[] noiseEffects = effects.Where(q => q.NoiseReduce.Size != 0).ToArray();
                Effect[] structureEffetcs = effects.Where(q => q.UseMatrix).ToArray();
                Effect[] colorEffects = effects.Where(q => q.IsColorAdjusts).ToArray();

                Cancel = false;

                FastPixel fastPixel = new FastPixel((Bitmap)bitmap.Clone());
                fastPixel.Lock();

                if (colorEffects.Length > 0 && bitmap != null)
                {
                    ApplayColorEffects(fastPixel, histogram, colorEffects, progress);
                }

                if (noiseEffects.Length > 0)
                {
                    ApplyNoiseEffects(fastPixel, histogram, noiseEffects, progress);
                }

                if (structureEffetcs.Length > 0)
                {
                    ApplyMatrix(fastPixel, structureEffetcs, histogram, progress);
                }

                fastPixel.Unlock();

                if (Cancel)
                {
                    return null;
                }

                return fastPixel.Bitmap;
            }
            else
            {
                Cancel = false;
            }
            return null;
        }

        public static Histogram GetHistogram(Bitmap bitmap, ImageEditorProgress progress)
        {
            FastPixel fastPixel = new FastPixel((Bitmap)bitmap.Clone());
            fastPixel.Lock();

            Histogram histogram = new Histogram();

            int loop_count = 0;
            Parallel.For(0, fastPixel.Height, (y, loopState) =>
            {
                if (Cancel) { loopState.Stop(); }
                progress(loop_count, fastPixel.Height);
                loop_count++;

                for (int x = 0; x < fastPixel.Width; x++)
                {
                    if (Cancel)
                    { break; }
                    Color color = fastPixel.GetPixel(x, y);


                    histogram.MajorFatorR = Math.Max(histogram.MajorFatorR, FactorR(color));
                    histogram.MajorFatorG = Math.Max(histogram.MajorFatorG, FactorG(color));
                    histogram.MajorFatorB = Math.Max(histogram.MajorFatorB, FactorB(color));
                    histogram.MajorFatorY = Math.Max(histogram.MajorFatorY, FactorY(color));
                    histogram.MajorFatorC = Math.Max(histogram.MajorFatorC, FactorC(color));
                    histogram.MajorFatorM = Math.Max(histogram.MajorFatorM, FactorM(color));
                    histogram.MajorFatorS = Math.Max(histogram.MajorFatorS, FactorS(color));

                    int avg = ColorHelper.GrayAvg(color);
                    histogram.Avg += avg;

                    histogram.Min = Math.Min(histogram.Min, avg);
                    histogram.Max = Math.Max(histogram.Max, avg);
                }
            });
            fastPixel.Unlock();

            progress(0, 0);
            histogram.Avg /= (fastPixel.Width * fastPixel.Height);

            //ao avaliar individualmente o cinza fica maior que o hs fator
            //histogram.MajorFatorS = Math.Min(histogram.MajorFatorS, histogram.Max);

            return histogram;
        }

        private static int PercColorLight(Effect effect, Histogram histogram, int avgPixel)
        {
            if (effect.Light.All)
            {
                return 100;
            }
            else
            {
                double l_tones = 0;
                double m_tones = 0;
                double d_tones = 0;

                double range = (histogram.Max - histogram.Min);
                double cut = range / 2;

                if (effect.Light.LightTones)
                {
                    l_tones = ((avgPixel - (histogram.Min + cut)) * 100 / (range - cut));
                }

                if (effect.Light.MidTones)
                {
                    m_tones = (1 - ((Math.Abs(avgPixel - (cut + histogram.Min))) / ((range + cut + histogram.Min) / 3))) * 100;
                }

                if (effect.Light.DarkTones)
                {
                    d_tones = (cut - avgPixel + histogram.Min) * 100 / (range - cut);
                }

                double perc = Math.Max(l_tones, Math.Max(m_tones, d_tones));
                return (int)Math.Max(0, Math.Min(perc, 100));
            }
        }

        public static int FactorR(Color color)
        {
            return (int)(color.R - Math.Max(color.G, color.B));
        }

        public static int FactorG(Color color)
        {
            return (int)(color.G - Math.Max(color.R, color.B));
        }

        public static int FactorB(Color color)
        {
            return (int)(color.B - Math.Max(color.R, color.G));
        }

        public static int FactorY(Color color)
        {
            return (int)(Math.Min(color.R, color.G) - color.B);
        }

        public static int FactorC(Color color)
        {
            return (int)(Math.Min(color.G, color.B) - color.R);
        }

        public static int FactorM(Color color)
        {
            return (int)(Math.Min(color.B, color.R) - color.G);
        }

        public static int FactorS(Color color)
        {
            int dif1 = Math.Abs(color.R - color.G);
            int dif2 = Math.Abs(color.G - color.B);
            int dif3 = Math.Abs(color.B - color.R);

            return 255 - (dif1 * dif2 * dif3);
        }

        private static int PercColorTone(Effect effect, Histogram histogram, Color color)
        {
            if (effect.Tone.All)
            {
                return 100;
            }
            else
            {
                int p_r = 0;
                int p_g = 0;
                int p_b = 0;
                int p_y = 0;
                int p_c = 0;
                int p_m = 0;
                int p_s = 0;


                if (effect.Tone.Red && histogram.MajorFatorR > 0)
                {
                    p_r = FactorR(color) * 100 / histogram.MajorFatorR;
                }

                if (effect.Tone.Green && histogram.MajorFatorG > 0)
                {
                    p_g = FactorG(color) * 100 / histogram.MajorFatorG;
                }

                if (effect.Tone.Blue && histogram.MajorFatorB > 0)
                {
                    p_b = FactorB(color) * 100 / histogram.MajorFatorB;
                }

                if (effect.Tone.Yellow && histogram.MajorFatorY > 0)
                {
                    p_y = FactorY(color) * 100 / histogram.MajorFatorY;
                }

                if (effect.Tone.Cyan && histogram.MajorFatorC > 0)
                {
                    p_c = FactorC(color) * 100 / histogram.MajorFatorC;
                }

                if (effect.Tone.Magenta && histogram.MajorFatorM > 0)
                {
                    p_m = FactorM(color) * 100 / histogram.MajorFatorM;
                }

                if (effect.Tone.Gray && histogram.MajorFatorS > 0)
                {
                    p_m = FactorS(color) * 100 / histogram.MajorFatorS;
                }

                int p_t = 0;
                p_t = Math.Max(p_t, p_r);
                p_t = Math.Max(p_t, p_g);
                p_t = Math.Max(p_t, p_b);
                p_t = Math.Max(p_t, p_y);
                p_t = Math.Max(p_t, p_c);
                p_t = Math.Max(p_t, p_m);
                p_t = Math.Max(p_t, p_s);

                return Math.Min(100, p_t * effect.Tone.Weight);
            }

        }

        private static PixelColor ColorAdjusts(PixelColor pixelColor, Effect effect, Histogram histogram)
        {
            if (effect.Balance != 0)
            {
                pixelColor = AjustaTemperatura(pixelColor, effect.Balance, effect.BalanceLeft, effect.BalanceRight);
            }
            if (effect.Saturation != 0)
            {
                pixelColor = AjustaSaturacao(pixelColor, effect.Saturation);
            }
            
            if (effect.Highlight != 0)
            {
                pixelColor = AjustaRealce(pixelColor, effect.Highlight, histogram.Avg);
            }
            if (effect.Exposition != 0)
            {
                pixelColor = AjustaExposition(pixelColor, effect.Exposition, histogram.Avg);
            }
            if (effect.Brightness != 0)
            {
                pixelColor = AjustaBrilho(pixelColor, effect.Brightness);
            }
            if (effect.Contrast != 0)
            {
                pixelColor = AjustaContraste(pixelColor, effect.Contrast, histogram.Avg);
            }
            return pixelColor;
        }

        public static int GetMatrixDivider(int[,] matrix)
        {
            int t = 0;

            if (matrix == null)
            {
                return 1;
            }

            for (int x = 0; x < matrix.GetLength(0); x++)
            {
                for (int y = 0; y < matrix.GetLength(1); y++)
                {
                    t += matrix[x, y];
                }
            }

            if (t == 0)
            { t = 1; };

            return t;
        }

        public static void SetPixelColor(FastPixel fastPixel, int x, int y, PixelColor pixelColor) {
            int newR = ColorHelper.LimitPixel(pixelColor.R);
            int newG = ColorHelper.LimitPixel(pixelColor.G);
            int newB = ColorHelper.LimitPixel(pixelColor.B);

            fastPixel.SetPixel(x, y, Color.FromArgb(newR, newG, newB));
        }

        public static void SetAndAtenuatePixelColor(FastPixel fastPixel, int x, int y, Histogram histogram, Effect effect, Color color, PixelColor pixelColor) {
            pixelColor = AttenuateColor(effect, histogram, color, new PixelColor(color), pixelColor);
            SetPixelColor(fastPixel, x, y, pixelColor);
        }

        public static void ApplyMatrix(FastPixel fastPixel, Effect[] effects, Histogram histogram, ImageEditorProgress progress)
        {
            foreach (var effect in effects)
            {
                ImagePixelMatrix.SetMatrix(fastPixel, histogram, effect, progress, SetAndAtenuatePixelColor);
                /*FastPixel fastPixelCopy = fastPixel.Clone();

                double div = GetMatrixDivider(effect.Matrix);
                // aplica o filtro na imagem
                int adjusts_count = 0;
                Parallel.For(1, fastPixelCopy.Width - 1, (x, loopState) =>
                {
                    if (Cancel) { loopState.Stop(); }
                    progress(adjusts_count, fastPixelCopy.Width);
                    adjusts_count++;
                    for (int y = 1; y < fastPixelCopy.Height - 1; y++)
                    {
                        Color color = fastPixel.GetPixel(x, y);
                        PixelColor pixelColor = new PixelColor(color);
                        //int avgPixel = ColorHelper.GrayAvg(color);

                        //if (CanEdit(pixelColor, effect, histogram, avgPixel)) {
                        int r = 0, g = 0, b = 0;

                        // realiza a convolução da matriz de filtro com o pixel e seus vizinhos
                        for (int i = -1; i <= 1; i++)
                        {
                            if (Cancel) { return; }
                            for (int j = -1; j <= 1; j++)
                            {
                                if (Cancel) { return; }
                                Color pixel = fastPixelCopy.GetPixel(x + i, y + j);
                                r += pixel.R * effect.Matrix[i + 1, j + 1];
                                g += pixel.G * effect.Matrix[i + 1, j + 1];
                                b += pixel.B * effect.Matrix[i + 1, j + 1];
                            }
                        }

                        pixelColor.R = r / div;
                        pixelColor.G = g / div;
                        pixelColor.B = b / div;

                        SetAndAtenuatePixelColor(fastPixel, x, y, histogram, effect, color, pixelColor);
                    }
                });*/
            }

            progress(0, 0);
        }

        public static void ApplyNoiseEffects(FastPixel fastPixel, Histogram histogram, Effect[] effects, ImageEditorProgress progress)
        {

            foreach (var item in effects)
            {
                ImageNoiseReducer.ReduceNoise(fastPixel, histogram, item, progress, SetAndAtenuatePixelColor);
            }
        }

        public static void ApplayColorEffects(FastPixel fastPixel, Histogram histogram, Effect[] effects, ImageEditorProgress progress)
        {
            int adjusts_count = 0;
            Parallel.For(0, fastPixel.Width, (x, loopState) =>
            {
                if (Cancel) { loopState.Stop(); }
                progress(adjusts_count, fastPixel.Width);
                adjusts_count++;

                for (int y = 0; y < fastPixel.Height; y++)
                {
                    //if (x == 973 && y == 475)
                    //{
                    //    Debug.WriteLine("point");
                    //}
                    if (Cancel) { return; }
                    Color color = fastPixel.GetPixel(x, y);
                    //int avgPixel = ColorHelper.GrayAvg(color);
                    PixelColor effetcColor = new PixelColor(color);

                    PixelColor pixelColor = new PixelColor(color);

                    foreach (var effect in effects)
                    {
                        if (Cancel) { return; }
                        pixelColor = ColorAdjusts(pixelColor, effect, histogram);
                        pixelColor = AttenuateColor(effect, histogram, color, effetcColor, pixelColor);
                        effetcColor = PixelColor.FromRGB(pixelColor.R, pixelColor.G, pixelColor.B);
                    }


                    SetPixelColor(fastPixel, x, y, pixelColor);
                }
            });
            progress(0, fastPixel.Width);
        }

        private static PixelColor AttenuateColor(Effect effect, Histogram histogram, Color color, PixelColor effetcColor, PixelColor pixelColor)
        {
            int avgPixel=ColorHelper.GrayAvg(color);
            int percLight = PercColorLight(effect, histogram, avgPixel);
            int percTone = PercColorTone(effect, histogram, color);
            int perc = Math.Min(percLight, percTone);
            if (perc != 100)
            {
                pixelColor = ColorHelper.GetColorWeight(effetcColor, pixelColor, perc);
            }

            if (!effect.Pixel.Red || !effect.Pixel.Green || !effect.Pixel.Blue)
            {
                pixelColor.R = (effect.Pixel.Red ? pixelColor.R : effetcColor.R);
                pixelColor.G = (effect.Pixel.Green ? pixelColor.G : effetcColor.G);
                pixelColor.B = (effect.Pixel.Blue ? pixelColor.B : effetcColor.B);
            }

            return pixelColor;
        }

        private static PixelColor AjustaTemperatura(PixelColor pixel, int value, string BalanceLeft, string BalanceRight)
        {
            int percR = 0;
            int percG = 0;
            int percB = 0;

            if (BalanceLeft == "Red" || BalanceLeft == "Yellow" || BalanceLeft == "Magenta")
            {
                percR -= value;
            }

            if (BalanceLeft == "Green" || BalanceLeft == "Yellow" || BalanceLeft == "Cyan")
            {
                percG -= value;
            }

            if (BalanceLeft == "Blue" || BalanceLeft == "Cyan" || BalanceLeft == "Magenta")
            {
                percB -= value;
            }

            ////////////////////////////////////

            if (BalanceRight == "Red" || BalanceRight == "Yellow" || BalanceRight == "Magenta")
            {
                percR += value;
            }

            if (BalanceRight == "Green" || BalanceRight == "Yellow" || BalanceRight == "Cyan")
            {
                percG += value;
            }

            if (BalanceRight == "Blue" || BalanceRight == "Cyan" || BalanceRight == "Magenta")
            {
                percB += value;
            }

            double newR = pixel.R * (1 + (percR / 100f));
            double newG = pixel.G * (1 + (percG / 100f));
            double newB = pixel.B * (1 + (percB / 100f));

            return PixelColor.FromRGB(newR, newG, newB);
        }


        private static PixelColor AjustaBrilho(PixelColor pixel, int value)
        {
            double factor = 1 + (value / 100f);
            return PixelColor.FromRGB(pixel.R * factor, pixel.G * factor, pixel.B * factor);
        }

        private static PixelColor AjustaExposition(PixelColor pixel, int value, int avg)
        {
            double factor = 1 + (value / 100f);
            double dif = ((avg * factor) - avg);
            return PixelColor.FromRGB(pixel.R + dif, pixel.G + dif, pixel.B + dif);
        }

        private static double CalcContrast(double current, int value, double avg)
        {
            double dist = current - avg;
            double factor = 1 + (value / 100f);
            return (avg + (dist * factor));
        }

        private static PixelColor AjustaSaturacao(PixelColor pixel, int value)
        {
            int avgCurrentPixel = ColorHelper.GrayAvg(pixel);
            return AjustaContraste(pixel, value, avgCurrentPixel);
        }


        private static PixelColor AjustaContraste(PixelColor pixel, int value, int avg)
        {
            return PixelColor.FromRGB(
                CalcContrast(pixel.R, value, avg),
                CalcContrast(pixel.G, value, avg),
                CalcContrast(pixel.B, value, avg));
        }

        private static PixelColor AjustaRealce(PixelColor pixel, int value, int avg)
        {
            double avgCurrentPixel = ColorHelper.GrayAvg(pixel);
            double calc = CalcContrast(avgCurrentPixel, value, avg) - avgCurrentPixel;
            double newRed = (pixel.R + calc);
            double newGreen = (pixel.G + calc);
            double newBlue = (pixel.B + calc);
            return PixelColor.FromRGB(newRed, newGreen, newBlue);
        }
    }
}
