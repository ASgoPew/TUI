using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using TUI.Base;
using TUI.Base.Style;
using TUI.Hooks;
using TUI.Hooks.Args;
using TUI.Widgets;

namespace TUI
{
    public static class TUI
    {
        #region Data

        public static int MaxUsers;
        public static bool ShowGrid = false;
        public static HookManager Hooks = new HookManager();
        public static UserSession[] Session = new UserSession[MaxUsers];
        public static int MaxHoldTouchMilliseconds = 30000;
        public const short FakeSignSTileHeader = 29728;
        private static List<RootVisualObject> Child = new List<RootVisualObject>();
        private static Timer Timer;
        //private static object Locker = new object();
        //internal static List<VisualObject> ObjectsToLoad = new List<VisualObject>();
        public static bool Active { get; set; } = false;

        #endregion

        #region Initialize

        public static void Initialize(int maxUsers = 256)
        {
            MaxUsers = maxUsers;
            Session = new UserSession[MaxUsers];
            Timer = new Timer(5000) { AutoReset = true };
            Timer.Elapsed += OnTimerElapsed;
        }

        #endregion
        #region Load

        public static void Load()
        {
            Timer.Start();

            // Locking for Child and Active
            lock (Child)
            {
                Active = true;

                foreach (VisualObject Child in Child)
                    Child.Load();
            }

            Hooks.Load.Invoke(new LoadArgs(MaxUsers));
        }

        #endregion
        #region Dispose

        public static void Dispose()
        {
            Hooks.Dispose.Invoke(new EventArgs());

            // Locking for Child and Active
            lock (Child)
            {
                Active = false;

                foreach (RootVisualObject child in Child)
                    child.Dispose();
                Child.Clear();
            }

            Timer.Stop();
        }

        #endregion
        #region TryToLoadChild

        internal static void TryToLoadChild(VisualObject node, VisualObject child)
        {
            // Locking for Child and Active
            lock (Child)
            {
                if (Child.Contains(node.GetRoot() as RootVisualObject) && Active)
                    child.Load();
            }
        }

        #endregion
        #region OnTimerElapsed

        public static void OnTimerElapsed(object sender, ElapsedEventArgs args)
        {
            for (int i = 0; i < Session.Length; i++)
            {
                UserSession session = Session[i];
                if (session != null)
                {
                    Touch previous = session.PreviousTouch;
                    Touch begin = session.BeginTouch;
                    if (previous != null && begin != null && previous.State != TouchState.End && (previous.Time - begin.Time).TotalMilliseconds >= MaxHoldTouchMilliseconds)
                        Hooks.TouchCancel.Invoke(new TouchCancelArgs(i, session, previous));
                }
            }
        }

        #endregion

        #region Create

        internal static RootVisualObject Create(RootVisualObject root)
        {
            if (!(root.Provider is MainTileProvider)
                    && (root.Width > root.Provider.Width || root.Height > root.Provider.Height))
                throw new ArgumentException("Provider size is less than RootVisualObject size: " + root.FullName);

            // Locking for Child and Active
            lock (Child)
            {
                Child.Add(root);

                if (Active)
                    root.Load();
            }
            return root;
        }

        public static RootVisualObject CreateRoot(string name, int x, int y, int width, int height,
                UIConfiguration configuration = null, ContainerStyle style = null, object provider = null) =>
            Create(new RootVisualObject(name, x, y, width, height, configuration, style, provider));

        public static Panel CreatePanel(string name, int x, int y, int width, int height, PanelDrag drag, PanelResize resize,
                UIConfiguration configuration = null, ContainerStyle style = null, object provider = null) =>
            Create(new Panel(name, x, y, width, height, drag, resize, configuration, style, provider)) as Panel;

        public static Panel CreatePanel(string name, int x, int y, int width, int height,
                UIConfiguration configuration = null, ContainerStyle style = null, object provider = null) =>
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
                TUI.Touched(userIndex, simulatedEndTouch);
            }

            lock (Child)
                foreach (RootVisualObject child in Child)
                    child.Players.Remove(userIndex);

            Session[userIndex] = null;
        }

        #endregion
        #region GetRoots

        public static List<RootVisualObject> GetRoots()
        {
            List<RootVisualObject> result = new List<RootVisualObject>();
            lock (Child)
                foreach (RootVisualObject child in Child)
                    result.Add(child);
            return result;
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

#if DEBUG
                Stopwatch sw = Stopwatch.StartNew();
#endif

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

#if DEBUG
                long elapsed = sw.ElapsedMilliseconds;
                sw.Stop();
                Console.WriteLine($"Touch ({touch.X},{touch.Y}): {touch.State} ({touch.Object}); elapsed: {elapsed}");
#endif

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

            if (o.Active && inside)
                insideUI = true;
            if (o.Active && (inside || o.Configuration.UseOutsideTouches))
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
                    if (o.Active && o.Contains(touch) && o.Players.Contains(touch.Session.UserIndex))
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
                TUI.Hooks.SetTop.Invoke(new SetTopArgs(o));
            
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

        public static (bool intersects, bool needsApply) ChildIntersectingOthers(RootVisualObject o)
        {
            bool intersects = false;
            foreach (RootVisualObject child in Child)
                if (child != o && child.Active && o.Intersecting(child))
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
                foreach (RootVisualObject child in Child)
                    if (child.Enabled)
                        child.Update();
        }

        #endregion
        #region Apply

        public static void Apply()
        {
            lock (Child)
                foreach (RootVisualObject child in Child)
                    if (child.Active)
                        child.Apply();
        }

        #endregion
        #region Draw

        public static void Draw()
        {
            lock (Child)
                foreach (RootVisualObject child in Child)
                    if (child.Active)
                        child.Draw();
        }

        #endregion
        #region DrawRect

        public static void DrawRect(VisualObject node, int x, int y, int width, int height, bool forcedSection, int userIndex = -1, int exceptUserIndex = -1, bool frame = true)
        {
            TUI.Hooks.Draw.Invoke(new DrawArgs(node, x, y, width, height, forcedSection, userIndex, exceptUserIndex, frame));
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
        #region DBGet

        public static string DBGet(string key)
        {
            DatabaseArgs args = new DatabaseArgs(DatabaseActionType.Get, key);
            Hooks.Database.Invoke(args);
            return args.Value;
        }

        #endregion
        #region DBSet

        public static void DBSet(string key, string value) =>
            Hooks.Database.Invoke(new DatabaseArgs(DatabaseActionType.Set, key, value));

        #endregion
        #region DBRemove

        public static void DBRemove(string key) =>
            Hooks.Database.Invoke(new DatabaseArgs(DatabaseActionType.Remove, key));

        #endregion
    }
}
