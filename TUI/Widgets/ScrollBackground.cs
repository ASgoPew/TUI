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
        public int BeginIndent { get; set; }

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
            LayoutStyle layout = Parent.Style.Layout;
            if (touch.State == TouchState.Begin)
                BeginIndent = layout.LayoutIndent;
            if (touch.State != TouchState.Begin)
            {
                int indentDelta = Vertical
                    ? touch.AbsoluteY - touch.Session.PreviousTouch.AbsoluteY
                    : touch.AbsoluteX - touch.Session.PreviousTouch.AbsoluteX;
                if (indentDelta != 0 || touch.State == TouchState.End)
                {
                    int newIndent = layout.LayoutIndent + indentDelta;
                    VisualObject first = layout.Objects.FirstOrDefault();
                    VisualObject last = layout.Objects.LastOrDefault();
                    if (first == null || last == null)
                        return true;
                    if (touch.State == TouchState.End)
                    {
                        ExternalOffset offset = layout.Offset;
                        if (layout.Direction == Direction.Left)
                        {
                            if (newIndent < BeginIndent && last.X > offset.Left)
                                newIndent += last.X - offset.Left;
                            else if (newIndent > BeginIndent && first.X + first.Width < Parent.Width - offset.Right)
                                newIndent -= (Parent.Width - offset.Right) - (first.X + first.Width);
                        }
                        else if (layout.Direction == Direction.Up)
                        {
                            if (newIndent < BeginIndent && last.Y > offset.Up)
                                newIndent += last.Y - offset.Up;
                            else if (newIndent > BeginIndent && first.Y + first.Height < Parent.Height - offset.Down)
                                newIndent -= (Parent.Height - offset.Down) - (first.Y + first.Height);
                        }
                        else if (layout.Direction == Direction.Right)
                        {
                            if (newIndent < BeginIndent && last.X + last.Width < Parent.Width - offset.Right)
                                newIndent += (Parent.Width - offset.Right) - (last.X + last.Width);
                            else if (newIndent > BeginIndent && first.X > offset.Left)
                                newIndent -= first.X - offset.Left;
                        }
                        else if (layout.Direction == Direction.Down)
                        {
                            if (newIndent < BeginIndent && last.Y + last.Height < Parent.Height - offset.Down)
                                newIndent += (Parent.Height - offset.Down) - (last.Y + last.Height);
                            else if (newIndent > BeginIndent && first.Y > offset.Up)
                                newIndent -= first.Y - offset.Up;
                        }
                    }
                    Parent.LayoutIndent(newIndent);
                    Parent.Update().Apply().Draw();
                }
            }
            return true;
        }
    }
}
