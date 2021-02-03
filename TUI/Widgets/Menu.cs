using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Widgets
{
    public class Menu : VisualContainer, IInput, IInput<string>
    {
        #region Data

        public Input<string> Input { get; protected set; }
        public object Value => Input.Value;
        public List<string> Values { get; protected set; }
        public string Title { get; protected set; }

        #endregion

        #region Constructor

        public Menu(int x, int y, IEnumerable<string> values, ButtonStyle style1, ButtonStyle style2 = null,
            string title = null, LabelStyle labelStyle = null, Input<string> input = null)
            : base(x, y, 0, 0, new UIConfiguration()
            {
                UseMoving = true,
                UseEnd = true,
                UseOutsideTouches = true
            })
        {
            Values = values.ToList();
            Input = input ?? new Input<string>(Values[0], Values[0]);
            Title = title;

            SetupLayout(Alignment.Center, Direction.Down, Side.Center, null, 0);
            if (Title != null)
                AddToLayout(new Label(0, 0, 0, 4, Title, labelStyle)).SetFullSize(true, false);
            int i = 0;
            foreach (var value in values)
                AddToLayout(new Button(0, 0, 0, 4, value, style: i++ % 2 == 0 ? style1 : style2 ?? style1))
                    .SetFullSize(true, false)
                    .Configuration.UseBegin = false;

            int width = Label.FindMaxWidth(values, style1.TextIndent.Horizontal) + 6;
            if (Title != null)
                width = Math.Max(width, Label.FindMaxWidth(new string[] { Title }, labelStyle.TextIndent.Horizontal) + 2);
            int height = 4 * values.Count();
            if (Title != null)
                height += 4;
            SetWH(width, height, false);
        }

        #endregion
        #region Invoke

        public override void Invoke(Touch touch)
        {
            Touch beginTouch = touch.Session.BeginTouch;
            if (beginTouch.Object == this && Contains(beginTouch) && beginTouch.Y >= 4 && Contains(touch) && touch.Y >= 4)
            {
                int index = touch.Y / 4 - 1;
                if (index < 0)
                    index = 0;
                else if (index >= Values.Count)
                    index = Values.Count - 1;
                string value = Values[index];

                if (touch.State == TouchState.Begin || touch.State == TouchState.Moving)
                    SetTempValue(value, true);
                else
                    SetValue(value, true, touch.PlayerIndex);
            }
            else if (Input.Temp != null)
                SetTempValue(null, true);
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
                string oldTemp = Input.Temp;
                Input.Temp = temp;
                if (draw)
                {
                    int oldIndex = Values.IndexOf(oldTemp);
                    if (oldIndex >= 0)
                    {
                        if (Title != null)
                            oldIndex++;
                        Button oldBtn = (Button)GetChild(oldIndex);
                        oldBtn.EndBlink(oldBtn.ButtonStyle.BlinkStyle);
                    }
                    int index = Values.IndexOf(temp);
                    if (index >= 0)
                    {
                        if (Title != null)
                            index++;
                        Button btn = (Button)GetChild(index);
                        btn.StartBlink(btn.ButtonStyle.BlinkStyle);
                    }
                }
            }
        }

        #endregion
        #region SetValue

        public void SetValue(string value, bool draw = false, int player = -1)
        {
            SetTempValue(value, false);
            if (draw)
            {
                int index = Values.IndexOf(value);
                if (index >= 0)
                {
                    if (Title != null)
                        index++;
                    Button btn = (Button)GetChild(index);
                    btn.EndBlink(btn.ButtonStyle.BlinkStyle);
                }
            }
            Input.SubmitTemp(this, player);
            Input.Value = null;
        }

        #endregion
    }
}
