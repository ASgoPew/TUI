namespace TUI.Base.Style
{
    /// <summary>
    /// Drawing styles for VisualObject.
    /// </summary>
    public class UIStyle
    {
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

        /// <summary>
        /// Drawing styles for VisualObject.
        /// </summary>
        public UIStyle() { }

        /// <summary>
        /// Drawing styles for VisualObject.
        /// </summary>
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