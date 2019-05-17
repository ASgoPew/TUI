using System;
using TUI.Base;

namespace TUI.Hooks.Args
{
    public class CreateSignArgs : EventArgs
    {
        public int X { get; }
        public int Y { get; }
        public dynamic Sign { get; set; }
        public VisualObject Node { get; set; }

        public CreateSignArgs(int x, int y, dynamic sign, VisualObject node)
        {
            X = x;
            Y = y;
            Sign = sign;
            Node = node;
        }
    }
}
