using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TUI.Base
{
    public class UIUserSession
    {
        /// <summary>
        /// Session enabled state: if set to true all touches up the TouchState.End would be ignored.
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// User index (index in Players array for TShock)
        /// </summary>
        public int UserIndex { get; internal set; }
        /// <summary>
        /// Identifier of touch interval (TouchState.Begin to TouchState.End). Increases after every TouchState.End.
        /// </summary>
        public int TouchSessionIndex { get; internal set; } = 0;
        public int Count { get; internal set; } = 0;
        public int ProjectileID { get; set; } = -1;
        public Touch BeginTouch { get; internal set; }
        public VisualObject BeginObject { get; internal set; }
        public VisualObject Acquired { get; internal set; }
        public Touch PreviousTouch { get; internal set; }
        public bool Used { get; internal set; }
        internal HashSet<VisualObject> LockedObjects { get; set; } = new HashSet<VisualObject>();
        internal bool EndTouchHandled { get; set; }
        public ConcurrentDictionary<object, object> Data { get; } = new ConcurrentDictionary<object, object>();

        public UIUserSession(int userIndex)
        {
            UserIndex = userIndex;
        }

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
            BeginObject = null;
            Acquired = null;
            LockedObjects.Clear();
        }
    }
}