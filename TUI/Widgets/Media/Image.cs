using System;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;
using TerrariaUI.Widgets.Data;
using TerrariaUI.Widgets.Media;

namespace TerrariaUI.Widgets
{
    public class Image : VisualObject
    {
        #region Data

        public const int BrokenImageSize = 5;
        public ImageData Data { get; protected set; }
        public string ImageName { get; protected set; }

        #endregion

        #region Constructor

        public Image(int x, int y, string name, UIConfiguration configuration = null,
                UIStyle style = null, Action<VisualObject, Touch> callback = null)
            : base(x, y, BrokenImageSize, BrokenImageSize, configuration, style, callback)
        {
            ImageName = name;
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

            if (ImageName == null && Data == null)
                return;

            if (ImageName != null)
            {
                ImageData image = ImageData.LoadImage(ImageName);
                if (image == null)
                {
                    TUI.Log(this, "Cannot find an image: " + ImageName, LogType.Error);
                    ImageName = null;
                    return;
                }
                Data = image;
            }

            //TODO: signs, chests, entities???

            SetWH(Data.Width, Data.Height, false);
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
