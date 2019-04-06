using System;
using System.Collections.Generic;
using System.Diagnostics;
using TUI.Base;
using TUI.Hooks;
using TUI.Hooks.Args;

namespace TUI
{
    public static class UI
    {
        #region Data

        public static int MaxUsers;
        public static bool ShowGrid = false;
        public static HookManager Hooks = new HookManager();
        public static List<RootVisualObject> Child = new List<RootVisualObject>();
        public static UIUserSession[] Session = new UIUserSession[MaxUsers];
        public static int SessionIndex = 0;

        #endregion

        #region Initialize

        public static void Initialize(int maxUsers = 256)
        {
            SessionIndex = 0;
            MaxUsers = maxUsers;
            Session = new UIUserSession[MaxUsers];

            Hooks.Initialize.Invoke(new InitializeArgs(maxUsers));
        }

        #endregion
        #region Deinitialize

        public static void Deinitialize()
        {
            Hooks.Deinitialize.Invoke(new EventArgs());
            Child.Clear();
        }

        #endregion
        #region Create

        public static RootVisualObject Create(string name, int x, int y, int width, int height, UITileProvider provider,
            UIConfiguration configuration = null, UIStyle style = null)
        {
            RootVisualObject result = new RootVisualObject(name, x, y, width, height, provider, configuration, style);
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
        #region InitializeUser

        public static void InitializeUser(int userIndex)
        {
            lock (Session)
                Session[userIndex] = new UIUserSession(userIndex);
        }

        #endregion
        #region RemoveUser

        public static void RemoveUser(int userIndex)
        {
            UIUserSession session = Session[userIndex];

            if (session.PreviousTouch != null && session.PreviousTouch.State != TouchState.End)
            {
                Touch simulatedEndTouch = session.PreviousTouch.SimulatedEndTouch();
                UI.Touched(userIndex, simulatedEndTouch);
            }

            Session[userIndex] = null;
        }

        #endregion
        #region Touched

        public static bool Touched(int userIndex, Touch touch)
        {
            UIUserSession session = Session[userIndex];
            touch.Session = session;

            lock (session)
            {
                Touch previous = session.PreviousTouch;
                if (touch.State == TouchState.Begin && previous != null
                        && (previous.State == TouchState.Begin || previous.State == TouchState.Moving))
                    throw new InvalidOperationException();
                if ((touch.State == TouchState.Moving || touch.State == TouchState.End)
                        && (previous == null || previous.State == TouchState.End))
                    throw new InvalidOperationException();
                if (touch.State == TouchState.Moving && touch.AbsoluteX == previous.AbsoluteX && touch.AbsoluteY == previous.AbsoluteY)
                    return session.Used;

                Stopwatch sw = Stopwatch.StartNew();

                if (touch.State == TouchState.Begin)
                {
                    session.Reset();
                    session.BeginTouch = touch;
                    session.TouchSessionIndex++;
                }

                bool used = false;
                if (session.Enabled)
                {
                    if (session.Acquired != null)
                        used = TouchedAcquired(touch);
                    else
                        used = TouchedChild(touch);
                }
                session.Used = session.Used || used;

                long elapsed = sw.ElapsedMilliseconds;
                Console.WriteLine($"{touch.X},{touch.Y} ({touch.State}): {elapsed}");

                session.Count++;
                session.PreviousTouch = touch;
                return session.Used;
            }
        }

        #endregion
        #region TouchedAcquired

        public static bool TouchedAcquired(Touch touch)
        {
            VisualObjectBase o = touch.Session.Acquired;
            (int saveX, int saveY) = o.AbsoluteXY();
            touch.MoveBack(saveX, saveY);
            if (o.Enabled && (touch.Intersecting(0, 0, o.Width, o.Height) || o.Configuration.UseOutsideTouches))
                if (o.Touched(touch))
                    return true;
            touch.Move(saveX, saveY);
            return false;
        }

        #endregion
        #region TouchedChild

        public static bool TouchedChild(Touch touch)
        {
            lock (Child)
                for (int i = Child.Count - 1; i >= 0; i--)
                {
                    RootVisualObject o = Child[i];
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
            if (result)
                UI.Hooks.SetTop.Invoke(new SetTopArgs(o));
            
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
                    if (o.Provider.Tile == child.Provider.Tile)
                        return (true, true);
                }
            return (intersects, false);
        }

        #endregion
        #region Update

        public static void Update()
        {
            lock (Child)
                foreach (VisualObjectBase child in Child)
                    if (child.Enabled)
                        child.Update();
        }

        #endregion
        #region Apply

        public static void Apply()
        {
            lock (Child)
                foreach (VisualObjectBase child in Child)
                    if (child.Enabled)
                        child.Apply(true);
        }

        #endregion
        #region Draw

        public static void Draw()
        {
            lock (Child)
                foreach (VisualObjectBase child in Child)
                    if (child.Enabled)
                        child.Draw();
        }

        #endregion
        #region DrawRect

        public static void DrawRect(int x, int y, int width, int height, bool forcedSection)
        {
            UI.Hooks.Draw.Invoke(new DrawArgs(x, y, width, height, forcedSection));
        }

        #endregion
        #region SaveTime

        public static void SaveTime<T>(T o, string name, string key = null)
            where T : VisualDOM
        {

        }

        #endregion
        #region ShowTime

        public static void ShowTime<T>(T o, string name, string key = null)
            where T : VisualDOM
        {

        }

        #endregion
    }
}
