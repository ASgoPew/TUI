using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public partial class GridCell<T> : IVisual<GridCell<T>>
    {
        public int Column { get; }
        public int Line { get; }
        public List<T> Objects { get; }

        public GridCell(int column, int line)
        {
            Column = column;
            Line = line;
        }
    }
}
