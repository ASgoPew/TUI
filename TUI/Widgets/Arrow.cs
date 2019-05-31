using System;
using System.Collections.Generic;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    #region ArrowStyle

    public class ArrowStyle : UIStyle
    {
        public Direction Direction { get; set; } = Direction.Right;

        public ArrowStyle()
            : base()
        {
        }

        public ArrowStyle(ArrowStyle arrowStyle)
            : base(arrowStyle)
        {
            Direction = arrowStyle.Direction;
        }
    }

    #endregion

    public class Arrow : VisualObject
    {
        #region Data

        protected static readonly Dictionary<Direction, byte[,]> Slope = new Dictionary<Direction, byte[,]>()
        {
            { Direction.Left, new byte[2, 2] { { 2, 3 }, { 4, 1 } } },
            { Direction.Up, new byte[2, 2] { { 2, 1 }, { 3, 4 } } },
            { Direction.Right, new byte[2, 2] { { 4, 1 }, { 2, 3 } } },
            { Direction.Down, new byte[2, 2] { { 1, 2 }, { 4, 3 } } }
        };

        public ArrowStyle ArrowStyle => Style as ArrowStyle;

        #endregion

        #region Initialize

        public Arrow(int x, int y, ArrowStyle style = null, Func<VisualObject, Touch, bool> callback = null)
            : base(x, y, 2, 2, null, style, callback)
        {
            Style.Active = true;
            if (Style.Tile == null)
                Style.Tile = 267;
        }

        #endregion
        #region Copy

        public Arrow(Arrow arrow)
            : this(arrow.X, arrow.Y, new ArrowStyle(arrow.ArrowStyle), arrow.Callback?.Clone() as Func<VisualObject, Touch, bool>)
        {
        }

        #endregion
        #region ApplyThisNative

        protected override void ApplyThisNative()
        {
            base.ApplyThisNative();

            foreach ((int x, int y) in Points)
                Tile(x, y)?.slope(Slope[ArrowStyle.Direction][y, x]);
        }

        #endregion
    }
}
