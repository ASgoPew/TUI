using System;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    #region ItemRackStyle

    public class ItemRackStyle : UIStyle
    {
        public short Type { get; set; } = 0;
        public bool Left { get; set; } = true;
        public byte Prefix { get; set; } = 0;

        public ItemRackStyle() : base() { }

        public ItemRackStyle(ItemRackStyle style)
            : base(style)
        {
            Type = style.Type;
            Left = style.Left;
            Prefix = style.Prefix;
        }
    }

    #endregion

    public class ItemRack : VisualObject
    {
        #region Data

        public ItemRackStyle ItemRackStyle => Style as ItemRackStyle;

        #endregion

        #region Initialize

        public ItemRack(int x, int y, ItemRackStyle style = null,
                Func<VisualObject, Touch, bool> callback = null)
            : base(x, y, 3, 3, new UIConfiguration(), style ?? new ItemRackStyle(), callback)
        {
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
            byte prefix = ItemRackStyle.Prefix;
            int fx = left ? 54 : 0;
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                {
                    dynamic tile = Provider[sx + x, sy + y];
                    tile.sTileHeader = 32;
                    tile.type = 334;
                    if (y == 1 && x < 2)
                    {
                        if (x == 0)
                            tile.frameX = (short)((left ? 20100 : 5100) + type);
                        else
                            tile.frameX = (short)((left ? 25000 : 10000) + prefix);
                    }
                    else
                        tile.frameX = (short)(fx + x * 18);
                    tile.frameY = (short)(y * 18);
                }
        }

        #endregion
    }
}
