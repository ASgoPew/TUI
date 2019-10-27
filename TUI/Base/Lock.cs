using System;

namespace TerrariaUI.Base
{
    public enum LockLevel
    {
        Self,
        Root,
        //UI
    }

    /// <summary>
    /// Lock configuration for VisualObject.
    /// </summary>
    public class Lock
    {
        /// <summary>
        /// Whether to lock this object for touching or the whole user interface tree.
        /// </summary>
        public LockLevel Level { get; set; }
        /// <summary>
        /// If false then lock is common for all players so that player2 won't
        /// be able to touch the object for some time after player1 touched it.
        /// </summary>
        public bool Personal { get; set; }
        /// <summary>
        /// Lock delay in milliseconds.
        /// </summary>
        public int Delay { get; set; }
        /// <summary>
        /// If not then first touch would lock the object so every next touch during touch session won't pass.
        /// </summary>
        public bool AllowThisTouchSession { get; set; }
        /// <summary>
        /// Whether lock has to be active for the whole touch session or not.
        /// </summary>
        public bool DuringTouchSession { get; set; }

        /// <summary>
        /// Lock configuration for VisualObject.
        /// </summary>
        /// <param name="level">Whether to lock this object for touching or the whole user interface tree.</param>
        /// <param name="personal">If false then lock is common for all players so that player2 won't
        /// be able to touch the object for some time after player1 touched it.</param>
        /// <param name="delay">Lock delay in milliseconds.</param>
        /// <param name="allowThisTouchSession">If not then first touch would lock the object so every next touch during touch session won't pass.</param>
        /// <param name="duringTouchSession">Whether lock has to be active for the whole touch session or not.</param>
        public Lock(LockLevel level = LockLevel.Self, bool personal = true, int delay = -1, bool allowThisTouchSession = true, bool duringTouchSession = false)
        {
            Level = level;
            Personal = personal;
            Delay = delay < 0 ? UIDefault.LockDelay : delay;
            AllowThisTouchSession = allowThisTouchSession;
            DuringTouchSession = duringTouchSession;
        }

        /// <summary>
        /// Lock configuration for VisualObject.
        /// </summary>
        public Lock(Lock config)
        {
            this.Level = config.Level;
            this.Personal = config.Personal;
            this.Delay = config.Delay;
        }
    }

    /// <summary>
    /// Locking event class.
    /// </summary>
    internal class Locked
    {
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
