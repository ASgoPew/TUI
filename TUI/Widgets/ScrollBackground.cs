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
        public int BeginIndent { get; protected set; }
        public bool AllowToPull { get; set; }
        public bool RememberTouchPosition { get; set; }

        public ScrollBackground(bool allowToPull = true, bool rememberTouchPosition = true)
            : base(0, 0, 0, 0, new UIConfiguration() { UseMoving=true, UseEnd=true, UseOutsideTouches=true })
        {
            SetFullSize(FullSize.Both);
            AllowToPull = allowToPull;
            RememberTouchPosition = rememberTouchPosition;
        }

        public override bool Invoke(Touch touch)
        {
            if (Parent?.Style?.Layout == null)
                throw new Exception("Scroll has no parent or parent doesn't have layout.");
            LayoutStyle layout = Parent.Style.Layout;
            int indent = layout.LayoutIndent;
            int limit = layout.IndentLimit;
            bool vertical = layout.Direction == Direction.Up || layout.Direction == Direction.Down;
            if (touch.State == TouchState.Begin)
                BeginIndent = indent;
            if (touch.State != TouchState.Begin)
            {
                int newIndent;
                if (RememberTouchPosition)
                {
                    int indentDelta = vertical
                        ? touch.AbsoluteY - touch.Session.PreviousTouch.AbsoluteY
                        : touch.AbsoluteX - touch.Session.PreviousTouch.AbsoluteX;
                    if (layout.Direction == Direction.Right || layout.Direction == Direction.Down)
                        newIndent = indent - indentDelta;
                    else
                        newIndent = indent + indentDelta;
                }
                else
                    newIndent = vertical
                        ? touch.Session.BeginTouch.AbsoluteY - touch.AbsoluteY
                        : touch.Session.BeginTouch.AbsoluteX - touch.AbsoluteX;
                if (newIndent != indent || touch.State == TouchState.End)
                {
                    VisualObject first = layout.Objects.FirstOrDefault();
                    VisualObject last = layout.Objects.LastOrDefault();
                    if (first == null || last == null)
                        return true;
                    if (touch.State == TouchState.End || !AllowToPull)
                    {
                        if (newIndent < 0)
                            newIndent = 0;
                        else if (newIndent > limit)
                            newIndent = limit;
                    }
                    if (Parent.Style.Layout.LayoutIndent != newIndent)
                    {
                        Parent.LayoutIndent(newIndent);
                        Parent.Update().Apply().Draw();
                    }
                }
            }
            return true;
        }
    }
}
