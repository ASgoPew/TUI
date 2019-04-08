namespace TUI.Base.Style
{
    public class InternalOffset
    {
        public int Horizontal = 0;
        public int Vertical = 0;

        public InternalOffset() { }

        public InternalOffset(InternalOffset indent)
        {
            this.Horizontal = indent.Horizontal;
            this.Vertical = indent.Vertical;
        }
    }

    public class ExternalOffset
    {
        public int Left = 0;
        public int Up = 0;
        public int Right = 0;
        public int Down = 0;

        public ExternalOffset() { }

        public ExternalOffset(ExternalOffset indent)
        {
            this.Left = indent.Left;
            this.Up = indent.Up;
            this.Right = indent.Right;
            this.Down = indent.Down;
        }
    }

    public class Offset
    {
        public int Left = 0;
        public int Up = 0;
        public int Right = 0;
        public int Down = 0;
        public int Horizontal = 0;
        public int Vertical = 0;

        public Offset() { }

        public Offset(Offset indent)
        {
            this.Left = indent.Left;
            this.Up = indent.Up;
            this.Right = indent.Right;
            this.Down = indent.Down;
            this.Horizontal = indent.Horizontal;
            this.Vertical = indent.Vertical;
        }
    }
}