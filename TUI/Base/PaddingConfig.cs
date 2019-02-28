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

        public PaddingConfig(int x, int y, int width, int height, Alignment alignment = Alignment.Center)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Alignment = alignment;
        }

        public object Clone() => MemberwiseClone();
    }
}
