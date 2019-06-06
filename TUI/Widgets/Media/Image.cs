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
        public string Path { get; protected set; }

        #endregion

        #region Constructor

        public Image(int x, int y, string path, UIConfiguration configuration = null,
                UIStyle style = null, Func<VisualObject, Touch, bool> callback = null)
            : base(x, y, 0, 0, configuration, style, callback)
        {
            Path = path;
        }

        #endregion
        #region DisposeThisNative

        protected override void DisposeThisNative()
        {
            base.DisposeThisNative();
            RemoveSigns();
        }

        #endregion

        #region Load

        public bool Load()
        {
            ImageData[] images = ImageData.Load(Path);
            if (images.Length != 1)
            {
                TUI.Hooks.Log.Invoke(new LogArgs("File not found or path is a folder: " + Path, LogType.Error));
                Path = null;
                SetWH(3, 3);
                return false;
            }
            Data = images[0];
            SetWH(Data.Width, Data.Height);
            (int x, int y) = AbsoluteXY();
            foreach (SignData sign in Data.Signs)
            {
                if (sign.Sign == null)
                {
                    CreateSignArgs args = new CreateSignArgs(x + sign.X, y + sign.Y, this);
                    TUI.Hooks.CreateSign.Invoke(args);
                    if (args.Sign == null)
                    {
                        TUI.Hooks.Log.Invoke(new LogArgs("Can't create new sign.", LogType.Error));
                        break;
                    }
                    sign.Sign = args.Sign;
                    sign.Sign.text = sign.Text;
                }
            }
            return true;
        }

        #endregion
        #region RemoveSigns

        protected virtual void RemoveSigns()
        {
            if (Data?.Signs != null)
                foreach (SignData sign in Data.Signs)
                    if (sign.Sign != null)
                        TUI.Hooks.RemoveSign.Invoke(new RemoveSignArgs(this, sign.Sign));
        }

        #endregion
        #region UpdateSigns

        protected virtual void UpdateSigns()
        {
            if (Data?.Signs != null)
            {
                (int x, int y) = AbsoluteXY();
                foreach (SignData sign in Data.Signs)
                {
                    if (sign.Sign != null)
                    {
                        dynamic tile = Tile(sign.X, sign.Y);
                        if (tile != null)
                        {
                            tile.type = 55;
                            tile.frameX = 0;
                            tile.frameY = 0;
                            sign.Sign.x = x + sign.X;
                            sign.Sign.y = y + sign.Y;
                            sign.Sign.text = sign.Text;
                        }
                        else
                            sign.Sign.text = "";
                    }
                    
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
                case PulseType.PositionChanged:
                    UpdateSigns();
                    break;
            }
        }

        #endregion
        #region UpdateThisNative

        protected override void UpdateThisNative()
        {
            base.UpdateThisNative();

            if (Path != null && Data == null)
                if (Load())
                {
                    Parent.UpdateThis();
                    Update();
                    return;
                }

            UpdateSigns();
        }

        #endregion
        #region ApplyThisNative

        protected override void ApplyThisNative()
        {
            base.ApplyThisNative();

            dynamic tiles = Data?.Tiles;
            if (tiles != null)
                foreach ((int x, int y) in Points)
                    Tile(x, y)?.CopyFrom(tiles[x, y]);
            else
                foreach ((int x, int y) in Points)
                {
                    dynamic tile = Tile(x, y);
                    if (tile == null)
                        continue;
                    tile.wall = 155;
                    tile.wallColor((x + y) % 2 == 0 ? 26 : 13);
                }
        }

        #endregion
    }
}
