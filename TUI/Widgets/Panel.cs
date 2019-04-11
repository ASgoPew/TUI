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
        #region Data

        internal int DragX { get; set; }
        internal int DragY { get; set; }
        internal int ResizeW { get; set; }
        internal int ResizeH { get; set; }

        public PanelDrag DragObject { get; set; }
        public PanelResize ResizeObject { get; set; }

        #endregion

        #region Initialize

        internal protected Panel(string name, int x, int y, int width, int height, PanelDrag drag, PanelResize resize,
            UIConfiguration configuration = null, UIStyle style = null, object provider = null)
            : base(name, x, y, width, height, configuration ?? new UIConfiguration() { UseBegin = false }, style, provider)
        {
            DragObject = drag;
            ResizeObject = resize;
            if (drag != null)
                Add(drag, 1000000);
            if (resize != null)
                Add(resize, 1000000);
        }

        internal protected Panel(string name, int x, int y, int width, int height, UIConfiguration configuration = null,
            UIStyle style = null, object provider = null)
            : this(name, x, y, width, height, new DefaultPanelDrag(), new DefaultPanelResize(), configuration, style, provider)
        { }

        #endregion
        #region Drag

        public void Drag(int x, int y)
        {
            if (x == X && y == Y)
                return;
            if (UsesDefaultMainProvider)
                Clear().Draw().SetXY(x, y).Apply().Draw();
            else
            {
                int oldX = X, oldY = Y;
                SetXY(x, y).Draw(oldX - x, oldY - y).Draw();
            }
        }

        #endregion
        #region Resize

        public void Resize(int width, int height)
        {
            if (width < Style.Grid.MinWidth)
                width = Style.Grid.MinWidth;
            if (height < Style.Grid.MinHeight)
                height = Style.Grid.MinHeight;
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
                Panel panel = (Panel)@this.Parent;
                panel.DragX = panel.X;
                panel.DragY = panel.Y;
                touch.Session[@this] = PanelState.Moving;
                return true;
            }
            else if (touch.Session[@this] is PanelState state && state == PanelState.Moving)
            {
                bool ending = touch.State == TouchState.End;
                if (@this.Configuration.UseMoving && touch.State == TouchState.Moving || ending)
                {
                    int dx = touch.AbsoluteX - touch.Session.BeginTouch.AbsoluteX;
                    int dy = touch.AbsoluteY - touch.Session.BeginTouch.AbsoluteY;
                    Panel panel = (Panel)@this.Parent;
                    panel.Drag(panel.DragX + dx, panel.DragY + dy);
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
                Panel panel = (Panel)@this.Parent;
                panel.ResizeW = panel.Width;
                panel.ResizeH = panel.Height;
                touch.Session[@this] = PanelState.Resizing;
                return true;
            }
            else if (touch.Session[@this] is PanelState state && state == PanelState.Resizing)
            {
                bool ending = touch.State == TouchState.End;
                if (@this.Configuration.UseMoving && touch.State == TouchState.Moving || ending)
                {
                    int dw = touch.AbsoluteX - touch.Session.BeginTouch.AbsoluteX;
                    int dh = touch.AbsoluteY - touch.Session.BeginTouch.AbsoluteY;
                    Panel panel = (Panel)@this.Parent;
                    panel.Resize(panel.ResizeW + dw, panel.ResizeH + dh);
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
