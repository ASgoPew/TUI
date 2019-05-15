using System;
using System.Collections.Generic;
using System.Linq;
using TUI.Base.Style;

namespace TUI.Base
{
    public class VisualObject : Touchable
    {
        #region Data

        public UIStyle Style { get; set; }
        public VisualObject[,] Grid { get; set; }
        public GridCell Cell { get; private set; }
        public bool ForceSection { get; protected set; } = false;

        public override bool Orderable => !Style.Positioning.InLayout;
        public virtual string Name => GetType().Name;
        public string FullName =>
            Parent != null
                ? (Cell != null
                    ? $"{Parent.FullName}[{Cell.Column},{Cell.Line} = {Name}]"
                    : $"{Parent.FullName}[{IndexInParent} = {Name}]")
                : Name;

        #endregion

        #region IDOM

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
                        if (child != o && child.Enabled && o.Intersecting(child))
                            return true;
                return false;
            }

            #endregion

        #endregion

        #region Initialize

        public VisualObject(int x, int y, int width, int height, UIConfiguration configuration = null, UIStyle style = null, Func<VisualObject, Touch, bool> callback = null)
            : base(x, y, width, height, configuration, callback)
        {
            Style = style ?? new UIStyle();

            if (Style.Grid != null)
                SetupGrid(Style.Grid);
        }

        public VisualObject()
            : this(0, 0, 0, 0, new UIConfiguration() { UseBegin = false })
        {
            Style.Positioning.FullSize = FullSize.Both;
        }

        public VisualObject(UIConfiguration configuration)
            : this(0, 0, 0, 0, configuration)
        {
            Style.Positioning.FullSize = FullSize.Both;
        }

        public VisualObject(UIStyle style)
            : this(0, 0, 0, 0, new UIConfiguration() { UseBegin = false }, style)
        {
        }

        #endregion
        #region operator[,]

        public VisualObject this[int column, int line]
        {
            get => Grid[column, line];
            set
            {
                if (value != null)
                {
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
        #region ToString

        public override string ToString() => FullName;

        #endregion
        #region SetupPositioning

        public VisualObject SetupPositioning(PositioningStyle positioning)
        {
            Style.Positioning = positioning;
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
        #region SetFullSize

        public VisualObject SetFullSize(bool horizontal = true, bool vertical = true)
        {
            if (horizontal && vertical)
                Style.Positioning.FullSize = FullSize.Both;
            else if (horizontal)
                Style.Positioning.FullSize = FullSize.Horizontal;
            else if (vertical)
                Style.Positioning.FullSize = FullSize.Vertical;
            else
                Style.Positioning.FullSize = FullSize.None;
            return this;
        }

        #endregion
        #region LayoutSkip

        public VisualObject LayoutSkip(ushort value)
        {
            Style.Layout.ObjectsOffset = value;
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

            protected virtual void PulseThisNative(PulseType type) { }

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

            protected virtual void UpdateThisNative()
            {
                if (Root == null)
                    Root = GetRoot() as RootVisualObject;
                UpdateFullSize();
                UpdateLayout();
                if (Style.Grid != null)
                    UpdateGrid();
            }

            #endregion
            #region UpdateFullSize

            public VisualObject UpdateFullSize()
            {
                ExternalOffset offset = Style.Layout.Offset ?? UIDefault.ExternalOffset;
                int layoutX = offset.Left, layoutY = offset.Up;
                int layoutW = Width - layoutX - offset.Right, layoutH = Height - layoutY - offset.Down;
                lock (Child)
                    foreach (VisualObject child in ChildrenFromTop)
                    {
                        FullSize fullSize = child.Style.Positioning.FullSize;
                        if (fullSize == FullSize.None)
                            continue;

                        // If InLayout then FullSize should match parent size minus layout offset
                        int x = 0, y = 0, width = Width, height = Height;
                        if (child.Style.Positioning.InLayout)
                        {
                            x = layoutX;
                            y = layoutY;
                            width = layoutW;
                            height = layoutH;
                        }

                        if (fullSize == FullSize.Both)
                            child.SetXYWH(x, y, width, height);
                        else if (fullSize == FullSize.Horizontal)
                            child.SetXYWH(x, child.Y, width, child.Height);
                        else if (fullSize == FullSize.Vertical)
                            child.SetXYWH(child.X, y, child.Width, height);
                        //Console.WriteLine($"FullSize: {child.FullName}, {child.XYWH()}");
                    }
                return this;
            }

            #endregion
            #region UpdateLayout

            public void UpdateLayout()
            {
                GridStyle parentGridStyle = Parent?.Style?.Grid;
                ExternalOffset offset = Style.Layout.Offset ?? parentGridStyle?.DefaultOffset ?? UIDefault.ExternalOffset;
                Alignment alignment = Style.Layout.Alignment ?? parentGridStyle?.DefaultAlignment ?? UIDefault.Alignment;
                Direction direction = Style.Layout.Direction ?? parentGridStyle?.DefaultDirection ?? UIDefault.Direction;
                Side side = Style.Layout.Side ?? parentGridStyle?.DefaultSide ?? UIDefault.Side;
                int indent = Style.Layout.ChildIndent ?? Parent?.Style?.Grid?.DefaultChildIndent ?? UIDefault.CellsIndent;

                (int layoutW, int layoutH, List<VisualObject> layoutChild) = CalculateLayoutSize(direction, indent);
                layoutChild = layoutChild.Skip(Style.Layout.ObjectsOffset).ToList();
                if (layoutChild.Count == 0)
                    return;

                // Calculating cell objects position
                int sx, sy;

                // Initializing sx
                if (alignment == Alignment.UpLeft || alignment == Alignment.Left || alignment == Alignment.DownLeft)
                    sx = offset.Left;
                else if (alignment == Alignment.UpRight || alignment == Alignment.Right || alignment == Alignment.DownRight)
                    sx = Width - offset.Right - layoutW;
                else
                    sx = (Width - layoutW + 1) / 2;

                // Initializing sy
                if (alignment == Alignment.UpLeft || alignment == Alignment.Up || alignment == Alignment.UpRight)
                    sy = offset.Up;
                else if (alignment == Alignment.DownLeft || alignment == Alignment.Down || alignment == Alignment.DownRight)
                    sy = Height - offset.Down - layoutH;
                else
                    sy = (Height - layoutH + 1) / 2;

                // Updating cell objects padding
                int cx = direction == Direction.Left ? layoutW - layoutChild[0].Width : 0;
                int cy = direction == Direction.Up ? layoutH - layoutChild[0].Height : 0;
                for (int k = 0; k < layoutChild.Count; k++)
                {
                    VisualObject child = layoutChild[k];
                    // Calculating side alignment
                    int sideDeltaX = 0, sideDeltaY = 0;
                    if (direction == Direction.Left)
                    {
                        if (side == Side.Left)
                            sideDeltaY = layoutH - child.Height;
                        else if (side == Side.Center)
                            sideDeltaY = (layoutH - child.Height) / 2;
                    }
                    else if (direction == Direction.Right)
                    {
                        if (side == Side.Right)
                            sideDeltaY = layoutH - child.Height;
                        else if (side == Side.Center)
                            sideDeltaY = (layoutH - child.Height) / 2;
                    }
                    else if (direction == Direction.Up)
                    {
                        if (side == Side.Right)
                            sideDeltaX = layoutW - child.Width;
                        else if (side == Side.Center)
                            sideDeltaX = (layoutW - child.Width) / 2;
                    }
                    else if (direction == Direction.Down)
                    {
                        if (side == Side.Left)
                            sideDeltaX = layoutW - child.Width;
                        else if (side == Side.Center)
                            sideDeltaX = (layoutW - child.Width) / 2;
                    }

                    child.SetXY(sx + cx + sideDeltaX, sy + cy + sideDeltaY);
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
            }

            #endregion
            #region CalculateLayoutSize

            private (int, int, List<VisualObject>) CalculateLayoutSize(Direction direction, int indent)
            {
                // Calculating total objects width and height
                int totalW = 0, totalH = 0;
                List<VisualObject> layoutChild = new List<VisualObject>();
                lock (Child)
                    foreach (VisualObject child in ChildrenFromBottom)
                    {
                        FullSize fullSize = child.Style.Positioning.FullSize;
                        if (!child.Enabled || !child.Style.Positioning.InLayout || fullSize == FullSize.Both
                                || (fullSize == FullSize.Horizontal && (direction == Direction.Left || direction == Direction.Right))
                                || (fullSize == FullSize.Vertical && (direction == Direction.Up || direction == Direction.Down)))
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
            #region UpdateGrid

            public VisualObject UpdateGrid()
            {
                if (Style.Grid == null)
                    return this;
                if (Grid == null)
                    SetupGrid();

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

                return this;
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

            public virtual VisualObject CustomUpdate()
            {
                Configuration.CustomUpdate?.Invoke(this);
                return this;
            }

            #endregion
            #region UpdateChild

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

            public virtual VisualObject Apply()
            {
                if (!Active())
                    throw new InvalidOperationException("Trying to call Apply() an not active object.");

                // Applying related to this node
                ApplyThis();

                // Recursive Apply call
                ApplyChild();

                return this;
            }

            #region ApplyThis

            public VisualObject ApplyThis()
            {
                ApplyThisNative();
                CustomApply();
                return this;
            }

            #endregion
            #region ApplyThisNative

            protected virtual void ApplyThisNative()
            {
                ForceSection = false;
                ApplyTiles();
                if (UI.ShowGrid && Style.Grid != null)
                    ShowGrid();
            }

            #endregion
            #region ApplyTiles

            public virtual VisualObject ApplyTiles()
            {
                if (Style.Active == null && Style.InActive == null && Style.Tile == null && Style.TileColor == null
                    && Style.Wall == null && Style.WallColor == null)
                    return this;

                foreach ((int x, int y) in ProviderPoints)
                {
                    dynamic tile = Provider[x, y];
                    if (tile == null)
                        throw new NullReferenceException($"tile is null: {x}, {y}");
                    if (Style.Active != null)
                        tile.active(Style.Active.Value);
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
                return this;
            }

            #endregion
            #region ShowGrid

            public void ShowGrid()
            {
                (int sx, int sy) = ProviderXY();
                for (int i = 0; i < Style.Grid.Columns.Length; i++)
                    for (int j = 0; j < Style.Grid.Lines.Length; j++)
                    {
                        (int columnX, int columnSize) = Style.Grid.ResultingColumns[i];
                        (int lineY, int lineSize) = Style.Grid.ResultingLines[j];
                        for (int x = columnX; x < columnX + columnSize; x++)
                            for (int y = lineY; y < lineY + lineSize; y++)
                            {
                                dynamic tile = Provider[sx + x, sy + y];
                                tile.wall = (byte)155;
                                tile.wallColor((byte)(25 + (i + j) % 2));
                            }
                    }
            }

            #endregion
            #region CustomApply

            public virtual VisualObject CustomApply()
            {
                Configuration.CustomApply?.Invoke(this);
                return this;
            }

            #endregion
            #region ApplyChild

            public virtual VisualObject ApplyChild()
            {
                bool forceSection = ForceSection;
                lock (Child)
                    foreach (VisualObject child in ChildrenFromBottom)
                        if (child.Enabled)
                        {
                            child.Apply();
                            forceSection = forceSection || child.ForceSection;
                        }
                ForceSection = forceSection;
                return this;
            }

            #endregion

        #endregion
        #region Draw

        public virtual VisualObject Draw(int dx = 0, int dy = 0, int width = -1, int height = -1, int userIndex = -1, int exceptUserIndex = -1, bool? forceSection = null, bool frame = true)
        {
            bool realForceSection = forceSection ?? ForceSection;
            (int ax, int ay) = AbsoluteXY();
            Console.WriteLine($"Draw ({Name}): {ax + dx}, {ay + dy}, {(width >= 0 ? width : Width)}, {(height >= 0 ? height : Height)}: {realForceSection}");
            UI.DrawRect(ax + dx, ay + dy, width >= 0 ? width : Width, height >= 0 ? height : Height, realForceSection, userIndex, exceptUserIndex, frame);
            return this;
        }

        #endregion
        #region DrawPoints

        public virtual VisualObject DrawPoints(List<(int, int)> list, int userIndex = -1, int exceptUserIndex = -1, bool? forceSection = null)
        {
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
            foreach ((int x, int y) in ProviderPoints)
                Provider[x, y].ClearEverything();
            return this;
        }

        #endregion

        #region Popup

        public virtual VisualObject Popup()
        {
            VisualObject popup = this["popup"] as VisualObject;
            if (popup != null)
                popup.Enable();
            else
            {
                popup = new VisualObject(0, 0, 0, 0, null, null, (self, touch) => Popdown() == this).SetFullSize();
                this["popup"] = Add(popup);
                Update();
            }
            return popup;
        }

        public virtual VisualObject Popdown()
        {
            (this["popup"] as VisualObject).Disable();
            return Apply().Draw();
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
