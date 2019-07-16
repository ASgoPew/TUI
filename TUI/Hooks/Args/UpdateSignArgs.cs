using System;
using TUI.Base;

namespace TUI.Hooks.Args
{
    public class UpdateSignArgs : EventArgs
    {
        public int X { get; }
        public int Y { get; }
        public string Text { get; }
        public object Sign { get; set; }
        public VisualObject Node { get; set; }

        public UpdateSignArgs(int x, int y, string text, VisualObject node)
        {
            X = x;
            Y = y;
            Text = text;
            Node = node;
        }
    }
}
