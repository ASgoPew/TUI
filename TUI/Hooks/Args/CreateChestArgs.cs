using System;
using TUI.Base;

namespace TUI.Hooks.Args
{
    public class CreateChestArgs : EventArgs
    {
        public int X { get; }
        public int Y { get; }
        public dynamic Chest { get; set; }
        public VisualObject Node { get; set; }
        
        public CreateChestArgs(int x, int y, VisualObject node)
        {
            X = x;
            Y = y;
            Node = node;
        }
    }
}
