using System;
using TerrariaUI.Base;

namespace TerrariaUI.Hooks.Args
{
    public class SetXYWHArgs : EventArgs
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
