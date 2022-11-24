using System;

namespace TerrariaUI.Hooks.Args
{
    public class DrawRectangleArgs : EventArgs
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool DrawWithSection { get; set; }
        public int PlayerIndex { get; set; }
        public int ExceptPlayerIndex { get; set; }
        public bool FrameSection { get; set; }

        public DrawRectangleArgs(int x, int y, int width, int height, bool forcedSection,
            int playerIndex, int exceptPlayerIndex, bool frame)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            DrawWithSection = forcedSection;
            PlayerIndex = playerIndex;
            ExceptPlayerIndex = exceptPlayerIndex;
            FrameSection = frame;
        }
    }
}