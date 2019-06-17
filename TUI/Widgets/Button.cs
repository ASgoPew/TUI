using System;
using System.Threading.Tasks;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    #region ButtonStyle

    public enum ButtonTriggerStyle
    {
        TouchBegin = 0,
        TouchEnd
    }

    public enum ButtonBlinkStyle
    {
        None = 0,
        Full,
        Left,
        Right
    }

    public class ButtonStyle : LabelStyle
    {
        /// <summary>
        /// When to invoke Callback: on TouchState.Begin or on TouchState.End.
        /// </summary>
        public ButtonTriggerStyle TriggerStyle { get; set; } = ButtonTriggerStyle.TouchBegin;
        /// <summary>
        /// Style of blinking. Currently supports: left border blinking, right border blinking, full object blinking.
        /// </summary>
        public ButtonBlinkStyle BlinkStyle { get; set; } = ButtonBlinkStyle.Left;
        /// <summary>
        /// Color of blink if BlinkStyle is not None.
        /// </summary>
        public byte BlinkColor { get; set; } = UIDefault.ButtonBlinkColor;
        /// <summary>
        /// Minimal interval of blinking.
        /// </summary>
        public int BlinkDelay { get; set; } = UIDefault.LockDelay;

        public ButtonStyle() : base() { }

        public ButtonStyle(ButtonStyle style)
            : base(style)
        {
            TriggerStyle = style.TriggerStyle;
            BlinkStyle = style.BlinkStyle;
            BlinkColor = style.BlinkColor;
            BlinkDelay = style.BlinkDelay;
        }
    }

    #endregion

    /// <summary>
    /// Widget for invoking callback and blinking on touch.
    /// </summary>
    public class Button : Label
    {
        #region Data

        private byte State;
        private object ButtonLocker = new object();
        private bool Pressed = false;

        public ButtonStyle ButtonStyle => Style as ButtonStyle;

        #endregion

        #region Constructor

        public Button(int x, int y, int width, int height, string text, UIConfiguration configuration = null,
                ButtonStyle style = null, Action<VisualObject, Touch> callback = null)
            : base(x, y, width, height, text, configuration, style ?? new ButtonStyle(), callback)
        {
            if (Configuration.Lock == null)
                Configuration.Lock = new Lock(LockLevel.Self, false, ButtonStyle.BlinkDelay, true, true);
            else
            {
                Configuration.Lock.Personal = false;
                Configuration.Lock.Delay = ButtonStyle.BlinkDelay;
                Configuration.Lock.AllowThisTouchSession = true;
                Configuration.Lock.DuringTouchSession = true;
            }
            Configuration.UseBegin = true;
            Configuration.UseEnd = true;
            Configuration.SessionAcquire = true;
            Configuration.BeginRequire = true;
            Configuration.UseOutsideTouches = true;

            Offset offset = ButtonStyle.TextOffset;
            int minOffset = ButtonStyle.BlinkStyle == ButtonBlinkStyle.Left ? 3 : ButtonStyle.BlinkStyle == ButtonBlinkStyle.Right ? 2 : 0;
            if (offset.Left < minOffset)
                offset.Left = minOffset;
            if (offset.Right < minOffset)
                offset.Right = minOffset;
        }

        #endregion
        #region Copy

        public Button(Button button)
            : this(button.X, button.Y, button.Width, button.Height, button.Get(), new UIConfiguration(
                button.Configuration), new ButtonStyle(button.ButtonStyle), button.Callback)
        {
        }

        #endregion
        #region Invoke

        public override void Invoke(Touch touch)
        {
            if (touch.State == TouchState.Begin)
            {
                State = 0;
                Pressed = true;
                ButtonBlinkStyle blinkStyle = ButtonStyle.BlinkStyle;
                bool blinking = blinkStyle != ButtonBlinkStyle.None && ButtonStyle.BlinkDelay > 0 && Style.WallColor != null;
                lock (ButtonLocker)
                {
                    if (blinking)
                        StartBlink(blinkStyle);

                    if (ButtonStyle.TriggerStyle == ButtonTriggerStyle.TouchBegin)
                        base.Invoke(touch);

                    if (blinking)
                        Task.Delay(ButtonStyle.BlinkDelay).ContinueWith(_ =>
                        {
                            lock (ButtonLocker)
                                TryEndBlink(blinkStyle, 2);
                        });
                }
            }
            else if (touch.State == TouchState.End)
            {
                lock (ButtonLocker)
                {
                    if (ButtonStyle.TriggerStyle == ButtonTriggerStyle.TouchEnd && !touch.Undo)
                        base.Invoke(touch);
                    TryEndBlink(ButtonStyle.BlinkStyle, 1);
                }
            }
        }

        #endregion
        #region ApplyTile

        protected override void ApplyTile(int x, int y)
        {
            dynamic tile = Tile(x, y);
            if (tile == null)
                return;
            if (Style.Active != null)
                tile.active(Style.Active.Value);
            else if (Style.Tile != null)
                tile.active(true);
            else if (Style.Wall != null)
                tile.active(false);
            if (Style.InActive != null)
                tile.inActive(Style.InActive.Value);
            if (Style.Tile != null)
                tile.type = Style.Tile.Value;
            if (Style.TileColor != null)
                tile.color(Style.TileColor.Value);
            if (Style.Wall != null)
                tile.wall = Style.Wall.Value;
            if (Style.WallColor != null)
            {
                ButtonBlinkStyle blinkStyle = ButtonStyle.BlinkStyle;
                tile.wallColor(Pressed && (blinkStyle == ButtonBlinkStyle.Full
                    || blinkStyle == ButtonBlinkStyle.Left && x == 0
                    || blinkStyle == ButtonBlinkStyle.Right && x == Width - 1)
                        ? ButtonStyle.BlinkColor : Style.WallColor.Value);
            }
        }

        #endregion
        #region Blink

        private void TryEndBlink(ButtonBlinkStyle blinkStyle, byte type)
        {
            State |= type;
            if (State == 3)
                EndBlink(blinkStyle);
        }

        protected virtual void StartBlink(ButtonBlinkStyle blinkStyle) =>
            Blink(blinkStyle, ButtonStyle.BlinkColor);

        protected virtual void EndBlink(ButtonBlinkStyle blinkStyle)
        {
            Pressed = false;
            Blink(blinkStyle, Style.WallColor.Value);
        }

        protected virtual void Blink(ButtonBlinkStyle blinkStyle, byte blinkColor)
        {
            if (!CalculateActive())
                return;

            if (blinkStyle == ButtonBlinkStyle.Full)
                Apply().Draw();
            else if (blinkStyle == ButtonBlinkStyle.Left)
            {
                for (int y = 0; y < Height; y++)
                    Tile(0, y)?.wallColor(blinkColor);
                Draw(-Height + 1, 0, Height, Height, forceSection: false);
            }
            else if (blinkStyle == ButtonBlinkStyle.Right)
            {
                for (int y = 0; y < Height; y++)
                    Tile(Width - 1, y)?.wallColor(blinkColor);
                Draw(Width - 1, 0, Height, Height, forceSection: false);
            }
        }

        #endregion
    }
}
