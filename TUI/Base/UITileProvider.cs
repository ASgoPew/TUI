namespace TUI
{
    public struct UITileProvider
    {
        internal dynamic Tile;
        internal int X;
        internal int Y;

        public UITileProvider(dynamic tile, int x, int y)
        {
            Tile = tile;
            X = x;
            Y = y;
        }

        public dynamic this[int x, int y]
        {
            get => Tile[X + x, Y + y];
        }
    }
}
