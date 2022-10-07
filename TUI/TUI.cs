﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
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

        private static List<RootVisualObject> _Child = new List<RootVisualObject>();
        private static Timer Timer;
        internal static object LoadLocker = new object();
        internal static object DisposeLocker = new object();
        public static bool TouchedDebug = false;
        public static bool PulseDebug = false;
        public static bool UpdateDebug = false;
        public static bool ApplyDebug = false;
        public static bool DrawDebug = false;

        public const string ControlPermission = "TUI.control";

        public static List<RootVisualObject> Roots => _Child.ToList();
        public static int MaxPlayers { get; private set; }
        public static int MaxTilesX { get; private set; }
        public static int MaxTilesY { get; private set; }
        public static int WorldID { get; private set; }
        public static HookManager Hooks { get; } = new HookManager();
        public static PlayerSession[] Session { get; private set; }
        public static int MaxHoldTouchMilliseconds = 30000;
        public static bool Active { get; private set; } = false;
        public static ConcurrentDictionary<string, ApplicationType> ApplicationTypes { get; } =
            new ConcurrentDictionary<string, ApplicationType>();
        public static List<ConcurrentDictionary<Application, byte>> ApplicationPlayerSessions { get; } =
            new List<ConcurrentDictionary<Application, byte>>();

        #endregion

        #region Initialize

        public static void Initialize(int maxPlayers = 256, int maxTilesX = 8400, int maxTilesY = 2400)
        {
            MaxPlayers = maxPlayers;
            MaxTilesX = maxTilesX;
            MaxTilesY = maxTilesY;
            Session = new PlayerSession[MaxPlayers];
            Timer = new Timer(5000) { AutoReset = true };
            Timer.Elapsed += OnTimerElapsed;
            for (int i = 0; i < maxPlayers; i++)
                ApplicationPlayerSessions.Add(new ConcurrentDictionary<Application, byte>());
        }

        #endregion
        #region Load

        public static void Load(int worldID)
        {
            Active = true; // Must be before Load() calls
            WorldID = worldID;

            foreach (VisualObject child in Roots)
                child.Load();

            foreach (var pair in ApplicationTypes)
                pair.Value.Load();

            Timer.Start();

            Hooks.Load.Invoke(new LoadArgs(MaxPlayers));
        }

        #endregion
        #region Dispose

        public static void Dispose()
        {
            Timer.Stop();

            Active = false;

            foreach (RootVisualObject child in Roots)
                child.Dispose();
            _Child.Clear();

            Hooks.Dispose.Invoke(new EventArgs());
        }

        #endregion
        #region TryToLoadChild

        internal static void TryToLoadChild(VisualObject node, VisualObject child)
        {
            if (Active && _Child.Contains(node.GetRoot() as RootVisualObject))
                child.Load();
        }

        #endregion
        #region OnTimerElapsed

        public static void OnTimerElapsed(object sender, ElapsedEventArgs args)
        {
            for (int i = 0; i < Session.Length; i++)
            {
                PlayerSession session = Session[i];
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
        public static T Create<T>(T root, bool draw = true)
            where T : RootVisualObject
        {
            if (Roots.Count(r => r.Name == root.Name) > 0)
                throw new ArgumentException($"TUI.Create: name {root.Name} is already taken.");

            List<RootVisualObject> _child = Roots;
            int index = _child.Count;
            while (index > 0 && _child[index - 1].Layer > root.Layer)
                index--;
            _Child.Insert(index, root);

            if (Active)
            {
                root.Load();
                root.Update();
                if (draw)
                    root.Apply().Draw();
            }
            return root;
        }

        #endregion
        #region Destroy

        public static void Destroy(RootVisualObject obj)
        {
            if (obj is Application app)
                app.Type.DisposeInstance(app);

            if (Active)
                obj.Disable(true);
            _Child.Remove(obj);
            obj.Dispose();
        }

        #endregion
        #region InitializePlayer

        public static void InitializePlayer(int playerIndex)
        {
            Session[playerIndex] = new PlayerSession(playerIndex);
        }

        #endregion
        #region RemovePlayer

        public static void RemovePlayer(int playerIndex)
        {
            PlayerSession session = Session[playerIndex];
            if (session == null)
                return;

            if (session.PreviousTouch != null && session.PreviousTouch.State != TouchState.End)
            {
                Touch simulatedEndTouch = session.PreviousTouch.SimulatedEndTouch();
                Touched(playerIndex, simulatedEndTouch);
            }

            foreach (RootVisualObject child in Roots)
            {
                if (child.Personal && child.Observers.Contains(playerIndex))
                {
                    child.Observers.Remove(playerIndex);
                    if (child is Application app)
                        app.OnObserverLeave(playerIndex);
                }
                child.Players.Remove(playerIndex);
            }

            foreach (var pair in ApplicationPlayerSessions[playerIndex])
                pair.Key.OnPlayerLeave(playerIndex);

            Session[playerIndex] = null;
        }

        #endregion

        #region Touched

        public static bool Touched(int playerIndex, Touch touch)
        {
            PlayerSession session = Session[playerIndex];
            touch.SetSession(session);

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
            Log($"Touch ({touch.X},{touch.Y}): {touch.State} ({touch.Object}); elapsed: {elapsed}");
#endif

            session.Count++;
            session.PreviousTouch = touch;

            return session.Used;
        }

        #endregion
        #region TouchedAcquired

        public static bool TouchedAcquired(Touch touch, ref bool insideUI)
        {
            VisualObject o = touch.Session.Acquired;
            (int saveX, int saveY) = o.AbsoluteXY();

            touch.Move(-saveX, -saveY);
            bool inside = o.ContainsRelative(touch);

            if (o.IsActive && inside)
                insideUI = true;

            if (o.IsActive && (inside || o.Configuration.UseOutsideTouches))
                if (o.Touched(touch))
                    return true;

            touch.Move(saveX, saveY);
            return false;
        }

        #endregion
        #region TouchedChild

        private static bool TouchedChild(Touch touch, ref bool insideUI)
        {
            List<RootVisualObject> _child = Roots;
            for (int i = _child.Count - 1; i >= 0; i--)
            {
                RootVisualObject o = _child[i];
                int saveX = o.X, saveY = o.Y;
                if (o.IsActive
                    && o.ContainsParent(touch)
                    && o.Players.Contains(touch.Session.PlayerIndex)
                    && (!o.Personal || o.Observers.Contains(touch.PlayerIndex))
                    && (!o.Freezed || touch.Priveleged))
                {
                    touch.Move(-saveX, -saveY);
                    if (o.Touched(touch))
                    {
                        insideUI = true;
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

        public static void SetTop(RootVisualObject root)
        {
            if (root.Personal)
                return;

            List<RootVisualObject> _child = Roots;
            int previousIndex = _child.IndexOf(root);
            int index = previousIndex;
            if (index < 0)
                throw new InvalidOperationException("Trying to SetTop an an object that is not in TUI roots list");
            int count = _child.Count;
            index++;
            while (index < count && _child[index].Layer <= root.Layer)
                index++;

            if (index == previousIndex + 1)
                return;

            _Child.Remove(root);
            _Child.Insert(index - 1, root);

            if (!root.UsesDefaultMainProvider)
                root.Provider.SetTop(false);

            // Should not apply if intersecting objects have different tile provider
            (bool intersects, bool needsApply) = ChildIntersectingOthers(root);
            if (intersects)
            {
                if (needsApply)
                    root.Apply();
                else
                    root.RequestDrawChanges();
                root.Draw();
            }
        }

        #endregion
        #region ChildIntersectingOthers

        public static (bool intersects, bool needsApply) ChildIntersectingOthers(RootVisualObject o)
        {
            bool intersects = false;
            foreach (RootVisualObject child in Roots)
                if (child != o && child.IsActive && child.Orderable && o.Intersecting(child))
                {
                    intersects = true;
                    dynamic provider1 = o.Provider;
                    dynamic provider2 = child.Provider;
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
            foreach (RootVisualObject child in Roots)
                try
                {
                    if (child.IsActive)
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
            foreach (RootVisualObject child in Roots)
                try
                {
                    if (child.IsActive)
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
            foreach (RootVisualObject child in Roots)
                try
                {
                    if (child.IsActive)
                        child.Draw();
                }
                catch (Exception e)
                {
                    HandleException(child, e);
                }
        }

        #endregion
        #region DrawObject

        internal static void DrawObject(VisualObject node, HashSet<int> targetPlayers, int x, int y, int width, int height,
            bool drawWithSection, bool frameSection = true)
        {
            var args = new DrawObjectArgs(node, targetPlayers, x, y, width, height, drawWithSection, frameSection);
            Hooks.DrawObject.Invoke(args);
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
        #region RequestDrawChanges

        public static void RequestDrawChanges()
        {
            foreach (RootVisualObject child in Roots)
                if (child.IsActive && !(child.Provider is MainTileProvider))
                    child.RequestDrawChanges();
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
        #region NDBGet

        public static int? NDBGet(int user, string key)
        {
            DatabaseArgs args = new DatabaseArgs(DatabaseActionType.Get, key, null, user, 0);
            Hooks.Database.Invoke(args);
            return args.Number;
        }

        #endregion
        #region NDBSet

        public static void NDBSet(int user, string key, int number) =>
            Hooks.Database.Invoke(new DatabaseArgs(DatabaseActionType.Set, key, null, user, number));

        #endregion
        #region NDBRemove

        public static void NDBRemove(int user, string key) =>
            Hooks.Database.Invoke(new DatabaseArgs(DatabaseActionType.Remove, key, null, user, 0));

        #endregion
        #region NDBSelect

        public static List<(int User, int Number, string Username)> NDBSelect(string key, bool ascending, int count, int offset, bool requestNames)
        {
            DatabaseArgs args = new DatabaseArgs(DatabaseActionType.Select, key, null, null, 0, ascending, count, offset, requestNames);
            Hooks.Database.Invoke(args);
            return args.Numbers;
        }

        #endregion

        #region RegisterApplication

        public static void RegisterApplication(ApplicationType app)
        {
            if (ApplicationTypes.ContainsKey(app.Name))
                throw new ArgumentException($"Application name already used: {app.Name}");

            ApplicationTypes[app.Name] = app;
        }

        #endregion
        #region DeregisterApplication

        public static void DeregisterApplication(string name, bool destroy = true)
        {
            if (!ApplicationTypes.TryGetValue(name, out ApplicationType appType))
                throw new KeyNotFoundException($"Application not found: {appType.Name}");

            if (destroy)
                appType.DestroyAll();
            ApplicationTypes.TryRemove(appType.Name, out _);
        }

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
