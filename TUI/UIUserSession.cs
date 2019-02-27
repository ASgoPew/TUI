namespace TUI
{
    public class UIUserSession<T>
        where T : VisualDOM<T>
    {
        public bool Enabled { get; set; }
        public UIUser User { get; set; }
        public int Index { get; set; }
        public int Count { get; set; }
        public Touch<T> BeginTouch { get; set; }
        public Touch<T> PreviousTouch { get; set; }
        public T BeginObject { get; set; }
    }

    public class UIUserSession : UIUserSession<VisualObject> { }
}