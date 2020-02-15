using System;

namespace TerrariaUI.Base
{
    public enum TouchState
    {
        Begin,
        Moving,
        End
    }

    /// <summary>
    /// Touch event information.
    /// </summary>
    public class Touch : IVisual<Touch>
    {
        #region Data

            #region IVisual

            /// <summary>
            /// Horizontal coordinate relative to left border of this object.
            /// </summary>
            public int X { get; set; }
            /// <summary>
            /// Vertical coordinate relative to top border of this object.
            /// </summary>
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            #endregion

        /// <summary>
        /// Horizontal coordinate relative to world left border.
        /// </summary>
        public int AbsoluteX { get; private set; }
        /// <summary>
        /// Vertical coordinate relative to world top border.
        /// </summary>
        public int AbsoluteY { get; private set; }
        /// <summary>
        /// Touch state. Can have one of these values: Begin, Moving, End.
        /// </summary>
        public TouchState State { get; internal set; }
        /// <summary>
        /// UserSession object of user who is touching.
        /// </summary>
        public UserSession Session { get; internal set; }
        /// <summary>
        /// Number of touch counting from TouchState.Begin.
        /// </summary>
        public int Index { get; private set; }
        /// <summary>
        /// VisualObject that was touched with this touch.
        /// </summary>
        public VisualObject Object { get; internal set; }
        /// <summary>
        /// Identifier of touch interval when this touch was touched.
        /// </summary>
        public int TouchSessionIndex { get; internal set; }
        public bool InsideUI { get; internal set; }
        //public Locked Locked { get; internal set; }
        /// <summary>
        /// True if it is TouchState.End and user ended his touch with pressing
        /// right mouse button (undo grand design action).
        /// </summary>
        public bool Undo { get; set; }
        /// <summary>
        /// Prefix of the touching grand desing
        /// </summary>
        public byte Prefix { get; private set; }
        private byte StateByte { get; set; }
        /// <summary>
        /// Time of pressing in Utc.
        /// </summary>
        public DateTime Time { get; private set; }

        public int PlayerIndex => Session.PlayerIndex;
        /// <summary>
        /// Whether this touch has red wire turned on. Actual only when State is TouchState.End.
        /// </summary>
        public bool Red      => (StateByte & 1) > 0;
        /// <summary>
        /// Whether this touch has green wire turned on. Actual only when State is TouchState.End.
        /// </summary>
        public bool Green    => (StateByte & 2) > 0;
        /// <summary>
        /// Whether this touch has blue wire turned on. Actual only when State is TouchState.End.
        /// </summary>
        public bool Blue     => (StateByte & 4) > 0;
        /// <summary>
        /// Whether this touch has yellow wire turned on. Actual only when State is TouchState.End.
        /// </summary>
        public bool Yellow   => (StateByte & 8) > 0;
        /// <summary>
        /// Whether this touch has actuator turned on. Actual only when State is TouchState.End.
        /// </summary>
        public bool Actuator => (StateByte & 16) > 0;
        /// <summary>
        /// Whether this touch has cutter turned on. Actual only when State is TouchState.End.
        /// </summary>
        public bool Cutter   => (StateByte & 32) > 0;

        #endregion

        #region IVisual

            #region InitializeVisual

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

            public Touch SetXYWH(int x, int y, int width = -1, int height = -1, bool draw = true)
            {
                X = x;
                Y = y;
                Width = width >= 0 ? width : Width;
                Height = height >= 0 ? height : Height;
                return this;
            }

            #endregion
            #region Move

            public Touch Move(int dx, int dy, bool draw = true)
            {
                X += dx;
                Y += dy;
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

        #region Constructor

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
            Index = Session.Count;
        }

        #endregion
        #region Copy

        public Touch(Touch touch)
        {
            this.AbsoluteX = touch.AbsoluteX;
            this.AbsoluteY = touch.AbsoluteY;
            this.X = this.AbsoluteX;
            this.Y = this.AbsoluteY;
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
