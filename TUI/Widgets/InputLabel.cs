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

        public InputLabelStyle InputLabelStyle => Style as InputLabelStyle;

        #endregion

        #region Initialize

        public InputLabel(int x, int y, string text, InputLabelStyle style = null)
            : base(x, y, text.Length * 2, style?.TextUnderline == LabelUnderline.Underline ? 3 : 2, text,
                  new UIConfiguration() { UseEnd=true, UseOutsideTouches=true }, style ?? new InputLabelStyle())
        {
            InputLabelStyle ilstyle = InputLabelStyle;
            ilstyle.TextOffset = new Offset() { Left = 0, Up = 0, Right = 0, Down = 0, Horizontal = 2, Vertical = 0 };
            ilstyle.TextAlignment = Alignment.UpLeft;
        }

        #endregion
        #region Copy

        public InputLabel(InputLabel ilabel)
            : this(ilabel.X, ilabel.Y, ilabel.RawText, ilabel.InputLabelStyle)
        {
        }

        #endregion
        #region Invoke

        public override bool Invoke(Touch touch)
        {
            if (touch.State == TouchState.End)
            {
                List<char> charShift = InputLabelStyle.Type == InputLabelType.Digits ? Digits : (InputLabelStyle.Type == InputLabelType.Characters ? Characters : All);
                int delta = touch.Session.BeginTouch.AbsoluteY - touch.AbsoluteY;
                if (delta % charShift.Count != 0)
                {
                    int charPosition = touch.Session.BeginTouch.X / 2;
                    int charIndex = charShift.IndexOf(RawText[charPosition]);
                    char newChar = charShift[((charIndex + delta) % charShift.Count + charShift.Count) % charShift.Count];
                    string newText = $"{RawText.Substring(0, charPosition)}{newChar}{RawText.Substring(charPosition + 1, (RawText.Length - charPosition - 1))}";
                    Console.WriteLine("New text: " + newText);
                    SetText(newText);
                    ApplyThis().Draw();
                }
            }
            return true;
        }

        #endregion
    }
}
