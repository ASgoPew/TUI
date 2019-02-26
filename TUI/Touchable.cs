using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public class Touchable<T> : VisualDOM<T>
        where T : Touchable<T>
    {
        #region Data

        public UILock<T> Lock { get; set; }
        public UILock<T>[] PersonalLock { get; set; } = new UILock<T>[UI.MaxUsers];
        public Func<T, Touch<T>, bool> Callback { get; set; }
        public Func<T, Touch<T>, bool> CustomCanTouch { get; set; }

        public virtual string Name => GetType().Name;
        public bool Contains(Touch<T> touch) => Contains(touch.X, touch.Y);

        #endregion

        #region Initialize

        public Touchable(int x, int y, int width, int height, UIConfiguration configuration, Func<T, Touch<T>, bool> callback)
            : base(x, y, width, height, configuration)
        {
            Callback = callback;
        }

        #endregion
        #region CanTouch

        public virtual bool CanTouch(Touch<T> touch)
        {
            return (Configuration.Permission == null || touch.User.HasPermission(Configuration.Permission))
                && CustomCanTouch?.Invoke((T)this, touch) != false;
        }

        #endregion
        #region Touched

        public virtual bool Touched(Touch<T> touch)
        {
            if (!Active())
                throw new InvalidOperationException("Trying to call Touched on object that is not active");

            if (IsLocked(touch))
                return true;
            if (!CanTouch(touch))
                return false;

            UI.SaveTime((T)this, "Touched");
            bool used = TouchedChild(touch);
            UI.SaveTime((T)this, "Touched", "Child");
            if (!used && CanTouchThis(touch))
                used = TouchedThis(touch);
            UI.ShowTime((T)this, "Touched", "This");

            if (Lock != null && touch.State == TouchState.End)
                Lock.Active = true;

            return used;
        }

        #endregion
        #region IsLocked

        public virtual bool IsLocked(Touch<T> touch)
        {
            if (Configuration.Lock == null)
                return false;

            UILock<T> uilock = Configuration.Lock.Type == LockType.Common ? Lock : PersonalLock[touch.User.Index];
            if (uilock != null && (DateTime.Now - uilock.Time) > TimeSpan.FromMilliseconds(uilock.Delay))
            {
                if (Configuration.Lock.Type == LockType.Common)
                    Lock = null;
                else
                    PersonalLock[touch.User.Index] = null;
                return false;
            }
            if (uilock != null &&
                (uilock.Active
                || touch.State == TouchState.Begin
                || touch.Session.Index != uilock.Touch.Session.Index))
            {
                touch.Session.Enabled = false;
                return true;
            }

            return false;
        }

        #endregion
        #region TouchedChild

        public virtual bool TouchedChild(Touch<T> touch)
        {
            for (int i = Child.Count - 1; i >= 0; i--)
            {
                var o = Child[i];
                int saveX = o.X, saveY = o.Y;
                if (o.Enabled && o.Contains(touch))
                {
                    touch.MoveBack(saveX, saveY);
                    if (o.Touched(touch))
                    {
                        // TODO: Main.tile objects can't be set on top of fake objects
                        if (Configuration.Ordered && SetTop(o))
                        {
                            //if (o.provider_set_top)
                                //o.provider_set_top();
                            // TODO: should not apply if intersecting object with different tile provider
                            if (ChildIntersectingOthers(o))
                                PostSetTop();
                        }
                        return true;
                    }
                    touch.Move(saveX, saveY);
                }
            }
            return false;
        }

        #endregion
        #region ChildInterSectingOthers

        public virtual bool ChildIntersectingOthers(T o)
        {
            foreach (T child in Child)
                if (child != o && child.Enabled && o.Intersecting(child))
                    return true;
            return false;
        }

        #endregion
        #region PostSetTop

        public virtual void PostSetTop() { }

        #endregion
        #region TouchedThis

        public virtual bool TouchedThis(Touch<T> touch)
        {
            if (touch.State == TouchState.Begin)
                touch.Session.BeginObject = (T)this;

            if (Configuration.Lock != null)
            {
                UILock<T> _lock = new UILock<T>(this, DateTime.Now, Configuration.Lock.Delay, touch);
                if (Configuration.Lock.Level == LockLevel.Self)
                    Lock = _lock;
                else if (Configuration.Lock.Level == LockLevel.Root)
                    touch.Root.Lock = _lock;
            }

            bool used = true;

            if (Callback != null)
            {
                UI.SaveTime((T)this, "invoke");
                used = Callback((T)this, touch);
                UI.ShowTime((T)this, "invoke", "action");
            }

            return used;
        }

        #endregion
        #region CanTouchThis

        public virtual bool CanTouchThis(Touch<T> touch) =>
            (touch.State == TouchState.Begin && Configuration.UseBegin
                || touch.State == TouchState.Moving && Configuration.UseMoving
                || touch.State == TouchState.End && Configuration.UseEnd)
            && (touch.State == TouchState.Begin || !Configuration.BeginRequire || touch.Session.BeginObject.Equals(this));

        #endregion
    }
}
