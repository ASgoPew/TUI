namespace TUI
{
    public class UIUserSession<T, U>
        where T : VisualDOM<T>
    {
        public bool Enabled { get; set; } = true;
        public U User { get; set; }
        public int Index { get; set; }
        public int Count { get; set; } = 0;
        public Touch<T, U> BeginTouch { get; set; }
        public Touch<T, U> PreviousTouch { get; set; }
        public T BeginObject { get; set; }
    }

    public class UIUserSession<U> : UIUserSession<VisualObject, U> { }
}