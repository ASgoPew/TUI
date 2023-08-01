using System.Collections.Generic;
using System.Linq;

namespace TerrariaUI.Base.Style
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
        /// Sets tile.fullbrightBlock() and/or tile.invisibleBlock() for every tile.
        /// </summary>
        public HashSet<byte> TileCoating { get; set; }
        /// <summary>
        /// Sets tile.wall = Style.Wall for every tile.
        /// </summary>
        public ushort? Wall { get; set; }
        /// <summary>
        /// Sets tile.wallColor(Style.WallColor) for every tile.
        /// </summary>
        public byte? WallColor { get; set; }
        /// <summary>
        /// Sets tile.fullbrightWall() and/or tile.invisibleWall() for every wall.
        /// </summary>
        public HashSet<byte> WallCoating { get; set; }
        /// <summary>
        /// Sets tile.inActive(Style.InActive) for every tile.
        /// </summary>
        public bool? InActive { get; set; }
        /// <summary>
        /// Forces <see cref="VisualObject.ApplyTiles"/> to iterate over all tiles
        /// and try to call <see cref="VisualObject.ApplyTile"/>
        /// </summary>
        public bool CustomApplyTile { get; set; } = false;

        /// <summary>
        /// Drawing styles for VisualObject.
        /// </summary>
        public UIStyle() { }

        /// <summary>
        /// Drawing styles for VisualObject.
        /// </summary>
        public UIStyle(UIStyle style)
        {
            Stratify(style);
        }

        public void Stratify(UIStyle style)
        {
            if (style.Active.HasValue)
                this.Active = style.Active.Value;
            if (style.Tile.HasValue)
                this.Tile = style.Tile.Value;
            if (style.TileColor.HasValue)
                this.TileColor = style.TileColor.Value;
            this.TileCoating = style.TileCoating?.ToHashSet();
            if (style.Wall.HasValue)
                this.Wall = style.Wall.Value;
            if (style.WallColor.HasValue)
                this.WallColor = style.WallColor.Value;
            this.WallCoating = style.WallCoating?.ToHashSet();
            if (style.InActive.HasValue)
                this.InActive = style.InActive.Value;
        }

        public ushort? SimilarWall()
        {
            switch (Wall)
            {
                case 153:
                    return 154;
                case 154:
                case 156:
                case 164:
                case 165:
                case 166:
                    return 153;
                case 157:
                    return 158;
                case 158:
                case 159:
                case 160:
                case 161:
                case 162:
                case 163:
                    return 157;
                default:
                    return Wall;
            }
        }
    }
}