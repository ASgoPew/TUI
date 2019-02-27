using System;

namespace TUI
{
    public class UIStyle
    {
        public ushort? Tile { get; set; }
        public byte? TileColor { get; set; }
        public byte? Wall { get; set; }
        public byte? WallColor { get; set; }
        public bool? InActive { get; set; }
    }
}