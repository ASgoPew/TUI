using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public class GridCell : IVisual<GridCell>
    {
        public int Column { get; }
        public int Line { get; }
        public List<VisualObject> Objects { get; }
        public Indentation Indentation { get; set; }
        public Alignment? Alignment { get; set; }
        public Direction? Direction { get; set; }
        public Side? Side { get; set; }
        public bool? Full { get; set; }
        public int I { get; set; }
        public int J { get; set; }

        #region IVisual

            #region Data

            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public IEnumerable<(int, int)> Points => GetPoints();
            public (int X, int Y, int Width, int Height) Padding(PaddingConfig paddingData) =>
                UI.Padding(X, Y, Width, Height, paddingData);

            #endregion

            #region Initialize

            public void InitializeVisual(int x, int y, int width, int height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }

            #endregion
            #region XYWH

            public (int X, int Y, int Width, int Height) XYWH(int dx = 0, int dy = 0)
            {
                return (X + dx, Y + dy, Width, Height);
            }

            public GridCell SetXYWH(int x, int y, int width = -1, int height = -1)
            {
                X = x;
                Y = y;
                Width = width >= 0 ? width : Width;
                Height = height >= 0 ? height : Height;
                return this;
            }

            #endregion
            #region Move

            public GridCell Move(int dx, int dy)
            {
                X = X + dx;
                Y = Y + dy;
                return this;
            }

            public GridCell MoveBack(int dx, int dy)
            {
                X = X - dx;
                Y = Y - dy;
                return this;
            }

            #endregion
            #region Contains, Intersects

            public bool Contains(int x, int y)
            {
                return x >= X && y >= Y && x < X + Width && y < Y + Height;
            }

            public bool Intersecting(int x, int y, int width, int height)
            {
                return x < X + Width && X < x + width && y < Y + Height && Y < y + height;
            }

            public bool Intersecting(GridCell o) => Intersecting(o.X, o.Y, o.Width, o.Height);

            #endregion
            #region Points

            private IEnumerable<(int, int)> GetPoints()
            {
                for (int x = X; x < X + Width; x++)
                    for (int y = Y; y < Y + Height; y++)
                        yield return (x, y);
            }

            #endregion

        #endregion

        public GridCell(int column, int line)
        {
            Column = column;
            Line = line;
        }
    }
}
