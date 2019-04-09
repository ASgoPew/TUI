using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    #region PanelStyle

    public enum PanelState
    {
        Moving = 0,
        Resizing
    }

    public class PanelStyle : UIStyle
    {
        public PanelStyle() { }

        public PanelStyle(PanelStyle style)
            : this() { }
    }

    #endregion

    public class Panel : RootVisualObject
    {
        #region Data

        public PanelStyle PanelStyle => Style as PanelStyle;

        #endregion

        #region Initialize

        public Panel(string name, int x, int y, int width, int height, PanelStyle style = null, object provider = null)
            : base(name, x, y, width, height, new UIConfiguration() { UseBegin = false }, style, provider)
        {
            
        }

        #endregion
        #region Invoke

        public sealed override bool Invoke(Touch touch)
        {
            if (touch.State == TouchState.Begin)
            {
                if (touch.X == 0 && touch.Y == 0)
                {
                    touch.Session[this] = PanelState.Moving;
                    return true;
                }
                else if (touch.X == Width - 1 && touch.Y == Height - 1)
                {
                    touch.Session[this] = PanelState.Resizing;
                    // 
                    return true;
                }
            }
            else if (touch.Session[this] is PanelState state)
            {
                if (touch.State == TouchState.Moving)
                {
                    int oldX = X, oldY = Y;
                    int dx = touch.AbsoluteX - touch.Session.PreviousTouch.AbsoluteX;
                    int dy = touch.AbsoluteY - touch.Session.PreviousTouch.AbsoluteY;
                    return true;
                }
                else
                {
                    touch.Session[this] = null;
                    return true;
                }
            }
            return false;
        }

        #endregion
    }

    public class PanelDragging : VisualObject
    {
        public PanelDragging()
            : base(0, 0, 1, 1, new UIConfiguration() { UseMoving = true, UseEnd = true, UseOutsideTouches = true })
        {
        }
    }
}
