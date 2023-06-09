using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoEditor.Helpers
{
    public class ImagePixelMatrix
    {
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
        public static void SetMatrix(FastPixel fastPixel, Histogram histogram, Effect effect, ImageEditorProgress progress, ImageEditorSetPixelColor setPixelColor) {
            FastPixel fastPixelCopy = fastPixel.Clone();

            double div = GetMatrixDivider(effect.Matrix);
            // aplica o filtro na imagem
            int adjusts_count = 0;
            Parallel.For(1, fastPixelCopy.Width - 1, (x, loopState) =>
            {
                if (ImageEditor.Cancel) { loopState.Stop(); }
                progress(adjusts_count, fastPixelCopy.Width);
                adjusts_count++;
                for (int y = 1; y < fastPixelCopy.Height - 1; y++)
                {
                    Color color = fastPixel.GetPixel(x, y);
                    PixelColor pixelColor = new PixelColor(color);

                    int r = 0, g = 0, b = 0;

                    // realiza a convolução da matriz de filtro com o pixel e seus vizinhos
                    for (int i = -1; i <= 1; i++)
                    {
                        if (ImageEditor.Cancel) { return; }
                        for (int j = -1; j <= 1; j++)
                        {
                            if (ImageEditor.Cancel) { return; }
                            Color pixel = fastPixelCopy.GetPixel(x + i, y + j);
                            r += pixel.R * effect.Matrix[i + 1, j + 1];
                            g += pixel.G * effect.Matrix[i + 1, j + 1];
                            b += pixel.B * effect.Matrix[i + 1, j + 1];
                        }
                    }

                    pixelColor.R = r / div;
                    pixelColor.G = g / div;
                    pixelColor.B = b / div;

                    setPixelColor(fastPixel, x, y, histogram, effect, color, pixelColor);
                }
            });
        }
    }
}
