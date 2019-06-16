using System;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    #region CheckboxStyle

    public class CheckboxStyle : UIStyle
    {
        public bool Default { get; set; } = false;
        public byte CheckedColor { get; set; } = 13;

        public CheckboxStyle() : base() { }

        public CheckboxStyle(CheckboxStyle style)
            : base(style)
        {
            Default = style.Default;
            CheckedColor = style.CheckedColor;
        }
    }

    #endregion

    public class Checkbox : VisualObject
    {
        #region Data

        private Action<Checkbox, bool> CheckboxCallback;
        private byte? OldWallColor;

        public CheckboxStyle CheckboxStyle => Style as CheckboxStyle;

        public bool Value { get; private set; }

        #endregion

        #region Constructor

        public Checkbox(int x, int y, int size, CheckboxStyle style = null, Action<Checkbox, bool> callback = null)
            : base(x, y, size, size, new UIConfiguration(), style ?? new CheckboxStyle())
        {
            Configuration.Lock = new Lock(LockLevel.Self, false, UIDefault.LockDelay, false, false);

            CheckboxCallback = callback;
            OldWallColor = Style.WallColor;
            Value = CheckboxStyle.Default;
        }

        #endregion
        #region Invoke

        public override void Invoke(Touch touch)
        {
            Set(!Value);
            Apply().Draw();
        }

        #endregion
        #region Set

        /// <summary>
        /// Change value and call callback without drawing.
        /// </summary>
        /// <param name="value">New value.</param>
        /// <returns>this</returns>
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
                Set(CheckboxStyle.Default);
        }

        #endregion
    }
}
