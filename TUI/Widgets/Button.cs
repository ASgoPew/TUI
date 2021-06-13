using System;
using System.Threading.Tasks;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Widgets
{
    #region ButtonStyle

    public enum ButtonTriggerStyle
    {
        TouchBegin = 0,
        TouchEnd,
        Both
    }

    public enum ButtonBlinkStyle
    {
        None = 0,
        Full,
        Left,
        Right
    }

    /// <summary>
    /// Drawing styles for Button widget.
    /// </summary>
    public class ButtonStyle : LabelStyle
    {
        /// <summary>
        /// When to invoke Callback: on TouchState.Begin, on TouchState.End or on both.
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
        private bool BlinkOn = false;

        public ButtonStyle ButtonStyle => Style as ButtonStyle;

        #endregion

        #region Constructor

        /// <summary>
        /// Widget for invoking callback and blinking on touch.
        /// </summary>
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

            Indent indent = ButtonStyle.TextIndent;
            int minIndent = ButtonStyle.BlinkStyle == ButtonBlinkStyle.Left ? 3 : ButtonStyle.BlinkStyle == ButtonBlinkStyle.Right ? 2 : 0;
            if (indent.Left < minIndent)
                indent.Left = minIndent;
            if (indent.Right < minIndent)
                indent.Right = minIndent;
        }

        #endregion
        #region Copy

        public Button(Button button)
            : this(button.X, button.Y, button.Width, button.Height, button.GetText(), new UIConfiguration(
                button.Configuration), new ButtonStyle(button.ButtonStyle), button.Callback)
        {
        }

        #endregion
        #region Invoke

        protected override void Invoke(Touch touch)
        {
            if (touch.State == TouchState.Begin)
            {
                State = 0;
                ButtonBlinkStyle blinkStyle = ButtonStyle.BlinkStyle;
                bool blinking = blinkStyle != ButtonBlinkStyle.None && ButtonStyle.BlinkDelay > 0 && Style.WallColor != null;
                if (blinking)
                    StartBlink(blinkStyle);

                if (ButtonStyle.TriggerStyle == ButtonTriggerStyle.TouchBegin || ButtonStyle.TriggerStyle == ButtonTriggerStyle.Both)
                    base.Invoke(touch);

                if (blinking)
                    Task.Delay(ButtonStyle.BlinkDelay).ContinueWith(_ =>
                    {
                        try
                        {
                            TryEndBlink(blinkStyle, 2);
                        }
                        catch (Exception e)
                        {
                            TUI.HandleException(this, e);
                        }
                    });
            }
            else if (touch.State == TouchState.End)
            {
                if ((ButtonStyle.TriggerStyle == ButtonTriggerStyle.TouchEnd || ButtonStyle.TriggerStyle == ButtonTriggerStyle.Both)
                        && !touch.Undo)
                    base.Invoke(touch);
                TryEndBlink(ButtonStyle.BlinkStyle, 1);
            }
        }

        #endregion
        #region ApplyTile

        protected override void ApplyTile(int x, int y, dynamic tile)
        {
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
                tile.wallColor(BlinkOn && (blinkStyle == ButtonBlinkStyle.Full
                    || blinkStyle == ButtonBlinkStyle.Left && x == 0
                    || blinkStyle == ButtonBlinkStyle.Right && x == Width - 1)
                        ? ButtonStyle.BlinkColor : Style.WallColor.Value);
            }
        }

        #endregion
        #region Blink

        public virtual void StartBlink(ButtonBlinkStyle blinkStyle)
        {
            BlinkOn = true;
            Blink(blinkStyle, ButtonStyle.BlinkColor);
        }

        private void TryEndBlink(ButtonBlinkStyle blinkStyle, byte type)
        {
            State |= type;
            if (State == 3)
                EndBlink(blinkStyle);
            else if (!IsActive)
                BlinkOn = false;
        }

        public virtual void EndBlink(ButtonBlinkStyle blinkStyle)
        {
            BlinkOn = false;
            Blink(blinkStyle, Style.WallColor.Value);
        }

        public virtual void Blink(ButtonBlinkStyle blinkStyle, byte blinkColor)
        {
            if (!IsActive)
            {
                BlinkOn = false;
                return;
            }

            if (blinkStyle == ButtonBlinkStyle.Full)
            {
                foreach (var point in Points)
                    Tile(point.Item1, point.Item2)?.wallColor(blinkColor);
                RequestDrawChanges();
                Draw();
            }
            else if (blinkStyle == ButtonBlinkStyle.Left)
            {
                for (int y = 0; y < Height; y++)
                    Tile(0, y)?.wallColor(blinkColor);
                RequestDrawChanges();
                Draw(0, 0, 1, Height, drawWithSection: false);
            }
            else if (blinkStyle == ButtonBlinkStyle.Right)
            {
                for (int y = 0; y < Height; y++)
                    Tile(Width - 1, y)?.wallColor(blinkColor);
                RequestDrawChanges();
                Draw(Width - 1, 0, 1, Height, drawWithSection: false);
            }
        }

        #endregion
    }
}
