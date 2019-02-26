using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public class PaddingConfig : ICloneable
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Alignment Alignment { get; set; }

        public object Clone() => MemberwiseClone();
    }
}
