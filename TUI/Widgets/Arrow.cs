using System;
using System.Collections.Generic;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Widgets
{
    #region ArrowStyle

    public enum ArrowSize
    {
        Small = 2,
        Big = 4
    }

    /// <summary>
    /// Drawing styles for Arrow widget.
    /// </summary>
    public class ArrowStyle : UIStyle
    {
        public Direction Direction { get; set; } = Direction.Right;
        public ArrowSize Size { get; set; } = ArrowSize.Small;

        public ArrowStyle() : base() { }

        public ArrowStyle(ArrowStyle arrowStyle)
            : base(arrowStyle)
        {
            Direction = arrowStyle.Direction;
            Size = arrowStyle.Size;
        }
    }

    #endregion

    /// <summary>
    /// Widget for drawing arrow in one of directions.
    /// </summary>
    public class Arrow : VisualObject
    {
        #region Data

        /// <summary>
        /// 1: down left
        /// 2: down right
        /// 3: up left
        /// 4: up right
        /// </summary>
        protected static readonly Dictionary<Direction, byte[,]> SmallSlope = new Dictionary<Direction, byte[,]>()
        {
            { Direction.Left, new byte[2, 2] {
                { 2, 3 },
                { 4, 1 }
            } },
            { Direction.Up, new byte[2, 2] {
                { 2, 1 },
                { 3, 4 }
            } },
            { Direction.Right, new byte[2, 2] {
                { 4, 1 },
                { 2, 3 }
            } },
            { Direction.Down, new byte[2, 2] {
                { 1, 2 },
                { 4, 3 }
            } }
        };

        protected static readonly Dictionary<Direction, byte[,]> BigSlope = new Dictionary<Direction, byte[,]>()
        {
            { Direction.Left, new byte[4, 4] {
                { 255, 2, 255, 255 },
                { 2,   0, 0,   0   },
                { 4,   0, 0,   0   },
                { 255, 4, 255, 255 }
            } },
            { Direction.Up, new byte[4, 4] {
                { 255, 2, 1, 255 },
                { 2,   0, 0, 1   },
                { 255, 0, 0, 255 },
                { 255, 0, 0, 255 }
            } },
            { Direction.Right, new byte[4, 4] {
                { 255, 255, 1, 255 },
                { 0,   0,   0, 1   },
                { 0,   0,   0, 3   },
                { 255, 255, 3, 255 }
            } },
            { Direction.Down, new byte[4, 4] {
                { 255, 0, 0, 255 },
                { 255, 0, 0, 255 },
                { 4,   0, 0, 3   },
                { 255, 4, 3, 255 }
            } }
        };

        public ArrowStyle ArrowStyle => Style as ArrowStyle;

        #endregion

        #region Constructor

        /// <summary>
        /// Widget for drawing arrow in one of directions.
        /// </summary>
        public Arrow(int x, int y, ArrowStyle style = null, Action<VisualObject, Touch> callback = null)
            : base(x, y, 0, 0, null, style ?? new ArrowStyle(), callback)
        {
            if (ArrowStyle.Size == ArrowSize.Small)
                SetWH(2, 2, false);
            else
                SetWH(4, 4, false);
        }

        #endregion
        #region Copy

        public Arrow(Arrow arrow)
            : this(arrow.X, arrow.Y, new ArrowStyle(arrow.ArrowStyle),
                  arrow.Callback?.Clone() as Action<VisualObject, Touch>)
        {
        }

        #endregion
        #region ApplyThisNative

        protected override void ApplyThisNative()
        {
            base.ApplyThisNative();

            if (ArrowStyle.Size == ArrowSize.Small)
                foreach ((int x, int y) in Points)
                {
                    dynamic tile = Tile(x, y);
                    if (tile == null)
                        continue;
                    tile.active(true);
                    tile.type = Style.Tile ?? 267;
                    tile.slope(SmallSlope[ArrowStyle.Direction][y, x]);
                }
            else
                foreach ((int x, int y) in Points)
                {
                    dynamic tile = Tile(x, y);
                    if (tile == null)
                        continue;
                    byte slope = BigSlope[ArrowStyle.Direction][y, x];
                    if (slope < 5)
                    {
                        tile.active(true);
                        tile.type = Style.Tile ?? 267;
                        tile.slope(slope);
                    }
                    else
                        tile.active(false);
                }
        }

        #endregion
    }
}
