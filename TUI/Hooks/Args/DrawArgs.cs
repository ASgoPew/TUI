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
        public int PlayerIndex { get; set; } = -1;
        public int ExceptPlayerIndex { get; set; } = -1;
        public bool Frame { get; set; } = true;
        public VisualObject Node { get; set; }

        public DrawArgs(VisualObject node, int x, int y, int width, int height, bool forcedSection, int playerIndex, int exceptPlayerIndex, bool frame)
        {
            Node = node;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            ForcedSection = forcedSection;
            PlayerIndex = playerIndex;
            ExceptPlayerIndex = exceptPlayerIndex;
            Frame = frame;
        }
    }
}