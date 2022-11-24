using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TerrariaUI.Base
{
    /// <summary>
    /// Collection of TUI related data that corresponds to a player.
    /// </summary>
    public class PlayerSession
    {
        /// <summary>
        /// Session enabled state: if set to false all touches up the TouchState.End would be ignored.
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// Player index (index in Players array for TShock)
        /// </summary>
        public int PlayerIndex { get; internal set; }
        /// <summary>
        /// Identifier of touch interval (TouchState.Begin to TouchState.End). Increases after every TouchState.End.
        /// </summary>
        public int TouchSessionIndex { get; internal set; } = 0;
        /// <summary>
        /// Index of touch since the moment of touch begin (TouchState.Begin).
        /// </summary>
        public int Count { get; internal set; } = 0;
        /// <summary>
        /// Index of corresponding grand design projectile in Main.projectile.
        /// </summary>
        public int ProjectileID { get; set; } = -1;
        /// <summary>
        /// Previous touch object. Has previous touch object even if this is TouchState.Begin.
        /// </summary>
        public Touch PreviousTouch { get; internal set; }
        /// <summary>
        /// Begin touch object. Null if this is TouchState.Begin.
        /// </summary>
        public Touch BeginTouch { get; internal set; }
        /// <summary>
        /// Acquired object during current touch interval. Once object is acquired all next touches
        /// within the same touch interval would pass only to this object.
        /// </summary>
        public VisualObject Acquired { get; internal set; }
        public bool Used { get; internal set; }
        internal HashSet<VisualObject> LockedObjects { get; set; } = new HashSet<VisualObject>();
        internal bool EndTouchHandled { get; set; }
        private ConcurrentDictionary<object, object> Data { get; } = new ConcurrentDictionary<object, object>();

        public PlayerSession(int playerIndex)
        {
            PlayerIndex = playerIndex;
        }

        /// <summary>
        /// Get/set user session related data from runtime storage.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[object key]
        {
            get
            {
                Data.TryGetValue(key, out object value);
                return value;
            }
            set => Data[key] = value;
        }

        public void Reset()
        {
            Enabled = true;
            Used = false;
            Count = 0;
            Acquired = null;
            LockedObjects.Clear();
        }
    }
}