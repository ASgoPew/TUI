using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public class GridConfiguration : ICloneable
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
        }

        public object Clone()
        {
            return new GridConfiguration
            {
                Columns = (ISize[])Columns.Clone(),
                Lines = (ISize[])Lines.Clone(),
                Indentation = (Indentation)Indentation.Clone(),
                Alignment = Alignment,
                Direction = Direction,
                Side = Side,
                Full = Full
            };
        }
    }
}
