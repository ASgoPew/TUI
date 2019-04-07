using System.Collections.Generic;

namespace TUI.Base
{
    public class GridCell
    {
        public int Column { get; }
        public int Line { get; }

        public GridCell(int column, int line)
        {
            Column = column;
            Line = line;
        }
    }
}
