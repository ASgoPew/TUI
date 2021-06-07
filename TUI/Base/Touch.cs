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
    public class Touch
    {
        #region Data

        /// <summary>
        /// Horizontal coordinate relative to left border of this object.
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// Vertical coordinate relative to top border of this object.
        /// </summary>
        public int Y { get; set; }
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
        public PlayerSession Session { get; internal set; }
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
        /// <summary>
        /// True if it is TouchState.End and user ended his touch with pressing
        /// right mouse button (undo grand design action).
        /// </summary>
        public bool Undo { get; set; }
        /// <summary>
        /// Prefix of the touching grand design
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

        #region Constructor

        public Touch(int x, int y, TouchState state, byte prefix = 0, byte stateByte = 0)
        {
            X = x;
            Y = y;
            AbsoluteX = X;
            AbsoluteY = Y;
            State = state;
            Prefix = prefix;
            StateByte = stateByte;
            Time = DateTime.UtcNow;
        }

        #endregion
        #region Move

        public void Move(int dx, int dy)
        {
            X += dx;
            Y += dy;
        }

        #endregion
        #region SetSession

        public void SetSession(PlayerSession session)
        {
            Session = session;
            TouchSessionIndex = Session.TouchSessionIndex;
            Index = Session.Count;
        }

        #endregion
        #region Copy

        public Touch(Touch touch)
        {
            this.X = touch.AbsoluteX;
            this.Y = touch.AbsoluteY;
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
