using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoEditor.Helpers
{
    public class Filter
    {
        public Filter() { 
            this.Effects = new List<Effect>();
        }
        public string Name { get; set; }
        public List<Effect> Effects { get; set; }
    }
}
