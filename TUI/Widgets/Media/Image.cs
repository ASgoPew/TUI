using System;
using TUI.Base;
using TUI.Base.Style;
using TUI.Hooks.Args;

namespace TUI.Widgets.Media
{
    public class Image : VisualObject
    {
        #region Data

        public ImageData Data { get; protected set; }

        #endregion

        #region Constructor

        public Image(int x, int y, string path, UIConfiguration configuration = null,
                UIStyle style = null, Func<VisualObject, Touch, bool> callback = null)
            : base(x, y, 0, 0, configuration, style, callback)
        {
            ImageData[] images = ImageData.Load(path);
            if (images.Length != 1)
                throw new System.IO.IOException("File not found or path is a folder: " + path);
            Data = images[0];
        }

        #endregion

        #region ApplyThisNative

        protected override void ApplyThisNative()
        {
            dynamic tiles = Data.Tiles;
            foreach ((int x, int y) in Points)
                Tile(x, y)?.CopyFrom(tiles[x, y]);
            base.ApplyThisNative();
        }

        #endregion
        #region UpdateThisNative

        protected override void UpdateThisNative()
        {
            base.UpdateThisNative();

            (int x, int y) = AbsoluteXY();
            foreach (SignData sign in Data.Signs)
            {
                if (sign.Sign != null)
                {
                    int sx = x + sign.X, sy = y + sign.Y;
                    Tile(sign.X, sign.Y).type = 55;
                    sign.Sign.x = sx;
                    sign.Sign.y = sy;
                    sign.Sign.text = sign.Text;
                }
            }
        }

        #endregion
        #region PulseThisNative

        protected override void PulseThisNative(PulseType type)
        {
            base.PulseThisNative(type);
            switch (type)
            {
                case PulseType.PreSetXYWH:
                case PulseType.Dispose:
                    foreach (SignData sign in Data.Signs)
                        if (sign.Sign != null)
                            TUI.Hooks.RemoveSign.Invoke(new RemoveSignArgs(this, sign.Sign));
                    break;
                case PulseType.PostSetXYWH:
                    (int x, int y) = AbsoluteXY();
                    foreach (SignData sign in Data.Signs)
                    {
                        CreateSignArgs args = new CreateSignArgs(x + sign.X, y + sign.Y, this);
                        TUI.Hooks.CreateSign.Invoke(args);
                        sign.Sign = args.Sign;
                        sign.Sign.text = sign.Text;
                    }
                    break;
            }
        }

        #endregion
    }
}
