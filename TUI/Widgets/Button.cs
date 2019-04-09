using System;
using TUI.Base;

namespace TUI.Widgets
{
    #region ButtonStyle

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
            : base(style) { }
    }

    #endregion

    public class Button : Label
    {
        #region Data

        public ButtonStyle ButtonStyle => Style as ButtonStyle;

        #endregion

        #region Initialize

        public Button(int x, int y, int width, int height, string text, UIConfiguration configuration = null, ButtonStyle style = null,
            Func<VisualObject, Touch, bool> callback = null) : base(x, y, width, height, text, configuration, style, callback)
        {

        }

        #endregion
        #region Copy

        public Button(Button button)
            : this(button.X, button.Y, button.Width, button.Height, button.GetText(), new UIConfiguration(button.Configuration), new ButtonStyle(button.ButtonStyle), button.Callback)
        {
        }

        #endregion
        #region Invoke

        public override bool Invoke(Touch touch)
        {
            return base.Invoke(touch);
        }

        #endregion
    }
}
