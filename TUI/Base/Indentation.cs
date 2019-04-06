using System;

namespace TUI.Base
{
    public class Indentation
    {
        public int Left = 0;
        public int Up = 0;
        public int Right = 0;
        public int Down = 0;
        public int Horizontal = 0;
        public int Vertical = 0;

        public Indentation() { }

        public Indentation(Indentation indentation)
        {
            this.Left = indentation.Left;
            this.Up = indentation.Up;
            this.Right = indentation.Right;
            this.Down = indentation.Down;
            this.Horizontal = indentation.Horizontal;
            this.Vertical = indentation.Vertical;
        }
    }
}