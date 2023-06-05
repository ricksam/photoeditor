using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoEditor.Helpers
{
    public class Effect
    {
        public Effect()
        {
            this.Tone = new EffectTone();
            this.Pixel = new EffectPixel();
            this.Light = new EffectLight();
            this.NoiseReduce = new NoiseReduce();
        }

        public int Brightness { get; set; }
        public int Contrast { get; set; }
        public int Saturation { get; set; }
        public int Temperature { get; set; }
        public int Highlight { get; set; }
        public int Exposition { get; set; }

        public string BalanceLeft { get; set; }
        public string BalanceRight { get; set; }
        public EffectTone Tone { get; set; }
        public EffectPixel Pixel { get; set; }
        public bool UseMatrix { get {
                int[,] defaultMatrix = new int[,] { { 0, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 } };
                
                if (Matrix == null) {
                    return false;
                }

                if (Matrix.GetLength(0) != defaultMatrix.GetLength(0) ||
                    Matrix.GetLength(1) != defaultMatrix.GetLength(1))
                {
                    return false;
                }

                for (int i = 0; i < Matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < Matrix.GetLength(1); j++)
                    {
                        if (Matrix[i, j] != defaultMatrix[i, j])
                        {
                            return true; // Matrices are not equal
                        }
                    }
                }

                return false; 
            }
        }
        public EffectLight Light { get; set; }
        public NoiseReduce NoiseReduce { get; set; }

        public int[,] Matrix { get; set; }

        public bool IsValid
        {
            get
            {
                return (Brightness != 0 || Contrast != 0 || Saturation != 0 || Temperature != 0 || Highlight != 0 || Exposition != 0 || UseMatrix || NoiseReduce.Size != 0)
                    && Tone.IsValid() && Pixel.IsValid() && Light.IsValid();
            }
        }

        public bool IsColorAdjusts
        {
            get
            {
                return (Brightness != 0 || Contrast != 0 || Saturation != 0 || Temperature != 0 || Highlight != 0 || Exposition != 0)
                    && Tone.IsValid() && Pixel.IsValid() && Light.IsValid();
            }

        }

        private string MatrixText() {
            return (UseMatrix ? string.Format("mtz({0})", ImageEditor.GetMatrixDivider(Matrix)) : "");
        }

        public override string ToString()
        {
            List<string> results = new List<string>();
            if (this.Brightness != 0)
            {
                results.Add("brilho:"+ this.Brightness);
            }
            if (this.Contrast != 0)
            {
                results.Add("contr:"+ this.Contrast);
            }
            if (this.Saturation != 0)
            {
                results.Add("sat:" + this.Saturation);
            }
            if (this.Temperature != 0)
            {
                results.Add("temp:"+ this.Temperature);
            }
            if (this.Highlight != 0)
            {
                results.Add("realce:"+ this.Highlight);
            }
            if (this.Exposition != 0)
            {
                results.Add("exposição:" + this.Exposition);
            }

            return string.Join(" ", results) + " " + Light.ToString() + Tone.ToString() + Pixel.ToString() + MatrixText() + NoiseReduce.ToString();
        }
    }

    public class EffectTone
    {
        public bool Red { get; set; }
        public bool Green { get; set; }
        public bool Blue { get; set; }
        public bool Yellow { get; set; }
        public bool Cyan { get; set; }
        public bool Magenta { get; set; }
        public int Weight { get; set; }
        public bool Gray { get; set; }

        public bool All
        {
            get { return Red && Green && Blue && Yellow && Cyan && Magenta && Gray; }
            
        }

        public bool IsValid()
        {
            return Red || Green || Blue || Yellow || Cyan || Magenta|| Gray;
        }

        public override string ToString()
        {
            if (Red && Green && Blue && Yellow && Cyan && Magenta && Gray)
            {
                return "";
            }

            List<string> result = new List<string>();
            if (Red) { result.Add("R"); }
            if (Green) { result.Add("G"); }
            if (Blue) { result.Add("B"); }
            if (Yellow) { result.Add("Y"); }
            if (Cyan) { result.Add("C"); }
            if (Magenta) { result.Add("M"); }
            if (Gray) { result.Add("S"); }
            return "tom(" + string.Join(",", result) + ")";
        }
    }

    public class EffectPixel
    {
        public bool Red { get; set; }
        public bool Green { get; set; }
        public bool Blue { get; set; }
        public bool IsValid()
        {
            return Red || Green || Blue;
        }
        public override string ToString()
        {
            if (Red && Green && Blue)
            {
                return "";
            }

            List<string> result = new List<string>();
            if (Red) { result.Add("r"); }
            if (Green) { result.Add("g"); }
            if (Blue) { result.Add("b"); }
            return "px(" + string.Join(",", result) + ")";
        }
    }

    public class NoiseReduce { 
        public int Size { get; set; }
        public int Limit { get; set; }
        public override string ToString()
        {
            return Size == 0 ? "" : string.Format("ruído({0})", Size);
        }
    }

    public class EffectLight
    {
        public bool DarkTones { get; set; }
        public bool MidTones { get; set; }
        public bool LightTones { get; set; }

        public bool All { get { return DarkTones && MidTones && LightTones; } }
        public bool IsValid()
        {
            return DarkTones || MidTones || LightTones ;
        }
        public override string ToString()
        {
            if (DarkTones&& MidTones&&LightTones)
            {
                return "";
            }

            List<string> result = new List<string>();
            if (DarkTones) { result.Add("D"); }
            if (MidTones) { result.Add("M"); }
            if (LightTones) { result.Add("L"); }
            return "luz(" + string.Join(",", result) + ")";
        }
    }
}
