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

        public static readonly int[,] BrokenVideo = new int[7, 5]
        {
            { 1, 2, 3, 4, 5 },
            { 1, 2, 3, 4, 5 },
            { 1, 2, 3, 4, 5 },
            { 1, 2, 3, 4, 5 },
            { 1, 2, 3, 4, 5 },
            { 1, 2, 3, 4, 5 },
            { 1, 2, 3, 4, 5 }
        };

        public string Path { get; protected set; }
        public int Delay { get; }
        protected List<Image> Images = new List<Image>();
        protected Timer Timer = new Timer() { AutoReset = true };
        protected int CurrentImage = -1;

        #endregion

        #region Constructor

        public Video(int x, int y, string path, int delay = 500, UIConfiguration configuration = null,
                UIStyle style = null, Func<VisualObject, Touch, bool> callback = null)
            : base(x, y, 7, 5, configuration, style, callback)
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
                SetWH(3, 3);
                return false;
            }
            
            foreach (ImageData data in images)
            {
                Image image = new Image(0, 0, data);
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
