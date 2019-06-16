using System;
using TUI.Base;
using TUI.Base.Style;
using TUI.Hooks.Args;

namespace TUI.Widgets
{
    public class VisualSign : VisualObject
    {
        #region Data

        protected string RawText { get; set; }
        protected dynamic Sign { get; set; }

        #endregion

        #region Constructor

        public VisualSign(int x, int y, int width, int height, string text, UIConfiguration configuration = null,
                UIStyle style = null, Action<VisualObject, Touch> callback = null)
            : base(x, y, width, height, configuration, style, callback)
        {
            if (width > 2 || height > 2)
                throw new ArgumentException("Sign can only have one of these sizes: 1x1, 1x2, 2x1, 2x2.");

            RawText = text ?? "";

            // Needed for ApplyTiles() (it checks if any of UIStyle parameters are set)
            Style.Active = true;
        }

        public VisualSign(int x, int y, string text, UIConfiguration configuration = null, UIStyle style = null,
                Action<VisualObject, Touch> callback = null)
            : this(x, y, 2, 2, text, configuration, style, callback)
        { }

        #endregion
        #region Copy

        public VisualSign(VisualSign visualSign)
            : this(visualSign.X, visualSign.Y, visualSign.Width, visualSign.Height, visualSign.RawText,
                  new UIConfiguration(visualSign.Configuration), new UIStyle(visualSign.Style), visualSign.Callback.Clone() as Action<VisualObject, Touch>)
        { }

        #endregion
        #region LoadThisNative

        protected override void LoadThisNative()
        {
            base.LoadThisNative();
            CreateSign();
        }

        #endregion
        #region DisposeThisNative

        protected override void DisposeThisNative()
        {
            base.DisposeThisNative();
            RemoveSign();
        }

        #endregion

        #region Set

        public virtual void Set(string value)
        {
            RawText = value ?? "";
        }

        #endregion
        #region Get

        public string Get() => RawText;

        #endregion
        #region CreateSign

        protected void CreateSign()
        {
            if (RawText == null)
                throw new NullReferenceException("CreateSign: Text is null");
            (int x, int y) = AbsoluteXY();
            CreateSignArgs args = new CreateSignArgs(x, y, this);
            TUI.Hooks.CreateSign.Invoke(args);
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

            UpdateSign();
        }

        #endregion
        #region ApplyTile

        protected override void ApplyTile(int x, int y)
        {
            dynamic tile = Tile(x, y);
            if (tile == null)
                return;
            tile.active(true);
            if (Style.InActive != null)
                tile.inActive(Style.InActive.Value);
            tile.type = (ushort)55;
            if (Style.TileColor != null)
                tile.color(Style.TileColor.Value);
            if (Style.Wall != null)
                tile.wall = Style.Wall.Value;
            if (Style.WallColor != null)
                tile.wallColor(Style.WallColor.Value);

            tile.frameX = (short)((x == 0) ? 144 : 162);
            tile.frameY = (short)((y == 0) ? 0 : 18);
        }

        #endregion
    }
}
