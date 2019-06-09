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

        #region Constructor

        internal protected Panel(string name, int x, int y, int width, int height, PanelDrag drag, PanelResize resize,
                UIConfiguration configuration = null, UIStyle style = null, object provider = null)
            : base(name, x, y, width, height, configuration ?? new UIConfiguration() { UseBegin = true,
                UseMoving = true, UseEnd = true }, style, provider)
        {
            if (drag != null)
                DragObject = Add(drag, 1000000) as PanelDrag;
            if (resize != null)
                ResizeObject = Add(resize, 1000000) as PanelResize;
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
                Clear().Draw().SetXY(x, y).Update().Apply().Draw();
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
            GridStyle grid = Style.Grid;
            int minWidth = grid?.MinWidth ?? 1;
            int minHeight = grid?.MinWidth ?? 1;
            if (width < minWidth)
                width = minWidth;
            if (height < minHeight)
                height = minHeight;
            if (width == Width && height == Height)
                return;
            if (UsesDefaultMainProvider)
                Clear().Draw(frame: false).SetWH(width, height).Update().Apply().Draw();
            else
            {
                int oldWidth = Width, oldHeight = Height;
                SetWH(width, height).Update().Apply().Draw(0, 0, oldWidth, oldHeight, frame: false).Draw();
            }
        }

        #endregion
    }

    #region PanelDrag

    public class PanelDrag : VisualObject
    {
        public PanelDrag(int x, int y, int width, int height, UIConfiguration configuration = null,
                UIStyle style = null, Action<VisualObject, Touch> callback = null)
            : base(x, y, width, height, configuration, style, callback)
        {
            Configuration.UseEnd = true;
            if (Callback == null)
                Callback = CustomCallback;
        }

        private static void CustomCallback(VisualObject @this, Touch touch)
        {
            if (touch.State == TouchState.Begin)
            {
                Panel panel = (Panel)@this.Parent;
                panel.DragX = panel.X;
                panel.DragY = panel.Y;
                touch.Session[@this] = PanelState.Moving;
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
                }
            }
        }
    }

    #endregion
    #region PanelResize

    public class PanelResize : VisualObject
    {
        public PanelResize(int x, int y, int width, int height, UIConfiguration configuration = null,
                UIStyle style = null, Action<VisualObject, Touch> callback = null)
            : base(x, y, width, height, configuration, style, callback)
        {
            Configuration.UseEnd = true;
            if (Callback == null)
                Callback = CustomCallback;
        }

        private static void CustomCallback(VisualObject @this, Touch touch)
        {
            if (touch.State == TouchState.Begin)
            {
                Panel panel = (Panel)@this.Parent;
                panel.ResizeW = panel.Width;
                panel.ResizeH = panel.Height;
                touch.Session[@this] = PanelState.Resizing;
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
                }
            }
        }
    }

    #endregion
    #region DefaultPanelDrag

    public sealed class DefaultPanelDrag : PanelDrag
    {
        public DefaultPanelDrag()
            : base(0, 0, 1, 1, new UIConfiguration() { UseMoving=true, UseEnd=true, UseOutsideTouches=true, Permission="TUI.Control" })
        {
        }
    }

    #endregion
    #region DefaultPanelResize

    public sealed class DefaultPanelResize : PanelResize
    {
        public DefaultPanelResize()
            : base(0, 0, 1, 1, new UIConfiguration() { UseMoving=true, UseEnd=true, UseOutsideTouches=true, Permission="TUI.Control" })
        {
            SetAlignmentInParent(new AlignmentStyle(Alignment.DownRight));
        }
    }

    #endregion
}
