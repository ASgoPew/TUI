using System;

namespace TUI.Base
{
    public class Indentation : ICloneable
    {
        public int Left = 0;
        public int Up = 0;
        public int Right = 0;
        public int Down = 0;
        public int Horizontal = 0;
        public int Vertical = 0;

        public object Clone() => MemberwiseClone();
    }
}