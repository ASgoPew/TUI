using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public static class UI
    {
        #region Data

        public const int MaxUsers = 256;
        public static bool ShowGrid = false;
        public static Hook<SetXYWHArgs> SetXYWHHook = new Hook<SetXYWHArgs>();
        public static Hook<SetTopArgs> SetTopHook = new Hook<SetTopArgs>();
        public static Hook<EnabledArgs> EnabledHook = new Hook<EnabledArgs>();

        #endregion

        #region Padding

        public static (int X, int Y, int Width, int Height) Padding(int X, int Y, int Width, int Height, PaddingConfig paddingData)
        {
            int x = paddingData.X;
            int y = paddingData.Y;
            int width = paddingData.Width;
            int height = paddingData.Height;
            Alignment alignment = paddingData.Alignment;
            if (alignment == Alignment.Up || alignment == Alignment.Center || alignment == Alignment.Down)
                x = x + Width / 2;
            else if (alignment == Alignment.UpRight || alignment == Alignment.Right || alignment == Alignment.DownRight)
                x = x + Width;
            if (alignment == Alignment.Left || alignment == Alignment.Center || alignment == Alignment.Right)
                y = y + Height / 2;
            else if (alignment == Alignment.DownRight || alignment == Alignment.Down || alignment == Alignment.DownRight)
                y = y + Height;
            if (width <= 0)
                width = Width + width - x;
            if (height <= 0)
                height = Height + height - y;
            return (x, y, width, height);
        }

        #endregion
        #region SaveTime

        public static void SaveTime<T>(T o, string name, string key = null)
            where T : Touchable<T>
        {

        }

        #endregion
        #region ShowTime

        public static void ShowTime<T>(T o, string name, string key = null)
            where T : Touchable<T>
        {

        }

        #endregion
    }
}
