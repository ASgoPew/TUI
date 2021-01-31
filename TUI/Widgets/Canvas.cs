using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            CustomApplyTile = true;
        }

        public CanvasStyle(CanvasStyle style)
            : base(style)
        {
            CustomApplyTile = true;

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

        #region SetXYWH

        public override VisualObject SetXYWH(int x, int y, int width, int height, bool draw)
        {
            int oldX = X, oldY = Y, oldWidth = Width, oldHeight = Height;
            base.SetXYWH(x, y, width, height, false);
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
            if (draw)
                DrawReposition(oldX, oldY, oldWidth, oldHeight);

            return this;
        }

        #endregion
        #region ApplyTile

        protected override void ApplyTile(int x, int y)
        {
            base.ApplyTile(x, y);
            if (CanvasStyle.CanvasType == CanvasType.Wall)
                Tile(x, y)?.wallColor(Paint[x, y]);
            else
                Tile(x, y)?.color(Paint[x, y]);
        }

        #endregion
        #region PaintPixel

        public Canvas PaintPixel(int x, int y, byte paint, bool draw = false)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height || paint >= PaintID2.Glow)
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
