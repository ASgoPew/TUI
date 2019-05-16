using System;

namespace TUI.Hooks.Args
{
    public class CreateSignArgs : EventArgs
    {
        public int X { get; }
        public int Y { get; }
        public dynamic Sign { get; set; }

        public CreateSignArgs(int x, int y, dynamic sign)
        {
            X = x;
            Y = y;
            Sign = sign;
        }
    }
}
