namespace TUI.Base.Style
{
    /// <summary>
    /// Object drawing style class.
    /// </summary>
    public class UIStyle
    {
        /// <summary>
        /// Whether to use walls from parents or not. By default true (use).
        /// </summary>
        //public bool InheritParentWall { get; set; } = false;

        /// <summary>
        /// Sets tile.active(Style.Active) for every tile.
        /// <para></para>
        /// If not specified sets to true in case Style.Tile is specified,
        /// otherwise to false in case Style.Wall is specified.
        /// </summary>
        public bool? Active { get; set; }
        /// <summary>
        /// Sets tile.type = Style.Tile for every tile.
        /// </summary>
        public ushort? Tile { get; set; }
        /// <summary>
        /// Sets tile.color(Style.TileColor) for every tile.
        /// </summary>
        public byte? TileColor { get; set; }
        /// <summary>
        /// Sets tile.wall = Style.Wall for every tile.
        /// </summary>
        public byte? Wall { get; set; }
        /// <summary>
        /// Sets tile.wallColor(Style.WallColor) for every tile.
        /// </summary>
        public byte? WallColor { get; set; }
        /// <summary>
        /// Sets tile.inActive(Style.InActive) for every tile.
        /// </summary>
        public bool? InActive { get; set; }

        public UIStyle() { }

        public UIStyle(UIStyle style)
        {
            if (style.Active.HasValue)
                this.Active = style.Active.Value;
            if (style.Tile.HasValue)
                this.Tile = style.Tile.Value;
            if (style.TileColor.HasValue)
                this.TileColor = style.TileColor.Value;
            if (style.Wall.HasValue)
                this.Wall = style.Wall.Value;
            if (style.WallColor.HasValue)
                this.WallColor = style.WallColor.Value;
            if (style.InActive.HasValue)
                this.InActive = style.InActive.Value;
        }
    }
}