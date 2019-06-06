using System;
using System.Collections.Generic;
using System.Linq;
using TUI.Base.Style;
using TUI.Widgets.Media;

namespace TUI.Base
{
    public class VisualObject : Touchable
    {
        #region Data

        public UIStyle Style { get; set; }
        public VisualObject[,] Grid { get; set; }
        public GridCell Cell { get; private set; }
        public bool ForceSection { get; protected internal set; } = false;
        public int ProviderX { get; protected set; }
        public int ProviderY { get; protected set; }
        public ExternalOffset Bounds { get; protected set; }

        public override bool Orderable => !Style.InLayout;
        public virtual string Name => GetType().Name;
        public string FullName =>
            Parent != null
                ? (Cell != null
                    ? $"{Parent.FullName}[{Cell.Column},{Cell.Line}].{Name}"
                    : $"{Parent.FullName}[{IndexInParent}].{Name}")
                : Name;

        #endregion

        #region IDOM

            #region AddToLayout

            /// <summary>
            /// Add object as a child in layout. Removes child alignment and grid positioning.
            /// </summary>
            /// <param name="child">Object to add as a child.</param>
            /// <param name="layer">Layer where to add the object.</param>
            /// <returns></returns>
            public virtual VisualObject AddToLayout(VisualObject child, int layer = 0)
            {
                child.Style.Alignment = null;
                if (child.Cell != null)
                {
                    Grid[child.Cell.Column, child.Cell.Line] = null;
                    child.Cell = null;
                }

                Add(child, layer);
                child.Style.InLayout = true;
                return child;
            }

            #endregion
            #region Remove

            public override VisualObject Remove(VisualObject child)
            {
                child = base.Remove(child);
                if (child != null)
                {
                    GridCell cell = child.Cell;
                    if (cell != null)
                    {
                        Grid[cell.Column, cell.Line] = null;
                        child.Cell = null;
                    }
                    if (child.Style.InLayout)
                        Style.Layout.Objects.Remove(child);
                }
                return child;
            }

            #endregion

        #endregion
        #region Touchable

            #region PostSetTop

            public override void PostSetTop(VisualObject o)
            {
                if (ChildIntersectingOthers(o))
                    o.Apply().Draw();
            }

            private bool ChildIntersectingOthers(VisualObject o)
            {
                lock (Child)
                    foreach (VisualObject child in ChildrenFromTop)
                        if (child != o && child.Active && o.Intersecting(child))
                            return true;
                return false;
            }

            #endregion

        #endregion

        #region Constructor

        public VisualObject(int x, int y, int width, int height, UIConfiguration configuration = null, UIStyle style = null, Func<VisualObject, Touch, bool> callback = null)
            : base(x, y, width, height, configuration, callback)
        {
            Style = style ?? new UIStyle();
        }

        public VisualObject()
            : this(0, 0, 0, 0, new UIConfiguration() { UseBegin = false })
        {
        }

        public VisualObject(UIConfiguration configuration)
            : this(0, 0, 0, 0, configuration)
        {
        }

        public VisualObject(UIStyle style)
            : this(0, 0, 0, 0, new UIConfiguration() { UseBegin = false }, style)
        {
        }

        #endregion
        #region Dispose

        public virtual void Dispose()
        {
            DisposeThisNative();

            lock (Child)
                foreach (VisualObject child in ChildrenFromTop)
                    child.Dispose();
        }

        protected virtual void DisposeThisNative() { }

        #endregion
        #region operator[,]

        /// <summary>
        /// Get: Get a child in grid.
        /// <para></para>
        /// Set: Add object as a child to grid. Removes child alignment and layout positioning.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public VisualObject this[int column, int line]
        {
            get => Grid[column, line];
            set
            {
                if (value != null)
                {
                    value.Style.InLayout = false;
                    value.Style.Alignment = null;

                    if (Grid[column, line] != null)
                        Remove(Grid[column, line]);
                    Grid[column, line] = Add(value);
                    value.Cell = new GridCell(column, line);
                }
                else
                {
                    Remove(Grid[column, line]);
                    Grid[column, line] = null;
                }
            }
        }

        #endregion
        #region Tile

        public virtual dynamic Tile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                throw new ArgumentOutOfRangeException($"{FullName}: Invalid tile x or y.");
            ExternalOffset bounds = Bounds;
            if (x < bounds.Left || x >= Width - bounds.Right || y < bounds.Up || y >= Height - bounds.Down)
                return null;
            return Provider[ProviderX + x, ProviderY + y];
        }

        #endregion
        #region ToString

        public override string ToString() => FullName;

        #endregion
        #region SetXYWH

        public override VisualObject SetXYWH(int x, int y, int width, int height)
        {
            int oldX = X, oldY = Y, oldWidth = Width, oldHeight = Height;
            if (oldX != x || oldY != y || oldWidth != width || oldHeight != height)
            {
                base.SetXYWH(x, y, width, height);
                Pulse(PulseType.PositionChanged);
            }
            return this;
        }

        #endregion
        #region SetupLayout

        public VisualObject SetupLayout(LayoutStyle layout)
        {
            Style.Layout = layout;
            return this;
        }

        #endregion
        #region SetupGrid

        public VisualObject SetupGrid(GridStyle gridStyle = null, bool fillWithEmptyObjects = true)
        {
            Style.Grid = gridStyle ?? new GridStyle();

            VisualObject[,] oldGrid = Grid;

            if (gridStyle.Columns == null)
                gridStyle.Columns = new ISize[] { new Relative(100) };
            if (gridStyle.Lines == null)
                gridStyle.Lines = new ISize[] { new Relative(100) };
            Grid = new VisualObject[gridStyle.Columns.Length, gridStyle.Lines.Length];
            if (fillWithEmptyObjects)
                for (int i = 0; i < gridStyle.Columns.Length; i++)
                    for (int j = 0; j < gridStyle.Lines.Length; j++)
                        if (i < oldGrid?.GetLength(0) && j < oldGrid?.GetLength(1) && oldGrid[i, j] != null)
                            this[i, j] = Remove(oldGrid[i, j]);
                        else
                            this[i, j] = new VisualObject();

            return this as VisualObject;
        }

        #endregion
        #region SetAlignmentInParent

        /// <summary>
        /// Setup alignment positioning inside parent. Removes layout and grid positioning.
        /// </summary>
        /// <param name="alignmentStyle"></param>
        /// <returns></returns>
        public VisualObject SetAlignmentInParent(AlignmentStyle alignmentStyle)
        {
            if (Cell != null)
            {
                Parent.Grid[Cell.Column, Cell.Line] = null;
                Cell = null;
            }
            Style.InLayout = false;

            Style.Alignment = alignmentStyle;
            return this;
        }

        #endregion
        #region SetFullSize

        /// <summary>
        /// Set automatic stretching to parent size. Removes grid positioning.
        /// </summary>
        /// <param name="horizontal"></param>
        /// <param name="vertical"></param>
        /// <returns></returns>
        public VisualObject SetFullSize(bool horizontal = true, bool vertical = true)
        {
            if (Cell != null)
            {
                Parent.Grid[Cell.Column, Cell.Line] = null;
                Cell = null;
            }

            if (horizontal && vertical)
                Style.FullSize = FullSize.Both;
            else if (horizontal)
                Style.FullSize = FullSize.Horizontal;
            else if (vertical)
                Style.FullSize = FullSize.Vertical;
            else
                Style.FullSize = FullSize.None;
            return this;
        }

        /// <summary>
        /// Set automatic stretching to parent size. Removes grid positioning.
        /// </summary>
        /// <param name="fullSize"></param>
        /// <returns></returns>
        public VisualObject SetFullSize(FullSize fullSize)
        {
            if (Cell != null)
            {
                Parent.Grid[Cell.Column, Cell.Line] = null;
                Cell = null;
            }

            Style.FullSize = fullSize;
            return this;
        }

        #endregion
        #region LayoutSkip

        public VisualObject LayoutSkip(ushort value)
        {
            if (Style.Layout == null)
                throw new Exception("Layout is not set for this object: " + FullName);

            Style.Layout.Index = value;
            return this;
        }

        #endregion
        #region LayoutIndent

        public VisualObject LayoutIndent(int value)
        {
            if (Style.Layout == null)
                throw new Exception("Layout is not set for this object: " + FullName);

            Style.Layout.LayoutIndent = value;
            return this;
        }

        #endregion

        #region Pulse

            public virtual VisualObject Pulse(PulseType type)
            {
                // Pulse event handling related to this node
                PulseThis(type);

                // Recursive Pulse call
                PulseChild(type);

                return this;
            }

            #region PulseThis

            public VisualObject PulseThis(PulseType type)
            {
                PulseThisNative(type);
                CustomPulse(type);
                return this;
            }

            #endregion
            #region PulseThisNative

            protected virtual void PulseThisNative(PulseType type)
            {
                switch (type)
                {
                    case PulseType.PositionChanged:
                        // Update position relative to Provider
                        if (Root != null)
                            (ProviderX, ProviderY) = ProviderXY();
                        break;
                }
            }

            #endregion
            #region CustomPulse

            public virtual VisualObject CustomPulse(PulseType type)
            {
                Configuration.CustomPulse?.Invoke(this, type);
                return this;
            }

            #endregion
            #region PulseChild

            public virtual VisualObject PulseChild(PulseType type)
            {
                lock (Child)
                    foreach (VisualObject child in ChildrenFromTop)
                        child.Pulse(type);
                return this;
            }

            #endregion

        #endregion
        #region Update

            public virtual VisualObject Update()
            {
                // Updates related to this node
                UpdateThis();

                // Recursive Update call
                UpdateChild();

                return this;
            }

            #region UpdateThis

            public VisualObject UpdateThis()
            {
                UpdateThisNative();
                CustomUpdate();
                return this;
            }

            #endregion
            #region UpdateThisNative

            /// <summary>
            /// Updates related to this node. At the moment of calling this method node position must be set up completely.
            /// </summary>
            protected virtual void UpdateThisNative()
            {
                // Find Root node
                if (Root == null)
                    Root = GetRoot() as RootVisualObject;
                // Update position relative to Provider
                (ProviderX, ProviderY) = ProviderXY();
                // Update apply tile bounds
                UpdateBounds();

                /////////////////////////// Child size updates ///////////////////////////

                // Update child objects with Style.FullSize
                UpdateFullSize();

                ///////////////////////// Child position updates /////////////////////////

                // Update child objects with alignment
                UpdateAlignment();
                // Update child objects in layout
                if (Style.Layout != null)
                    UpdateLayout();
                // Update child objects in grid
                if (Style.Grid != null)
                    UpdateGrid();
            }

            #endregion
            #region UpdateBounds

            protected void UpdateBounds()
            {
                bool layoutBounds = Style.InLayout && Parent.Style.Layout.BoundsIsOffset;
                bool alignmentBounds = Style.Alignment != null && Style.Alignment.BoundsIsOffset;
                if (layoutBounds || alignmentBounds)
                {
                    ExternalOffset parentOffset = layoutBounds ? Parent.Style.Layout.Offset : Style.Alignment.Offset;
                    Bounds = new ExternalOffset()
                    {
                        Left = Math.Max(0, parentOffset.Left - X),
                        Up = Math.Max(0, parentOffset.Up - Y),
                        Right = Math.Max(0, (X + Width) - (Parent.Width - parentOffset.Right)),
                        Down = Math.Max(0, (Y + Height) - (Parent.Height - parentOffset.Down)),
                    };
                }
                else
                    Bounds = new ExternalOffset(UIDefault.ExternalOffset);

                // Intersecting bounds with parent's Bounds
                if (Parent != null)
                {
                    ExternalOffset parentBounds = Parent.Bounds;
                    if (parentBounds == null)
                        return;
                    Bounds = new ExternalOffset()
                    {
                        Left = Math.Max(Bounds.Left, parentBounds.Left - X),
                        Up = Math.Max(Bounds.Up, parentBounds.Up - Y),
                        Right = Math.Max(Bounds.Right, parentBounds.Right - (Parent.Width - (X + Width))),
                        Down = Math.Max(Bounds.Down, parentBounds.Down - (Parent.Height - (Y + Height)))
                    };
                }
            }

            #endregion
            #region UpdateFullSize

            protected void UpdateFullSize()
            {
                ExternalOffset offset = Style.Layout?.Offset;
                lock (Child)
                    foreach (VisualObject child in ChildrenFromTop)
                    {
                        FullSize fullSize = child.Style.FullSize;
                        if (fullSize == FullSize.None)
                            continue;

                        // If InLayout is set then FullSize should match parent size minus layout offset.
                        // If Alignment is set then FullSize should match parent size minus alignment offset.
                        int x = 0, y = 0, width = Width, height = Height;
                        if (child.Style.InLayout || child.Style.Alignment != null)
                        {
                            if (child.Style.Alignment != null)
                                offset = child.Style.Alignment.Offset;
                            x = offset.Left;
                            y = offset.Up;
                            width = Width - x - offset.Right;
                            height = Height - y - offset.Down;
                        }

                        if (fullSize == FullSize.Both)
                            child.SetXYWH(x, y, width, height);
                        else if (fullSize == FullSize.Horizontal)
                            child.SetXYWH(x, child.Y, width, child.Height);
                        else if (fullSize == FullSize.Vertical)
                            child.SetXYWH(child.X, y, child.Width, height);
                        //Console.WriteLine($"FullSize: {child.FullName}, {child.XYWH()}");
                    }
            }

            #endregion
            #region UpdateAlignment

            protected void UpdateAlignment()
            {
                lock (Child)
                    foreach (VisualObject child in ChildrenFromTop)
                    {
                        AlignmentStyle positioning = child.Style.Alignment;
                        if (positioning  == null)
                            continue;

                        ExternalOffset offset = positioning.Offset ?? UIDefault.ExternalOffset;
                        Alignment alignment = positioning.Alignment;
                        int x, y;
                        if (alignment == Alignment.UpLeft || alignment == Alignment.Left || alignment == Alignment.DownLeft)
                            x = offset.Left;
                        else if (alignment == Alignment.UpRight || alignment == Alignment.Right || alignment == Alignment.DownRight)
                            x = Width - offset.Right - child.Width;
                        else
                            x = (int)Math.Floor((Width - child.Width) / 2f);

                        if (alignment == Alignment.UpLeft || alignment == Alignment.Up || alignment == Alignment.UpRight)
                            y = offset.Up;
                        else if (alignment == Alignment.DownLeft || alignment == Alignment.Down || alignment == Alignment.DownRight)
                            y = Height - offset.Down - child.Height;
                        else
                            y = (int)Math.Floor((Height - child.Height) / 2f);

                        child.SetXY(x, y);
                    }
            }

            #endregion
            #region UpdateLayout

            protected void UpdateLayout()
            {
                ExternalOffset offset = Style.Layout.Offset;
                Alignment alignment = Style.Layout.Alignment;
                Direction direction = Style.Layout.Direction;
                Side side = Style.Layout.Side;
                int indent = Style.Layout.ChildIndent;
                int layoutIndent = Style.Layout.LayoutIndent;

                (int abstractLayoutW, int abstractLayoutH, List<VisualObject> layoutChild) = CalculateLayoutSize(direction, indent);
                Style.Layout.Objects = layoutChild;
                for (int i = 0; i < Style.Layout.Index; i++)
                    layoutChild[i].Visible = false;
                if (layoutChild.Count - Style.Layout.Index <= 0)
                    return;
                layoutChild = layoutChild.Skip(Style.Layout.Index).ToList();

                // Calculating layout box position
                int layoutX, layoutY, layoutW, layoutH;

                // Initializing sx
                if (alignment == Alignment.UpLeft || alignment == Alignment.Left || alignment == Alignment.DownLeft)
                    layoutX = offset.Left;
                else if (alignment == Alignment.UpRight || alignment == Alignment.Right || alignment == Alignment.DownRight)
                    layoutX = Width - offset.Right - abstractLayoutW;
                else
                    layoutX = (Width - abstractLayoutW + 1) / 2;
                layoutX = Math.Max(layoutX, offset.Left);
                layoutW = Math.Min(abstractLayoutW, Width - offset.Left - offset.Right);

                // Initializing sy
                if (alignment == Alignment.UpLeft || alignment == Alignment.Up || alignment == Alignment.UpRight)
                    layoutY = offset.Up;
                else if (alignment == Alignment.DownLeft || alignment == Alignment.Down || alignment == Alignment.DownRight)
                    layoutY = Height - offset.Down - abstractLayoutH;
                else
                    layoutY = (Height - abstractLayoutH + 1) / 2;
                layoutY = Math.Max(layoutY, offset.Up);
                layoutH = Math.Min(abstractLayoutH, Height - offset.Up - offset.Down);

                // Updating cell objects padding
                int cx = direction == Direction.Left
                    ? Math.Min(abstractLayoutW - layoutChild[0].Width, Width - layoutX - offset.Right - layoutChild[0].Width)
                    : 0;
                int cy = direction == Direction.Up
                    ? Math.Min(abstractLayoutH - layoutChild[0].Height, Height - layoutY - offset.Down - layoutChild[0].Height)
                    : 0;

                // Layout indent for smooth scrolling
                if (direction == Direction.Right)
                    cx = cx - layoutIndent;
                else if (direction == Direction.Left)
                    cx = cx + layoutIndent;
                else if (direction == Direction.Down)
                    cy = cy - layoutIndent;
                else if (direction == Direction.Up)
                    cy = cy + layoutIndent;

                int k = 0;
                for (; k < layoutChild.Count; k++)
                {
                    // Calculating side alignment
                    VisualObject child = layoutChild[k];
                    int sideDeltaX = 0, sideDeltaY = 0;
                    if (direction == Direction.Left)
                    {
                        if (side == Side.Left)
                            sideDeltaY = abstractLayoutH - child.Height;
                        else if (side == Side.Center)
                            sideDeltaY = (abstractLayoutH - child.Height) / 2;
                    }
                    else if (direction == Direction.Right)
                    {
                        if (side == Side.Right)
                            sideDeltaY = abstractLayoutH - child.Height;
                        else if (side == Side.Center)
                            sideDeltaY = (abstractLayoutH - child.Height) / 2;
                    }
                    else if (direction == Direction.Up)
                    {
                        if (side == Side.Right)
                            sideDeltaX = abstractLayoutW - child.Width;
                        else if (side == Side.Center)
                            sideDeltaX = (abstractLayoutW - child.Width) / 2;
                    }
                    else if (direction == Direction.Down)
                    {
                        if (side == Side.Left)
                            sideDeltaX = abstractLayoutW - child.Width;
                        else if (side == Side.Center)
                            sideDeltaX = (abstractLayoutW - child.Width) / 2;
                    }

                    int resultX = layoutX + cx + sideDeltaX;
                    int resultY = layoutY + cy + sideDeltaY;
                    //Console.WriteLine($"{Width}, {Height}: {resultX}, {resultY}, {resultX + child.Width - 1}, {resultY + child.Height - 1}");
                    //child.Visible = LayoutContains(resultX, resultY, offset)
                    //    && LayoutContains(resultX + child.Width - 1, resultY + child.Height - 1, offset);
                    if (Style.Layout.BoundsIsOffset)
                        child.Visible = Intersecting(resultX, resultY, child.Width, child.Height, offset.Left, offset.Up,
                            Width - offset.Right - offset.Left, Height - offset.Down - offset.Up);
                    else
                        child.Visible = Intersecting(resultX, resultY, child.Width, child.Height, 0, 0, Width, Height);

                    child.SetXY(resultX, resultY);
                    //Console.WriteLine($"Layout: {child.FullName}, {child.XYWH()}");

                    if (k == layoutChild.Count - 1)
                        break;

                    if (direction == Direction.Right)
                        cx = cx + indent + child.Width;
                    else if (direction == Direction.Left)
                        cx = cx - indent - layoutChild[k + 1].Width;
                    else if (direction == Direction.Down)
                        cy = cy + indent + child.Height;
                    else if (direction == Direction.Up)
                        cy = cy - indent - layoutChild[k + 1].Height;
                }

                if (direction == Direction.Left || direction == Direction.Right)
                    Style.Layout.IndentLimit = abstractLayoutW - layoutW;
                else if (direction == Direction.Up || direction == Direction.Down)
                    Style.Layout.IndentLimit = abstractLayoutH - layoutH;
            }

            #endregion
            #region CalculateLayoutSize

            private (int absoluteLayoutW, int absoluteLayoutH, List<VisualObject> objects) CalculateLayoutSize(
                Direction direction, int indent)
            {
                // Calculating total objects width and height
                int totalW = 0, totalH = 0;
                List<VisualObject> layoutChild = new List<VisualObject>();
                lock (Child)
                    foreach (VisualObject child in ChildrenFromBottom)
                    {
                        FullSize fullSize = child.Style.FullSize;
                        if (!child.Enabled || !child.Style.InLayout || fullSize == FullSize.Both)
                                //|| (fullSize == FullSize.Horizontal && (direction == Direction.Left || direction == Direction.Right))
                                //|| (fullSize == FullSize.Vertical && (direction == Direction.Up || direction == Direction.Down)))
                            continue;

                        layoutChild.Add(child);
                        if (direction == Direction.Left || direction == Direction.Right)
                        {
                            if (child.Height > totalH)
                                totalH = child.Height;
                            totalW += child.Width + indent;
                        }
                        else if (direction == Direction.Up || direction == Direction.Down)
                        {
                            if (child.Width > totalW)
                                totalW = child.Width;
                            totalH += child.Height + indent;
                        }
                    }
                if ((direction == Direction.Left || direction == Direction.Right) && totalW > 0)
                    totalW -= indent;
                if ((direction == Direction.Up || direction == Direction.Down) && totalH > 0)
                    totalH -= indent;

                return (totalW, totalH, layoutChild);
            }

            #endregion
            #region Intersecting

            public bool Intersecting(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2) =>
                x1 < x2 + w2 && y1 < y2 + h2 && x2 < x1 + w1 && y2 < y1 + h1;

            //public virtual bool LayoutContains(int x, int y, ExternalOffset offset) =>
                //x >= offset.Left && y >= offset.Up && x < Width - offset.Right && y < Height - offset.Down;

            #endregion
            #region UpdateGrid

            protected void UpdateGrid()
            {
                CalculateGridSizes();

                // Main cell loop
                ISize[] columnSizes = Style.Grid.Columns;
                ISize[] lineSizes = Style.Grid.Lines;
                
                for (int i = 0; i < columnSizes.Length; i++)
                {
                    (int columnX, int columnSize) = Style.Grid.ResultingColumns[i];
                    for (int j = 0; j < lineSizes.Length; j++)
                    {
                        (int lineX, int lineSize) = Style.Grid.ResultingLines[j];
                        Grid[i, j]?.SetXYWH(columnX, lineX, columnSize, lineSize);
                        //Console.WriteLine($"Grid: {cell.FullName}, {cell.XYWH()}");
                    }
                }
            }

            #endregion
            #region CalculateGridSizes

            public void CalculateGridSizes()
            {
                Offset offset = Style.Grid.Offset ?? UIDefault.Offset;
                CalculateSizes(Style.Grid.Columns, Width, offset.Left, offset.Horizontal, offset.Right,
                    ref Style.Grid.ResultingColumns, ref Style.Grid.MinWidth, "width");
                CalculateSizes(Style.Grid.Lines, Height, offset.Up, offset.Vertical, offset.Down,
                    ref Style.Grid.ResultingLines, ref Style.Grid.MinHeight, "height");
            }

            #endregion
            #region CalculateSizes

            private void CalculateSizes(ISize[] sizes, int absoluteSize, int startOffset, int middleOffset, int endOffset, ref (int Position, int Size)[] resulting, ref int minSize, string sizeName)
            {
                // Initializing min size
                minSize = startOffset + endOffset;
                int defaultMinSize = minSize;

                // Initializing resulting array
                resulting = new (int, int)[sizes.Length];

                // First calculating absolute and relative sum
                int absoluteSum = 0, relativeSum = 0;
                int notZeroSizes = 0;
                for (int i = 0; i < sizes.Length; i++)
                {
                    ISize size = sizes[i];
                    int value = size.Value;
                    if (size.IsAbsolute)
                        absoluteSum += value;
                    else
                        relativeSum += value;
                    if (value > 0)
                        notZeroSizes++;
                }
                if (absoluteSum > absoluteSize)
                    throw new ArgumentException($"{FullName} (UpdateGrid): absolute {sizeName} is more that object {sizeName}");
                if (relativeSum > 100)
                    throw new ArgumentException($"{FullName} (UpdateGrid): relative {sizeName} is more than 100");

                // Now calculating actual column/line sizes
                int relativeSize = absoluteSize - absoluteSum - middleOffset * (notZeroSizes - 1) - startOffset - endOffset;
                // ???
                if (relativeSize < 0)
                    relativeSize = 0;
                int relativeSizeUsed = 0;
                List<(int, float)> descendingFractionalPart = new List<(int, float)>();
                int sizeCounter = startOffset;
                for (int i = 0; i < sizes.Length; i++)
                {
                    ISize size = sizes[i];
                    float sizeValue = size.IsAbsolute
                        ? size.Value
                        : size.Value * relativeSize / 100f;
                    int realSize = (int)Math.Floor(sizeValue);
                    resulting[i] = (0, realSize);

                    if (realSize == 0)
                        continue;

                    if (size.IsRelative)
                    {
                        relativeSizeUsed += realSize;
                        InsertSort(descendingFractionalPart, i, sizeValue);
                    }
                    else
                        minSize += realSize + middleOffset;
                    sizeCounter += realSize + middleOffset;
                }
                if (minSize > defaultMinSize)
                    minSize -= middleOffset;
                if (minSize == 0)
                    minSize = 1;

                // There could be some unused relative size left since we are calculating relative size with Math.Floor
                // Adding +1 to relative sizes with the largest fractional parts.
                int j = 0;
                int sizeLeft = relativeSize - relativeSizeUsed;
                while (sizeLeft > 0 && j < descendingFractionalPart.Count)
                {
                    resulting[descendingFractionalPart[j++].Item1].Size++;
                    sizeLeft--;
                }

                // Here the sizes are already calculated finally. Calculating positions.
                sizeCounter = startOffset;
                for (int i = 0; i < sizes.Length; i++)
                {
                    int columnSize = resulting[i].Size;
                    resulting[i].Position = sizeCounter;
                    if (columnSize != 0)
                        sizeCounter += columnSize + middleOffset;
                }
            }

            #endregion
            #region InsertSort

            private void InsertSort(List<(int, float)> list, int index, float value)
            {
                value -= (float)Math.Floor(value);
                for (int i = 0; i < list.Count; i++)
                    if (list[i].Item2 < value)
                    {
                        list.Insert(i, (index, value));
                        return;
                    }
                list.Add((index, value));
            }

            #endregion
            #region CustomUpdate

            protected virtual void CustomUpdate()
            {
                Configuration.CustomUpdate?.Invoke(this);
            }

            #endregion
            #region UpdateChild

            /// <summary>
            /// Updates all Enabled child objects.
            /// </summary>
            /// <returns></returns>
            public virtual VisualObject UpdateChild()
            {
                lock (Child)
                    foreach (VisualObject child in ChildrenFromTop)
                        if (child.Enabled)
                            child.Update();
                return this;
            }

            #endregion

        #endregion
        #region Apply

            /// <summary>
            /// Draws everything related to this VisualObject incluing all child objects (directly changes tiles on tile provider).
            /// </summary>
            /// <returns></returns>
            public virtual VisualObject Apply()
            {
#if DEBUG
                if (!CalculateActive())
                    throw new InvalidOperationException("Trying to call Apply() an not active object.");
#endif

                lock (ApplyLocker)
                {
                    // Applying related to this node
                    ApplyThis();

                    // Recursive Apply call
                    ApplyChild();
                }

                return this;
            }

            #region ApplyThis

            /// <summary>
            /// Draws everything related to this particular VisualObject. Doesn't include drawing child objects.
            /// </summary>
            /// <returns></returns>
            public VisualObject ApplyThis()
            {
                lock (ApplyLocker)
                {
                    ApplyThisNative();
                    CustomApply();
                }
                return this;
            }

            #endregion
            #region ApplyThisNative

            /// <summary>
            /// By default draws tiles/walls and grid if UI.ShowGrid is true. Overwrite this method for own widgets drawing.
            /// Don't call this method directly, call ApplyThis() instead.
            /// </summary>
            protected virtual void ApplyThisNative()
            {
                //ForceSection = false;
                ApplyTiles();
                if (TUI.ShowGrid && Style.Grid != null)
                    ShowGrid();
            }

            #endregion
            #region ApplyTiles

            public VisualObject ApplyTiles()
            {
                lock (ApplyLocker)
                {
                    if (Style.Active == null && Style.InActive == null && Style.Tile == null && Style.TileColor == null
                            && Style.Wall == null && Style.WallColor == null)
                        return this;

                    foreach ((int x, int y) in Points)
                    {
                        dynamic tile = Tile(x, y);
                        if (tile == null)
                            continue;
                        ApplyTile(x, y, tile);
                    }
                }
                return this;
            }

            #endregion
            #region ApplyTile

            protected virtual void ApplyTile(int x, int y, dynamic tile)
            {
                if (Style.Active != null)
                    tile.active(Style.Active.Value);
                else if (Style.Tile != null)
                    tile.active(true);
                else if (Style.Wall != null)
                    tile.active(false);
                if (Style.InActive != null)
                    tile.inActive(Style.InActive.Value);
                if (Style.Tile != null)
                    tile.type = Style.Tile.Value;
                if (Style.TileColor != null)
                    tile.color(Style.TileColor.Value);
                if (Style.Wall != null)
                    tile.wall = Style.Wall.Value;
                if (Style.WallColor != null)
                    tile.wallColor(Style.WallColor.Value);
            }

            #endregion
            #region ShowGrid

            public void ShowGrid()
            {
                lock (ApplyLocker)
                {
                    for (int i = 0; i < Style.Grid.Columns.Length; i++)
                        for (int j = 0; j < Style.Grid.Lines.Length; j++)
                        {
                            (int columnX, int columnSize) = Style.Grid.ResultingColumns[i];
                            (int lineY, int lineSize) = Style.Grid.ResultingLines[j];
                            for (int x = columnX; x < columnX + columnSize; x++)
                                for (int y = lineY; y < lineY + lineSize; y++)
                                {
                                    dynamic tile = Tile(x, y);
                                    if (tile == null)
                                        continue;
                                    tile.wall = (byte)155;
                                    tile.wallColor((byte)(25 + (i + j) % 2));
                                }
                        }
                }
            }

            #endregion
            #region CustomApply

            public virtual VisualObject CustomApply()
            {
                lock (ApplyLocker)
                    Configuration.CustomApply?.Invoke(this);
                return this;
            }

            #endregion
            #region ApplyChild

            public virtual VisualObject ApplyChild()
            {
                lock (ApplyLocker)
                {
                    bool forceSection = ForceSection;
                    lock (Child)
                        foreach (VisualObject child in ChildrenFromBottom)
                            if (child.Active)
                            {
                                child.Apply();
                                forceSection = forceSection || child.ForceSection;
                            }
                    ForceSection = forceSection;
                }
                return this;
            }

            #endregion

        #endregion
        #region Draw

        public virtual VisualObject Draw(int dx = 0, int dy = 0, int width = -1, int height = -1, int userIndex = -1, int exceptUserIndex = -1, bool? forceSection = null, bool frame = true)
        {
            bool realForceSection = forceSection ?? ForceSection;
            (int ax, int ay) = AbsoluteXY();
#if DEBUG
            Console.WriteLine($"Draw ({Name}): {ax + dx}, {ay + dy}, {(width >= 0 ? width : Width)}, {(height >= 0 ? height : Height)}: {realForceSection}");
#endif
            TUI.DrawRect(this, ax + dx, ay + dy, width >= 0 ? width : Width, height >= 0 ? height : Height, realForceSection, userIndex, exceptUserIndex, frame);
            return this;
        }

        #endregion
        #region DrawPoints

        public virtual VisualObject DrawPoints(IEnumerable<(int, int)> points, int userIndex = -1, int exceptUserIndex = -1, bool? forceSection = null)
        {
            List<(int, int)> list = points.ToList();
            if (list.Count == 0)
                return this;

            int minX = list[0].Item1, minY = list[0].Item2;
            int maxX = minX, maxY = minY;

            foreach ((int x, int y) in list)
            {
                if (x < minX)
                    minX = x;
                if (x > maxX)
                    maxX = x;
                if (y < minY)
                    minY = y;
                if (y > maxY)
                    maxY = y;
            }

            return Draw(minX, minY, maxX - minX + 1, maxY - minY + 1, userIndex, exceptUserIndex, forceSection);
        }

        #endregion
        #region Clear

        public virtual VisualObject Clear()
        {
            foreach ((int x, int y) in Points)
                Tile(x, y)?.ClearEverything();
            return this;
        }

        #endregion

        #region Database

        public virtual void Database()
        {

        }

        public virtual void UserDatabase()
        {

        }

        #endregion
        #region Copy

        public VisualObject(VisualObject visualObject)
            : this(visualObject.X, visualObject.Y, visualObject.Width, visualObject.Height, new UIConfiguration(visualObject.Configuration),
                  new UIStyle(visualObject.Style), visualObject.Callback?.Clone() as Func<VisualObject, Touch, bool>)
        {
        }

        #endregion
    }
}
