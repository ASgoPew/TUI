using System;
using System.Collections.Generic;
using System.Diagnostics;
using TUI.Base;
using TUI.Base.Style;
using TUI.Hooks;
using TUI.Hooks.Args;
using TUI.Widgets;

namespace TUI
{
    public static class UI
    {
        #region Data

        public static int MaxUsers;
        public static bool ShowGrid = false;
        public static HookManager Hooks = new HookManager();
        public static List<RootVisualObject> Child = new List<RootVisualObject>();
        public static UserSession[] Session = new UserSession[MaxUsers];
        public static int SessionIndex = 0;

        #endregion

        #region Initialize

        public static void Initialize(int maxUsers = 256)
        {
            SessionIndex = 0;
            MaxUsers = maxUsers;
            Session = new UserSession[MaxUsers];

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

        internal static RootVisualObject Create(RootVisualObject root)
        {
            lock (Child)
                Child.Add(root);
            return root;
        }

        public static RootVisualObject CreateRoot(string name, int x, int y, int width, int height,
                UIConfiguration configuration = null, UIStyle style = null, object provider = null) =>
            Create(new RootVisualObject(name, x, y, width, height, configuration, style, provider));

        public static Panel CreatePanel(string name, int x, int y, int width, int height, PanelDrag drag, PanelResize resize,
                UIConfiguration configuration = null, UIStyle style = null, object provider = null) =>
            Create(new Panel(name, x, y, width, height, drag, resize, configuration, style, provider)) as Panel;

        public static Panel CreatePanel(string name, int x, int y, int width, int height,
                UIConfiguration configuration = null, UIStyle style = null, object provider = null) =>
            Create(new Panel(name, x, y, width, height, configuration, style, provider)) as Panel;

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
                Session[userIndex] = new UserSession(userIndex);
        }

        #endregion
        #region RemoveUser

        public static void RemoveUser(int userIndex)
        {
            UserSession session = Session[userIndex];

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
            UserSession session = Session[userIndex];
            touch.SetSession(session);

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

                Console.WriteLine($"{touch.X},{touch.Y} {touch.State}");

                Stopwatch sw = Stopwatch.StartNew();

                if (touch.State == TouchState.Begin)
                {
                    session.Reset();
                    session.BeginTouch = touch;
                }

                bool insideUI = false;
                bool used = false;
                if (session.Enabled)
                {
                    if (session.Acquired != null)
                        used = TouchedAcquired(touch, ref insideUI);
                    else
                        used = TouchedChild(touch, ref insideUI);
                }
                session.Used = session.Used || used;
                touch.InsideUI = insideUI;

                if (touch.State == TouchState.End)
                {
                    session.TouchSessionIndex++;
                    session.EndTouchHandled = touch.InsideUI || session.BeginTouch.InsideUI;
                }

                long elapsed = sw.ElapsedMilliseconds;
                Console.WriteLine("Elapsed: " + elapsed);

                session.Count++;
                session.PreviousTouch = touch;

                return session.Used;
            }
        }

        #endregion
        #region TouchedAcquired

        public static bool TouchedAcquired(Touch touch, ref bool insideUI)
        {
            VisualObject o = touch.Session.Acquired;
            (int saveX, int saveY) = o.AbsoluteXY();

            touch.MoveBack(saveX, saveY);
            bool inside = touch.Intersecting(0, 0, o.Width, o.Height);

            if (o.Enabled && inside)
                insideUI = true;
            if (o.Enabled && (inside || o.Configuration.UseOutsideTouches))
                if (o.Touched(touch))
                    return true;

            touch.Move(saveX, saveY);
            return false;
        }

        #endregion
        #region TouchedChild

        public static bool TouchedChild(Touch touch, ref bool insideUI)
        {
            lock (Child)
                for (int i = Child.Count - 1; i >= 0; i--)
                {
                    RootVisualObject o = Child[i];
                    int saveX = o.X, saveY = o.Y;
                    if (o.Enabled && o.Contains(touch))
                    {
                        insideUI = true;
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
        #region EndTouchHandled

        public static bool EndTouchHandled(int userIndex) =>
            Session[userIndex]?.EndTouchHandled ?? false;

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

            // Let a custom provider actually become top
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
                    o.Apply();
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
                foreach (VisualObject child in Child)
                    if (child.Enabled)
                        child.Update();
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

        public static void Draw()
        {
            lock (Child)
                foreach (VisualObject child in Child)
                    if (child.Enabled)
                        child.Draw();
        }

        #endregion
        #region DrawRect

        public static void DrawRect(int x, int y, int width, int height, bool forcedSection, int userIndex = -1, int exceptUserIndex = -1, bool frame = true)
        {
            UI.Hooks.Draw.Invoke(new DrawArgs(x, y, width, height, forcedSection, userIndex, exceptUserIndex, frame));
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
