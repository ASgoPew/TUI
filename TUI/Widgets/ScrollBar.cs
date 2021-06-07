using System;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Widgets
{
    #region ScrollBarStyle

    /// <summary>
    /// Drawing styles for ScrollBar widget.
    /// </summary>
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

    /// <summary>
    /// Widget for scrolling parent's layout with a sidebar.
    /// </summary>
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

        /// <summary>
        /// Widget for scrolling parent's layout with a sidebar.
        /// </summary>
        /// <param name="side">Side where to adjoin sidebar</param>
        public ScrollBar(Direction side = Direction.Right, int width = 1, ScrollBarStyle style = null)
            : base(0, 0, 0, 0, new UIConfiguration(), style ?? new ScrollBarStyle())
        {
            Layer = Int32.MaxValue - 1;
            Vertical = side == Direction.Left || side == Direction.Right;
            _Width = width;
            if (Vertical)
            {
                Width = width;
                SetParentStretch(FullSize.Vertical);
                if (side == Direction.Left)
                    SetParentAlignment(Alignment.Left);
                else
                    SetParentAlignment(Alignment.Right);
                SetupLayout(Alignment.Up, Direction.Up, Side.Center, null, 0);
            }
            else
            {
                Height = width;
                SetParentStretch(FullSize.Horizontal);
                if (side == Direction.Up)
                    SetParentAlignment(Alignment.Up);
                else
                    SetParentAlignment(Alignment.Down);
                SetupLayout(Alignment.Left, Direction.Left, Side.Center, null, 0);
            }
            Empty1 = AddToLayout(new Separator(0));
            Empty1.Configuration.UseBegin = false;
            // Adding ScrollBackground with 0 layer.
            Slider = AddToLayout(new ScrollBackground(false, true, true, ScrollAction), 0);
            Empty2 = AddToLayout(new Separator(0));
            Empty2.Configuration.UseBegin = false;
            Slider.SetParentStretch(FullSize.None);
            Slider.Style.WallColor = ScrollBarStyle.SliderColor;
            if (Style.WallColor == null)
                Style.WallColor = 0;
        }

        #endregion
        #region ScrollAction

        public static void ScrollAction(ScrollBackground @this, int value)
        {
            //int newIndent = (int)Math.Round((value / (float)@this.Limit) * @this.Parent.Configuration.Layout.IndentLimit);
            //Console.WriteLine(newIndent);
            @this.Parent.Parent.LayoutOffset(value)
                .Update()
                .Apply()
                .Draw();
        }

        #endregion
        #region UpdateThisNative

        protected override void UpdateThisNative()
        {
            base.UpdateThisNative();

            Configuration.Layout.LayoutOffset = Parent.Configuration.Layout.LayoutOffset;
            int limit = Parent.Configuration.Layout.OffsetLimit;
            Slider.Style.WallColor = ScrollBarStyle.SliderColor;
            if (Vertical)
            {
                int size = Math.Max(Height - limit, 1);
                if (size >= Height)
                {
                    Slider.Disable(false);
                    Configuration.UseBegin = false;
                    return;
                }
                else
                {
                    Slider.Enable(false);
                    Configuration.UseBegin = true;
                }
                Slider.SetWH(_Width, size, false);
                Empty1.SetWH(_Width, Height - Slider.Height, false);
                Empty2.SetWH(_Width, limit, false);
            }
            else
            {
                int size = Math.Max(Width - limit, 1);
                if (size >= Width)
                {
                    Slider.Disable(false);
                    Configuration.UseBegin = false;
                    return;
                }
                else
                {
                    Slider.Enable(false);
                    Configuration.UseBegin = true;
                }
                Slider.SetWH(size, _Width, false);
                Empty1.SetWH(Width - Slider.Width, _Width, false);
                Empty2.SetWH(limit, _Width, false);
            }
            DrawWithSection = Parent.DrawWithSection;
            switch (Parent.Configuration.Layout.Direction)
            {
                case Direction.Left:
                    Configuration.Layout.Alignment = Alignment.Right;
                    Configuration.Layout.Direction = Direction.Right;
                    break;
                case Direction.Up:
                    Configuration.Layout.Alignment = Alignment.Down;
                    Configuration.Layout.Direction = Direction.Down;
                    break;
                case Direction.Right:
                    Configuration.Layout.Alignment = Alignment.Left;
                    Configuration.Layout.Direction = Direction.Left;
                    break;
                case Direction.Down:
                    Configuration.Layout.Alignment = Alignment.Up;
                    Configuration.Layout.Direction = Direction.Up;
                    break;
            }
        }

        #endregion
        #region Invoke

        public override void Invoke(Touch touch)
        {
            int forward = Parent.Configuration.Layout.Direction == Direction.Right || Parent.Configuration.Layout.Direction == Direction.Down ? 1 : -1;
            if (Vertical)
            {
                if (touch.Y > Slider.Y)
                    ScrollAction(Slider, Configuration.Layout.LayoutOffset + (touch.Y - (Slider.Y + Slider.Height) + 1) * forward);
                else
                    ScrollAction(Slider, Configuration.Layout.LayoutOffset - (Slider.Y - touch.Y) * forward);
            }
            else
            {
                if (touch.X > Slider.X)
                    ScrollAction(Slider, Configuration.Layout.LayoutOffset + (touch.X - (Slider.X + Slider.Width) + 1) * forward);
                else
                    ScrollAction(Slider, Configuration.Layout.LayoutOffset - (Slider.X - touch.X) * forward);
            }
        }

        #endregion
    }
}
