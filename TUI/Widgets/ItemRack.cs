using System;
using TUI.Base;
using TUI.Base.Style;
using TUI.Hooks.Args;

namespace TUI.Widgets
{
    #region ItemRackStyle

    public class ItemRackStyle : UIStyle
    {
        public short Type { get; set; } = 0;
        public bool Left { get; set; } = true;

        public ItemRackStyle() : base() { }

        public ItemRackStyle(ItemRackStyle style)
            : base(style)
        {
            Type = style.Type;
            Left = style.Left;
        }
    }

    #endregion

    public class ItemRack : VisualObject
    {
        #region Data

        protected dynamic Sign { get; set; } = null;

        public ItemRackStyle ItemRackStyle => Style as ItemRackStyle;
        public string Text => Sign?.text;

        #endregion

        #region Initialize

        public ItemRack(int x, int y, ItemRackStyle style = null,
                Func<VisualObject, Touch, bool> callback = null)
            : base(x, y, 3, 3, new UIConfiguration(), style ?? new ItemRackStyle(), callback)
        {
            style.Active = true;
        }

        #endregion
        #region Copy

        public ItemRack(ItemRack rack)
            : this(rack.X, rack.Y, rack.ItemRackStyle, rack.Callback)
        {
        }

        #endregion
        #region ApplyThisNative

        protected override void ApplyThisNative()
        {
            base.ApplyThisNative();
            (int sx, int sy) = ProviderXY();
            short type = ItemRackStyle.Type;
            bool left = ItemRackStyle.Left;
            int fx = left ? 54 : 0;
            bool sign = !string.IsNullOrWhiteSpace(Text);
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                {
                    dynamic tile = Provider[sx + x, sy + y];
                    if (sign && y == 0)
                    {
                        tile.type = 55;
                        tile.frameX = (x == 0) ? 144 : (x == 1) ? 126 : 162;
                    }
                    else
                        tile.type = 334;
                    if (y == 1 && x < 2)
                    {
                        if (x == 0)
                            tile.frameX = (short)((left ? 20100 : 5100) + type);
                        else
                            tile.frameX = (short)((left ? 25000 : 10000));
                    }
                    else
                        tile.frameX = (short)(fx + x * 18);
                    tile.frameY = sign ? 0 : (short)(y * 18);
                }
        }

        #endregion
        #region SetText

        public void SetText(string text)
        {
            (int x, int y) = AbsoluteXY();
            SignTextArgs args = new SignTextArgs(text, x, y, Sign);
            UI.Hooks.SignText.Invoke(args);
            Sign = args.Sign;
        }

        #endregion
        protected override void UpdateThisNative()
        {
            base.UpdateThisNative();
        }
    }
}
