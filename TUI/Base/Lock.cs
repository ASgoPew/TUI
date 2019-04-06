using System;

namespace TUI.Base
{
    public enum LockLevel
    {
        Self,
        Root
    }

    public enum LockType
    {
        Common,
        Personal
    }

    public class LockConfig
    {
        public const int DefaultDelay = 300;

        public LockLevel Level { get; set; }
        public LockType Type { get; set; }
        public int Delay { get; set; }

        public LockConfig(LockLevel level, LockType type, int delay = DefaultDelay)
        {
            Level = level;
            Type = type;
            Delay = delay;
        }

        public LockConfig(LockConfig config)
        {
            this.Level = config.Level;
            this.Type = config.Type;
            this.Delay = config.Delay;
        }
    }

    public class UILock
    {
        public object Locker { get; set; }
        public DateTime Time { get; set; }
        public int Delay { get; set; }
        public Touch Touch { get; set; }
        public bool Active { get; set; }

        public UILock(object locker, DateTime time, int delay, Touch touch)
        {
            Locker = locker;
            Time = time;
            Delay = delay;
            Touch = touch;
            Active = false;
        }
    }
}
