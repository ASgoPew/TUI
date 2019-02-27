using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public class SetXYWHArgs
    {
        public RootVisualObject Root { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public SetXYWHArgs(RootVisualObject root, int x, int y, int width, int height)
        {
            Root = root;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
