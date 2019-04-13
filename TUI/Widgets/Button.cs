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
        public ButtonTriggerStyle TriggerStyle { get; set; } = ButtonTriggerStyle.TouchBegin;
        public ButtonBlinkStyle BlinkStyle { get; set; } = ButtonBlinkStyle.Left;
        public byte BlinkColor { get; set; } = UIDefault.ButtonBlinkColor;
        public int BlinkDelay { get; set; } = 300;

        public ButtonStyle() : base() { }

        public ButtonStyle(ButtonStyle style)
            : base(style)
        {
            BlinkStyle = style.BlinkStyle;
            BlinkColor = style.BlinkColor;
            BlinkDelay = style.BlinkDelay;
        }
    }

    #endregion

    public class Button : Label
    {
        #region Data

        private byte State;
        private object ButtonLocker = new object();
        private bool Pressed = false;

        public ButtonStyle ButtonStyle => Style as ButtonStyle;

        #endregion

        #region Initialize

        public Button(int x, int y, int width, int height, string text, UIConfiguration configuration = null,
                ButtonStyle style = null, Func<VisualObject, Touch, bool> callback = null)
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
            : this(button.X, button.Y, button.Width, button.Height, button.GetText(), new UIConfiguration(button.Configuration), new ButtonStyle(button.ButtonStyle), button.Callback)
        {
        }

        #endregion
        #region Invoke

        public override bool Invoke(Touch touch)
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
            return true;
        }

        #endregion
        #region ApplyTiles

        public override VisualObject ApplyTiles()
        {
            if (Style.Active == null && Style.InActive == null && Style.Tile == null && Style.TileColor == null
                    && Style.Wall == null && Style.WallColor == null)
                return this;

            ButtonBlinkStyle blinkStyle = ButtonStyle.BlinkStyle;
            byte blinkColor = ButtonStyle.BlinkColor;
            bool full = blinkStyle == ButtonBlinkStyle.Full;
            bool left = blinkStyle == ButtonBlinkStyle.Left;
            bool right = blinkStyle == ButtonBlinkStyle.Right;
            (int sx, int sy) = ProviderXY();
            foreach ((int x, int y) in ProviderPoints)
            {
                dynamic tile = Provider[x, y];
                if (tile == null)
                    throw new NullReferenceException($"tile is null: {x}, {y}");
                if (Style.Active != null)
                    tile.active(Style.Active.Value);
                if (Style.InActive != null)
                    tile.inActive(Style.InActive.Value);
                if (Style.Tile != null)
                    tile.type = Style.Tile.Value;
                if (Style.TileColor != null)
                    tile.color(Style.TileColor.Value);
                if (Style.Wall != null)
                    tile.wall = Style.Wall.Value;
                if (Style.WallColor != null)
                    tile.wallColor(Pressed && (full || left && x == sx || right && x == sx + Width - 1)
                        ? blinkColor : Style.WallColor.Value);
            }
            return this;
        }

        #endregion
        #region Blink

        private void TryEndBlink(ButtonBlinkStyle blinkStyle, byte type)
        {
            State |= type;
            Console.WriteLine(State);
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
            if (blinkStyle == ButtonBlinkStyle.Full)
                Apply().Draw();
            else if (blinkStyle == ButtonBlinkStyle.Left)
            {
                (int sx, int sy) = ProviderXY();
                for (int y = sy; y < sy + Height; y++)
                    Provider[sx, y].wallColor(blinkColor);
                Draw(-Height + 1, 0, Height, Height, forceSection: false);
            }
            else if (blinkStyle == ButtonBlinkStyle.Right)
            {
                (int sx, int sy) = ProviderXY(Width - 1);
                for (int y = sy; y < sy + Height; y++)
                    Provider[sx, y].wallColor(blinkColor);
                Draw(Width - 1, 0, Height, Height, forceSection: false);
            }
        }

        #endregion
    }
}
