using System;
using System.Collections.Generic;
using System.Linq;
using TerrariaUI.Base.Style;
using TerrariaUI.Widgets;

namespace TerrariaUI
{
    #region ApplicationStyle

    /// <summary>
    /// Drawing styles for Application widget.
    /// </summary>
    public class ApplicationStyle : PanelStyle
    {
        /// <summary>
        /// Time of application life, in seconds
        /// </summary>
        public int Timeout { get; set; } = -1;
        /// <summary>
        /// Max distance from player to app allowed, in tiles
        /// </summary>
        public int MaxDistance { get; set; } = 100;
        /// <summary>
        /// Player index to track
        /// </summary>
        public int TrackingPlayer { get; set; } = -1;
        /// <summary>
        /// Alignment of tracking player relative to app
        /// </summary>
        public Alignment TrackAlignment { get; set; } = Alignment.Down;
        /// <summary>
        /// Distance the tracking player have to go for app to teleport
        /// </summary>
        public int TrackingDistance { get; set; } = 1;
        /// <summary>
        /// Whether to track the player when he is moving
        /// </summary>
        public bool TrackInMotion { get; set; } = false;

        public ApplicationStyle() : base() { }

        public ApplicationStyle(ApplicationStyle style)
            : base(style)
        {
            Timeout = style.Timeout;
            MaxDistance = style.MaxDistance;
            TrackAlignment = style.TrackAlignment;
            TrackingPlayer = style.TrackingPlayer;
        }
    }

    #endregion

    /*public enum PlayerSessionEndReason
    {
        Disconnect,
        Logout,
        TooFar,
        Timeout
    }*/

    /// <summary>
    /// Basic UI application
    /// </summary>
    public class Application : Panel
    {
        #region Data

        public ApplicationType Type { get; internal set; }
        public int Index { get; internal set; }
        /// <summary>
        /// Time of app creation moment
        /// </summary>
        public DateTime CreateTime { get; protected set; }
        /// <summary>
        /// Array of players in user session (if it is active)
        /// </summary>
        public int[] SessionPlayers { get; protected set; } = null;
        /// <summary>
        /// Time of app player session begin moment
        /// </summary>
        public DateTime PlayerSessionCreateTime { get; protected set; }
        /// <summary>
        /// App player session time until stopping, in seconds
        /// </summary>
        public int PlayerSessionTimeout { get; protected set; } = -1;
        private object PlayerSessionLocker = new object();

        public ApplicationStyle ApplicationStyle => Style as ApplicationStyle;
        /// <summary>
        /// If app is tracking a player
        /// </summary>
        public bool Tracking => ApplicationStyle.TrackingPlayer >= 0;

        #endregion

        #region Constructor

        public Application(string name, int width, int height, ApplicationStyle style = null, object provider = null, HashSet<int> observers = null)
            : base(name, 0, 0, width, height, null, style ?? new ApplicationStyle(), provider, observers)
        {
            CreateTime = DateTime.UtcNow;
            if (Tracking && provider == null)
                throw new ArgumentException("Tracking applications must have a custom tile provider.");
        }

        #endregion
        #region StartPlayerSession

        bool StartingPlayerSession = false;
        public void StartPlayerSession(IEnumerable<int> players, int timeout = -1)
        {
            lock (PlayerSessionLocker)
            {
                if (StartingPlayerSession || EndingPlayerSession)
                    return;
                StartingPlayerSession = true;
            }

            if (SessionPlayers != null)
            {
                TUI.Log($"Application ({Name}) is trying to start player session when it has already started.", LogType.Warning);
                return;
            }

            SessionPlayers = players.ToArray();
            foreach (int player in SessionPlayers)
                TUI.ApplicationPlayerSessions[player].TryAdd(this, 0);
            PlayerSessionCreateTime = DateTime.UtcNow;
            PlayerSessionTimeout = timeout;

            try
            {
                StartPlayerSessionNative();
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }

            StartingPlayerSession = false;
        }

        #endregion
        #region StartPlayerSessionNative

        protected virtual void StartPlayerSessionNative() { }

        #endregion
        #region EndPlayerSession

        bool EndingPlayerSession = false;
        public void EndPlayerSession()
        {
            lock (PlayerSessionLocker)
            {
                if (StartingPlayerSession || EndingPlayerSession)
                    return;
                EndingPlayerSession = true;
            }

            if (SessionPlayers == null)
            {
                //TUI.Log($"Application ({Name}) is trying to end player session when it has already ended.", LogType.Warning);
                return;
            }

            try
            {
                EndPlayerSessionNative();
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }

            PlayerSessionTimeout = -1;
            foreach (int player in SessionPlayers)
                TUI.ApplicationPlayerSessions[player].TryRemove(this, out _);
            SessionPlayers = null;

            EndingPlayerSession = false;
        }

        #endregion
        #region EndPlayerSessionNative

        protected virtual void EndPlayerSessionNative() { }

        #endregion
        #region TrackingTeleport

        public void TrackingTeleport(int playerX, int playerY)
        {
            try
            {
                TrackingTeleportNative(playerX, playerY);
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
        }

        #endregion
        #region TrackingTeleportPosition

        public virtual void TrackingTeleportNative(int playerX, int playerY)
        {
            (int x, int y) = PlayerAlignment(playerX, playerY);
            SetXY(x, y, true);
        }

        #endregion
        #region PlayerAlignment

        public (int, int) PlayerAlignment(int playerX, int playerY)
        {
            Alignment alignment = ApplicationStyle.TrackAlignment;

            int x;
            if (alignment == Alignment.UpLeft || alignment == Alignment.Left || alignment == Alignment.DownLeft)
                x = playerX + 2;
            else if (alignment == Alignment.UpRight || alignment == Alignment.Right || alignment == Alignment.DownRight)
                x = playerX - 2 - Width;
            else
                x = playerX - Width / 2;

            int y;
            if (alignment == Alignment.UpLeft || alignment == Alignment.Up || alignment == Alignment.UpRight)
                y = playerY + 2;
            else if (alignment == Alignment.DownLeft || alignment == Alignment.Down || alignment == Alignment.DownRight)
                y = playerY - 2 - Height;
            else
                y = playerY - Height / 2;
            return (x, y);
        }

        #endregion
        #region OnTimeout

        public void OnTimeout()
        {
            try
            {
                OnTimeoutNative();
            } catch (Exception e)
            {
                TUI.HandleException(e);
            }
        }

        #endregion
        #region OnTimeoutNative

        protected virtual void OnTimeoutNative() => TUI.Destroy(this);

        #endregion
        #region OnObserverLeave

        public void OnObserverLeave(int player)
        {
            try
            {
                OnObserverLeaveNative(player);
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
        }

        #endregion
        #region OnObserverLeaveNative

        protected virtual void OnObserverLeaveNative(int player)
        {
            if (Observers.Count == 0)
                TUI.Destroy(this);
        }

        #endregion
        #region OnPlayerLeave

        public void OnPlayerLeave(int player)
        {
            try
            {
                OnPlayerLeaveNative(player);
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
        }

        #endregion
        #region OnPlayerLeaveNative

        protected virtual void OnPlayerLeaveNative(int player) => EndPlayerSession();

        #endregion
        #region OnPlayerLogout

        public void OnPlayerLogout(int player)
        {
            try
            {
                OnPlayerLogoutNative(player);
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
        }

        #endregion
        #region OnPlayerLogoutNative

        public virtual void OnPlayerLogoutNative(int player) => EndPlayerSession();

        #endregion
        #region OnPlayerTooFar

        public void OnPlayerTooFar(int player)
        {
            try
            {
                OnPlayerTooFarNative(player);
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
        }

        #endregion
        #region OnPlayerTooFarNative

        public virtual void OnPlayerTooFarNative(int player) => EndPlayerSession();

        #endregion
        #region OnPlayerSessionTimeout

        public void OnPlayerSessionTimeout()
        {
            try
            {
                OnPlayerSessionTimeoutNative();
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
        }

        #endregion
        #region OnPlayerSessionTimeoutNative

        public virtual void OnPlayerSessionTimeoutNative() => EndPlayerSession();

        #endregion
    }
}
