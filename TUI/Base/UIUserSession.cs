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
        public int UserIndex { get; set; }
        /// <summary>
        /// Index of touch interval (TouchState.Begin to TouchState.End). Increases every TouchState.Begin.
        /// </summary>
        public int TouchSessionIndex { get; set; } = -1;
        public int Count { get; set; } = 0;
        public int ProjectileID { get; set; } = -1;
        public Touch BeginTouch { get; set; }
        public VisualObject BeginObject { get; set; }
        public VisualObject Acquired { get; set; }
        public Touch PreviousTouch { get; set; }
        public bool Used { get; set; }

        public UIUserSession(int userIndex)
        {
            UserIndex = userIndex;
        }

        public void Reset()
        {
            Enabled = true;
            Used = false;
            Count = 0;
            BeginObject = null;
            Acquired = null;
        }
    }
}