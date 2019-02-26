using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public class Touchable<T> : VisualDOM<T>
        where T : Touchable<T>
    {
        public Touchable(int x, int y, int width, int height)
            : base(x, y, width, height)
        {

        }
    }
}
