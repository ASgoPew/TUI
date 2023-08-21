using System;
using System.Collections.Generic;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Widgets
{
    #region CanvasStyle

    public enum CanvasType
    {
        Wall,
        Tile
    }

    public class CanvasStyle : ContainerStyle
    {
        public CanvasType CanvasType = CanvasType.Wall;

        public CanvasStyle()
            : base()
        {
        }

        public CanvasStyle(CanvasStyle style)
            : base(style)
        {
            CanvasType = style.CanvasType;
        }
    }

    #endregion

    public class Canvas : VisualObject
    {
        #region Data

        protected byte[,] Paint { get; set; }

        public CanvasStyle CanvasStyle => Style as CanvasStyle;
        public byte DefaultPaint => (CanvasStyle.CanvasType == CanvasType.Wall
            ? Style.WallColor
            : Style.TileColor) ?? PaintID2.White;

        #endregion

        #region Constructor

        public Canvas(int x, int y, int width, int height, UIConfiguration configuration = null, CanvasStyle style = null, Action<VisualObject, Touch> callback = null)
            : base(x, y, width, height, configuration, style ?? new CanvasStyle(), callback)
        {
            Paint = new byte[Width, Height];
            for (int _x = 0; _x < Width; _x++)
                for (int _y = 0; _y < Height; _y++)
                    Paint[_x, _y] = DefaultPaint;
        }

        #endregion

        #region ApplyHasAnything

        protected override bool ApplyHasAnything() => true;

        #endregion
        #region SetXYWH

        public override VisualObject SetXYWH(int x, int y, int width, int height, bool draw)
        {
            int oldWidth = Width, oldHeight = Height;
            base.SetXYWH(x, y, width, height, draw);
            if (oldWidth != Width || oldHeight != Height)
            {
                byte[,] oldPaint = Paint;
                Paint = new byte[Width, Height];
                int minWidth = Width < oldWidth ? Width : oldWidth;
                int minHeight = Height < oldHeight ? Height : oldHeight;
                for (int _x = 0; _x < Width; _x++)
                    for (int _y = 0; _y < Height; _y++)
                        if (_x < minWidth && _y < minHeight)
                            Paint[_x, _y] = oldPaint[_x, _y];
                        else
                            Paint[_x, _y] = DefaultPaint;
            }
            return this;
        }

        #endregion
        #region ApplyTile

        protected override void ApplyTile(int x, int y, dynamic tile)
        {
            if (Style.Active.HasValue)
                tile.active(Style.Active.Value);
            else if (Style.Tile.HasValue)
                tile.active(true);
            else if (Style.Wall.HasValue)
                tile.active(false);
            if (Style.InActive.HasValue)
                tile.inActive(Style.InActive.Value);
            if (Style.Tile.HasValue)
                tile.type = Style.Tile.Value;
            if (Style.TileColor.HasValue)
                tile.color(Style.TileColor.Value);
            if (Style.TileCoating is HashSet<byte> tileCoating)
            {
                tile.fullbrightBlock(tileCoating.Contains(PaintCoatingID2.Glow));
                tile.invisibleBlock(tileCoating.Contains(PaintCoatingID2.Echo));
            }
            if (Style.Wall.HasValue)
                tile.wall = Style.Wall.Value;
            if (Style.WallColor.HasValue)
                tile.wallColor(Style.WallColor.Value);
            if (Style.WallCoating is HashSet<byte> wallCoating)
            {
                tile.fullbrightWall(wallCoating.Contains(PaintCoatingID2.Glow));
                tile.invisibleWall(wallCoating.Contains(PaintCoatingID2.Echo));
            }

            if (CanvasStyle.CanvasType == CanvasType.Wall)
                tile.wallColor(Paint[x, y]);
            else
                tile.color(Paint[x, y]);
        }

        #endregion
        #region PaintPixel

        public Canvas PaintPixel(int x, int y, byte paint, bool draw = false)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height || paint > PaintID2.Negative)
                throw new ArgumentOutOfRangeException();

            Paint[x, y] = paint;
            if (draw)
                Apply().Draw();

            return this;
        }

        #endregion
        #region GetPixel

        public byte GetPixel(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                throw new ArgumentOutOfRangeException();

            return Paint[x, y];
        }

        #endregion
    }
}
