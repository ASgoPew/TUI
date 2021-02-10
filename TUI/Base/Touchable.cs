using System;
using System.Collections.Concurrent;
using TerrariaUI.Hooks.Args;

namespace TerrariaUI.Base
{
    public abstract class Touchable : VisualDOM
    {
        #region Data

        internal Locked Locked { get; set; }
        internal ConcurrentDictionary<int, Locked> PersonalLocked { get; set; } = new ConcurrentDictionary<int, Locked>();
        /// <summary>
        /// Function to call on touching this object with the grand design.
        /// </summary>
        public Action<VisualObject, Touch> Callback { get; set; }

        public bool Contains(Touch touch) => Contains(touch.X, touch.Y);

        #endregion

        #region Constructor

        public Touchable(int x, int y, int width, int height, UIConfiguration configuration = null,
                Action<VisualObject, Touch> callback = null)
            : base(x, y, width, height, configuration)
        {
            Callback = callback;
        }

        #endregion
        #region Touched

        /// <summary>
        /// This function is called when touch falls into the coordinates of this node.
        /// </summary>
        /// <param name="touch"></param>
        /// <returns></returns>
        internal bool Touched(Touch touch)
        {
            if (!CalculateActive())
                return false;

            if (IsLocked(touch))
                return true;
            if (!CanTouch(touch))
                return !touch.Session.Enabled;

            bool used = TouchedChild(touch);
            if (!used && CanTouchThis(touch))
            {
                TouchedThis(touch);
                used = true;
            }

            return used;
        }

        #endregion
        #region IsLocked

        private bool IsLocked(Touch touch)
        {
            // We must check both personal and common lock
            PersonalLocked.TryGetValue(touch.Session.PlayerIndex, out Locked personalLocked);
            return IsLocked(Locked, touch) || IsLocked(personalLocked, touch);
        }

        private bool IsLocked(Locked locked, Touch touch)
        {
            if (locked == null)
                return false;

            Lock holderLock = locked.Holder.Configuration.Lock;

            // Checking whether lock is still active
            if ((DateTime.UtcNow - locked.Time) > TimeSpan.FromMilliseconds(locked.Delay)
                && (!holderLock.DuringTouchSession || locked.Touch.TouchSessionIndex != locked.Touch.Session.TouchSessionIndex))
            {
                if (holderLock.Personal)
                    PersonalLocked.TryRemove(touch.Session.PlayerIndex, out _);
                else
                    Locked = null;
                return false;
            }

            // Immidiately blocking if user who set locked is different from current user
            // or if it is already new TouchSessionIndex since locked set
            bool userInitializedLock = locked.Touch.Session.PlayerIndex == touch.Session.PlayerIndex;
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

        /// <summary>
        /// Checks if specified touch can press this object or one of child objects in sub-tree.
        /// </summary>
        /// <param name="touch">Touch to check</param>
        public bool CanTouch(Touch touch)
        {
            bool result = false;
            try
            {
                result = CanTouchNative(touch) && Configuration.Custom.CanTouch?.Invoke(this as VisualObject, touch) != false;
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
            return result;
        }

        #endregion
        #region CanTouchNative

        protected virtual bool CanTouchNative(Touch touch)
        {
            VisualObject @this = this as VisualObject;
            CanTouchArgs args = new CanTouchArgs(@this, touch);
            TUI.Hooks.CanTouch.Invoke(args);
            lock (Locker)
                return Loaded && !Disposed && args.CanTouch;
        }

        #endregion
        #region TouchedChild

        private bool TouchedChild(Touch touch)
        {
            foreach (VisualObject child in ChildrenFromTop)
            {
                int saveX = child.X, saveY = child.Y;
                if (child.Active && child.Contains(touch))
                {
                    touch.Move(-saveX, -saveY);
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

        private void PostSetTop(VisualObject o)
        {
            try
            {
                PostSetTopNative(o);
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
        }

        #endregion
        #region PostSetTopNative

        /// <summary>
        /// Overridable function that is called when object comes on top of the layer.
        /// </summary>
        /// <param name="o"></param>
        protected virtual void PostSetTopNative(VisualObject o) { }

        #endregion
        #region CanTouchThis

        /// <summary>
        /// Checks if specified touch can press this object. Not to be confused with <see cref="CanTouch(Touch)"/>.
        /// </summary>
        /// <param name="touch"></param>
        /// <returns></returns>
        public virtual bool CanTouchThis(Touch touch) =>
            (touch.State == TouchState.Begin && Configuration.UseBegin
                || touch.State == TouchState.Moving && Configuration.UseMoving
                || touch.State == TouchState.End && Configuration.UseEnd)
            && (touch.State == TouchState.Begin || !Configuration.BeginRequire || touch.Session.BeginTouch.Object == this);

        #endregion
        #region TouchedThis

        private void TouchedThis(Touch touch)
        {
            VisualObject @this = this as VisualObject;
            touch.Object = @this;

            TrySetLock(touch);

            try
            {
                Invoke(touch);
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }

            if (Configuration.SessionAcquire)
                touch.Session.Acquired = @this;
        }

        #endregion
        #region TrySetLock

        /// <summary>
        /// Tries to lock this node with specified touch object according to node locking configuration.
        /// </summary>
        /// <param name="touch"></param>
        internal void TrySetLock(Touch touch)
        {
            VisualObject @this = this as VisualObject;
            // You can't lock the same object twice per touch session
            if (Configuration.Lock != null && !touch.Session.LockedObjects.Contains(@this))
            {
                Lock lockConfig = Configuration.Lock;
                int userIndex = touch.Session.PlayerIndex;
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

        /// <summary>
        /// Overridable function which is called when touch satisfies the conditions of pressing this object.
        /// Invokes Callback function by default.
        /// </summary>
        /// <param name="touch"></param>
        /// <returns></returns>
        public virtual void Invoke(Touch touch) =>
            Callback?.Invoke(this as VisualObject, touch);

        #endregion
    }
}
