using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace TUI
{
    public class VisualDOM<T> : IDOM<T>, IVisual<T>
        where T : VisualDOM<T>
    {
        #region Data

        public static Indentation DefaultGridIndentation = new Indentation();
        public static Indentation DefaultIndentation = new Indentation();
        public static Alignment DefaultAlignment = Alignment.Center;
        public static Direction DefaultDirection = Direction.Down;
        public static Side DefaultSide = Side.Center;
        public static bool DefaultFull = false;

        IEnumerable<Point> AbsolutePoints => GetAbsolutePoints();
        
        //public UI UI { get; private set; }
        public bool Enabled { get; set; }
        public GridConfiguration GridConfig { get; private set; }
        public GridCell<T>[,] Grid { get; private set; }
        public GridCell<T> Cell { get; private set; }
        public PaddingData PaddingData { get; set; }
        public Action<T> CustomUpdateAction { get; set; } = null;

        #endregion

        #region IDOM

            #region Data

            public List<T> Child { get; private set; }
            public T Parent { get; private set; }
            public bool Rootable { get; set; }

            public IEnumerable<T> DescendantDFS => GetDescendantDFS();
            public IEnumerable<T> DescendantBFS => GetDescendantBFS();

            #endregion

            #region Initialize

            public void InitializeDOM(bool rootable)
            {
                Child = new List<T>();
                Parent = null;
                Rootable = rootable;
            }

            #endregion
            #region Add

            public virtual T Add(T child)
            {
                Child.Add(child);
                child.Parent = (T)this;

                if (UI != null && child.UI == null)
                    foreach (T o in child.GetDescendantBFS())
                        o.UI = UI;

                return child;
            }

            #endregion
            #region Remove

            public virtual bool Remove(T child)
            {
                bool result = Child.Remove(child);

                if (result)
                {
                    if (child.Cell != null)
                    {
                        child.Cell.Objects.Remove(child);
                        child.Cell = null;
                    }
                    return true;
                }
                return false;
            }

            #endregion
            #region Select

            public virtual T Select(T o)
            {
                if (!Child.Contains(o))
                    throw new InvalidOperationException("Trying to Select an object that isn't a child of current VisualDOM");

                foreach (T child in Child)
                    child.Enabled = false;
                o.Enabled = true;

                return (T)this;
            }

            public virtual T Deselect()
            {
                foreach (T child in Child)
                    child.Enabled = true;

                return (T)this;
            }

            #endregion
            #region GetRoot

            public T GetRoot()
            {
                T node = (T)this;

                while (node.Parent != null)
                {
                    node = (T)node.Parent;
                    if (node.Rootable)
                        return node;
                }

                return node;
            }

            #endregion
            #region IsAncestorFor

            public bool IsAncestorFor(T o)
            {
                T node = (T)Parent;

                while (node != null)
                {
                    if (this == node)
                        return true;
                    node = node.Parent;
                }

                return false;
            }

            #endregion
            #region SetTop

            public virtual bool SetTop(T o)
            {
                int index = Child.IndexOf(o);
                if (index > 0)
                {
                    Child.Remove(o);
                    Child.Insert(0, o);
                    return true;
                }
                else if (index == 0)
                    return false;
                else
                    throw new InvalidOperationException("Trying to SetTop an object that isn't a child of current VisualDOM");
            }

            #endregion
            #region DFS, BFS

        private void DFS(List<T> list)
        {
            list.Add((T)this);
            foreach (T child in Child)
                child.DFS(list);
        }

        private void BFS(List<T> list)
        {
            list.Add((T)this);
            int index = 0;
            while (index < list.Count)
            {
                foreach (T o in list[index].Child)
                    list.Add(o);
                index++;
            }
        }

        private IEnumerable<T> GetDescendantDFS()
        {
            List<T> list = new List<T>();
            DFS(list);

            foreach (T o in list)
                yield return o;
        }

        private IEnumerable<T> GetDescendantBFS()
        {
            List<T> list = new List<T>();
            BFS(list);

            foreach (T o in list)
                yield return o;
        }

        #endregion

        #endregion
        #region IVisual

            #region Data

            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public IEnumerable<Point> Points => GetPoints();
            public (int X, int Y, int Width, int Height) Padding(PaddingData paddingData) =>
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

            public T SetXYWH(int x, int y, int width = -1, int height = -1)
            {
                X = x;
                Y = y;
                Width = width >= 0 ? width : Width;
                Height = height >= 0 ? height : Height;
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

            #endregion
            #region Move

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

            #endregion
            #region Contains, Intersecting

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

        #endregion
            #region Points

            private IEnumerable<Point> GetPoints()
            {
                for (int x = X; x < X + Width; x++)
                    for (int y = Y; y < Y + Height; y++)
                        yield return new Point(x, y);
            }
            #endregion

        #endregion

        #region Initialize

        public VisualDOM(int x, int y, int width, int height, GridConfiguration gridConfig = null, bool rootable = false)
        {
            InitializeDOM(rootable);
            InitializeVisual(x, y, width, height);

            Enabled = true;

            if (gridConfig != null)
                SetupGrid(gridConfig);
        }

        #endregion
        #region Active

        public bool Active()
        {
            T node = (T)this;

            HashSet<T> was = new HashSet<T>();
            was.Add(node);
            while (node != null)
            {
                if (!node.Enabled)
                    return false;
                was.Add(node);
                node = node.Parent;
            }
            return true;
        }

        #endregion
        #region Add

        public virtual T Add(T child, int column, int line)
        {
            Add(child);

            GridCell<T> cell = Grid[column, line];
            cell.Objects.Add(child);
            child.Cell = cell;

            return (T)this;
        }

        #endregion
        #region SetupGrid

        public void SetupGrid(GridConfiguration gridConfig)
        {
            GridConfig = gridConfig;

            if (GridConfig.Columns == null)
                GridConfig.Columns = new ISize[] { new Relative(100) };
            if (GridConfig.Lines == null)
                GridConfig.Lines = new ISize[] { new Relative(100) };
            Grid = new GridCell<T>[GridConfig.Columns.Length, GridConfig.Lines.Length];
            for (int i = 0; i < GridConfig.Columns.Length; i++)
                for (int j = 0; j < GridConfig.Lines.Length; j++)
                    Grid[i, j] = new GridCell<T>(i, j);
        }

        #endregion
        #region AbsoluteXYWH

        public (int X, int Y, int Width, int Height) AbsoluteXYWH(int x = 0, int y = 0, int width = Int32.MinValue, int height = Int32.MinValue, T parent = null)
        {
            if (width == Int32.MinValue)
                width = Width;
            if (height == Int32.MinValue)
                height = Height;

            T node = (T)this;
            while (node != null && node != parent)
            {
                x = x + node.X;
                y = y + node.Y;
                node = node.Parent;
            }

            return (x, y, width, height);
        }

        #endregion
        #region AbsolutePoints
        
        private IEnumerable<Point> GetAbsolutePoints()
        {
            (int x, int y, int width, int height) = AbsoluteXYWH();
            for (int _x = x; _x < x + Width; _x++)
                for (int _y = y; _y < y + Height; _y++)
                    yield return new Point(_x, _y);
        }

        #endregion
        #region Update

        public virtual T Update(bool updateChild = true)
        {
            // Updates related to this node
            UpdateThis();
            // Recursive update call
            if (updateChild)
                UpdateChild();
            return (T)this;
        }

        #region UpdateThis

        public virtual T UpdateThis()
        {
            if (GridConfig != null)
                UpdateGrid();
            UpdateChildPadding();
            CustomUpdate();
            return (T)this;
        }

        #endregion
        #region UpdateGrid

        public T UpdateGrid()
        {
            // Checking grid validity
            ISize[] columnSizes = GridConfig.Columns;
            ISize[] lineSizes = GridConfig.Lines;
            Indentation gridIndentation = GridConfig.Indentation ?? DefaultGridIndentation;
            int maxW = 0, maxRelativeW = 0;
		    for (int i = 0; i < columnSizes.Length; i++)
            {
                if (columnSizes[i].Value < 0)
                    throw new ArgumentException("UpdateGrid: invalid column size");
                if (columnSizes[i].Value >= 1)
                    maxW = maxW + columnSizes[i].Value;
                else
                    maxRelativeW = maxRelativeW + (columnSizes[i].Value * 10);
            }
            if (maxW > Width - gridIndentation.Horizontal * (columnSizes.Length - 1) - gridIndentation.Left - gridIndentation.Right)
                throw new ArgumentException("UpdateGrid: maxW is too big");
		    if (maxRelativeW > 1)
                throw new ArgumentException("UpdateGrid: maxRelativeW is too big");

		    int maxH = 0, maxRelativeH = 0;
            for (int i = 1; i < lineSizes.Length; i++)
            {
                if (lineSizes[i].Value < 0)
                    throw new ArgumentException("UpdateGrid: invalid line size");

                if (lineSizes[i].Value >= 1)
                    maxH = maxH + lineSizes[i].Value;
                else
                    maxRelativeH = maxRelativeH + (lineSizes[i].Value * 10);
            }
            if (maxH > Height - gridIndentation.Vertical * (lineSizes.Length - 1) - gridIndentation.Up - gridIndentation.Down)
                throw new ArgumentException("UpdateGrid: maxH is too big");
            if (maxRelativeH > 1)
                throw new ArgumentException("UpdateGrid: maxRelativeH is too big");

            // Main cell loop
            int WCounter = gridIndentation.Left;
            int relativeW = Width - maxW - gridIndentation.Horizontal * (columnSizes.Length - 1) - gridIndentation.Left - gridIndentation.Right;
			int relativeH = Width - maxH - gridIndentation.Vertical * (lineSizes.Length - 1) - gridIndentation.Up - gridIndentation.Down;
            for (int i = 0; i < columnSizes.Length; i++)
            {
                int columnSize = columnSizes[i].Value;
                int movedWCounter;
                if (columnSize >= 1)
                    movedWCounter = WCounter + columnSize + gridIndentation.Horizontal;
                else
                    movedWCounter = WCounter + (columnSize * 10) * relativeW + gridIndentation.Horizontal;

                int HCounter = gridIndentation.Up;
                for (int j = 0; j < lineSizes.Length; j++)
                {
                    int lineSize = lineSizes[j].Value;
                    int movedHCounter;
                    if (lineSize >= 1)
                        movedHCounter = HCounter + lineSize + gridIndentation.Vertical;
                    else
                        movedHCounter = HCounter + (lineSize * 10) * relativeH + gridIndentation.Vertical;
                    GridCell<T> cell = Grid[i, j];

                    Direction direction = cell.Direction ?? GridConfig.Direction ?? DefaultDirection;
                    Alignment alignment = cell.Alignment ?? GridConfig.Alignment ?? DefaultAlignment;
                    Side side = cell.Side ?? GridConfig.Side ?? DefaultSide;
                    Indentation indentation = cell.Indentation ?? GridConfig.Indentation ?? DefaultIndentation;
                    bool full = cell.Full ?? GridConfig.Full ?? DefaultFull;

                    // Calculating cell position
                    cell.X = WCounter;
                    cell.Y = HCounter;
                    cell.Width = movedWCounter - cell.X - gridIndentation.Horizontal;
                    cell.Height = movedHCounter - cell.Y - gridIndentation.Vertical;
                    cell.I = i;
                    cell.J = j;

                    if (full)
                    {
                        if (cell.Objects.Count > 1)
                            throw new ArgumentException("UpdateGrid: More than one object in FULL cell");
                        if (cell.Objects.Count == 1)
                        {
                            cell.Objects[0].SetXYWH(cell.X + indentation.Left, cell.Y + indentation.Up,
                                cell.Width - indentation.Left - indentation.Right,
                                cell.Height - indentation.Up - indentation.Down);
                            cell.Objects[0].Cell = cell;
                        }
                    }
                    else if (cell.Objects.Count > 0)
                    {
                        // Calculating total objects width and height
                        int totalW = 0, totalH = 0;
                        for (int k = 0; k < cell.Objects.Count; k++)
                        {
                            T obj = cell.Objects[k];
                            obj.Cell = cell;
                            if (direction == Direction.Left || direction == Direction.Right)
                            {
                                if (obj.Height > totalH)
                                    totalH = obj.Height;
                                totalW = totalW + obj.Width;
                                if (k != cell.Objects.Count)
                                    totalW = totalW + indentation.Horizontal;
                            }
                            else if (direction == Direction.Up || direction == Direction.Down)
                            {
                                if (obj.Width > totalW)
                                    totalW = obj.Width;
                                totalH = totalH + obj.Height;
                                if (k != cell.Objects.Count)
                                    totalH = totalH + indentation.Vertical;
                            }
                        }

                        // Calculating cell objects position
                        int sx, sy, dx, dy;

                        // Initializing sx
                        if (alignment == Alignment.UpLeft || alignment == Alignment.Left || alignment == Alignment.DownLeft)
                            sx = indentation.Left;
                        else if (alignment == Alignment.UpRight || alignment == Alignment.Right || alignment == Alignment.DownRight)
                            sx = cell.Width - indentation.Right - totalW;
                        else
                            sx = (cell.Width - totalW + 1) / 2;

                        // Initializing sy
                        if (alignment == Alignment.UpLeft || alignment == Alignment.Up || alignment == Alignment.UpRight)
                            sy = indentation.Up;
                        else if (alignment == Alignment.DownLeft || alignment == Alignment.Down || alignment == Alignment.DownRight)
                            sy = cell.Height - indentation.Up - totalH;
                        else
                            sy = (cell.Height - totalH + 1) / 2;

                        // Updating cell objects padding
                        int cx = direction == Direction.Left ? totalW - cell.Objects[0].Width : 0;
                        int cy = direction == Direction.Up ? totalH - cell.Objects[0].Height : 0;
                        for (int k = 0; k < cell.Objects.Count; k++)
                        {
                            T obj = cell.Objects[k];
						    if (obj.Enabled && obj.PaddingData == null)
                            {
                                // Calculating side alignment
                                int sideDeltaX = 0, sideDeltaY = 0;
                                if (direction == Direction.Left)
                                {
                                    if (side == Side.Left)
                                        sideDeltaY = totalH - obj.Height;
                                    else if (side == Side.Center)
                                        sideDeltaY = (totalH - obj.Height) / 2;
                                }
                                else if (direction == Direction.Right)
                                {
                                    if (side == Side.Right)
                                        sideDeltaY = totalH - obj.Height;
                                    else if (side == Side.Center)
                                        sideDeltaY = (totalH - obj.Height) / 2;
                                }
                                else if (direction == Direction.Up)
                                {
                                    if (side == Side.Right)
                                        sideDeltaX = totalW - obj.Width;
                                    else if (side == Side.Center)
                                        sideDeltaX = (totalW - obj.Width) / 2;
                                }
                                else if (direction == Direction.Down)
                                {
                                    if (side == Side.Left)
                                        sideDeltaX = totalW - obj.Width;
                                    else if (side == Side.Center)
                                        sideDeltaX = (totalW - obj.Width) / 2;
                                }

                                obj.SetXYWH(cell.X + sx + cx + sideDeltaX, cell.Y + sy + cy + sideDeltaY);

                                if (k == cell.Objects.Count - 1)
                                    break;

                                if (direction == Direction.Right)
                                    cx = cx + indentation.Horizontal + obj.Width;
                                else if (direction == Direction.Left)
                                    cx = cx - indentation.Horizontal - cell.Objects[k + 1].Width;
                                else if (direction == Direction.Down)
                                    cy = cy + indentation.Vertical + obj.Height;
                                else if (direction == Direction.Up)
                                    cy = cy - indentation.Vertical - cell.Objects[k + 1].Height;
                            }
                        }
                    }
                    HCounter = movedHCounter;
                }
                WCounter = movedWCounter;
            }
            return (T)this;
        }

        #endregion
        #region UpdateChildPadding

        public virtual T UpdateChildPadding()
        {
            foreach (T child in Child)
            {
			    if (child.PaddingData != null)
                {
                    if (child.Cell != null)
                        child.SetXYWH(child.Cell.Padding(child.PaddingData));
                    else
                        child.SetXYWH(Padding(child.PaddingData));
                }
            }
            return (T)this;
        }

        #endregion
        #region CustomUpdate

        public virtual T CustomUpdate()
        {
            CustomUpdateAction?.Invoke((T)this);
            return (T)this;
        }

        #endregion
        #region UpdateChild

        public virtual T UpdateChild()
        {
            foreach (T child in Child)
                child.Update();
            return (T)this;
        }

        #endregion

        #endregion

        /*public void TeleportPlayer(TSPlayer player)
        {

        }*/
    }
}
