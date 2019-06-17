using System;

namespace TUI.Base
{
    public enum LockLevel
    {
        Self,
        Root,
        //UI
    }

    public class Lock
    {
        public const int DefaultDelay = 300;

        public LockLevel Level { get; set; }
        public bool Personal { get; set; }
        public int Delay { get; set; }
        public bool AllowThisTouchSession { get; set; }
        public bool DuringTouchSession { get; set; }

        public Lock(LockLevel level = LockLevel.Self, bool personal = true, int delay = DefaultDelay, bool allowThisTouchSession = true, bool duringTouchSession = false)
        {
            Level = level;
            Personal = personal;
            Delay = delay;
            AllowThisTouchSession = allowThisTouchSession;
            DuringTouchSession = duringTouchSession;
        }

        public Lock(Lock config)
        {
            this.Level = config.Level;
            this.Personal = config.Personal;
            this.Delay = config.Delay;
        }
    }

    public class Locked
    {
        //public VisualObject Target { get; }
        public VisualObject Holder { get; }
        /// <summary>
        /// Lock time in UTC.
        /// </summary>
        public DateTime Time { get; }
        public int Delay { get; }
        public Touch Touch { get; }

        public Locked(VisualObject holder, DateTime time, int delay, Touch touch)
        {
            Holder = holder;
            Time = time;
            Delay = delay;
            Touch = touch;
        }
    }
}
