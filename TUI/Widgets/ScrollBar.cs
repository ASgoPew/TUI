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
        public Separator Empty1 { get; private set; }
        public Separator Empty2 { get; private set; }

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
                SetupLayout(new LayoutStyle(Alignment.Up, Direction.Up, Side.Center, null, 0));
            }
            else
            {
                Height = width;
                SetFullSize(FullSize.Horizontal);
                if (side == Direction.Up)
                    SetAlignmentInParent(new AlignmentStyle(Alignment.Up));
                else
                    SetAlignmentInParent(new AlignmentStyle(Alignment.Down));
                SetupLayout(new LayoutStyle(Alignment.Left, Direction.Left, Side.Center, null, 0));
            }
            Empty1 = AddToLayout(new Separator(0)) as Separator;
            Slider = AddToLayout(new ScrollBackground(false, true, true, ScrollAction)) as ScrollBackground;
            Empty2 = AddToLayout(new Separator(0)) as Separator;
            Slider.SetFullSize(FullSize.None);
            Slider.Style.WallColor = ScrollBarStyle.SliderColor;
            if (Style.WallColor == null)
                Style.WallColor = 0;
        }

        #endregion
        #region ScrollAction

        public static void ScrollAction(ScrollBackground @this, int value)
        {
            //Console.WriteLine(value);
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
            base.UpdateThisNative();

            Style.Layout.LayoutIndent = Parent.Style.Layout.LayoutIndent;
            int limit = Parent.Style.Layout.IndentLimit;
            Slider.Style.WallColor = ScrollBarStyle.SliderColor;
            if (Vertical)
            {
                Slider.SetWH(_Width, Math.Min(Math.Max(Height - limit, 1), Height));
                Empty1.SetWH(_Width, Height - Slider.Height);
                Empty2.SetWH(_Width, limit);
            }
            else
            {
                Slider.SetWH(Math.Min(Math.Max(Width - limit, 1), Width), _Width);
                Empty1.SetWH(Width - Slider.Width, _Width);
                Empty2.SetWH(limit, _Width);
            }
            ForceSection = Parent.ForceSection;
            switch (Parent.Style.Layout.Direction)
            {
                case Direction.Left:
                    Style.Layout.Alignment = Alignment.Right;
                    Style.Layout.Direction = Direction.Right;
                    break;
                case Direction.Up:
                    Style.Layout.Alignment = Alignment.Down;
                    Style.Layout.Direction = Direction.Down;
                    break;
                case Direction.Right:
                    Style.Layout.Alignment = Alignment.Left;
                    Style.Layout.Direction = Direction.Left;
                    break;
                case Direction.Down:
                    Style.Layout.Alignment = Alignment.Up;
                    Style.Layout.Direction = Direction.Up;
                    break;
            }
        }

        #endregion
        #region Invoke

        public override void Invoke(Touch touch)
        {
            int forward = Parent.Style.Layout.Direction == Direction.Right || Parent.Style.Layout.Direction == Direction.Down ? 1 : -1;
            if (Vertical)
            {
                if (touch.Y > Slider.Y)
                    ScrollAction(Slider, Style.Layout.LayoutIndent + (touch.Y - (Slider.Y + Slider.Height) + 1) * forward);
                else
                    ScrollAction(Slider, Style.Layout.LayoutIndent - (Slider.Y - touch.Y) * forward);
            }
            else
            {
                if (touch.X > Slider.X)
                    ScrollAction(Slider, Style.Layout.LayoutIndent + (touch.X - (Slider.X + Slider.Width) + 1) * forward);
                else
                    ScrollAction(Slider, Style.Layout.LayoutIndent - (Slider.X - touch.X) * forward);
            }
        }

        #endregion
        #region PulseThisNative

        protected override void PulseThisNative(PulseType type)
        {
            base.PulseThisNative(type);
            if (type == PulseType.Reset)
                Parent.LayoutIndent(0);
        }

        #endregion
    }
}
