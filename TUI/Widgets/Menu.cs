using System;
using System.Collections.Generic;
using System.Linq;
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
        protected Action<Menu> TitleCallback { get; set; }

        public bool HasTitle => Title != null;
        public bool HasBlinks
        {
            get
            {
                foreach (var child in ChildrenFromTop)
                    if (child.Style is ButtonStyle style && style.BlinkStyle != ButtonBlinkStyle.None)
                        return true;
                return false;
            }
        }

        #endregion

        #region Constructor

        public Menu(int x, int y, int width, int height, IEnumerable<string> values, ButtonStyle style1 = null, ButtonStyle style2 = null,
            string title = null, LabelStyle titleStyle = null, Input<string> input = null, Action<Menu> titleCallback = null)
            : base(x, y, width, height, new UIConfiguration()
            {
                UseMoving = true,
                UseEnd = true,
                BeginRequire = true,
                UseOutsideTouches = true
            })
        {
            Values = values.ToList();
            Input = input ?? new Input<string>(Values[0], Values[0]);
            Title = title;
            TitleCallback = titleCallback;

            style1 = style1 ?? new ButtonStyle() { Wall = 153, WallColor = PaintID2.Gray, TextColor = PaintID2.Shadow,
                BlinkStyle = ButtonBlinkStyle.None };
            style2 = style2 ?? new ButtonStyle(style1) { Wall = style1.SimilarWall() };
            titleStyle = titleStyle ?? new LabelStyle(style1) { Wall = style1.SimilarWall(),
                WallColor = PaintID2.White, TextColor = PaintID2.Shadow };

            SetupLayout(Alignment.Center, Direction.Down, Side.Center, null, 0);
            if (HasTitle)
            {
                Label titleLabel = new Label(0, 0, 0, 4, Title, titleStyle);
                AddToLayout(titleLabel).SetFullSize(true, false);
                titleLabel.Update();
            }
            int i = 0;
            foreach (var value in values)
            {
                Button button = new Button(0, 0, 0, 4, value, style: i++ % 2 == 0 ? style1 : style2);
                button.Configuration.UseBegin = false;
                button.Configuration.UseMoving = false;
                button.Configuration.UseEnd = false;
                AddToLayout(button.SetFullSize(true, false));
                button.Update();
            }
        }

        #endregion
        #region Invoke

        public override void Invoke(Touch touch)
        {
            Touch beginTouch = touch.Session.BeginTouch;
            if (!ContainsRelative(beginTouch) || !ContainsRelative(touch))
                return;
            int titleHeight = HasTitle ? 4 : 0;
            if (TitleCallback != null && touch.State == TouchState.End &&
                    beginTouch.Y < titleHeight && touch.Y < titleHeight)
                TitleCallback.Invoke(this);
            else if (beginTouch.Y >= titleHeight && touch.Y >= titleHeight)
            {
                int index = touch.Y / 4;
                if (HasTitle)
                    index--;
                string value = Values[index];

                if (touch.State == TouchState.Begin || touch.State == TouchState.Moving)
                    SetTempValue(value, HasBlinks);
                else if (touch.Undo)
                {
                    int childIndex = Values.IndexOf(Input.Temp);
                    if (childIndex >= 0)
                    {
                        if (HasTitle)
                            childIndex++;
                        Button btn = (Button)_Child[childIndex];
                        btn.EndBlink(btn.ButtonStyle.BlinkStyle);
                    }
                }
                else
                    SetValue(value, HasBlinks, touch.PlayerIndex);
            }
            else if (Input.Temp != null)
                SetTempValue(null, HasBlinks);
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
                    Blink(oldTemp, true);
                    Blink(Input.Temp, false);
                }
            }
        }

        #endregion
        #region SetValue

        public void SetValue(string value, bool draw = false, int player = -1)
        {
            SetTempValue(value, false);
            if (draw)
                Blink(value, true);
            Input.SubmitTemp(this, player);
            Input.Temp = Input.Value = null;
        }

        #endregion
        #region Blink

        public void Blink(string temp, bool end)
        {

            int index = Values.IndexOf(temp);
            if (index >= 0)
            {
                if (Title != null)
                    index++;
                Button btn = (Button)_Child[index];
                if (end)
                    btn.EndBlink(btn.ButtonStyle.BlinkStyle);
                else
                    btn.StartBlink(btn.ButtonStyle.BlinkStyle);
            }
        }

        #endregion
    }
}
