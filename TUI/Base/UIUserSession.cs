namespace TUI
{
    public class UIUserSession<T>
        where T : VisualDOM<T>
    {
        public bool Enabled { get; set; } = true;
        public int UserIndex { get; set; }
        public int Index { get; set; }
        public int Count { get; set; } = 0;
        public Touch<T> BeginTouch { get; set; }
        public Touch<T> PreviousTouch { get; set; }
        public T BeginObject { get; set; }
        public int ProjectileID { get; set; }
    }

    public class UIUserSession : UIUserSession<VisualObject> { }
}