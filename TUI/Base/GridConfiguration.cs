using System;

namespace TUI.Base
{
    public class GridConfiguration : ICloneable
    {
        public ISize[] Columns;
        public ISize[] Lines;
        public Indentation Indentation = UIDefault.Indentation;
        public Alignment Alignment = UIDefault.Alignment;
        public Direction Direction = UIDefault.Direction;
        public Side Side = UIDefault.Side;

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
                Side = Side
            };
        }
    }
}
