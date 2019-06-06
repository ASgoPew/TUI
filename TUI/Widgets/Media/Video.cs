using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TUI.Base;
using TUI.Base.Style;
using TUI.Hooks.Args;

namespace TUI.Widgets.Media
{
    public class Video : VisualObject
    {
        #region Data

        public static readonly byte[,] BrokenVideo = new byte[8, 5]
        {
            { 25, 25, 25, 25, 21 },
            { 26, 26, 26, 26, 29 },
            { 20, 20, 20, 20, 24 },
            { 17, 17, 17, 17, 29 },
            { 24, 24, 24, 24, 20 },
            { 13, 13, 13, 13, 29 },
            { 21, 21, 21, 21, 26 },
            { 29, 29, 29, 29, 29 }
        };

        public string Path { get; protected set; }
        public int Delay { get; }
        protected List<Image> Images = new List<Image>();
        protected Timer Timer = new Timer() { AutoReset = true };
        protected int CurrentImage = -1;

        public bool Playing => Timer.Enabled;

        #endregion

        #region Constructor

        public Video(int x, int y, string path, int delay = 500, UIConfiguration configuration = null,
                UIStyle style = null, Func<VisualObject, Touch, bool> callback = null)
            : base(x, y, 8, 5, configuration, style, callback)
        {
            Path = path;
            Delay = delay;
            Timer.Interval = delay;
            Timer.Elapsed += Next;
        }

        #endregion
        #region DisposeThisNative

        public override void Dispose()
        {
            base.Dispose();
            Stop();
        }

        #endregion

        #region Start

        public Video Start()
        {
            if (Images.Count != 0)
                Timer.Start();
            return this;
        }

        #endregion
        #region Stop

        public Video Stop()
        {
            Timer.Stop();
            return this;
        }

        #endregion
        #region Load

        public bool Load()
        {
            ImageData[] images = ImageData.Load(Path);
            if (images.Length == 0)
            {
                TUI.Hooks.Log.Invoke(new LogArgs("Invalid video folder: " + Path, LogType.Error));
                Path = null;
                return false;
            }
            
            foreach (ImageData data in images)
            {
                Image image = new Image(0, 0, data, new UIConfiguration() { UseBegin=false }, new UIStyle(Style));
                Add(image);
                Images.Add(image);
                image.Load();
            }
            return true;
        }

        #endregion
        #region UpdateThisNative

        protected override void UpdateThisNative()
        {
            base.UpdateThisNative();

            if (Path != null && Images.Count == 0)
                if (Load())
                {
                    SetWH(Images.Max(i => i.Width), Images.Max(i => i.Height));
                    Parent.UpdateThis();
                    Update();
                    return;
                }
        }

        #endregion
        #region ApplyThisNative

        protected override void ApplyThisNative()
        {
            base.ApplyThisNative();

            if (Images.Count == 0)
                foreach ((int x, int y) in Points)
                {
                    dynamic tile = Tile(x, y);
                    if (tile == null)
                        continue;
                    tile.wall = 155;
                    tile.wallColor((byte)BrokenVideo[x, y]);
                }
        }

        #endregion
        #region Next

        protected virtual void Next(object sender, ElapsedEventArgs args)
        {
            if (Root == null || Root.Players.Count == 0 || !Visible)
                return;

            CurrentImage = (CurrentImage + 1) % Images.Count;
            Select(Images[CurrentImage]).Update().Apply().Draw();
        }

        #endregion
    }
}
