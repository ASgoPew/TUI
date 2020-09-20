using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;
using TerrariaUI.Widgets;

namespace TerrariaUI.Widgets
{
    #region ColorPickerStyle

    public class ColorPickerStyle : UIStyle
    {
        public bool CurrentAsPipet = false;

        public ColorPickerStyle() : base() { }

        public ColorPickerStyle(ColorPickerStyle style) : base(style)
        {
            CurrentAsPipet = style.CurrentAsPipet;
        }
    }

    #endregion

    public class ColorPicker : VisualObject, IInput, IInput<int>
	{
        #region Data

        // Hardcoded {x, y, paint, size} of color buttons
        private static int[][] Colors =
		{
			new int[] {2,2,26,3},
            new int[] {5,2,27,3},
            new int[] {5,5,25,3},
            new int[] {2,5,30,3},
            new int[] {1,1,23,2},
            new int[] {3,0,24,2},
            new int[] {5,0,13,2},
            new int[] {7,1,14,2},
            new int[] {8,3,15,2},
            new int[] {8,5,16,2},
            new int[] {7,7,17,2},
            new int[] {5,8,18,2},
            new int[] {3,8,19,2},
            new int[] {1,7,20,2},
            new int[] {0,5,21,2},
            new int[] {0,3,22,2}
		};
		private static int[] SelectedColor = {4,4,30,2};

		public Input<int> Input { get; protected set; }
		public object Value => Input.Value;
        public ColorPickerStyle ColorPickerStyle => Style as ColorPickerStyle;
        public VisualObject Current { get; protected set; }

        #endregion

        #region Constructor

        public ColorPicker(int X, int Y, ColorPickerStyle style = null, Input<int> input = null)
            : base(X, Y, 10, 10, new UIConfiguration() { UseBegin = false }, style ?? new ColorPickerStyle())
        {
            Input = input ?? new Input<int>(30, 30, null);

            foreach (var color in Colors)
            {
                Add(new VisualObject(color[0], color[1], color[3], color[3], null, new UIStyle()
                {
                    Wall = 155,
                    WallColor = (byte)color[2],
                }, (self, touch) =>
                {
                    SetValue((int)self["value"], true, touch.PlayerIndex);
                }))["value"] = color[2];
            }

            Current = Add(new VisualObject(SelectedColor[0], SelectedColor[1],
                SelectedColor[3], SelectedColor[3], null, new UIStyle()
                {
                    Wall = 155,
                    WallColor = (byte)Input.DefaultValue
                }));

            if (ColorPickerStyle.CurrentAsPipet)
            {
                // TODO
            }
        }

        #endregion

        #region GetValue

        public int GetValue() => Input.Value;

        #endregion
        #region SetTempValue

        public void SetTempValue(int temp, bool draw)
        {
            if (temp < 0)
                temp = 0;
            else if (temp > 30)
                temp = 30;

            if (Input.Temp != temp)
            {
                Input.Temp = temp;
                Current.Style.WallColor = (byte)Input.Temp;
                if (draw)
                    Apply().Draw();
            }
        }

        #endregion
        #region SetValue

        public void SetValue(int value, bool draw = false, int player = -1)
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
