using System;
using TUI.Base;
using TUI.Base.Style;
using TUI.Hooks.Args;
using TUI.Widgets.Media;

namespace TUI.Widgets
{
    public class Image : VisualObject
    {
        #region Data

        public const int BrokenImageSize = 5;
        public ImageData Data { get; protected set; }
        public string Path { get; protected set; }

        #endregion

        #region Constructor

        public Image(int x, int y, string path, UIConfiguration configuration = null,
                UIStyle style = null, Action<VisualObject, Touch> callback = null)
            : base(x, y, BrokenImageSize, BrokenImageSize, configuration, style, callback)
        {
            Path = path;
        }

        public Image(int x, int y, ImageData data, UIConfiguration configuration = null,
                UIStyle style = null, Action<VisualObject, Touch> callback = null)
            : base(x, y, BrokenImageSize, BrokenImageSize, configuration, style, callback)
        {
            Data = data;
        }

        #endregion
        #region Copy

        public Image(Image image)
            : this(image.X, image.Y, new ImageData(image.Data), new UIConfiguration(image.Configuration),
                new UIStyle(image.Style), image.Callback.Clone() as Action<VisualObject, Touch>)
        {
        }

        #endregion
        #region LoadThisNative

        protected override void LoadThisNative()
        {
            base.LoadThisNative();

            if (Path == null && Data == null)
                return;

            if (Path != null)
            {
                ImageData[] images = ImageData.Load(Path);
                if (images.Length != 1)
                {
                    TUI.Hooks.Log.Invoke(new LogArgs("File not found or path is a folder: " + Path, LogType.Error));
                    Path = null;
                    return;
                }
                Data = images[0];
            }

            foreach (SignData sign in Data.Signs)
                Add(new VisualSign(sign.X, sign.Y, sign.Text));
            SetWH(Data.Width, Data.Height);
        }

        #endregion

        #region ApplyThisNative

        protected override void ApplyThisNative()
        {
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
                    tile.wallColor((byte)((x + y) % 2 == 0 ? 29 : 24));
                }

            base.ApplyThisNative();
        }

        #endregion
    }
}
