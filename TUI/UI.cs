using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TUI
{
    public static class UI<U>
    {
        #region Data

        public const int MaxUsers = 256;
        public static bool ShowGrid = false;
        public static Hook<DrawArgs> DrawHook = new Hook<DrawArgs>();
        public static Hook<SetXYWHArgs> SetXYWHHook = new Hook<SetXYWHArgs>();
        public static Hook<SetTopArgs> SetTopHook = new Hook<SetTopArgs>();
        public static Hook<EnabledArgs> EnabledHook = new Hook<EnabledArgs>();
        private static List<RootVisualObject> Child = new List<RootVisualObject>();
        public static Dictionary<U, UIUserSession<U>> Session = new Dictionary<U, UIUserSession<U>>();
        public static int SessionIndex = 0;

        #endregion

        #region Create

        public static RootVisualObject Create(string name, int x, int y, int width, int height, ITileCollection tileCollection = null)
        {
            RootVisualObject result = new RootVisualObject(name, x, y, width, height, tileCollection);
            lock (Child)
                Child.Add(result);
            return result;
        }

        #endregion
        #region Destroy

        public static void Destroy(RootVisualObject obj)
        {
            lock (Child)
                Child.Remove(obj);
        }

        #endregion
        #region Touched

        public static bool Touched(U user, Touch<U> touch)
        {
            UIUserSession<U> session;
            lock (Session)
            {
                if (Session[user] == null)
                {
                    if (touch.State != TouchState.Begin)
                        throw new Exception();

                    Session[user] = new UIUserSession<U>()
                    {
                        Enabled = true,
                        User = user,
                        //Index = Interlocked.Increment(ref SessionIndex)
                        Index = SessionIndex++
                    };
                }
                session = Session[user];
                touch.Session = session;
            }

            lock (session)
            {
                Touch<VisualObject, U> previous = session.PreviousTouch;
                if (touch.State == TouchState.Begin && previous != null
                        && (previous.State == TouchState.Begin || previous.State == TouchState.Moving))
                    throw new InvalidOperationException();
                if ((touch.State == TouchState.Moving || touch.State == TouchState.End)
                        && (previous == null || previous.State == TouchState.End))
                    throw new InvalidOperationException();

                if (touch.State == TouchState.Begin)
                {
                    session.BeginTouch = touch;
                    session.Enabled = true;
                }

                bool used = false;
                if (session.Enabled)
                    used = TouchedChild(touch);

                session.PreviousTouch = touch;

                return used;
            }
        }

        #endregion
        #region TouchedChild

        public static bool TouchedChild(Touch<VisualObject, U> touch)
        {
            lock (Child)
                for (int i = Child.Count - 1; i >= 0; i--)
                {
                    var o = Child[i];
                    int saveX = o.X, saveY = o.Y;
                    if (o.Enabled && o.Contains(touch))
                    {
                        touch.MoveBack(saveX, saveY);
                        if (o.Touched(touch))
                        {
                            if (SetTop(o))
                                PostSetTop(o);
                            return true;
                        }
                        touch.Move(saveX, saveY);
                    }
                }
            return false;
        }

        #endregion
        #region SetTop

        // TODO: Main.tile objects can't be set on top of fake objects
        public static bool SetTop(RootVisualObject o)
        {
            bool result;
            int index = Child.IndexOf(o);
            if (index > 0)
            {
                Child.Remove(o);
                Child.Insert(0, o);
                result = true;
            }
            else if (index == 0)
                result = false;
            else
                throw new InvalidOperationException("Trying to SetTop an object that isn't a child of current VisualDOM");

            // Let the fake provider actually become top
            if (result && o.TileCollection != null)
                UI.SetTopHook.Invoke(new SetTopArgs(o));
            
            return result;
        }

        #endregion
        #region PostSetTop

        public static void PostSetTop(RootVisualObject o)
        {
            // Should not apply if intersecting objects have different tile provider
            (bool intersects, bool needsApply) = ChildIntersectingOthers(o);
            if (intersects)
            {
                if (needsApply)
                    o.Apply(true);
                o.Draw();
            }
        }

        #endregion
        #region ChildInterSectingOthers

        public static (bool, bool) ChildIntersectingOthers(RootVisualObject o)
        {
            bool intersects = false;
            foreach (RootVisualObject child in Child)
                if (child != o && child.Enabled && o.Intersecting(child))
                {
                    intersects = true;
                    if (o.TileCollection == child.TileCollection)
                        return (true, true);
                }
            return (intersects, false);
        }

        #endregion
        #region Apply

        public static void Apply()
        {
            lock (Child)
                foreach (VisualObject child in Child)
                    if (child.Enabled)
                        child.Apply();
        }

        #endregion
        #region Draw

        public static void Draw(int x, int y, int width, int height, bool forcedSection)
        {
            DrawHook.Invoke(new DrawArgs(x, y, width, height, forcedSection));
        }

        #endregion
        #region Padding

        public static (int X, int Y, int Width, int Height) Padding(int X, int Y, int Width, int Height, PaddingConfig paddingData)
        {
            int x = paddingData.X;
            int y = paddingData.Y;
            int width = paddingData.Width;
            int height = paddingData.Height;
            Alignment alignment = paddingData.Alignment;
            if (alignment == Alignment.Up || alignment == Alignment.Center || alignment == Alignment.Down)
                x = x + Width / 2;
            else if (alignment == Alignment.UpRight || alignment == Alignment.Right || alignment == Alignment.DownRight)
                x = x + Width;
            if (alignment == Alignment.Left || alignment == Alignment.Center || alignment == Alignment.Right)
                y = y + Height / 2;
            else if (alignment == Alignment.DownRight || alignment == Alignment.Down || alignment == Alignment.DownRight)
                y = y + Height;
            if (width <= 0)
                width = Width + width - x;
            if (height <= 0)
                height = Height + height - y;
            return (x, y, width, height);
        }

        #endregion
        #region SaveTime

        public static void SaveTime<T>(T o, string name, string key = null)
            where T : Touchable<T>
        {

        }

        #endregion
        #region ShowTime

        public static void ShowTime<T>(T o, string name, string key = null)
            where T : Touchable<T>
        {

        }

        #endregion
    }
}
