namespace TerrariaUI.Base.Style
{
    public class InternalIndent
    {
        public int Horizontal = 0;
        public int Vertical = 0;

        public InternalIndent() { }

        public InternalIndent(InternalIndent indent)
        {
            this.Horizontal = indent.Horizontal;
            this.Vertical = indent.Vertical;
        }

        public override string ToString() =>
            $"InternalIndent {{ Horizontal={Horizontal}, Vertical={Vertical} }}";
    }

    public class ExternalIndent
    {
        public int Left = 0;
        public int Up = 0;
        public int Right = 0;
        public int Down = 0;

        public ExternalIndent() { }

        public ExternalIndent(ExternalIndent indent)
        {
            this.Left = indent.Left;
            this.Up = indent.Up;
            this.Right = indent.Right;
            this.Down = indent.Down;
        }

        public override string ToString() =>
            $"ExternalIndent {{ Left={Left}, Up={Up}, Right={Right}, Down={Down} }}";
    }

    public class Indent
    {
        public int Left = 0;
        public int Up = 0;
        public int Right = 0;
        public int Down = 0;
        public int Horizontal = 0;
        public int Vertical = 0;

        public Indent() { }

        public Indent(Indent indent)
        {
            this.Left = indent.Left;
            this.Up = indent.Up;
            this.Right = indent.Right;
            this.Down = indent.Down;
            this.Horizontal = indent.Horizontal;
            this.Vertical = indent.Vertical;
        }

        public override string ToString() =>
            $"Indent {{ Left={Left}, Up={Up}, Right={Right}, Down={Down}, Horizontal={Horizontal}, Vertical={Vertical} }}";
    }
}