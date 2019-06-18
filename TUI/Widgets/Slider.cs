using System;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    #region SliderStyle

    /// <summary>
    /// Drawing styles for Slider widget.
    /// </summary>
    public class SliderStyle : UIStyle
    {
        /// <summary>
        /// Whether to invoke input callback on TouchState.Moving touches.
        /// </summary>
        public bool TriggerInRuntime { get; set; } = false;
        /// <summary>
        /// Color of left part that corresponds to *used* part of value.
        /// </summary>
        public byte UsedColor { get; set; } = UIDefault.SliderUsedColor;
        /// <summary>
        /// Color of small separator between *used* part and *unused* one.
        /// </summary>
        public byte SeparatorColor { get; set; } = UIDefault.SliderSeparatorColor;

        public SliderStyle() : base() { }

        public SliderStyle(SliderStyle style) : base(style)
        {
            TriggerInRuntime = style.TriggerInRuntime;
            UsedColor = style.UsedColor;
            SeparatorColor = style.SeparatorColor;
        }
    }

    #endregion

    /// <summary>
    /// Input widget for integer values between 0 and width-1.
    /// </summary>
    public class Slider : VisualObject, IInput, IInput<int>
    {
        #region Data

        public Input<int> Input { get; protected set; }
        public object Value => Input.Value;
        public SliderStyle SliderStyle => Style as SliderStyle;

        public double RelativeValue => Input.Value / (Width - 1d);

        #endregion

        #region Constructor

        public Slider(int x, int y, int width, int height, SliderStyle style = null, Input<int> input = null)
            : base(x, y, width, height, new UIConfiguration() { UseMoving = true, UseEnd = true, UseOutsideTouches = true }, style)
        {
            Input = input ?? new Input<int>(0, 0, null);

            Configuration.Lock = new Lock(LockLevel.Self, false, UIDefault.LockDelay, true, true);
        }

        #endregion
        #region Invoke

        public override void Invoke(Touch touch)
        {
            if (touch.State == TouchState.Begin)
                Input.Value = Input.Temp;

            int newValue = touch.Undo ? Input.Value : touch.X;
            if (touch.State == TouchState.End || SliderStyle.TriggerInRuntime)
                SetValue(newValue, true, touch.Session.PlayerIndex);
            else
                SetTempValue(newValue, true);
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
                tile.wallColor((x > Input.Temp) ? Style.WallColor.Value : (x == Input.Temp) ? SliderStyle.SeparatorColor : SliderStyle.UsedColor);
        }

        #endregion
        #region GetValue

        public int GetValue() => Input.Value;

        #endregion
        #region SetTempValue

        public void SetTempValue(int temp, bool draw)
        {
            if (temp < 0)
                temp = 0;
            else if (temp >= Width)
                temp = Width - 1;

            if (Input.Temp != temp)
            {
                int oldTemp = Input.Temp;
                Input.Temp = temp;
                if (draw)
                    ApplyTiles().Draw(temp < oldTemp ? temp : oldTemp, 0,
                    temp > oldTemp ? temp + 1 - oldTemp : oldTemp + 1 - temp);
            }
        }

        #endregion
        #region SetValue

        public void SetValue(int value, bool draw = false, int player = -1)
        {
            SetTempValue(value, draw);
            Input.SubmitTemp(this, player);
        }

        #endregion
        #region PulseThisNative

        protected override void PulseThisNative(PulseType type)
        {
            base.PulseThisNative(type);
            if (type == PulseType.Reset)
                SetValue(Input.DefaultValue, false, -1);
        }

        #endregion
    }
}
