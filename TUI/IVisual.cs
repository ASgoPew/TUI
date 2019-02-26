using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    #region IVisual<T>

    internal interface IVisual<T>
    {
        int X { get; set; }
        int Y { get; set; }
        int Width { get; set; }
        int Height { get; set; }

        (int X, int Y, int Width, int Height) XYWH(int dx, int dy);
        T SetXYWH(int x, int y, int width, int height);
        T SetXYWH((int x, int y, int width, int height) data);
        T Move(int dx, int dy);
        T MoveBack(int dx, int dy);
        bool Contains(int x, int y);
        bool Intersecting(int x, int y, int width, int height);
        bool Intersecting((int x, int y, int width, int height) data);
        (int X, int Y, int Width, int Height) Padding(PaddingData paddingData);
        IEnumerable<Point> Points { get; }
    }

    #endregion

    #region VisualDOM<T> : IVisual<T>

    public partial class VisualDOM<T> : IDOM<T>, IVisual<T>
        where T : VisualDOM<T>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public IEnumerable<Point> Points => GetPoints();

        public void InitializeVisual(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public (int X, int Y, int Width, int Height) XYWH(int dx = 0, int dy = 0)
        {
            return (X + dx, Y + dy, Width, Height);
        }

        public T SetXYWH(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            return (T)this;
        }

        public T SetXYWH((int x, int y, int width, int height) data)
        {
            X = data.x;
            Y = data.y;
            Width = data.width;
            Height = data.height;
            return (T)this;
        }

        public T Move(int dx, int dy)
        {
            X = X + dx;
            Y = Y + dy;
            return (T)this;
        }

        public T MoveBack(int dx, int dy)
        {
            X = X - dx;
            Y = Y - dy;
            return (T)this;
        }

        public bool Contains(int x, int y)
        {
            return x >= X && y >= Y && x < X + Width && y < Y + Height;
        }

        public bool Intersecting(int x, int y, int width, int height)
        {
            return x < X + Width && X < x + width && y < Y + Height && Y < y + height;
        }

        public bool Intersecting((int x, int y, int width, int height) data)
        {
            return data.x < X + Width && X < data.x + data.width && data.y < Y + Height && Y < data.y + data.height;
        }

        public (int X, int Y, int Width, int Height) Padding(PaddingData paddingData)
        {
            int x = paddingData.X;
            int y = paddingData.Y;
            int width = paddingData.Width;
            int height = paddingData.Height;
            Alignment alignment = paddingData.Alignment;
            if (alignment == Alignment.Up || alignment == Alignment.Center || alignment == Alignment.Down)
                x = x + Width / 2;
            else if (alignment == Alignment.UpRight || alignment == Alignment.Right || alignment == Alignment.DownRight)
                x = x + Width;
            if (alignment == Alignment.Left || alignment == Alignment.Center || alignment == Alignment.Right)
                y = y + Height / 2;
            else if (alignment == Alignment.DownRight || alignment == Alignment.Down || alignment == Alignment.DownRight)
                y = y + Height;
            if (width <= 0)
                width = Width + width - x;
            if (height <= 0)
                height = Height + height - y;
		    return (x, y, width, height);
        }

        private IEnumerable<Point> GetPoints()
        {
            for (int x = X; x < X + Width; x++)
                for (int y = Y; y < Y + Height; y++)
                    yield return new Point(x, y);
        }
    }

    #endregion
    #region GridCell<T> : IVisual<T>

    public partial class GridCell<T> : IVisual<GridCell<T>>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public IEnumerable<Point> Points => GetPoints();

        public void InitializeVisual(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public (int X, int Y, int Width, int Height) XYWH(int dx = 0, int dy = 0)
        {
            return (X + dx, Y + dy, Width, Height);
        }

        public GridCell<T> SetXYWH(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            return (GridCell<T>)this;
        }

        public GridCell<T> SetXYWH((int x, int y, int width, int height) data)
        {
            X = data.x;
            Y = data.y;
            Width = data.width;
            Height = data.height;
            return (GridCell<T>)this;
        }

        public GridCell<T> Move(int dx, int dy)
        {
            X = X + dx;
            Y = Y + dy;
            return (GridCell<T>)this;
        }

        public GridCell<T> MoveBack(int dx, int dy)
        {
            X = X - dx;
            Y = Y - dy;
            return (GridCell<T>)this;
        }

        public bool Contains(int x, int y)
        {
            return x >= X && y >= Y && x < X + Width && y < Y + Height;
        }

        public bool Intersecting(int x, int y, int width, int height)
        {
            return x < X + Width && X < x + width && y < Y + Height && Y < y + height;
        }

        public bool Intersecting((int x, int y, int width, int height) data)
        {
            return data.x < X + Width && X < data.x + data.width && data.y < Y + Height && Y < data.y + data.height;
        }

        public (int X, int Y, int Width, int Height) Padding(PaddingData paddingData)
        {
            int x = paddingData.X;
            int y = paddingData.Y;
            int width = paddingData.Width;
            int height = paddingData.Height;
            Alignment alignment = paddingData.Alignment;
            if (alignment == Alignment.Up || alignment == Alignment.Center || alignment == Alignment.Down)
                x = x + Width / 2;
            else if (alignment == Alignment.UpRight || alignment == Alignment.Right || alignment == Alignment.DownRight)
                x = x + Width;
            if (alignment == Alignment.Left || alignment == Alignment.Center || alignment == Alignment.Right)
                y = y + Height / 2;
            else if (alignment == Alignment.DownRight || alignment == Alignment.Down || alignment == Alignment.DownRight)
                y = y + Height;
            if (width <= 0)
                width = Width + width - x;
            if (height <= 0)
                height = Height + height - y;
            return (x, y, width, height);
        }

        private IEnumerable<Point> GetPoints()
        {
            for (int x = X; x < X + Width; x++)
                for (int y = Y; y < Y + Height; y++)
                    yield return new Point(x, y);
        }
    }

    #endregion
}
