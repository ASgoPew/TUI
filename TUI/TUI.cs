using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using TerrariaUI.Base;
using TerrariaUI.Hooks;
using TerrariaUI.Hooks.Args;

namespace TerrariaUI
{
    /// <summary>
    /// Main user interface class.
    /// </summary>
    public static class TUI
    {
        #region Data

        public static int MaxPlayers;
        public static int MaxTilesX;
        public static int MaxTilesY;
        public static HookManager Hooks = new HookManager();
        public static UserSession[] Session = new UserSession[MaxPlayers];
        public static int MaxHoldTouchMilliseconds = 30000;
        public const short FakeSignSTileHeader = 29728;
        private static List<RootVisualObject> Child = new List<RootVisualObject>();
        private static Timer Timer;
        public static bool Active { get; set; } = false;

        #endregion

        #region Initialize

        public static void Initialize(int maxPlayers = 256, int maxTilesX = 8400, int maxTilesY = 2400)
        {
            MaxPlayers = maxPlayers;
            MaxTilesX = maxTilesX;
            MaxTilesY = maxTilesY;
            Session = new UserSession[MaxPlayers];
            Timer = new Timer(5000) { AutoReset = true };
            Timer.Elapsed += OnTimerElapsed;
        }

        #endregion
        #region Load

        public static void Load()
        {
            // Locking for Child and Active
            lock (Child)
            {
                Active = true;

                foreach (VisualObject child in Child)
                    child.Load();
            }

            Timer.Start();

            Hooks.Load.Invoke(new LoadArgs(MaxPlayers));
        }

        #endregion
        #region Dispose

        public static void Dispose()
        {
            Timer.Stop();

            // Locking for Child and Active
            lock (Child)
            {
                Active = false;

                foreach (RootVisualObject child in Child)
                    child.Dispose();
                Child.Clear();
            }

            Hooks.Dispose.Invoke(new EventArgs());
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

        /// <summary>
        /// Create user interface tree and add it to TUI.
        /// </summary>
        /// <param name="root">Root of user interface tree.
        /// <para></para>
        /// Consider using Panel widget for ability to automatically save position and size.</param>
        public static T Create<T>(T root)
            where T : RootVisualObject
        {
            if (!(root.Provider is MainTileProvider)
                    && (root.Width > root.Provider.Width || root.Height > root.Provider.Height))
                throw new ArgumentException("Provider size is less than RootVisualObject size: " + root.FullName);

            // Locking for Child and Active
            lock (Child)
            {
                if (Child.Count(r => r.Name == root.Name) > 0)
                    throw new ArgumentException($"TUI.Create: name {root.Name} is already taken.");

                int index = Child.Count;
                while (index > 0 && Child[index - 1].Layer > root.Layer)
                    index--;
                Child.Insert(index, root);

                if (Active)
                {
                    root.Load();
                    root.Update().Apply().Draw();
                }
            }
            return root;
        }

        #endregion
        #region Destroy

        public static void Destroy(RootVisualObject obj)
        {
            lock (Child)
            {
                if (Active)
                    obj.Disable(true);
                Child.Remove(obj);
                obj.Dispose();
            }
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
        #region InitializePlayer

        public static void InitializePlayer(int playerIndex)
        {
            lock (Session)
                Session[playerIndex] = new UserSession(playerIndex);
        }

        #endregion
        #region RemovePlayer

        public static void RemovePlayer(int playerIndex)
        {
            UserSession session = Session[playerIndex];
            if (session == null)
                return;

            if (session.PreviousTouch != null && session.PreviousTouch.State != TouchState.End)
            {
                Touch simulatedEndTouch = session.PreviousTouch.SimulatedEndTouch();
                Touched(playerIndex, simulatedEndTouch);
            }

            lock (Child)
                foreach (RootVisualObject child in Child)
                    child.Players.Remove(playerIndex);

            Session[playerIndex] = null;
        }

        #endregion

        #region Touched

        public static bool Touched(int playerIndex, Touch touch)
        {
            UserSession session = Session[playerIndex];
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

            touch.Move(-saveX, -saveY);
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
                    if (o.Active && o.Contains(touch) && o.Players.Contains(touch.Session.PlayerIndex))
                    {
                        insideUI = true;
                        touch.Move(-saveX, -saveY);
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

        public static bool SetTop(RootVisualObject root)
        {
            lock (Child)
            {
                int previousIndex = Child.IndexOf(root);
                int index = previousIndex;
                if (index < 0)
                    throw new InvalidOperationException("Trying to SetTop an object that isn't a child of current VisualDOM");
                int count = Child.Count;
                index++;
                while (index < count && Child[index].Layer <= root.Layer)
                    index++;

                if (index == previousIndex + 1)
                    return false;

                Child.Remove(root);
                Child.Insert(index - 1, root);

                if (!root.UsesDefaultMainProvider)
                    root.Provider.SetTop();

                return true;
            }
        }

        #endregion
        #region PostSetTop

        public static void PostSetTop(RootVisualObject child)
        {
            // Should not apply if intersecting objects have different tile provider
            (bool intersects, bool needsApply) = ChildIntersectingOthers(child);
            if (intersects)
            {
                if (needsApply)
                    child.Apply();
                child.Draw();
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
                    dynamic provider1 = o.Provider.Tile;
                    dynamic provider2 = child.Provider.Tile;
                    if (provider1.GetType() == provider2.GetType() && provider1 == provider2)
                        return (true, true);
                }
            return (intersects, false);
        }

        #endregion
        #region TrySetLockForObject

        public static void TrySetLockForObject(VisualObject node, Touch touch) =>
            node.TrySetLock(touch);

        #endregion

        #region Update

        public static void Update()
        {
            lock (Child)
                foreach (RootVisualObject child in Child)
                    try
                    {
                        if (child.Enabled)
                            child.Update();
                    }
                    catch (Exception e)
                    {
                        HandleException(child, e);
                    }
        }

        #endregion
        #region Apply

        public static void Apply()
        {
            lock (Child)
                foreach (RootVisualObject child in Child)
                    try
                    {
                        if (child.Active)
                            child.Apply();
                    }
                    catch (Exception e)
                    {
                        HandleException(child, e);
                    }
        }

        #endregion
        #region Draw

        public static void Draw()
        {
            lock (Child)
                foreach (RootVisualObject child in Child)
                    try
                    {
                        if (child.Active)
                            child.Draw();
                    }
                    catch (Exception e)
                    {
                        HandleException(child, e);
                    }
        }

        #endregion
        #region DrawObject

        internal static void DrawObject(VisualObject node, int x, int y, int width, int height,
            bool drawWithSection, int playerIndex = -1, int exceptPlayerIndex = -1, bool frameSection = true,
            bool toEveryone = false)
        {
            Hooks.DrawObject.Invoke(new DrawObjectArgs(node, x, y, width, height, drawWithSection,
                playerIndex, exceptPlayerIndex, frameSection, toEveryone));
        }

        #endregion
        #region DrawRectangle

        public static void DrawRectangle(int x, int y, int width, int height, bool drawWithSection,
            int playerIndex = -1, int exceptPlayerIndex = -1, bool frameSection = true)
        {
            Hooks.DrawRectangle.Invoke(new DrawRectangleArgs(x, y, width, height, drawWithSection,
                playerIndex, exceptPlayerIndex, frameSection));
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

        public static byte[] DBGet(string key)
        {
            DatabaseArgs args = new DatabaseArgs(DatabaseActionType.Get, key);
            Hooks.Database.Invoke(args);
            return args.Data;
        }

        #endregion
        #region DBSet

        public static void DBSet(string key, byte[] data) =>
            Hooks.Database.Invoke(new DatabaseArgs(DatabaseActionType.Set, key, data));

        #endregion
        #region DBRemove

        public static void DBRemove(string key) =>
            Hooks.Database.Invoke(new DatabaseArgs(DatabaseActionType.Remove, key));

        #endregion
        #region UDBGet

        public static byte[] UDBGet(int user, string key)
        {
            DatabaseArgs args = new DatabaseArgs(DatabaseActionType.Get, key, null, user);
            Hooks.Database.Invoke(args);
            return args.Data;
        }

        #endregion
        #region UDBSet

        public static void UDBSet(int user, string key, byte[] data) =>
            Hooks.Database.Invoke(new DatabaseArgs(DatabaseActionType.Set, key, data, user));

        #endregion
        #region UDBRemove

        public static void UDBRemove(int user, string key) =>
            Hooks.Database.Invoke(new DatabaseArgs(DatabaseActionType.Remove, key, null, user));

        #endregion

        #region Exceptions

        public static void HandleException(Exception e) =>
            Log(e.ToString(), LogType.Error);

        public static void HandleException(VisualObject node, Exception e) =>
            Log(node, e.ToString(), LogType.Error);

        public static void Throw(string text)
        {
            Log(text, LogType.Error);
            throw new Exception(text);
        }

        public static void Throw(VisualObject node, string text)
        {
            Log(node, text, LogType.Error);
            throw new Exception(text);
        }

        #endregion
        #region Log

        public static void Log(string text, LogType logType = LogType.Info) =>
            Hooks.Log.Invoke(new LogArgs(text, logType));

        public static void Log(VisualObject node, string text, LogType logType = LogType.Info) =>
            Hooks.Log.Invoke(new LogArgs(node, text, logType));

        #endregion
    }
}
