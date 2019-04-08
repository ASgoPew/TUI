using System;
using TUI.Base;

namespace TUI.Widgets
{
    public enum ButtonBlinkStyle
    {
        None = 0,
        SideLine,
        Full
    }

    public class ButtonStyle : LabelStyle
    {
        public ButtonBlinkStyle BlinkStyle { get; set; } = ButtonBlinkStyle.SideLine;

        public ButtonStyle() { }

        public ButtonStyle(ButtonStyle style)
            : base(style)
        {

        }
    }

    public class Button : Label
    {
        public Button(int x, int y, int width, int height, string text, UIConfiguration configuration = null, ButtonStyle style = null,
            Func<VisualObject, Touch, bool> callback = null) : base(x, y, width, height, text, configuration, style, callback)
        {

        }

        public override bool Invoke(Touch touch)
        {
            return base.Invoke(touch);
        }
    }
}
