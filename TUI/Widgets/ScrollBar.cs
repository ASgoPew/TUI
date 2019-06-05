using System;
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


    public class ScrollBar : VisualObject
    {
        #region Data

        protected int _Width { get; set; }
        protected bool Vertical { get; set; }
        public ScrollBackground Slider { get; internal set; }

        public ScrollBarStyle ScrollBarStyle => Style as ScrollBarStyle;

        #endregion

        #region Constructor

        public ScrollBar(Direction side = Direction.Right, int width = 1, ScrollBarStyle style = null)
            : base(0, 0, 0, 0, new UIConfiguration(), style ?? new ScrollBarStyle())
        {
            Vertical = side == Direction.Left || side == Direction.Right;
            _Width = width;
            if (Vertical)
            {
                Width = width;
                SetFullSize(FullSize.Vertical);
                if (side == Direction.Left)
                    SetAlignmentInParent(new AlignmentStyle(Alignment.Left));
                else
                    SetAlignmentInParent(new AlignmentStyle(Alignment.Right));
                SetupLayout(new LayoutStyle(Alignment.Up, Direction.Up));
            }
            else
            {
                Height = width;
                SetFullSize(FullSize.Horizontal);
                if (side == Direction.Up)
                    SetAlignmentInParent(new AlignmentStyle(Alignment.Up));
                else
                    SetAlignmentInParent(new AlignmentStyle(Alignment.Down));
                SetupLayout(new LayoutStyle(Alignment.Left, Direction.Left));
            }
            Slider = AddToLayout(new ScrollBackground(false, true, true, ScrollAction)) as ScrollBackground;
            Slider.SetFullSize(FullSize.None);
            Slider.Style.WallColor = ScrollBarStyle.SliderColor;
            if (Style.WallColor == null)
                Style.WallColor = 0;
        }

        #endregion
        #region ScrollAction

        public static void ScrollAction(ScrollBackground @this, int value)
        {
            //int newIndent = (int)Math.Round((value / (float)@this.Limit) * @this.Parent.Style.Layout.IndentLimit);
            //Console.WriteLine(newIndent);
            @this.Parent.Parent
                .LayoutIndent(value)
                .Update()
                .Apply()
                .Draw();
        }

        #endregion
        #region UpdateThisNative

        protected override void UpdateThisNative()
        {
            Style.Layout.LayoutIndent = Parent.Style.Layout.LayoutIndent;
            int limit = Parent.Style.Layout.IndentLimit;
            Slider.Style.WallColor = ScrollBarStyle.SliderColor;
            if (Vertical)
                Slider.SetWH(_Width, Math.Min(Math.Max(Height - limit, 1), Height));
            else
                Slider.SetWH(Math.Min(Math.Max(Width - limit, 1), Width), _Width);
            ForceSection = Parent.ForceSection;

            base.UpdateThisNative();

            Style.Layout.IndentLimit = limit;
        }

        #endregion
        #region Invoke

        public override bool Invoke(Touch touch)
        {
            if (Vertical)
            {
                if (touch.Y > Slider.Y)
                    ScrollAction(Slider, Style.Layout.LayoutIndent + touch.Y - (Slider.Y + Slider.Height) + 1);
                else
                    ScrollAction(Slider, Style.Layout.LayoutIndent - (Slider.Y - touch.Y));
            }
            else
            {
                if (touch.X > Slider.X)
                    ScrollAction(Slider, Style.Layout.LayoutIndent + touch.X - (Slider.X + Slider.Width) + 1);
                else
                    ScrollAction(Slider, Style.Layout.LayoutIndent - (Slider.X - touch.X));
            }
            return true;
        }

        #endregion
    }
}
