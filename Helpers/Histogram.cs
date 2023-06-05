using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoEditor.Helpers
{
    public class Histogram
    {
        public int Avg { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }

        public int MajorFatorR { get; set; }
        public int MajorFatorG { get; set; }
        public int MajorFatorB { get; set; }
        public int MajorFatorY { get; set; }
        public int MajorFatorC { get; set; }
        public int MajorFatorM { get; set; }
        public int MajorFatorS { get; set; }
    }
}
