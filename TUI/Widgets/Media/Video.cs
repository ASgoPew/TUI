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

        public string Path { get; protected set; }
        public int Delay { get; }
        protected List<Image> Images = new List<Image>();
        protected Timer Timer = new Timer() { AutoReset = true };
        protected int CurrentImage = -1;

        #endregion

        #region Constructor

        public Video(int x, int y, string path, int delay = 500, UIConfiguration configuration = null,
                UIStyle style = null, Func<VisualObject, Touch, bool> callback = null)
            : base(x, y, 0, 0, configuration, style, callback)
        {
            Path = path;
            Delay = delay;
            Timer.Interval = delay;
            Timer.Elapsed += Next;
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
                Image image = new Image(0, 0, data, null, Style);
                Images.Add(Add(image) as Image);
                image.Load();
            }
            return true;
        }

        #endregion
        #region Start, Stop

        public void Start() => Timer.Start();
        public void Stop() => Timer.Stop();

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
            if (Root == null || Root.Players.Count == 0)
                return;

            int index = ++CurrentImage;
            if (CurrentImage >= Images.Count)
            {
                CurrentImage = -1;
                index = 0;
            }

            Select(Images[index]).Update().Apply().Draw();
        }

        #endregion
    }
}
