using System;
using System.Collections.Generic;

namespace TUI.Base
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

            #region IVisual

            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public IEnumerable<(int, int)> Points { get { yield return (X, Y); } }

            #endregion

        public int AbsoluteX { get; private set; }
        public int AbsoluteY { get; private set; }
        public TouchState State { get; internal set; }
        public UserSession Session { get; internal set; }
        public int TouchSessionIndex { get; internal set; }
        public bool InsideUI { get; internal set; }
        //public Locked Locked { get; internal set; }
        public bool Undo { get; set; }
        public byte Prefix { get; private set; }
        public byte StateByte { get; private set; }
        public DateTime Time { get; private set; }

        public bool Red      => (StateByte & 1) > 0;
        public bool Green    => (StateByte & 2) > 0;
        public bool Blue     => (StateByte & 4) > 0;
        public bool Yellow   => (StateByte & 8) > 0;
        public bool Actuator => (StateByte & 16) > 0;
        public bool Cutter   => (StateByte & 32) > 0;

        #endregion

        #region IVisual

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

            #endregion
            #region Move

            public Touch Move(int dx, int dy)
            {
                X += dx;
                Y += dy;
                return this;
            }

            public Touch MoveBack(int dx, int dy)
            {
                X -= dx;
                Y -= dy;
                return this;
            }

            #endregion
            #region Contains, Intersects

            public bool Contains(int x, int y) =>
                x >= X && y >= Y && x < X + Width && y < Y + Height;
            public bool ContainsRelative(int x, int y) =>
                x >= 0 && y >= 0 && x < Width && y < Height;
            public bool Intersecting(int x, int y, int width, int height) =>
                x < X + Width && X < x + width && y < Y + Height && Y < y + height;
            public bool Intersecting(Touch o) => false;

            #endregion

        #endregion

        #region Initialize

        public Touch(int x, int y, TouchState state, byte prefix = 0, byte stateByte = 0)
        {
            InitializeVisual(x, y);
            AbsoluteX = X;
            AbsoluteY = Y;
            State = state;
            Prefix = prefix;
            StateByte = stateByte;
            Time = DateTime.UtcNow;
        }

        #endregion
        #region SetSession

        public void SetSession(UserSession session)
        {
            Session = session;
            TouchSessionIndex = Session.TouchSessionIndex;
        }

        #endregion
        #region Copy

        public Touch(Touch touch)
        {
            this.AbsoluteX = touch.AbsoluteX;
            this.AbsoluteY = touch.AbsoluteY;
            this.State = touch.State;
            this.Prefix = touch.Prefix;
            this.Session = touch.Session;
            this.Undo = touch.Undo;
            this.StateByte = touch.StateByte;
        }

        #endregion
        #region SimulatedEndTouch

        public Touch SimulatedEndTouch()
        {
            Touch touch = new Touch(this);
            touch.State = TouchState.End;
            return touch;
        }

        #endregion
    }
}
