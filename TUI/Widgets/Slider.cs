using System;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    #region SliderStyle

    public class SliderStyle : UIStyle
    {
        public int Default { get; set; } = 0;
        public bool TriggerOnDrag { get; set; } = false;
        public byte UsedColor { get; set; } = UIDefault.SliderUsedColor;
        public byte SeparatorColor { get; set; } = UIDefault.SliderSeparatorColor;

        public SliderStyle() : base() { }

        public SliderStyle(SliderStyle style) : base(style)
        {
            UsedColor = style.UsedColor;
            SeparatorColor = style.SeparatorColor;
        }
    }

    #endregion

    public class Slider : VisualObject
    {
        #region Data

        private Action<Slider, int> SliderCallback;
        private int OldValue;

        public int Value { get; set; }

        public SliderStyle SliderStyle => Style as SliderStyle;
        public double RelativeValue => Value / (Width - 1d);

        #endregion

        #region Constructor

        public Slider(int x, int y, int width, int height, SliderStyle style = null, Action<Slider, int> callback = null)
            : base(x, y, width, height, new UIConfiguration() { UseMoving = true, UseEnd = true, UseOutsideTouches = true }, style)
        {
            Configuration.Lock = new Lock(LockLevel.Self, false, UIDefault.LockDelay, true, true);

            SliderCallback = callback;
            Value = SliderStyle.Default;
        }

        #endregion
        #region Invoke

        public override void Invoke(Touch touch)
        {
            int oldValue;
            if (touch.State == TouchState.Begin)
                OldValue = Value;
            oldValue = Value;
            int value = touch.Undo ? OldValue : touch.X;
            if (value < 0)
                value = 0;
            if (value >= Width)
                value = Width - 1;
            if (Value != value)
            {
                Value = value;
                ApplyTiles().Draw(Value < oldValue ? Value : oldValue, 0, Value > oldValue ? Value + 1 - oldValue : oldValue + 1 - value);
            }
            if (touch.State == TouchState.End && Value != OldValue && (!SliderStyle.TriggerOnDrag || oldValue != value)
                    || SliderStyle.TriggerOnDrag && oldValue != value)
                SliderCallback?.Invoke(this, Value);
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
                tile.wallColor((x > Value) ? Style.WallColor.Value : (x == Value) ? SliderStyle.SeparatorColor : SliderStyle.UsedColor);
        }

        #endregion
        #region Set

        public Slider Set(int value)
        {
            if (value < 0)
                value = 0;
            if (value >= Width)
                value = Width - 1;
            if (value != Value)
            {
                Value = value;
                SliderCallback?.Invoke(this, Value);
            }
            return this;
        }

        #endregion
        #region PulseThisNative

        protected override void PulseThisNative(PulseType type)
        {
            base.PulseThisNative(type);
            if (type == PulseType.Reset)
                Set(SliderStyle.Default);
        }

        #endregion
    }
}
