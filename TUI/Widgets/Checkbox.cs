using TerrariaUI.Base;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Widgets
{
    #region CheckboxStyle

    /// <summary>
    /// Drawing styles for Checkbox widget.
    /// </summary>
    public class CheckboxStyle : UIStyle
    {
        /// <summary>
        /// Color of pressed checkbox.
        /// </summary>
        public byte CheckedColor { get; set; } = 13;

        public CheckboxStyle() : base() { }

        public CheckboxStyle(CheckboxStyle style)
            : base(style)
        {
            CheckedColor = style.CheckedColor;
        }
    }

    #endregion

    /// <summary>
    /// Input widget for boolean values.
    /// </summary>
    public class Checkbox : VisualObject, IInput, IInput<bool>
    {
        #region Data

        public Input<bool> Input { get; protected set; }
        public object Value => Input.Value;
        public CheckboxStyle CheckboxStyle => Style as CheckboxStyle;

        private byte? OldWallColor;

        #endregion

        #region Constructor

        /// <summary>
        /// Input widget for boolean values.
        /// </summary>
        public Checkbox(int x, int y, int size, CheckboxStyle style = null, Input<bool> input = null)
            : base(x, y, size, size, new UIConfiguration(), style ?? new CheckboxStyle())
        {
            Input = input ?? new Input<bool>(false, false, null);
            Configuration.Lock = new Lock(LockLevel.Self, false, UIDefault.LockDelay, false, false);
            OldWallColor = Style.WallColor;
        }

        #endregion
        #region Invoke

        protected override void Invoke(Touch touch) =>
            SetValue(!Input.Value, true, touch.Session.PlayerIndex);

        #endregion
        #region ApplyTile

        protected override void ApplyTile(int x, int y, dynamic tile)
        {
            if (Style.Active.HasValue)
                tile.active(Style.Active.Value);
            else if (Style.Tile.HasValue)
                tile.active(true);
            else if (Style.Wall.HasValue)
                tile.active(false);
            if (Style.InActive.HasValue)
                tile.inActive(Style.InActive.Value);
            if (Style.Tile.HasValue)
                tile.type = Style.Tile.Value;
            if (Style.TileColor.HasValue)
                tile.color(Style.TileColor.Value);
            if (Style.Wall.HasValue)
                tile.wall = Style.Wall.Value;
            if (Style.WallColor.HasValue)
                tile.wallColor((byte)(Input.Temp ? CheckboxStyle.CheckedColor : Style.WallColor));
        }

        #endregion
        #region GetValue

        public bool GetValue() => Input.Value;

        #endregion
        #region SetTempValue

        public void SetTempValue(bool temp, bool draw)
        {
            if (Input.Temp != temp)
            {
                Input.Temp = temp;
                if (draw)
                    Apply().Draw();
            }
        }

        #endregion
        #region SetValue

        public void SetValue(bool value, bool draw = false, int player = -1)
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
                SetValue(Input.DefaultValue, false);
        }

        #endregion
    }
}
