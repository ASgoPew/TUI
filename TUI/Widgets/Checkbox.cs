using System;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    #region CheckboxStyle

    public class CheckboxStyle : UIStyle
    {
        public byte CheckedColor { get; set; } = 13;

        public CheckboxStyle() : base() { }

        public CheckboxStyle(CheckboxStyle style)
            : base(style)
        {
            CheckedColor = style.CheckedColor;
        }
    }

    #endregion

    public class Checkbox : VisualObject
    {
        #region Data

        private Action<Checkbox, bool> CheckboxCallback;
        private byte? OldWallColor;

        public bool DefaultValue { get; set; }

        public CheckboxStyle CheckboxStyle => Style as CheckboxStyle;

        public bool Value { get; private set; }

        #endregion

        #region Initialize

        public Checkbox(int x, int y, int size, CheckboxStyle style = null, Action<Checkbox, bool> callback = null, bool defaultValue = false, int lockDelay = 300)
            : base(x, y, size, size, new UIConfiguration(), style ?? new CheckboxStyle())
        {
            Configuration.Lock = new Lock(LockLevel.Self, false, lockDelay, false, false);

            CheckboxCallback = callback;
            OldWallColor = Style.WallColor;
            DefaultValue = defaultValue;
            Value = defaultValue;
        }

        #endregion
        #region Invoke

        public override bool Invoke(Touch touch)
        {
            Set(!Value);
            Apply().Draw();
            return true;
        }

        #endregion
        #region Set

        public Checkbox Set(bool value)
        {
            if (value != Value)
            {
                if (value)
                {
                    OldWallColor = Style.WallColor;
                    Style.WallColor = CheckboxStyle.CheckedColor;
                }
                else
                    Style.WallColor = OldWallColor;
                Value = value;
                CheckboxCallback?.Invoke(this, Value);
            }
            return this;
        }

        #endregion
        #region PulseThisNative

        protected override void PulseThisNative(PulseType type)
        {
            base.PulseThisNative(type);
            if (type == PulseType.Reset)
                Set(DefaultValue);
        }

        #endregion
    }
}
