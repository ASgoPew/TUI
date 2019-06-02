using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    public class ScrollBackground : VisualObject
    {
        public bool Vertical { get; set; }

        public ScrollBackground(bool vertical = true)
            : base(0, 0, 0, 0, new UIConfiguration() { UseMoving=true, UseEnd=true, UseOutsideTouches=true })
        {
            SetFullSize(FullSize.Both);
            Vertical = vertical;
        }

        public override bool Invoke(Touch touch)
        {
            if (Parent?.Style?.Layout == null)
                throw new Exception("Scroll added to object without layout.");
            if (touch.State != TouchState.Begin)
            {
                int indent = Vertical
                    ? touch.AbsoluteY - touch.Session.BeginTouch.AbsoluteY
                    : touch.AbsoluteX - touch.Session.BeginTouch.AbsoluteX;
                if (indent != Parent.Style.Layout.LayoutIndent)
                {
                    Parent.LayoutIndent(indent);
                    Parent.Update().Apply().Draw();
                }
            }
            return true;
        }
    }
}
