using System;

namespace TUI.Hooks.Args
{
    public class SignTextArgs : EventArgs
    {
        public string Text { get; }
        public int X { get; }
        public int Y { get; }
        public dynamic Sign { get; set; }

        public SignTextArgs(string text, int x, int y, dynamic sign)
        {
            Text = text;
            X = x;
            Y = y;
            Sign = sign;
        }
    }
}
