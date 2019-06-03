using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    #region ScrollBarStyle

    public class ScrollBarStyle : UIStyle
    {
        

        public ScrollBarStyle() : base() { }

        public ScrollBarStyle(ScrollBarStyle style) : base(style)
        {
        }
    }

    #endregion


    class ScrollBar : VisualObject
    {
        public ScrollBar(Direction side = Direction.Right, int width = 1    , UIStyle style = null)
            : base(0, 0, 0, 0, new UIConfiguration() { UseMoving=true, UseEnd=true, UseOutsideTouches=true }, style)
        {
        }
    }
}
