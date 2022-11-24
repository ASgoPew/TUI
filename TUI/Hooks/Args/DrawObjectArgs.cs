using System;
using System.Collections.Generic;
using TerrariaUI.Base;

namespace TerrariaUI.Hooks.Args
{
    public class DrawObjectArgs : EventArgs
    {
        public VisualObject Node { get; set; }
        public HashSet<int> TargetPlayers { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool DrawWithSection { get; set; }
        public bool FrameSection { get; set; }

        public DrawObjectArgs(VisualObject node, HashSet<int> targetPlayers, int x, int y, int width, int height, bool drawWithSection,
            bool frameSection)
        {
            Node = node;
            TargetPlayers = targetPlayers;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            DrawWithSection = drawWithSection;
            FrameSection = frameSection;
        }
    }
}
