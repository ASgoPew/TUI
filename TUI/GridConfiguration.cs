using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public class GridConfiguration
    {
        public ISize[] Columns;
        public ISize[] Lines;
        public int DeltaX, DeltaY;
        public Direction Direction;
        public Alignment Alignment;
        public Side Side;
        public Indentation Indentation;
        public bool Full;

        public GridConfiguration()
        {
            Columns = new ISize[] { new Relative(100) };
            Lines = new ISize[] { new Relative(100) };
            DeltaX = 0;
            DeltaY = 0;
            Direction = Direction.Down;
            Alignment = Alignment.Center;
            Side = Side.Center;
            Indentation = new Indentation()
            {
                Left = 1,
                Up = 1,
                Right = 1,
                Down = 1,
                Horizontal = 1,
                Vertical = 1
            };
            Full = false;
        }
    }
}
