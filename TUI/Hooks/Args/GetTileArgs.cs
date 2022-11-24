using System;

namespace TerrariaUI.Hooks.Args
{
    public class GetTileArgs : EventArgs
    {
        public int X { get; set; }
        public int Y { get; set; }
        public dynamic Tile { get; set; }

        public GetTileArgs(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}