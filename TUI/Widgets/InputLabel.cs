using System;
using System.Collections.Generic;
using System.Linq;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    #region InputLabelStyle

    public enum InputLabelType
    {
        Digits = 0,
        Characters,
        All
    }

    public class InputLabelStyle : LabelStyle
    {
        public string Default { get; set; } = "000";
        public InputLabelType Type { get; set; } = InputLabelType.All;

        public InputLabelStyle() : base() { }

        public InputLabelStyle(LabelStyle style)
            : base(style)
        {
        }
    }

    #endregion

    public class InputLabel : Label
    {
        #region Data

        public static List<char> Digits = new List<char> { '0','1','2','3','4','5','6','7','8','9' };
        public static List<char> Characters = new List<char> { 'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',' ' };
        public static List<char> All = Digits.Concat(Characters).ToList();

        public Action<InputLabel, string> InputLabelCallback;
        protected string BeginValue;

        public InputLabelStyle InputLabelStyle => Style as InputLabelStyle;

        #endregion

        #region Constructor

        public InputLabel(int x, int y, InputLabelStyle style = null, Action<InputLabel, string> callback = null)
            : base(x, y, style.Default.Length * 2, style?.TextUnderline == LabelUnderline.Underline ? 3 : 2, style.Default,
                  new UIConfiguration() { UseMoving=true, UseEnd=true, UseOutsideTouches=true }, style ?? new InputLabelStyle())
        {
            InputLabelStyle ilstyle = InputLabelStyle;
            ilstyle.TextOffset = new Offset() { Left = 0, Up = 0, Right = 0, Down = 0, Horizontal = 2, Vertical = 0 };
            ilstyle.TextAlignment = Alignment.UpLeft;
            InputLabelCallback = callback;
        }

        #endregion
        #region Copy

        public InputLabel(InputLabel ilabel)
            : this(ilabel.X, ilabel.Y, ilabel.InputLabelStyle)
        {
        }

        #endregion
        #region Invoke

        public override void Invoke(Touch touch)
        {
            if (touch.State == TouchState.Begin)
                BeginValue = Get();
            if (touch.State == TouchState.End || touch.State == TouchState.Moving)
            {
                List<char> charShift = InputLabelStyle.Type == InputLabelType.Digits
                    ? Digits
                    : (InputLabelStyle.Type == InputLabelType.Characters ? Characters : All);
                int delta = touch.Session.PreviousTouch.AbsoluteY - touch.AbsoluteY;
                if (delta % charShift.Count != 0)
                {
                    int charPosition = touch.Session.BeginTouch.X / 2;
                    int charIndex = charShift.IndexOf(RawText[charPosition]);
                    char newChar = charShift[((charIndex + delta) % charShift.Count + charShift.Count) % charShift.Count];
                    string newText = $"{RawText.Substring(0, charPosition)}{newChar}{RawText.Substring(charPosition + 1, (RawText.Length - charPosition - 1))}";
                    Set(newText);
                    UpdateThis().ApplyThis().Draw();
                }
                string value = Get();
                if (touch.State == TouchState.End && value != BeginValue)
                    InputLabelCallback?.Invoke(this, value);
            }
        }

        #endregion
        #region PulseThisNative

        protected override void PulseThisNative(PulseType type)
        {
            base.PulseThisNative(type);
            if (type == PulseType.Reset)
            {
                string defualt = InputLabelStyle.Default;
                if (Get() != defualt)
                {
                    Set(defualt);
                    InputLabelCallback?.Invoke(this, defualt);
                }
            }
        }

        #endregion
    }
}
