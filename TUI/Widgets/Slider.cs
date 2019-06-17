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
            int oldValue;
            if (touch.State == TouchState.Begin)
                Input.OldValue = Input.Value;
            oldValue = Input.Value;
            int newValue = touch.Undo ? Input.OldValue : touch.X;
            if (newValue < 0)
                newValue = 0;
            if (newValue >= Width)
                newValue = Width - 1;
            if (Input.Value != newValue)
            {
                Input.Value = newValue;
                ApplyTiles().Draw(newValue < oldValue ? newValue : oldValue, 0,
                    newValue > oldValue ? newValue + 1 - oldValue : oldValue + 1 - newValue);
            }
            if (touch.State == TouchState.End && newValue != Input.OldValue && (!SliderStyle.TriggerOnDrag || oldValue != newValue)
                    || SliderStyle.TriggerOnDrag && oldValue != newValue)
                Input.Callback?.Invoke(this, newValue);
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
                tile.wallColor((x > Input.Value) ? Style.WallColor.Value : (x == Input.Value) ? SliderStyle.SeparatorColor : SliderStyle.UsedColor);
        }

        #endregion
        #region GetValue

        public int GetValue() => Input.Value;

        #endregion
        #region SetValue

        public void SetValue(int value)
        {
            if (value < 0)
                value = 0;
            if (value >= Width)
                value = Width - 1;
            if (value != Input.Value)
            {
                Input.Value = value;
                Input.Callback?.Invoke(this, Input.Value);
            }
        }

        #endregion
        #region PulseThisNative

        protected override void PulseThisNative(PulseType type)
        {
            base.PulseThisNative(type);
            if (type == PulseType.Reset)
                SetValue(Input.DefaultValue);
        }

        #endregion
    }
}
