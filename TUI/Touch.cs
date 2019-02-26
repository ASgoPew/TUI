using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public enum TouchState
    {
        Begin,
        Moving,
        End
    }

    public class Touch : IVisual<Touch>
    {
        #region Data

        public int AbsoluteX { get; set; }
        public int AbsoluteY { get; set; }
        public TouchState State { get; set; }
        public string Prefix { get; set; }
        public byte StateByte { get; set; }

        public bool Red => (StateByte & 1) > 0;
        public bool Green => (StateByte & 2) > 0;
        public bool Blue => (StateByte & 4) > 0;
        public bool Yellow => (StateByte & 8) > 0;
        public bool Actuator => (StateByte & 16) > 0;
        public bool Cutter => (StateByte & 32) > 0;

        #endregion

        #region IVisual

        #region Data

        public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public IEnumerable<Point> Points { get { yield return new Point(X, Y); } }
            public (int X, int Y, int Width, int Height) Padding(PaddingData paddingData) =>
                UI.Padding(X, Y, Width, Height, paddingData);

            #endregion

            #region Initialize

            public void InitializeVisual(int x, int y, int width = 1, int height = 1)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }

            #endregion
            #region XYWH

            public (int X, int Y, int Width, int Height) XYWH(int dx = 0, int dy = 0)
            {
                return (X + dx, Y + dy, Width, Height);
            }

            public Touch SetXYWH(int x, int y, int width = -1, int height = -1)
            {
                X = x;
                Y = y;
                Width = width >= 0 ? width : Width;
                Height = height >= 0 ? height : Height;
                return this;
            }

            public Touch SetXYWH((int x, int y, int width, int height) data)
            {
                X = data.x;
                Y = data.y;
                Width = data.width;
                Height = data.height;
                return this;
            }

            #endregion
            #region Move

            public Touch Move(int dx, int dy)
            {
                X = X + dx;
                Y = Y + dy;
                return this;
            }

            public Touch MoveBack(int dx, int dy)
            {
                X = X - dx;
                Y = Y - dy;
                return this;
            }

            #endregion
            #region Contains, Intersects

            public bool Contains(int x, int y)
            {
                return x >= X && y >= Y && x < X + Width && y < Y + Height;
            }

            public bool Intersecting(int x, int y, int width, int height)
            {
                return x < X + Width && X < x + width && y < Y + Height && Y < y + height;
            }

            public bool Intersecting((int x, int y, int width, int height) data)
            {
                return data.x < X + Width && X < data.x + data.width && data.y < Y + Height && Y < data.y + data.height;
            }

            #endregion

        #endregion
    }
}
