namespace TUI
{
    public class UIUserSession
    {
        public bool Enabled { get; set; } = true;
        public int UserIndex { get; set; }
        public int Index { get; set; }
        public int Count { get; set; } = 0;
        public Touch BeginTouch { get; set; }
        public Touch PreviousTouch { get; set; }
        public VisualObject BeginObject { get; set; }
        public int ProjectileID { get; set; }
    }
}