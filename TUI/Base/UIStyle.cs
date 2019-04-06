using System;

namespace TUI.Base
{
    public class UIStyle : ICloneable
    {
        public bool? Active { get; set; }
        public ushort? Tile { get; set; }
        public byte? TileColor { get; set; }
        public byte? Wall { get; set; }
        public byte? WallColor { get; set; }
        public bool? InActive { get; set; }

        public virtual object Clone()
        {
            UIStyle result = new UIStyle();
            if (Active.HasValue)
                result.Active = Active.Value;
            if (Tile.HasValue)
                result.Tile = Tile.Value;
            if (TileColor.HasValue)
                result.TileColor = TileColor.Value;
            if (Wall.HasValue)
                result.Wall = Wall.Value;
            if (WallColor.HasValue)
                result.WallColor = WallColor.Value;
            if (InActive.HasValue)
                result.InActive = InActive.Value;
            return result;
        }
    }
}