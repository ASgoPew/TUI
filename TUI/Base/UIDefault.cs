using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI.Base
{
    public static class UIDefault
    {
        public static Indentation Indentation { get; set; } = new Indentation();
        public static Alignment Alignment { get; set; } = Alignment.Center;
        public static Direction Direction { get; set; } = Direction.Down;
        public static Side Side { get; set; } = Side.Center;

        public static Indentation LabelIndentation { get; set; } = new Indentation() { Horizontal = 1, Vertical = 1 };
        public static byte LabelTextColor { get; set; } = 25;
    }
}
