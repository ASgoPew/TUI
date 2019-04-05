using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public static class UIDefault
    {
        public static Indentation Indentation { get; set; } = new Indentation();
        public static Alignment Alignment { get; set; } = Alignment.Center;
        public static Direction Direction { get; set; } = Direction.Down;
        public static Side Size { get; set; } = Side.Center;
    }
}
