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
        public Indentation Indentation;
        public Alignment? Alignment;
        public Direction? Direction;
        public Side? Side;
        public bool? Full = false;

        public GridConfiguration(ISize[] columns = null, ISize[] lines = null)
        {
            Columns = columns ?? new ISize[] { new Relative(100) };
            Lines = lines ?? new ISize[] { new Relative(100) };
            /*Direction = Direction.Down;
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
            Full = false;*/
        }
    }
}
