using System;
using TerrariaUI.Base;

namespace TerrariaUI.Hooks.Args
{
    public class DrawObjectArgs : EventArgs
    {
        public VisualObject Node { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool DrawWithSection { get; set; }
        public int PlayerIndex { get; set; }
        public int ExceptPlayerIndex { get; set; }
        public bool FrameSection { get; set; }
        public bool ToEveryone { get; set; }

        public DrawObjectArgs(VisualObject node, int x, int y, int width, int height, bool forcedSection,
            int playerIndex, int exceptPlayerIndex, bool frame, bool toEveryone)
        {
            Node = node;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            DrawWithSection = forcedSection;
            PlayerIndex = playerIndex;
            ExceptPlayerIndex = exceptPlayerIndex;
            FrameSection = frame;
            ToEveryone = toEveryone;
        }
    }
}