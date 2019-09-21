using System;
using TUI.Base;
using TUI.Base.Style;
using TUI.Hooks.Args;

namespace TUI.Widgets
{
    #region ItemRackStyle

    public enum ItemSize
    {
        Smallest = 0,
        Small,
        Normal,
        Big,
        Biggest
    }

    /// <summary>
    /// Drawing styles of ItemRack widget.
    /// </summary>
    public class ItemRackStyle : UIStyle
    {
        /// <summary>
        /// Item NetID.
        /// </summary>
        public short Type { get; set; } = 0;
        /// <summary>
        /// Side of weapon rack.
        /// </summary>
        public bool Left { get; set; } = true;
        /// <summary>
        /// Size of item (prefix).
        /// </summary>
        public ItemSize Size { get; set; } = ItemSize.Normal;
        /// <summary>
        /// Style of lighting.
        /// </summary>
        public Light Light { get; set; } = null;

        public ItemRackStyle() : base() { }

        public ItemRackStyle(ItemRackStyle style)
            : base(style)
        {
            Type = style.Type;
            Left = style.Left;
            Size = style.Size;
            Light = new Light(style.Light);
        }
    }

    #endregion

    /// <summary>
    /// Widget for drawing item (with ability to draw sign with text on topS).
    /// </summary>
    public class ItemRack : VisualObject
    {
        #region Data

        protected static readonly int[] Sizes = new int[] { 7, 9, 0, 1, 2 };

        public dynamic Sign { get; set; } = null;

        public ItemRackStyle ItemRackStyle => Style as ItemRackStyle;
        protected string RawText { get; set; } = null;

        #endregion

        #region Constructor

        /// <summary>
        /// Widget for drawing item (with ability to draw sign with text on topS).
        /// </summary>
        public ItemRack(int x, int y, ItemRackStyle style = null, Action<VisualObject, Touch> callback = null)
            : base(x, y, 3, 3, new UIConfiguration(), style ?? new ItemRackStyle(), callback)
        {
            ForceSection = true;
        }

        #endregion
        #region Copy

        public ItemRack(ItemRack rack)
            : this(rack.X, rack.Y, rack.ItemRackStyle, rack.Callback)
        {
        }

        #endregion
        #region LoadThisNative

        protected override void LoadThisNative()
        {
            base.LoadThisNative();

            if (RawText != null)
                CreateSign();
        }

        #endregion
        #region Dispose

        protected override void DisposeThisNative()
        {
            base.DisposeThisNative();
            RemoveSign();
        }

        #endregion

        #region SetText

        // Use this only after adding ItemRack to parent.
        public void SetText(string text)
        {
            RawText = text;
        }

        #endregion
        #region GetText

        public string GetText() => RawText;

        #endregion
        #region CreateSign

        protected void CreateSign()
        {
            if (RawText == null)
                throw new NullReferenceException("CreateSign: Text is null");
            (int x, int y) = AbsoluteXY();
            UpdateSignArgs args = new UpdateSignArgs(x, y, null, this);
            TUI.Hooks.UpdateSign.Invoke(args);
            if (args.Sign == null)
            {
                TUI.Hooks.Log.Invoke(new LogArgs("Can't create new sign.", LogType.Error));
                return;
            }
            Sign = args.Sign;
            Sign.text = RawText;
        }

        #endregion
        #region RemoveSign

        protected void RemoveSign()
        {
            if (Sign == null)
                return;
            TUI.Hooks.RemoveSign.Invoke(new RemoveSignArgs(this, Sign));
            Sign = null;
        }

        #endregion
        #region UpdateSign

        protected void UpdateSign()
        {
            if (RawText != null && Sign != null)
            {
                (int x, int y) = AbsoluteXY();
                dynamic tile = Tile(0, 0);
                if (tile != null)
                {
                    if (UsesDefaultMainProvider && tile.type != 55)
                    {
                        tile.type = 55;
                        tile.frameX = 0;
                        tile.frameY = 0;
                    }
                    Sign.x = x;
                    Sign.y = y;
                    Sign.text = RawText;
                }
                else
                    Sign.text = "";
            }
        }

        #endregion

        #region PulseThisNative

        protected override void PulseThisNative(PulseType type)
        {
            base.PulseThisNative(type);

            if (type == PulseType.PositionChanged)
                UpdateSign();
        }

        #endregion
        #region UpdateThisNative

        protected override void UpdateThisNative()
        {
            base.UpdateThisNative();

            UpdateSign();
        }

        #endregion
        #region ApplyThisNative

        protected override void ApplyThisNative()
        {
            base.ApplyThisNative();

            short type = ItemRackStyle.Type;
            bool left = ItemRackStyle.Left;
            Light light = ItemRackStyle.Light;
            int fx = left ? 54 : 0;
            bool sign = Sign != null;
            int prefix = Sizes[(int)ItemRackStyle.Size];
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                {
                    dynamic tile = Tile(x, y);
                    if (tile == null)
                        continue;
                    tile.active(true);
                    tile.type = (ushort)(sign && y == 0 ? 55 : 334);
                    if (sign && y == 0)
                        tile.frameX = (short)((x == 0) ? 144 : (x == 1) ? 126 : 162);
                    else if (y == 1 && x < 2)
                        tile.frameX = (short)
                            (x == 0
                            ? (left ? 20100 : 5100) + type
                            : (left ? 25000 : 10000) + prefix);
                    else
                        tile.frameX = (short)(fx + x * 18);
                    tile.frameY = (short)(sign && y == 0 ? 0 : y * 18);
                    if (x == 1 && y == 1 && light != null)
                    {
                        tile.wall = (byte)light.Wall;
                        if (light.Color > 0)
                            tile.wallColor((byte)light.Color);
                    }
                }
        }

        #endregion
    }
}
