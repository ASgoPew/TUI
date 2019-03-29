using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public class VisualDOM : IDOM<VisualObject>, IVisual<VisualObject>, ICloneable
    {
        #region Data

        public readonly Indentation DefaultGridIndentation = new Indentation();
        public readonly Indentation DefaultIndentation = new Indentation();
        public const Alignment DefaultAlignment = Alignment.Center;
        public const Direction DefaultDirection = Direction.Down;
        public const Side DefaultSide = Side.Center;
        public const bool DefaultFull = false;

        public virtual string Name
        {
            get => GetType().Name;
            protected set => throw new InvalidOperationException();
        }
        IEnumerable<(int, int)> AbsolutePoints => GetAbsolutePoints();

        protected RootVisualObject _Root { get; set; }
        public bool Enabled { get; set; }
        public GridCell[,] Grid { get; private set; }
        public GridCell Cell { get; private set; }
        public UIConfiguration Configuration { get; set; }

        #endregion

        #region IDOM

            #region Data

            public List<VisualObject> Child { get; private set; }
            public VisualObject Parent { get; private set; }

            public IEnumerable<VisualObject> DescendantDFS => GetDescendantDFS();
            public IEnumerable<VisualObject> DescendantBFS => GetDescendantBFS();

            #endregion

            #region Initialize

            public void InitializeDOM()
            {
                Child = new List<VisualObject>();
                Parent = null;
            }

            #endregion
            #region Add

            public virtual VisualObject Add(VisualObject child)
            {
                lock (Child)
                    Child.Add(child);
                child.Parent = this as VisualObject;
                return child;
            }

            public virtual VisualObject Add(VisualObject child, int column, int line)
            {
                Add(child);

                GridCell cell = Grid[column, line];
                cell.Objects.Add(child);
                child.Cell = cell;

                return child;
            }

            #endregion
            #region Remove

            public virtual VisualObject Remove(VisualObject child)
            {
                bool removed;
                lock (Child)
                    removed = Child.Remove(child);
                if (removed)
                {
                    child.Parent = null;
                    if (child.Cell != null)
                    {
                        child.Cell.Objects.Remove(child);
                        child.Cell = null;
                    }
                    return child;
                }
                return null;
            }

            #endregion
            #region Select

            public virtual VisualObject Select(VisualObject o)
            {
                if (!Child.Contains(o))
                    throw new InvalidOperationException("Trying to Select an object that isn't a child of current VisualDOM");

                lock (Child)
                    foreach (VisualObject child in Child)
                        child.Enabled = false;
                o.Enabled = true;

                return this as VisualObject;
            }

            public virtual VisualObject Deselect()
            {
                lock (Child)
                    foreach (VisualObject child in Child)
                        child.Enabled = true;

                return this as VisualObject;
            }

            #endregion
            #region GetRoot

            public VisualObject GetRoot()
            {
                VisualDOM node = this;
                while (node.Parent != null)
                    node = node.Parent;
                return node as VisualObject;
            }

            #endregion
            #region IsAncestorFor

            public bool IsAncestorFor(VisualObject o)
            {
                VisualObject node = Parent;

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

            public virtual bool SetTop(VisualObject o)
            {
                lock (Child)
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
            }

            #endregion
            #region DFS, BFS

            private void DFS(List<VisualObject> list)
            {
                list.Add(this as VisualObject);
                lock (Child)
                    foreach (VisualObject child in Child)
                        child.DFS(list);
            }

            private void BFS(List<VisualObject> list)
            {
                list.Add(this as VisualObject);
                int index = 0;
                while (index < list.Count)
                {
                    lock (list[index].Child)
                        foreach (VisualObject o in list[index].Child)
                            list.Add(o);
                    index++;
                }
            }

            private IEnumerable<VisualObject> GetDescendantDFS()
            {
                List<VisualObject> list = new List<VisualObject>();
                DFS(list);

                foreach (VisualObject o in list)
                    yield return o;
            }

            private IEnumerable<VisualObject> GetDescendantBFS()
            {
                List<VisualObject> list = new List<VisualObject>();
                BFS(list);

                foreach (VisualObject o in list)
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
            #region XYWH, SetXYWH

            public virtual (int X, int Y, int Width, int Height) XYWH(int dx = 0, int dy = 0)
            {
                return (X + dx, Y + dy, Width, Height);
            }

            public virtual VisualObject SetXYWH(int x, int y, int width = -1, int height = -1)
            {
                X = x;
                Y = y;
                Width = width >= 0 ? width : Width;
                Height = height >= 0 ? height : Height;
                return this as VisualObject;
            }

            public virtual VisualObject SetXYWH((int x, int y, int width, int height) data)
            {
                X = data.x;
                Y = data.y;
                Width = data.width >= 0 ? data.width : Width;
                Height = data.height >= 0 ? data.height : Height;
                return this as VisualObject;
            }

            #endregion
            #region Move

            public virtual VisualObject Move(int dx, int dy)
            {
                X = X + dx;
                Y = Y + dy;
                return this as VisualObject;
            }

            public virtual VisualObject MoveBack(int dx, int dy)
            {
                X = X - dx;
                Y = Y - dy;
                return this as VisualObject;
            }

            #endregion
            #region Contains, Intersecting

            public virtual bool Contains(int x, int y) =>
                x >= X && y >= Y && x < X + Width && y < Y + Height;

            public virtual bool Intersecting(int x, int y, int width, int height) =>
                x < X + Width && X < x + width && y < Y + Height && Y < y + height;

            public virtual bool Intersecting(VisualObject o) => Intersecting(o.X, o.Y, o.Width, o.Height);

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
        #region ICloneable

        public virtual object Clone() =>
            new VisualDOM(X, Y, Width, Height, (UIConfiguration)Configuration.Clone());

        #endregion

        #region Initialize

        public VisualDOM(int x, int y, int width, int height, UIConfiguration configuration = null)
        {
            InitializeDOM();
            InitializeVisual(x, y, width, height);

            Enabled = true;
            Configuration = configuration ?? new UIConfiguration();

            if (Configuration.Grid != null)
                SetupGrid(Configuration.Grid);
        }

        #endregion
        #region Active

        public bool Active()
        {
            VisualDOM node = this;

            HashSet<VisualDOM> was = new HashSet<VisualDOM>();
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
        #region SetupGrid

        public VisualObject SetupGrid(GridConfiguration gridConfig)
        {
            Configuration.Grid = gridConfig;

            if (gridConfig.Columns == null)
                gridConfig.Columns = new ISize[] { new Relative(100) };
            if (gridConfig.Lines == null)
                gridConfig.Lines = new ISize[] { new Relative(100) };
            Grid = new GridCell[gridConfig.Columns.Length, gridConfig.Lines.Length];
            for (int i = 0; i < gridConfig.Columns.Length; i++)
                for (int j = 0; j < gridConfig.Lines.Length; j++)
                    Grid[i, j] = new GridCell(i, j);

            return this as VisualObject;
        }

        #endregion
        #region Update

            public virtual VisualObject Update(bool updateChild = true)
            {
                // Updates related to this node
                UpdateThis();

                // Recursive update call
                if (updateChild)
                    UpdateChild();

                return this as VisualObject;
            }

            #region UpdateThis

            public virtual VisualObject UpdateThis()
            {
                if (_Root == null)
                    _Root = GetRoot() as RootVisualObject;
                if (Configuration.Grid != null)
                    UpdateGrid();
                UpdateChildPadding();
                CustomUpdate();
                return this as VisualObject;
            }

            #endregion
            #region UpdateGrid

            public VisualObject UpdateGrid()
            {
                // Checking grid validity
                GridConfiguration gridConfig = Configuration.Grid;
                ISize[] columnSizes = gridConfig.Columns;
                ISize[] lineSizes = gridConfig.Lines;
                Indentation gridIndentation = gridConfig.Indentation ?? DefaultGridIndentation;
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
                        GridCell cell = Grid[i, j];

                        Direction direction = cell.Direction ?? gridConfig.Direction ?? DefaultDirection;
                        Alignment alignment = cell.Alignment ?? gridConfig.Alignment ?? DefaultAlignment;
                        Side side = cell.Side ?? gridConfig.Side ?? DefaultSide;
                        Indentation indentation = cell.Indentation ?? gridConfig.Indentation ?? DefaultIndentation;
                        bool full = cell.Full ?? gridConfig.Full ?? DefaultFull;

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
                                VisualObject obj = cell.Objects[k];
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
                            int sx, sy;

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
                                VisualObject obj = cell.Objects[k];
						        if (obj.Enabled && obj.Configuration.Padding == null)
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
                return this as VisualObject;
            }

            #endregion
            #region UpdateChildPadding

            public virtual VisualObject UpdateChildPadding()
            {
                lock (Child)
                    foreach (VisualObject child in Child)
                    {
			            if (child.Configuration.Padding != null)
                        {
                            if (child.Cell != null)
                                child.SetXYWH(child.Cell.Padding(child.Configuration.Padding));
                            else
                                child.SetXYWH(Padding(child.Configuration.Padding));
                        }
                    }
                return this as VisualObject;
            }

            #endregion
            #region CustomUpdate

            public virtual VisualObject CustomUpdate() =>
                Configuration.CustomUpdate?.Invoke(this as VisualObject);

            #endregion
            #region UpdateChild

            public virtual VisualObject UpdateChild()
            {
                lock (Child)
                    foreach (VisualObject child in Child)
                        if (child.Enabled)
                            child.Update();
                return this as VisualObject;
            }

            #endregion

        #endregion
        #region AbsoluteXY

        public (int X, int Y) AbsoluteXY(int x = 0, int y = 0, VisualDOM parent = null)
        {
            VisualDOM node = this;
            while (node != null && node != parent)
            {
                x = x + node.X;
                y = y + node.Y;
                node = node.Parent;
            }
            if (node != parent)
                throw new Exception("AbsoluteXY: parent-object not found.");

            return (x, y);
        }

        #endregion
        #region AbsolutePoints

        private IEnumerable<(int, int)> GetAbsolutePoints()
        {
            (int x, int y) = AbsoluteXY();
            for (int _x = x; _x < x + Width; _x++)
                for (int _y = y; _y < y + Height; _y++)
                    yield return (_x, _y);
        }

        #endregion
    }
}
