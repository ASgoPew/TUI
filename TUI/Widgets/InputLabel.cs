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

    /// <summary>
    /// Drawing styles for InputLabel widget.
    /// </summary>
    public class InputLabelStyle : LabelStyle
    {
        /// <summary>
        /// Whether to invoke input callback on TouchState.Moving touches.
        /// </summary>
        public bool TriggerInRuntime { get; set; } = false;
        /// <summary>
        /// Determines which set of characters to use.
        /// </summary>
        public InputLabelType Type { get; set; } = InputLabelType.All;

        public InputLabelStyle() : base() { }

        public InputLabelStyle(InputLabelStyle style)
            : base(style)
        {
            Type = style.Type;
        }
    }

    #endregion

    /// <summary>
    /// Input widget for string/int values using *drag and slide the digit* method.
    /// </summary>
    public class InputLabel : Label, IInput, IInput<string>
    {
        #region Data

        public static List<char> Digits = new List<char> { '0','1','2','3','4','5','6','7','8','9' };
        public static List<char> Characters = new List<char> { 'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',' ' };
        public static List<char> All = Digits.Concat(Characters).ToList();

        public Input<string> Input { get; protected set; }
        public object Value => Input.Value;
        public InputLabelStyle InputLabelStyle => Style as InputLabelStyle;

        #endregion

        #region Constructor

        /// <summary>
        /// Input widget for string/int values using *drag and slide the digit* method.
        /// </summary>
        public InputLabel(int x, int y, InputLabelStyle style = null, Input<string> input = null)
            : base(0, 0, 0, 0, "", new UIConfiguration() { UseMoving=true, UseEnd=true, UseOutsideTouches=true },
                  style ?? new InputLabelStyle())
        {
            Input = input ?? new Input<string>("", "", null);
            SetXYWH(x, y, Input.DefaultValue.Length* 2, style?.TextUnderline == LabelUnderline.Underline? 3 : 2);
            SetText(Input.DefaultValue);
            InputLabelStyle ilstyle = InputLabelStyle;
            ilstyle.TextIndent = new Indent() { Left = 0, Up = 0, Right = 0, Down = 0, Horizontal = 2, Vertical = 2 };
            ilstyle.TextAlignment = Alignment.UpLeft;
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
                Input.Value = GetText();
            if (touch.State == TouchState.End || touch.State == TouchState.Moving)
            {
                List<char> charShift = InputLabelStyle.Type == InputLabelType.Digits
                    ? Digits
                    : (InputLabelStyle.Type == InputLabelType.Characters ? Characters : All);
                string newValue = Input.Temp;
                int delta = touch.Session.PreviousTouch.AbsoluteY - touch.AbsoluteY;
                if (delta % charShift.Count != 0)
                {
                    int charPosition = touch.Session.BeginTouch.X / 2;
                    int charIndex = charShift.IndexOf(RawText[charPosition]);
                    char newChar = charShift[((charIndex + delta) % charShift.Count + charShift.Count) % charShift.Count];
                    newValue = $"{RawText.Substring(0, charPosition)}{newChar}{RawText.Substring(charPosition + 1, (RawText.Length - charPosition - 1))}";
                }

                if (touch.State == TouchState.End || InputLabelStyle.TriggerInRuntime)
                    SetValue(newValue, true, touch.Session.PlayerIndex);
                else
                    SetTempValue(newValue, true);
            }
        }

        #endregion
        #region GetValue

        public string GetValue() => Input.Value;

        #endregion
        #region SetTempValue

        public void SetTempValue(string temp, bool draw)
        {
            if (Input.Temp != temp)
            {
                Input.Temp = temp;
                SetText(temp);
                if (draw)
                    UpdateThis().ApplyThis().Draw();
            }
        }

        #endregion
        #region SetValue

        public void SetValue(string value, bool draw = false, int player = -1)
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
