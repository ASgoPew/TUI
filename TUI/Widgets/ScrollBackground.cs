using System;
using System.Linq;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    /// <summary>
    /// Widget for scrolling parent's layout by pulling layout background.
    /// </summary>
    public class ScrollBackground : VisualObject
    {
        #region Data

        private Action<ScrollBackground, int> ScrollBackgroundCallback;
        public int BeginIndent { get; protected set; }
        public int Limit { get; protected set; }
        public bool AllowToPull { get; set; }
        public bool RememberTouchPosition { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Widget for scrolling parent's layout by pulling layout background.
        /// </summary>
        /// <param name="allowToPull">Ability to pull beyond a border</param>
        /// <param name="rememberTouchPosition">Pulling the same point of layout background during touch session</param>
        public ScrollBackground(bool allowToPull = true, bool rememberTouchPosition = true, bool useMoving = true, Action<ScrollBackground, int> callback = null)
            : base(0, 0, 0, 0, new UIConfiguration() { UseMoving=useMoving, UseEnd=true, UseOutsideTouches=true })
        {
            AllowToPull = allowToPull;
            RememberTouchPosition = rememberTouchPosition;
            ScrollBackgroundCallback = callback;

            SetFullSize(FullSize.Both);
        }

        #endregion
        #region Invoke

        public override void Invoke(Touch touch)
        {
            if (Parent?.Configuration?.Layout == null)
                throw new Exception("Scroll has no parent or parent doesn't have layout.");
            LayoutConfiguration layout = Parent.Configuration.Layout;
            int indent = layout.LayoutIndent;
            Limit = layout.IndentLimit;
            bool vertical = layout.Direction == Direction.Up || layout.Direction == Direction.Down;
            bool forward = layout.Direction == Direction.Right || layout.Direction == Direction.Down;
            if (touch.State == TouchState.Begin)
                BeginIndent = indent;
            if (touch.State == TouchState.End || (Configuration.UseMoving && touch.State == TouchState.Moving))
            {
                int newIndent;
                int indentDelta;
                if (RememberTouchPosition)
                {
                    indentDelta = vertical
                        ? touch.AbsoluteY - touch.Session.BeginTouch.AbsoluteY
                        : touch.AbsoluteX - touch.Session.BeginTouch.AbsoluteX;
                    newIndent = BeginIndent + (forward ? -indentDelta : indentDelta);
                }
                else
                {
                    indentDelta = vertical
                        ? touch.AbsoluteY - touch.Session.PreviousTouch.AbsoluteY
                        : touch.AbsoluteX - touch.Session.PreviousTouch.AbsoluteX;
                    newIndent = indent + (forward ? -indentDelta : indentDelta);
                }
                if (newIndent != indent || touch.State == TouchState.End)
                {
                    VisualObject first = layout.Objects.FirstOrDefault();
                    VisualObject last = layout.Objects.LastOrDefault();
                    if (first == null)
                        return;
                    if (touch.State == TouchState.End || !AllowToPull)
                    {
                        if (newIndent < 0)
                            newIndent = 0;
                        else if (newIndent > Limit)
                            newIndent = Limit;
                    }
                    if (Parent.Configuration.Layout.LayoutIndent != newIndent)
                    {
                        Parent.LayoutIndent(newIndent);
                        Action<ScrollBackground, int> callback = ScrollBackgroundCallback;
                        if (callback != null)
                            callback.Invoke(this, newIndent);
                        else
                            Parent.Update().Apply().Draw();
                    }
                }
            }
        }

        #endregion
    }
}
