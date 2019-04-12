using System;
using System.Threading.Tasks;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    #region ButtonStyle

    public enum ButtonBlinkStyle
    {
        None = 0,
        Full,
        Left,
        Right
    }

    public class ButtonStyle : LabelStyle
    {
        public ButtonBlinkStyle BlinkStyle { get; set; } = ButtonBlinkStyle.Left;
        public byte BlinkColor { get; set; } = UIDefault.ButtonBlinkColor;
        public int BlinkDelay { get; set; } = 300;

        public ButtonStyle() { }

        public ButtonStyle(ButtonStyle style)
            : base(style) { }
    }

    #endregion

    public class Button : Label
    {
        #region Data

        private object BlinkLocker = new object();
        public ButtonStyle ButtonStyle => Style as ButtonStyle;

        #endregion

        #region Initialize

        public Button(int x, int y, int width, int height, string text, UIConfiguration configuration = null,
                ButtonStyle style = null, Func<VisualObject, Touch, bool> callback = null)
            : base(x, y, width, height, text, configuration, style, callback)
        {
            Configuration.Lock = Configuration.Lock ?? new Lock(LockLevel.Self, true, ButtonStyle.BlinkDelay, true, true);
            Offset offset = ButtonStyle.TextOffset;
            if (ButtonStyle.BlinkStyle == ButtonBlinkStyle.Left || ButtonStyle.BlinkStyle == ButtonBlinkStyle.Right)
            {
                if (offset.Left < 3)
                    offset.Left = 3;
                if (offset.Right < 3)
                    offset.Right = 3;
            }
        }

        #endregion
        #region Copy

        public Button(Button button)
            : this(button.X, button.Y, button.Width, button.Height, button.GetText(), new UIConfiguration(button.Configuration), new ButtonStyle(button.ButtonStyle), button.Callback)
        {
        }

        #endregion
        #region UpdateThisNative

        protected override void UpdateThisNative()
        {
            base.UpdateThisNative();
        }

        #endregion
        #region ApplyThisNative

        protected override void ApplyThisNative()
        {
            base.ApplyThisNative();
        }

        #endregion
        #region Invoke

        public override bool Invoke(Touch touch)
        {
            if (base.Invoke(touch))
            {
                if (ButtonStyle.BlinkStyle != ButtonBlinkStyle.None)
                {
                    Blink();
                    Task.Delay(ButtonStyle.BlinkDelay).ContinueWith(_ => PostBlink());
                }
                return true;
            }
            return false;
        }

        #endregion
        #region Blink

        public virtual void Blink()
        {
            ButtonBlinkStyle blinkStyle = ButtonStyle.BlinkStyle;
            if (blinkStyle == ButtonBlinkStyle.Full)
            {
                //lock (BlinkLocker)
            }
            else if (blinkStyle == ButtonBlinkStyle.Left)
            {

            }
            else if (blinkStyle == ButtonBlinkStyle.Right)
            {

            }
        }

        #endregion
        #region PostBlink

        public virtual void PostBlink()
        {

        }

        #endregion
    }
}
