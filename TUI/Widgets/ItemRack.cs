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

        public dynamic Sign { get; set; } = null;

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
            bool sign = Sign != null;
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                {
                    dynamic tile = Provider[sx + x, sy + y];
                    tile.type = (ushort)(sign && y == 0 ? 55 : 334);
                    if (sign && y == 0)
                        tile.frameX = (short)((x == 0) ? 144 : (x == 1) ? 126 : 162);
                    else if (y == 1 && x < 2)
                        tile.frameX = (short)
                            (x == 0
                            ? (left ? 20100 : 5100) + type
                            : left ? 25000 : 10000);
                    else
                        tile.frameX = (short)(fx + x * 18);
                    tile.frameY = (short)(sign && y == 0 ? 0 : y * 18);
                }
        }

        #endregion
        #region SetText

        public void SetText(string text)
        {
            if (Sign == null)
                CreateSign();
            Sign.text = text;
        }

        #endregion
        #region CreateSign

        public void CreateSign()
        {
            (int x, int y) = AbsoluteXY();
            CreateSignArgs args = new CreateSignArgs(x, y, Sign);
            UI.Hooks.CreateSign.Invoke(args);
            if (args.Sign == null)
                throw new Exception("Can't create new sign.");
            Sign = args.Sign;
        }

        #endregion
        #region RemoveSign

        public string RemoveSign()
        {
            if (Sign == null)
                throw new Exception("EBLAN TI BLYAT");
            string text = Sign.text;
            UI.Hooks.RemoveSign.Invoke(new RemoveSignArgs(Sign));
            Sign = null;
            return text;
        }

        #endregion
        #region PulseThisNative

        protected override void PulseThisNative(PulseType type)
        {
            base.PulseThisNative(type);
            if (Sign == null)
                return;
            if (type == PulseType.SetXYWH)
            {
                (int x, int y) = AbsoluteXY();
                SetText(RemoveSign());
            }
            else if (type == PulseType.Dispose)
                RemoveSign();
        }

        #endregion
    }
}
