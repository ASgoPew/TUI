using System;
using TUI.Base;

namespace TUI.Hooks.Args
{
    public class DrawArgs : EventArgs
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool ForcedSection { get; set; }
        public int UserIndex { get; set; } = -1;
        public int ExceptUserIndex { get; set; } = -1;
        public bool Frame { get; set; } = true;
        public VisualObject Node { get; set; }

        public DrawArgs(VisualObject node, int x, int y, int width, int height, bool forcedSection, int userIndex, int exceptUserIndex, bool frame)
        {
            Node = node;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            ForcedSection = forcedSection;
            UserIndex = userIndex;
            ExceptUserIndex = exceptUserIndex;
            Frame = frame;
        }
    }
}