﻿using System;
using System.Collections.Generic;
using System.Timers;
using TUI.Base;
using TUI.Base.Style;
using TUI.Hooks.Args;
using TUI.Widgets.Media;

namespace TUI.Widgets
{
    #region VideoStyle

    public class VideoStyle : UIStyle
    {
        public string Path { get; set; }
        public int Delay { get; set; } = 500;
        public bool Repeat { get; set; } = false;

        public VideoStyle() : base() { }

        public VideoStyle(VideoStyle style)
            : base(style)
        {
            Path = style.Path;
            Delay = style.Delay;
            Repeat = style.Repeat;
        }
    }

    #endregion

    public class Video : VisualObject
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

        public Video(int x, int y, UIConfiguration configuration = null, UIStyle style = null,
                Action<VisualObject, Touch> callback = null)
            : base(x, y, 8, 5, configuration, style, callback)
        {
            Timer.Interval = VideoStyle.Delay;
            Timer.Elapsed += Next;
        }

        #endregion
        #region LoadThisNative

        protected override void LoadThisNative()
        {
            base.LoadThisNative();

            ImageData[] images = ImageData.Load(VideoStyle.Path);
            if (images.Length == 0)
            {
                TUI.Hooks.Log.Invoke(new LogArgs("Invalid video folder: " + VideoStyle.Path, LogType.Error));
                VideoStyle.Path = null;
                return;
            }
            
            foreach (ImageData data in images)
            {
                Image image = new Image(0, 0, data, new UIConfiguration() { UseBegin=false }, new UIStyle(Style));
                Add(image.Disable());
                Images.Add(image);
            }
            Images[0].Enable();
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
        #region UpdateSizeNative

        protected override (int, int) UpdateSizeNative() =>
            Images?.Count > 0
            //? (Images.Max(i => i.Width), Images.Max(i => i.Height))
            ? (Images[Frame].Width, Images[Frame].Height)
            : (8, 5);

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
            if (Root == null || Root.Players.Count == 0 || !Active)
                return;

            Frame = (Frame + 1) % Images.Count;
            Select(Images[Frame]).Update().Apply().Draw();

            if (Frame == Images.Count - 1 && !VideoStyle.Repeat)
                Stop();
        }

        #endregion
    }
}
