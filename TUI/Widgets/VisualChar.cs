using System;
using System.Collections.Generic;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Widgets
{
    public class VisualChar : VisualObject
    {
        #region Data

        #region Characters

        private const bool _ = false;
        private const bool G = true;
        public static Dictionary<char, bool[,]> Characters { get; } =
            new Dictionary<char, bool[,]>()
            {
                ['0'] = new bool[,]
                {
                    { G, G, G },
                    { G, _, G },
                    { G, _, G },
                    { G, _, G },
                    { G, G, G }
                },
                ['1'] = new bool[,]
                {
                    { G },
                    { G },
                    { G },
                    { G },
                    { G }
                },
                ['2'] = new bool[,]
                {
                    { G, G, G },
                    { _, _, G },
                    { G, G, G },
                    { G, _, _ },
                    { G, G, G }
                },
                ['3'] = new bool[,]
                {
                    { G, G, G },
                    { _, _, G },
                    { G, G, G },
                    { _, _, G },
                    { G, G, G }
                },
                ['4'] = new bool[,]
                {
                    { G, _, G },
                    { G, _, G },
                    { G, G, G },
                    { _, _, G },
                    { _, _, G }
                },
                ['5'] = new bool[,]
                {
                    { G, G, G },
                    { G, _, _ },
                    { G, G, G },
                    { _, _, G },
                    { G, G, G }
                },
                ['6'] = new bool[,]
                {
                    { G, G, G },
                    { G, _, _ },
                    { G, G, G },
                    { G, _, G },
                    { G, G, G }
                },
                ['7'] = new bool[,]
                {
                    { G, G, G },
                    { _, _, G },
                    { _, _, G },
                    { _, _, G },
                    { _, _, G }
                },
                ['7'] = new bool[,]
                {
                    { G, G, G },
                    { _, _, G },
                    { _, G, _ },
                    { G, _, _ },
                    { G, _, _ }
                },
                ['8'] = new bool[,]
                {
                    { G, G, G },
                    { G, _, G },
                    { G, G, G },
                    { G, _, G },
                    { G, G, G }
                },
                ['9'] = new bool[,]
                {
                    { G, G, G },
                    { G, _, G },
                    { G, G, G },
                    { _, _, G },
                    { G, G, G }
                }
            };

        #endregion

        public char Character { get; protected set; }
        protected UIStyle CharStyle { get; set; }
        protected bool[,] Schema { get; set; }

        #endregion

        #region Constructor

        public VisualChar(int x, int y, char character, UIConfiguration configuration = null,
                UIStyle backgroundStyle = null, UIStyle charStyle = null)
            : base(x, y, 0, 0, configuration, backgroundStyle)
        {
            if (!Characters.TryGetValue(character, out bool[,] schema))
                throw new ArgumentException($"'{character}' is not supported.", nameof(character));

            Schema = schema;
            Character = character;
            Width = Schema.GetLength(1);
            Height = Schema.GetLength(0);
            CharStyle = charStyle ?? new UIStyle()
            {
                Wall = 155,
                WallColor = PaintID2.DeepRed
            };
        }

        #endregion
        #region Copy

        public VisualChar(VisualChar wallNumber)
            : this(wallNumber.X, wallNumber.Y, wallNumber.Character,
                  new UIConfiguration(wallNumber.Configuration),
                  new UIStyle(wallNumber.Style), new UIStyle(wallNumber.CharStyle))
        {
        }

        #endregion

        #region ApplyTile

        protected override void ApplyTile(int x, int y)
        {
            UIStyle style = Schema[y, x] ? CharStyle : Style;
            dynamic tile = Tile(x, y);
            if (tile == null)
                return;
            if (style.Active != null)
                tile.active(style.Active.Value);
            else if (style.Tile != null)
                tile.active(true);
            else if (style.Wall != null)
                tile.active(false);
            if (style.InActive != null)
                tile.inActive(style.InActive.Value);
            if (style.Tile != null)
                tile.type = style.Tile.Value;
            if (style.TileColor != null)
                tile.color(style.TileColor.Value);
            if (style.Wall != null)
                tile.wall = style.Wall.Value;
            if (style.WallColor != null)
                tile.wallColor(style.WallColor.Value);
        }

        #endregion
    }
}
