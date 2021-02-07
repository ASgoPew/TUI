using System;
using System.Collections.Generic;
using System.Linq;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Widgets
{
    public class DropdownButton : Button, IInput, IInput<string>
    {
        #region Data

        public Input<string> Input { get; protected set; }
        public object Value => Input.Value;
        public List<string> Values { get; protected set; }
        public Dropdown Dropdown { get; protected set; }

        #endregion

        #region Constructor

        public DropdownButton(int x, int y, int width, int height, IEnumerable<string> values, UIConfiguration configuration = null,
                ButtonStyle style = null, Input<string> input = null)
            : base(x, y, width, height, input?.DefaultValue ?? values.First(), configuration, style ?? new ButtonStyle())
        {
            Values = values.ToList();
            Input = input ?? new Input<string>(Values[0], Values[0]);

            Configuration.UseBegin = true;
            Configuration.UseMoving = false;
            Configuration.UseEnd = false;
            Configuration.SessionAcquire = false;

            SetText(Input.DefaultValue);
        }

        #endregion
        #region Invoke

        public override void Invoke(Touch touch)
        {
            if (Root is Panel panel)
            {
                if (Dropdown == null)
                    Dropdown = new Dropdown(this);
                panel.ShowPopUp(Dropdown);
                touch.Session.Acquired = Dropdown;
                Input.Temp = null;
                SetTempValue(Values[0], true);
            }
            else
                throw new InvalidOperationException("Dropdown is only supported in Panel widgets");
        }

        #endregion
        #region UpdateThisNative

        protected override void UpdateThisNative()
        {
            base.UpdateThisNative();
            (int x, int y) = RelativeXY(0, 0, Root);
            Dropdown?.SetXYWH(x, y, Width, Height * Values.Count, false).Update();
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
                        Button oldBtn = (Button)Dropdown.GetChild(oldIndex);
                        oldBtn.EndBlink(oldBtn.ButtonStyle.BlinkStyle);
                    }
                    int index = Values.IndexOf(temp);
                    if (index >= 0)
                    {
                        Button btn = (Button)Dropdown.GetChild(index);
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
            SetText(value);
            Update();
            if (draw && Root is Panel panel)
            {
                int index = Values.IndexOf(value);
                if (index >= 0)
                {
                    Button btn = (Button)Dropdown.GetChild(index);
                    bool enabled = btn.Enabled;
                    if (enabled)
                        btn.Disable(false);
                    btn.EndBlink(btn.ButtonStyle.BlinkStyle);
                    if (enabled)
                        btn.Enable(false);
                }
                panel.HidePopUp();
            }
            Input.SubmitTemp(this, player);
        }

        #endregion
    }

    public class Dropdown : VisualContainer
    {
        #region Data

        public DropdownButton Origin { get; protected set; }

        #endregion

        #region Constructor

        public Dropdown(DropdownButton origin)
            : base(0, 0, 0, 0, new UIConfiguration()
            {
                UseMoving = true,
                UseEnd = true,
                BeginRequire = false,
                UseOutsideTouches = true
            })
        {
            Origin = origin;

            SetupLayout(Alignment.Up, Direction.Down, Side.Center, null, 0);
            foreach (var value in Origin.Values)
            {
                var btn = AddToLayout(new Button(Origin));
                btn.Configuration.UseBegin = false;
                btn.SetText(value);
            }
        }

        #endregion
        #region Invoke

        public override void Invoke(Touch touch)
        {
            int index = touch.Y / Origin.Height;
            if (index < 0)
                index = 0;
            else if (index >= Origin.Values.Count)
                index = Origin.Values.Count - 1;
            string value = Origin.Values[index];

            if (touch.State == TouchState.Begin || touch.State == TouchState.Moving)
                Origin.SetTempValue(value, true);
            else
                Origin.SetValue(value, true, touch.PlayerIndex);
        }

        #endregion
    }
}
