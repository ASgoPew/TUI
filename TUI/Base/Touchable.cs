using System;
using System.Collections.Concurrent;
using TUI.Hooks.Args;

namespace TUI.Base
{
    public abstract class Touchable : VisualDOM
    {
        #region Data

        internal Locked Locked { get; set; }
        internal ConcurrentDictionary<int, Locked> PersonalLocked { get; set; } = new ConcurrentDictionary<int, Locked>();
        public Func<VisualObject, Touch, bool> Callback { get; set; }

        public bool Contains(Touch touch) => Contains(touch.X, touch.Y);

        #endregion

        #region Constructor

        public Touchable(int x, int y, int width, int height, UIConfiguration configuration = null, Func<VisualObject, Touch, bool> callback = null)
            : base(x, y, width, height, configuration)
        {
            Callback = callback;
        }

        #endregion
        #region Touched

        public virtual bool Touched(Touch touch)
        {
#if DEBUG
            if (!CalculateActive())
                throw new InvalidOperationException("Trying to call Touched on object that is not active");
#endif

            if (IsLocked(touch))
                return true;
            if (!CanTouch(touch))
                return false;

            bool used = TouchedChild(touch);
            if (!used && CanTouchThis(touch))
                used = TouchedThis(touch);

            return used;
        }

        #endregion
        #region IsLocked

        public virtual bool IsLocked(Touch touch)
        {
            // We must check both personal and common lock
            PersonalLocked.TryGetValue(touch.Session.UserIndex, out Locked personalLocked);
            return IsLocked(Locked, touch) || IsLocked(personalLocked, touch);
        }

        public bool IsLocked(Locked locked, Touch touch)
        {
            if (locked == null)
                return false;

            Lock holderLock = locked.Holder.Configuration.Lock;

            // Checking whether lock is still active
            if ((DateTime.UtcNow - locked.Time) > TimeSpan.FromMilliseconds(locked.Delay)
                && (!holderLock.DuringTouchSession || locked.Touch.TouchSessionIndex != locked.Touch.Session.TouchSessionIndex))
            {
                if (holderLock.Personal)
                    PersonalLocked.TryRemove(touch.Session.UserIndex, out _);
                else
                    Locked = null;
                return false;
            }

            // Immidiately blocking if user who set locked is different from current user
            // or if it is already new TouchSessionIndex since locked set
            bool userInitializedLock = locked.Touch.Session.UserIndex == touch.Session.UserIndex;
            bool lockingTouchSession = touch.TouchSessionIndex == locked.Touch.TouchSessionIndex;
            if (!userInitializedLock || !lockingTouchSession)
            {
                touch.Session.Enabled = false;
                return true;
            }

            // Here lock exists, active for current user and TouchSessionIndex is the same as when lock was activated.
            if (holderLock.AllowThisTouchSession)
                return false;
            else
            {
                touch.Session.Enabled = false;
                return true;
            }
        }

        #endregion
        #region CanTouch

        public virtual bool CanTouch(Touch touch)
        {
            VisualObject @this = this as VisualObject;
            CanTouchArgs args = new CanTouchArgs(@this, touch);
            TUI.Hooks.CanTouch.Invoke(args);
            return args.CanTouch && Configuration.CustomCanTouch?.Invoke(@this, touch) != false;
        }

        #endregion
        #region TouchedChild

        public virtual bool TouchedChild(Touch touch)
        {
            lock (Child)
                foreach (VisualObject child in ChildrenFromTop)
                {
                    int saveX = child.X, saveY = child.Y;
                    if (child.Active && child.Contains(touch))
                    {
                        touch.MoveBack(saveX, saveY);
                        if (child.Touched(touch))
                        {
                            if (Configuration.Ordered && child.Orderable && SetTop(child))
                                PostSetTop(child);
                            return true;
                        }
                        touch.Move(saveX, saveY);
                    }
                }
            return false;
        }

        #endregion
        #region PostSetTop

        public virtual void PostSetTop(VisualObject o) { }

        #endregion
        #region CanTouchThis

        public virtual bool CanTouchThis(Touch touch) =>
            (touch.State == TouchState.Begin && Configuration.UseBegin
                || touch.State == TouchState.Moving && Configuration.UseMoving
                || touch.State == TouchState.End && Configuration.UseEnd)
            && (touch.State == TouchState.Begin || !Configuration.BeginRequire || touch.Session.BeginObject == this);

        #endregion
        #region TouchedThis

        public virtual bool TouchedThis(Touch touch)
        {
            VisualObject @this = this as VisualObject;
            if (touch.State == TouchState.Begin)
                touch.Session.BeginObject = @this;

            touch.Object = @this;

            TrySetLock(touch);

            bool used = Invoke(touch);

            if (Configuration.SessionAcquire && used)
                touch.Session.Acquired = @this;

            return used;
        }

        #endregion
        #region TrySetLock

        public void TrySetLock(Touch touch)
        {
            VisualObject @this = this as VisualObject;
            // You can't lock the same object twice per touch session
            if (Configuration.Lock != null && !touch.Session.LockedObjects.Contains(@this))
            {
                Lock lockConfig = Configuration.Lock;
                int userIndex = touch.Session.UserIndex;
                VisualObject target = lockConfig.Level == LockLevel.Self ? @this : Root;

                // We are going to set lock only if target doesn't have an existing one
                lock (target.PersonalLocked)
                    if ((lockConfig.Personal && !target.PersonalLocked.ContainsKey(userIndex))
                        || (!lockConfig.Personal && target.Locked == null))
                        {
                            Locked locked = new Locked(@this, DateTime.UtcNow, lockConfig.Delay, touch);
                            touch.Session.LockedObjects.Add(@this);
                            if (lockConfig.Personal)
                                target.PersonalLocked[userIndex] = locked;
                            else
                                target.Locked = locked;
                        }
            }
        }

        #endregion
        #region Invoke

        public virtual bool Invoke(Touch touch) =>
            Callback?.Invoke(this as VisualObject, touch) ?? true;

        #endregion
    }
}
