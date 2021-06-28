using System;
using System.Collections.Generic;
using System.Timers;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;
using TerrariaUI.Widgets.Media;

namespace TerrariaUI.Widgets
{
    #region VideoStyle

    public class VideoStyle : ContainerStyle
    {
        public string VideoName { get; set; }
        public int Delay { get; set; } = 500;
        public bool Repeat { get; set; } = false;

        public VideoStyle() : base() { }

        public VideoStyle(VideoStyle style)
            : base(style)
        {
            VideoName = style.VideoName;
            Delay = style.Delay;
            Repeat = style.Repeat;
        }
    }

    #endregion

    public class Video : VisualContainer
    {
        #region Data

        public static readonly byte[,] BrokenVideo = new byte[8, 5]
        {
            { 26, 26, 26, 26, 21 },
            { 15, 15, 15, 15, 29 },
            { 20, 20, 20, 20, 24 },
            { 17, 17, 17, 17, 29 },
            { 24, 24, 24, 24, 20 },
            { 13, 13, 13, 13, 29 },
            { 21, 21, 21, 21, 26 },
            { 29, 29, 29, 29, 29 }
        };

        protected List<Image> Images = new List<Image>();
        protected Timer Timer = new Timer() { AutoReset = true };

        public int Frame { get; set; } = 0;
        public int FrameCount => Images.Count;
        public bool Playing => Timer.Enabled;
        public VideoStyle VideoStyle => Style as VideoStyle;

        #endregion

        #region Constructor

        public Video(int x, int y, int width = -1, int height = -1, UIConfiguration configuration = null, VideoStyle style = null,
                Action<VisualObject, Touch> callback = null)
            : base(x, y, width == -1 ? 8 : width, height == -1 ? 5 : height, configuration, style, callback)
        {
            Timer.Interval = VideoStyle.Delay;
            Timer.Elapsed += Next;
        }

        #endregion
        #region LoadThisNative

        protected override void LoadThisNative()
        {
            base.LoadThisNative();

            List<ImageData> images = ImageData.LoadVideo(VideoStyle.VideoName);
            if (images == null)
            {
                TUI.Log(this, "Cannot find a video: " + VideoStyle.VideoName, LogType.Error);
                VideoStyle.VideoName = null;
                return;
            }
            
            foreach (ImageData data in images)
            {
                Image image = new Image(0, 0, data, new UIConfiguration() { UseBegin=false }, new UIStyle(Style));
                Add(image.Disable(false));
                Images.Add(image);
            }
            Images[0].Enable(false);

            SetWH(Images[0].Width, Images[0].Height, false);
            return;
        }

        #endregion
        #region DisposeThisNative

        protected override void DisposeThisNative()
        {
            base.DisposeThisNative();
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
        #region Toggle

        public Video ToggleStart() => Playing ? Stop() : Start();

        #endregion

        #region PulseThisNative

        protected override void PulseThisNative(PulseType type)
        {
            base.PulseThisNative(type);
            if (type == PulseType.Reset)
                Frame = 0;
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
            if (Root == null || Root.Players.Count == 0 || !IsActive)
                return;

            Frame = (Frame + 1) % Images.Count;
            Select(Images[Frame], true);

            if (Frame == Images.Count - 1 && !VideoStyle.Repeat)
                Stop();
        }

        #endregion
    }
}
