using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    #region ScrollBarStyle

    public class ScrollBarStyle : UIStyle
    {
        public byte SliderColor { get; set; } = UIDefault.SliderSeparatorColor;

        public ScrollBarStyle() : base() { }

        public ScrollBarStyle(ScrollBarStyle style) : base(style)
        {
            SliderColor = style.SliderColor;
        }
    }

    #endregion


    class ScrollBar : VisualObject
    {
        public ScrollBackground Slider { get; internal set; }

        public ScrollBarStyle ScrollBarStyle => Style as ScrollBarStyle;

        public ScrollBar(Direction side = Direction.Right, int width = 1, ScrollBarStyle style = null)
            : base(0, 0, 0, 0, new UIConfiguration() { UseMoving=true, UseEnd=true, UseOutsideTouches=true }, style)
        {
            if (side == Direction.Left || side == Direction.Right)
            {
                Width = width;
                SetFullSize(FullSize.Vertical);
                if (side == Direction.Left)
                    SetAlignmentInParent(new AlignmentStyle(Alignment.Left));
                else
                    SetAlignmentInParent(new AlignmentStyle(Alignment.Right));
                SetupLayout(new LayoutStyle(Alignment.Center, Direction.Down));
            }
            else
            {
                Height = width;
                SetFullSize(FullSize.Horizontal);
                if (side == Direction.Up)
                    SetAlignmentInParent(new AlignmentStyle(Alignment.Up));
                else
                    SetAlignmentInParent(new AlignmentStyle(Alignment.Down));
                SetupLayout(new LayoutStyle(Alignment.Center, Direction.Right));
            }
            Slider = AddToLayout(new ScrollBackground(false, true)) as ScrollBackground;
            Slider.SetFullSize(FullSize.None);
            Slider.SetXYWH(0, 0, width, width);
            Slider.Style.WallColor = ScrollBarStyle.SliderColor;
        }

        protected override void UpdateThisNative()
        {
            base.UpdateThisNative();
            Slider.Style.WallColor = ScrollBarStyle.SliderColor;
        }
    }
}
