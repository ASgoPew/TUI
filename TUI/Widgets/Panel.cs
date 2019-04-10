using System;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    public enum PanelState
    {
        Moving = 0,
        Resizing
    }

    public class Panel : RootVisualObject
    {
        #region Initialize

        internal protected Panel(string name, int x, int y, int width, int height, PanelDrag drag, PanelResize resize,
            UIConfiguration configuration = null, UIStyle style = null, object provider = null)
            : base(name, x, y, width, height, configuration ?? new UIConfiguration() { UseBegin = false }, style, provider)
        {
            if (drag != null)
                Add(drag);
            if (resize != null)
                Add(resize);
        }

        internal protected Panel(string name, int x, int y, int width, int height, UIConfiguration configuration = null,
            UIStyle style = null, object provider = null)
            : this(name, x, y, width, height, new DefaultPanelDrag(), new DefaultPanelResize(), configuration, style, provider)
        { }

        #endregion
        #region Drag

        public void Drag(int dx, int dy)
        {
            if (dx == 0 && dy == 0)
                return;
            if (UsesDefaultMainProvider)
                Clear().Draw().Move(dx, dy).Apply().Draw();
            else
                Move(dx, dy).Draw(-dx, -dy).Draw();
        }

        #endregion
        #region Resize

        public void Resize(int width, int height)
        {
            if (width == Width && height == Height)
                return;
            if (UsesDefaultMainProvider)
                Clear().Draw().SetWH(width, height).Update().Apply().Draw();
            else
            {
                int oldWidth = Width, oldHeight = Height;
                SetWH(width, height).Update().Apply().Draw(0, 0, oldWidth, oldHeight).Draw();
            }
        }

        #endregion
    }

    public class PanelDrag : VisualObject
    {
        public PanelDrag(int x, int y, int width, int height, UIConfiguration configuration = null, UIStyle style = null, Func<VisualObject, Touch, bool> callback = null)
            : base(x, y, width, height, configuration, style, callback)
        {
            Configuration.UseEnd = true;
            if (Callback == null)
                Callback = CustomCallback;
        }

        private static bool CustomCallback(VisualObject @this, Touch touch)
        {
            if (touch.State == TouchState.Begin)
            {
                touch.Session[@this] = PanelState.Moving;
                return true;
            }
            else if (touch.Session[@this] is PanelState state && state == PanelState.Moving)
            {
                bool ending = touch.State == TouchState.End;
                if (@this.Configuration.UseMoving && touch.State == TouchState.Moving || ending)
                {
                    int dx = touch.AbsoluteX - touch.Session.PreviousTouch.AbsoluteX;
                    int dy = touch.AbsoluteY - touch.Session.PreviousTouch.AbsoluteY;
                    ((Panel)@this.Parent).Drag(dx, dy);
                    if (ending)
                        touch.Session[@this] = null;
                    return true;
                }
            }
            return false;
        }
    }

    public class PanelResize : VisualObject
    {
        public PanelResize(int x, int y, int width, int height, UIConfiguration configuration = null, UIStyle style = null, Func<VisualObject, Touch, bool> callback = null)
            : base(x, y, width, height, configuration, style, callback)
        {
            Configuration.UseEnd = true;
            if (Callback == null)
                Callback = CustomCallback;
        }

        private static bool CustomCallback(VisualObject @this, Touch touch)
        {
            if (touch.State == TouchState.Begin)
            {
                touch.Session[@this] = PanelState.Resizing;
                return true;
            }
            else if (touch.Session[@this] is PanelState state && state == PanelState.Resizing)
            {
                bool ending = touch.State == TouchState.End;
                if (@this.Configuration.UseMoving && touch.State == TouchState.Moving || ending)
                {
                    int dw = touch.AbsoluteX - touch.Session.PreviousTouch.AbsoluteX;
                    int dh = touch.AbsoluteY - touch.Session.PreviousTouch.AbsoluteY;
                    Panel panel = (Panel)@this.Parent;
                    panel.Resize(panel.Width + dw, panel.Height + dh);
                    if (ending)
                        touch.Session[@this] = null;
                    return true;
                }
            }
            return false;
        }
    }

    public sealed class DefaultPanelDrag : PanelDrag
    {
        public DefaultPanelDrag()
            : base(0, 0, 1, 1, new UIConfiguration() { UseMoving = true, UseEnd = true, UseOutsideTouches = true })
        {
        }
    }

    public sealed class DefaultPanelResize : PanelResize
    {
        public DefaultPanelResize()
            : base(0, 0, 1, 1, new UIConfiguration() { UseMoving = true, UseEnd = true, UseOutsideTouches = true })
        {
        }

        public override VisualObject Update() =>
            SetXY(Parent.Width - 1, Parent.Height - 1);
    }
}
