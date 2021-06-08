using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Base
{
    /// <summary>
    /// Basic TUI object. Every TUI object is VisualObject or is an object of class inherited from VisualObject.
    /// </summary>
    public class VisualObject : Touchable
    {
        #region Data

        /// <summary>
        /// Object tile and wall style.
        /// </summary>
        public UIStyle Style { get; set; }
        /// <summary>
        /// Objects draw with SentTileSquare by default. Set this field to force drawing this object with SendSection.
        /// </summary>
        public bool DrawWithSection { get; set; } = false;
        /// <summary>
        /// Frame the section if <see cref="DrawWithSection"/> is set. True by default.
        /// </summary>
        public bool FrameSection { get; set; } = true;
        /// <summary>
        /// Whether the object is visible. Object becomes invisible fox example when it is outside of bounds of layout.
        /// </summary>
        public bool Visible { get; private set; } = true;
        private bool _DrawMode = false;
        public bool IsActiveThis => Enabled && Visible && Loaded && !Disposed;
        public virtual bool IsActive => IsActiveThis && (this is RootVisualObject || Parent?.IsActive == true);
        protected bool InDrawMode => _DrawMode && (this is RootVisualObject || Parent?.InDrawMode == true);

        //protected bool CanSend => Root?.DrawState > 0;
        private string _Name;
        protected object ApplyLocker = new object();
        /// <summary>
        /// Object name. Class type name by default.
        /// </summary>
        public virtual string Name
        {
            get => _Name ?? GetType().Name;
            set => _Name = value;
        }
        /// <summary>
        /// Object full name (names of all objects up to the Root).
        /// </summary>
        public string FullName =>
            Parent == null
                ? Name
                : _Name != null
                    ? $"{Parent.FullName}.{_Name}"
                    : Positioning is InGrid inGrid
                        ? $"{Parent.FullName}[{inGrid.Column},{inGrid.Line}].{Name}"
                        : $"{Parent.FullName}[{IndexInParent}].{Name}";

        private int? _MinWidth;
        private int? _MinHeight;
        protected virtual int MinWidth => Math.Max(_MinWidth ?? 0, GridConfiguration != null ? MinGridWidth() : 0);
        protected virtual int MinHeight => Math.Max(_MinHeight ?? 0, GridConfiguration != null ? MinGridHeight() : 0);

        protected IPositioning Positioning { get; set; }
        protected IResizing WidthResizing { get; set; }
        protected IResizing HeightResizing { get; set; }

        /// <summary>
        /// Child objects positioning in Layout. Not set by default (null).
        /// <para></para>
        /// Use <see cref="VisualObject.SetupLayout"/>
        /// </summary>
        public LayoutConfiguration LayoutConfiguration { get; private set; }
        /// <summary>
        /// Child objects positioning in grid. Not set by default (null).
        /// <para></para>
        /// Use <see cref="VisualObject.SetupGrid"/> to initialize grid.
        /// </summary>
        public GridConfiguration GridConfiguration { get; private set; }

        public bool HasLayout => LayoutConfiguration != null;
        public bool HasGrid => GridConfiguration != null;
        public bool HasWidthChildStretch => WidthResizing is InChildStretch;
        public bool HasHeightChildStretch => HeightResizing is InChildStretch;
        public bool HasChildStretch => HasWidthChildStretch || HasHeightChildStretch;
        public bool HasParentAlignment => Positioning is InParentAlignment;
        public bool HasWidthParentStretch => WidthResizing is InParentStretch;
        public bool HasHeightParentStretch => HeightResizing is InParentStretch;
        public bool HasParentStretch => HasWidthParentStretch || HasHeightParentStretch;
        public bool InLayout => Positioning is InLayout;
        public bool InGrid => Positioning is InGrid;
        public bool InGridDynamic => Positioning is InGrid p &&
            (Parent.GridConfiguration.Columns[p.Column].IsDynamic ||
            Parent.GridConfiguration.Lines[p.Line].IsDynamic);
        public bool InGridRelative => Positioning is InGrid p &&
            (Parent.GridConfiguration.Columns[p.Column].IsRelative ||
            Parent.GridConfiguration.Lines[p.Line].IsRelative);
        // TODO: if only last column/line is Relative then false;
        public bool HasRelativesInGrid =>
            GridConfiguration.Columns.Any(column => column.IsRelative) ||
            GridConfiguration.Lines.Any(line => line.IsRelative);

        protected virtual bool RepositionRequiresParentResize => Parent != null &&
            (Parent.HasWidthChildStretch && X + Width > Parent.Width ||
            Parent.HasHeightChildStretch && Y + Height > Parent.Height);

        protected virtual bool ResizeRequiresParentResize => Parent != null &&
            (Parent.HasLayout && InLayout ||
            Parent.HasGrid && InGridDynamic ||
            Parent.HasWidthChildStretch && X + Width > Parent.Width ||
            Parent.HasHeightChildStretch && Y + Height > Parent.Height);

        protected virtual bool ResizeRequiresChildrenRepositionAndResize =>
            HasLayout ||
            HasGrid && HasRelativesInGrid ||
            Child.Any(child => child.HasParentAlignment) ||
            Child.Any(child => child.HasParentStretch);

        /*protected virtual bool ResizeRequiresChildrenReposition =>
            HasLayout ||
            Child.Any(child => child.HasParentAlignment) ||
            HasGrid && HasRelativesInGrid;
        protected virtual bool ResizeRequiresChildrenResize =>
            Child.Any(child => child.HasParentStretch) ||
            HasGrid && HasRelativesInGrid;*/

        protected virtual bool AddRequiresResize(VisualObject child) =>
            child.InGridDynamic ||
            child.InGridRelative ||
            HasWidthChildStretch && (child.X + child.Width > Width) ||
            HasHeightChildStretch && (child.Y + child.Height > Height);

        protected virtual bool AddRequiresChildrenRepositionAndResize(VisualObject child) =>
            child.InLayout ||
            child.InGridDynamic;

        #endregion

        #region Add

        public override T Add<T>(T child, int? layer = null)
        {
            base.Add(child, layer);

            if (IsActive)
                child.Update();

            hmmmmm

            if (AddRequiresResize(child))
                UpdateSize();
            if (AddRequiresChildrenRepositionAndResize(child))
                UpdateChildrenPositionAndSize();

            // Update grid min sizes
            SetXY(0, 0);

            return child;
        }

        #endregion
        #region Remove

        public override VisualObject Remove(VisualObject child)
        {
            VisualObject result = base.Remove(child);
            if (GridConfiguration is GridConfiguration grid)
            {
                grid.MinWidth = MinGridWidth();
                grid.MinHeight = MinGridHeight();
            }
            return result;
        }

        #endregion
        #region SetTop

        public override bool SetTop(VisualObject child)
        {
            if (base.SetTop(child) && IsActive &&
                Child.Any(o => o != child &&
                    o.IsActiveThis &&
                    o.Intersecting(child)))
            {
                child.Apply().Draw();
                return true;
            }
            return false;
        }

        #endregion
        #region LoadThisNative

        protected override void LoadThisNative()
        {
            base.LoadThisNative();
            DrawMode();
        }

        #endregion

        #region Constructor

        public VisualObject(int x, int y, int width, int height, UIConfiguration configuration = null,
                UIStyle style = null, Action<VisualObject, Touch> callback = null)
            : base(x, y, width, height, configuration, callback)
        {
            Style = style ?? new UIStyle();
        }

        #endregion
        #region Copy

        public VisualObject(VisualObject visualObject)
            : this(visualObject.X, visualObject.Y, visualObject.Width, visualObject.Height, new UIConfiguration(visualObject.Configuration),
                  new UIStyle(visualObject.Style), visualObject.Callback?.Clone() as Action<VisualObject, Touch>)
        {
        }

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
        public IEnumerable<VisualObject> this[int column, int line]
        {
            get => Child.Where(child => child.Positioning is InGrid positioning &&
                (positioning.Column == column || column == -1) &&
                (positioning.Line == line || line == -1));
            set
            {
                if (column < 0 || line < 0 || column >= GridConfiguration.Columns.Length || line >= GridConfiguration.Lines.Length)
                    throw new IndexOutOfRangeException("Wrong grid column or line number");
                foreach (var child in value)
                    AddToGrid(child, column, line);
            }
        }

        #endregion
        #region Tile

        /// <summary>
        /// Returns tile relative to this node point (x=0, y=0 is a top left point of this object)
        /// </summary>
        /// <param name="x">x coordinate counting from left node border</param>
        /// <param name="y">y coordinate counting from top node border</param>
        /// <returns>ITile</returns>
        public virtual dynamic Tile(int x, int y)
        {
            ExternalIndent bounds = Bounds;
            if (x < bounds.Left || x >= bounds.Left + bounds.Right || y < bounds.Up || y >= bounds.Up + bounds.Down)
                return null;
            return Provider?[ProviderX + x, ProviderY + y];
        }

        #endregion
        #region Enable

        public override VisualObject Enable()
        {
            if (!Enabled)
            {
                base.Enable();
                if (IsActive && InDrawMode)
                    DrawEnable();
            }
            return this;
        }

        #endregion
        #region Disable

        public override VisualObject Disable()
        {
            if (Enabled)
            {
                base.Disable();
                if (IsActive && InDrawMode)
                    DrawDisable();
            }
            return this;
        }

        #endregion

        #region SetXYWH

        /// <summary>
        /// Use this method to change object position or/and size.
        /// </summary>
        /// <param name="x">New x coordinate</param>
        /// <param name="y">New y coordinate</param>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        /// <returns>Most far ancestor that has changed size</returns>
        public override VisualObject SetXYWH(int x, int y, int width, int height)
        {
            if (GridConfiguration is GridConfiguration grid)
            {
                grid.MinWidth = MinGridWidth();
                grid.MinHeight = MinGridHeight();
            }

            width = Math.Max(width, MinWidth);
            height = Math.Max(height, MinHeight);

            int oldX = X, oldY = Y, oldWidth = Width, oldHeight = Height;
            if (oldX != x || oldY != y || oldWidth != width || oldHeight != height)
            {
                base.SetXYWH(x, y, width, height);

                if (oldX != X || oldY != Y)
                {
                    if (RepositionRequiresParentResize)
                        Parent.UpdateSize(); // parent's stretch
                }
                if (oldWidth != Width || oldHeight != Height)
                {
                    if (ResizeRequiresParentResize)
                    {
                        Parent.UpdateSize(); // parent's layout AND parent's grid AND parent's stretch
                        update_parents_children
                    }
                    if (ResizeRequiresChildrenRepositionAndResize)
                        UpdateChildrenPositionAndSize(); // children's ParentStretch AND children's ParentAlignment AND layout AND grid
                    //if (ResizeRequiresChildrenResize)
                        //UpdateChildrenSize(); // children's ParentStretch AND grid
                    //if (ResizeRequiresChildrenReposition)
                        //UpdateChildrenPosition(); // children's ParentAlignment AND layout AND grid
                }

                // Update apply tile bounds
                UpdateBounds();

                if (IsActive && InDrawMode)
                {
                    // Update entities
                    // TODO: entities reposition should happen at Draw()
                    Pulse(PulseType.SetXYWH);
                    DrawReposition(oldX, oldY, oldWidth, oldHeight);
                }
            }

            return this;
        }

        #endregion
        #region DrawMode

        public VisualObject DrawMode()
        {
            _DrawMode = true;
            return this;
        }

        #endregion
        #region NoDrawMode

        public VisualObject NoDrawMode()
        {
            _DrawMode = false;
            return this;
        }

        #endregion
        #region UpdateSize

        protected virtual void UpdateSize()
        {
            if (HasChildStretch)
                ChildStretch();
        }

        #endregion
        #region ChildStretch

        protected void ChildStretch()
        {
            int width = 0;
            int height = 0;
            foreach (var child in Child)
            {
                if (child.HasParentAlignment)
                    throw new InvalidOperationException($"Attempt to ChildStretch while child has ParentAlignment: {FullName}");
                if (HasWidthChildStretch && child.HasWidthParentStretch)
                    throw new InvalidOperationException($"Attempt to WidthChildStretch while child has WidthParentStretch: {FullName}");
                if (HasHeightChildStretch && child.HasHeightParentStretch)
                    throw new InvalidOperationException($"Attempt to HeightChildStretch while child has HeightParentStretch: {FullName}");
                width = Math.Max(width, child.X + child.Width);
                height = Math.Max(height, child.Y + child.Height);
            }
            if (HasWidthChildStretch && HasHeightChildStretch)
                SetWH(width, height);
            else if (HasWidthChildStretch)
                SetWH(width, Height);
            else if (HasHeightChildStretch)
                SetWH(Width, height);
            throw new InvalidOperationException("Trying to ChildStretch object without setup");
        }

        #endregion
        #region UpdateChildrenPositionAndSize

        protected virtual void UpdateChildrenPositionAndSize()
        {
            NoDrawMode(); // ?????????????????????????????????????????????????

            UpdateParentStretch();
            UpdateChildrenParentAlignment();
            if (HasLayout)
                UpdateLayout();
            if (HasGrid)
                UpdateGrid();

            DrawMode();
        }

        #endregion
        #region UpdateParentStretch

        private void UpdateParentStretch()
        {
            foreach (VisualObject child in Child)
            {
                if (child.HasWidthParentStretch)
                {
                    if (HasWidthChildStretch)
                        throw new InvalidOperationException($"Attempt to WidthParentStretch child while having WidthChildStretch: {FullName}");
                    child.SetWH(Width, child.Height);
                }
                if (child.HasHeightParentStretch)
                {
                    if (HasHeightChildStretch)
                        throw new InvalidOperationException($"Attempt to HeightParentStretch child while having HeightChildStretch: {FullName}");
                    child.SetWH(child.Width, Height);
                }
            }
        }

        #endregion
        #region UpdateParentAlignment

        private void UpdateChildrenParentAlignment()
        {
            foreach (var child in Child)
                if (child.HasParentAlignment)
                    child.ParentAlignment();
        }

        #endregion
        #region ParentAlignment

        private void ParentAlignment()
        {
            if (Parent.HasChildStretch)
                throw new InvalidOperationException($"Attempt to ParentAlignment while parent has ChildStretch: {FullName}");
            InParentAlignment positioning = (InParentAlignment)Positioning;
            ExternalIndent indent = positioning.Indent;
            Alignment alignment = positioning.Alignment;
            int x, y;
            if (alignment == Alignment.UpLeft || alignment == Alignment.Left || alignment == Alignment.DownLeft)
                x = indent.Left;
            else if (alignment == Alignment.UpRight || alignment == Alignment.Right || alignment == Alignment.DownRight)
                x = Parent.Width - indent.Right - Width;
            else
                x = (int)Math.Floor((Parent.Width - indent.Left - indent.Right - Width) / 2f) + indent.Left;

            if (alignment == Alignment.UpLeft || alignment == Alignment.Up || alignment == Alignment.UpRight)
                y = indent.Up;
            else if (alignment == Alignment.DownLeft || alignment == Alignment.Down || alignment == Alignment.DownRight)
                y = Parent.Height - indent.Down - Height;
            else
                y = (int)Math.Floor((Parent.Height - indent.Up - indent.Down - Height) / 2f) + indent.Up;

            SetXY(x, y);
        }

        #endregion
        #region UpdateLayout

        private void UpdateLayout()
        {
            ExternalIndent indent = LayoutConfiguration.Indent;
            Alignment alignment = LayoutConfiguration.Alignment;
            Direction direction = LayoutConfiguration.Direction;
            Side side = LayoutConfiguration.Side;
            int offset = LayoutConfiguration.ChildOffset;
            int layoutIndent = LayoutConfiguration.LayoutOffset;

            (int abstractLayoutW, int abstractLayoutH, List<VisualObject> layoutChild) = CalculateLayoutSize(direction, offset);

            // Calculating layout box position
            int layoutX, layoutY, layoutW, layoutH;

            // Initializing sx
            if (alignment == Alignment.UpLeft || alignment == Alignment.Left || alignment == Alignment.DownLeft)
                layoutX = indent.Left;
            else if (alignment == Alignment.UpRight || alignment == Alignment.Right || alignment == Alignment.DownRight)
                layoutX = Width - indent.Right - abstractLayoutW;
            else
                layoutX = (Width - abstractLayoutW + 1) / 2;
            layoutX = Math.Max(layoutX, indent.Left);
            layoutW = Math.Min(abstractLayoutW, Width - indent.Left - indent.Right);

            // Initializing sy
            if (alignment == Alignment.UpLeft || alignment == Alignment.Up || alignment == Alignment.UpRight)
                layoutY = indent.Up;
            else if (alignment == Alignment.DownLeft || alignment == Alignment.Down || alignment == Alignment.DownRight)
                layoutY = Height - indent.Down - abstractLayoutH;
            else
                layoutY = (Height - abstractLayoutH + 1) / 2;
            layoutY = Math.Max(layoutY, indent.Up);
            layoutH = Math.Min(abstractLayoutH, Height - indent.Up - indent.Down);

            // Updating cell objects padding
            int cx = direction == Direction.Left
                ? Math.Min(abstractLayoutW - layoutChild[0].Width, Width - layoutX - indent.Right - layoutChild[0].Width)
                : 0;
            int cy = direction == Direction.Up
                ? Math.Min(abstractLayoutH - layoutChild[0].Height, Height - layoutY - indent.Down - layoutChild[0].Height)
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

                child.SetXY(resultX, resultY);

                if (k == layoutChild.Count - 1)
                    break;

                if (direction == Direction.Right)
                    cx = cx + offset + child.Width;
                else if (direction == Direction.Left)
                    cx = cx - offset - layoutChild[k + 1].Width;
                else if (direction == Direction.Down)
                    cy = cy + offset + child.Height;
                else if (direction == Direction.Up)
                    cy = cy - offset - layoutChild[k + 1].Height;
            }

            if (direction == Direction.Left || direction == Direction.Right)
                LayoutConfiguration.OffsetLimit = abstractLayoutW - layoutW;
            else if (direction == Direction.Up || direction == Direction.Down)
                LayoutConfiguration.OffsetLimit = abstractLayoutH - layoutH;
        }

        #endregion
        #region CalculateLayoutSize

        private (int absoluteLayoutW, int absoluteLayoutH, List<VisualObject> objects) CalculateLayoutSize(
            Direction direction, int offset)
        {
            // Calculating total objects width and height
            int totalW = 0, totalH = 0;
            List<VisualObject> layoutChild = new List<VisualObject>();
            foreach (VisualObject child in ChildrenFromBottom)
            {
                if (!child.Enabled || !child.InLayout)
                    //|| (fullSize == FullSize.Horizontal && (direction == Direction.Left || direction == Direction.Right))
                    //|| (fullSize == FullSize.Vertical && (direction == Direction.Up || direction == Direction.Down)))
                    continue;

                layoutChild.Add(child);
                if (direction == Direction.Left || direction == Direction.Right)
                {
                    if (child.Height > totalH)
                        totalH = child.Height;
                    totalW += child.Width + offset;
                }
                else if (direction == Direction.Up || direction == Direction.Down)
                {
                    if (child.Width > totalW)
                        totalW = child.Width;
                    totalH += child.Height + offset;
                }
            }
            if ((direction == Direction.Left || direction == Direction.Right) && totalW > 0)
                totalW -= offset;
            if ((direction == Direction.Up || direction == Direction.Down) && totalH > 0)
                totalH -= offset;

            return (totalW, totalH, layoutChild);
        }

        #endregion
        #region UpdateGrid

        private void UpdateGrid()
        {
            Indent indent = GridConfiguration.Indent;
            ISize[] columnSizes = GridConfiguration.Columns;
            ISize[] lineSizes = GridConfiguration.Lines;
            CalculateSizes(columnSizes, Width, indent.Left, indent.Horizontal, indent.Right,
                ref GridConfiguration.ResultingColumns, ref GridConfiguration.MinWidth, true);
            CalculateSizes(lineSizes, Height, indent.Up, indent.Vertical, indent.Down,
                ref GridConfiguration.ResultingLines, ref GridConfiguration.MinHeight, false);

            // Main cell loop
            for (int i = 0; i < columnSizes.Length; i++)
            {
                (int columnX, int columnSize) = GridConfiguration.ResultingColumns[i];
                if (columnSizes[i].IsDynamic)
                    columnSize = -1;
                for (int j = 0; j < lineSizes.Length; j++)
                {
                    (int lineY, int lineSize) = GridConfiguration.ResultingLines[j];
                    if (lineSizes[j].IsDynamic)
                        lineSize = -1;
                    foreach (var cell in this[i, j])
                        if (cell != null)
                            cell.SetXYWH(columnX, lineY,
                                columnSize >= 0 && cell.Positioning == cell.WidthResizing ? columnSize : cell.Width,
                                lineSize >= 0 && cell.Positioning == cell.HeightResizing ? lineSize : cell.Height);
                }
            }
        }

        #endregion
        #region CalculateSizes

        private void CalculateSizes(ISize[] sizes, int absoluteSize, int startIndent, int middleIndent, int endIndent, ref (int Position, int Size)[] resulting, ref int minSize, bool isWidth)
        {
            // Initializing resulting array
            resulting = new (int, int)[sizes.Length];

            // First calculating absolute, relative sums and the number of non-zero sizes
            int absoluteSum = 0;
            int relativeSum = 0;
            int notZeroSizes = 0;
            for (int i = 0; i < sizes.Length; i++)
            {
                int value;
                ISize size = sizes[i];
                if (size.IsDynamic)
                {
                    int max = 0;
                    if (isWidth)
                        for (int line = 0; line < GridConfiguration.Lines.Count(); line++)
                            max = Math.Max(max, this[i, line].Max(o => o.Width));
                    else
                        for (int column = 0; column < GridConfiguration.Columns.Count(); column++)
                            max = Math.Max(max, this[column, i].Max(o => o.Height));
                    value = max;
                    ((Dynamic)size).Value = value;
                    absoluteSum += value;
                }
                else
                {
                    value = size.Value;
                    if (size.IsAbsolute)
                        absoluteSum += value;
                    else
                        relativeSum += value;
                }
                if (value > 0)
                    notZeroSizes++;
            }
            // Every non-zero size pair means that the middleIndent between them is a must have
            minSize = startIndent + (notZeroSizes - 1) * middleIndent + endIndent;

            // Checking if absolute sizes sum is not more than object allowed size
            int absoluteSpace = absoluteSize - minSize;
            if (absoluteSpace < absoluteSum)
            {
                string sizeName = isWidth ? "width" : "height";
                throw new ArgumentException(
                    $"{FullName} (UpdateGrid): absolute {sizeName} ({absoluteSum}) is more than {sizeName} allowed space ({absoluteSpace}), (object {sizeName}: {absoluteSize}): {FullName}");
            }
            // Checking if relative sizes sum is not more than 100%
            if (relativeSum > 100)
            {
                string sizeName = isWidth ? "width" : "height";
                throw new ArgumentException(
                    $"{FullName} (UpdateGrid): relative {sizeName} ({relativeSum}) is more than 100: {FullName}");
            }

            // Now calculating actual column/line sizes
            int relativeSpace = absoluteSpace - absoluteSum;
            int relativeSpaceUsed = 0;
            List<(int, float)> descendingFractionalPart = new List<(int, float)>();
            for (int i = 0; i < sizes.Length; i++)
            {
                ISize size = sizes[i];
                if (size.Value == 0)
                    continue;

                int realSize;
                if (size.IsRelative)
                {
                    realSize = (int)Math.Floor(size.Value * relativeSpace / 100f);
                    relativeSpaceUsed += realSize;
                    InsertSort(descendingFractionalPart, i, size.Value * relativeSpace / 100f);
                }
                else
                {
                    realSize = size.Value;
                    minSize += realSize;
                    if (i < sizes.Length - 1)
                        minSize += middleIndent;
                }

                resulting[i] = (0, realSize);
            }

            // Now we have a final minSize
            if (minSize < 1)
                minSize = 1;

            // There could be some unused relative size left since we are calculating relative size with Math.Floor
            // Adding +1 to relative sizes with the largest fractional parts
            int j = 0;
            int sizeLeft = relativeSpace - relativeSpaceUsed;
            while (sizeLeft > 0)
            {
                resulting[descendingFractionalPart[j++].Item1].Size++;
                if (j >= descendingFractionalPart.Count - 1)
                    j = 0;
                sizeLeft--;
            }

            // Here the sizes are already calculated finally, calculating positions
            int sizeCounter = startIndent;
            for (int i = 0; i < sizes.Length; i++)
            {
                resulting[i].Position = sizeCounter;
                int columnSize = resulting[i].Size;
                if (columnSize > 0)
                    sizeCounter += columnSize + middleIndent;
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
        #region UpdateBounds

        /// <summary>
        /// Calculate Bounds for this node (intersection of Parent's layout indent/alignment indent and Parent's Bounds)
        /// </summary>
        private void UpdateBounds()
        {
            // Intersecting bounds with parent's Bounds
            ExternalIndent bounds = new ExternalIndent() { Left = 0, Up = 0, Right = Width, Down = Height };
            int deltaX = X;
            int deltaY = Y;
            for (VisualObject node = Parent; node != null; node = node.Parent)
            {
                ExternalIndent pBounds = node.Bounds;
                if (Intersect(0, 0, Width, Height, pBounds.Left - deltaX, pBounds.Up - deltaY, pBounds.Right - deltaX, pBounds.Down - deltaY,
                    out int x, out int y, out int width, out int height))
                {
                    bounds.Left = x;
                    bounds.Right = y;
                    bounds.Right = width;
                    bounds.Down = height;
                }
                else
                {
                    bounds.Left = 0;
                    bounds.Right = 0;
                    bounds.Right = 0;
                    bounds.Down = 0;
                    Visible = false;
                    return;
                }
                deltaX += node.X;
                deltaY += node.Y;
            }
            Bounds = bounds;
            Visible = true;
        }

        #endregion
        #region Intersect

        //public static bool Intersecting(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2) =>
            //x1 < x2 + w2 && y1 < y2 + h2 && x2 < x1 + w1 && y2 < y1 + h1;

        public static bool Intersect(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2, out int x3, out int y3, out int w3, out int h3)
        {
            x3 = Math.Max(x1, x2);
            y3 = Math.Max(y1, y2);
            w3 = Math.Min(x1 + w1, x2 + w2) - x3;
            h3 = Math.Min(y1 + h1, y2 + h2) - y3;
            return w3 > 0 && h3 > 0;
        }

        #endregion

        #region DrawReposition

        protected virtual void DrawReposition(int oldX, int oldY, int oldWidth, int oldHeight)
        {
            Update();
            Parent.Apply().Draw();
        }

        #endregion
        #region DrawEnable

        protected virtual void DrawEnable()
        {
            Parent.Update();
            if (InLayout)
                Parent.Apply().Draw();
            else
                Apply().Draw();
        }

        #endregion
        #region DrawDisable

        protected virtual void DrawDisable()
        {
            Parent.Update().Apply().Draw();
        }

        #endregion
        #region RequestDrawChanges

        public VisualObject RequestDrawChanges()
        {
            if (Root is RootVisualObject root)
                root.DrawState++;
            return this;
        }

        #endregion
        #region CollectStyle

        public UIStyle CollectStyle(bool includeThis = true)
        {
            UIStyle result = new UIStyle();
            foreach (VisualObject node in WayFromRoot)
                if (node != this || includeThis)
                    result.Stratify(node.Style);
            return result;
        }

        #endregion
        #region ToString

        public override string ToString() => FullName;

        #endregion

        // Setup
        #region SetMinSize

        public VisualObject SetMinSize(int? width = null, int? height = null)
        {
            _MinWidth = width;
            _MinHeight = height;
            return this;
        }

        #endregion
        #region SetPositioning

        public VisualObject SetPositioning(IPositioning positioning)
        {
            if (Positioning is InGrid inGrid && inGrid != positioning)
            {
                if (WidthResizing == inGrid)
                    SetWidthResizing(null);
                if (HeightResizing == inGrid)
                    SetHeightResizing(null);
            }
            Positioning = positioning;
            return this;
        }

        #endregion
        #region SetWidthResizing

        public VisualObject SetWidthResizing(IResizing resizing)
        {
            WidthResizing = resizing;
            return this;
        }

        #endregion
        #region SetHeightResizing

        public VisualObject SetHeightResizing(IResizing resizing)
        {
            HeightResizing = resizing;
            return this;
        }

        #endregion
        #region SetupLayout

        /// <summary>
        /// Setup layout for child positioning.
        /// </summary>
        /// <param name="alignment">Where to place all layout objects row/line</param>
        /// <param name="direction">Direction of placing objects</param>
        /// <param name="side">Side to which objects adjoin, relative to direction</param>
        /// <param name="indent">Layout indent</param>
        /// <param name="childIndent">Distance between objects in layout</param>
        /// <param name="boundsIsIndent">Whether to draw objects/ object tiles that are outside of bounds of indent or not</param>
        /// <returns>this</returns>
        public VisualObject SetupLayout(Alignment alignment = Alignment.Center, Direction direction = Direction.Down,
            Side side = Side.Center, ExternalIndent indent = null, int childIndent = 1)
        {
            LayoutConfiguration = new LayoutConfiguration(alignment, direction, side, indent, childIndent);
            return this;
        }

        #endregion
        #region LayoutOffset

        /// <summary>
        /// Scrolling offset of layout. Used in ScrollBackground and ScrollBar.
        /// </summary>
        /// <param name="value">Indent value</param>
        /// <returns>this</returns>
        public VisualObject LayoutOffset(int value)
        {
            LayoutConfiguration.LayoutOffset = value;
            return this;
        }

        #endregion
        #region SetupGrid

        /// <summary>
        /// Setup grid for child positioning. Use Absolute and Relative classes for specifying sizes.
        /// </summary>
        /// <param name="columns">Column sizes (i.e. new ISize[] { new Absolute(10), new Relative(100) })</param>
        /// <param name="lines">Line sizes (i.e. new ISize[] { new Absolute(10), new Relative(100) })</param>
        /// <param name="indent">Grid indent</param>
        /// <param name="fillWithEmptyObjects">Whether to fills all grid cells with empty VisualContainers</param>
        /// <returns>this</returns>
        public VisualObject SetupGrid(IEnumerable<ISize> columns = null, IEnumerable<ISize> lines = null,
            Indent indent = null)
        {
            columns = columns ?? new ISize[] { new Relative(100) };
            lines = lines ?? new ISize[] { new Relative(100) };
            GridConfiguration = new GridConfiguration(columns, lines, indent);
            return SetWH(Width, Height);
        }

        #endregion
        #region MinGridWidth

        private int MinGridWidth()
        {
            int result = 0;
            for (int i = 0; i < GridConfiguration.Columns.Length; i++)
            {
                ISize column = GridConfiguration.Columns[i];
                if (column.IsAbsolute)
                    result += column.Value;
                else
                    result += this[i, -1].Max(o => o.MinWidth);
            }
            return result;
        }

        #endregion
        #region MinGridHeight

        private int MinGridHeight()
        {
            int result = 0;
            for (int i = 0; i < GridConfiguration.Lines.Length; i++)
            {
                ISize line = GridConfiguration.Lines[i];
                if (line.IsAbsolute)
                    result += line.Value;
                else
                    result += this[-1, i].Max(o => o.MinHeight);
            }
            return result;
        }

        #endregion

        // Positioning
        #region AddToLayout

        /// <summary>
        /// Add object as a child in layout. Removes child alignment and grid positioning.
        /// </summary>
        /// <param name="child">Object to add as a child.</param>
        /// <param name="layer">Layer where to add the object. Null by default (don't change object layer).</param>
        /// <returns></returns>
        public virtual T AddToLayout<T>(T child, int? layer = null)
            where T : VisualObject
        {
            child.SetPositioning(new InLayout());
            return Add(child, layer);
        }

        #endregion
        #region SetParentAlignment

        /// <summary>
        /// Setup alignment positioning inside parent. Removes layout and grid positioning.
        /// </summary>
        /// <param name="alignment">Where to place this object in parent</param>
        /// <param name="indent">Alignment indent from Parent's borders</param>
        /// <param name="boundsIsIndent">Whether to draw tiles of this object that are outside of bounds of indent or not</param>
        /// <returns>this</returns>
        public VisualObject SetParentAlignment(Alignment alignment, ExternalIndent indent = null, bool boundsIsIndent = true)
        {
            SetPositioning(new InParentAlignment(alignment, indent, boundsIsIndent))
                .ParentAlignment();
            return this;
        }

        #endregion

        // Resizing
        #region SetWidthParentStretch

        public VisualObject SetWidthParentStretch()
        {
            WidthResizing = new InParentStretch();
            if (Parent is VisualObject parent)
                SetWH(parent.Width, Height);
            return this;
        }

        #endregion
        #region SetHeightParentStretch

        public VisualObject SetHeightParentStretch()
        {
            HeightResizing = new InParentStretch();
            if (Parent is VisualObject parent)
                SetWH(Width, parent.Height);
            return this;
        }

        #endregion
        #region SetWidthChildStretch

        public VisualObject SetWidthChildStretch()
        {
            WidthResizing = new InChildStretch();
            ChildStretch();
            return this;
        }

        #endregion
        #region SetHeightChildStretch

        public VisualObject SetHeightChildStretch()
        {
            HeightResizing = new InChildStretch();
            ChildStretch();
            return this;
        }

        #endregion

        // Positioning and resizing
        #region AddToGrid

        public VisualObject AddToGrid(VisualObject child, int column, int line,
            bool widthResize = true, bool heightResize = true, int? layer = null)
        {
            InGrid inGrid = new InGrid(column, line);
            SetPositioning(inGrid);
            if (widthResize)
                SetWidthResizing(inGrid);
            if (heightResize)
                SetHeightResizing(inGrid);
            return Add(child, layer);
        }

        #endregion

        #region Pulse

        /// <summary> Send specified signal to all sub-tree including this node. </summary>
        /// <param name="type"> Type of signal </param>
        /// <returns> this </returns>
        public VisualObject Pulse(PulseType type)
        {
            Root?.PrePulseObject(this, type);

            // Pulse event handling related to this node
            PulseThis(type);

            // Recursive Pulse call
            PulseChild(type);

            // Post pulse event handling related to this node
            PostPulseThis(type);

            Root?.PostPulseObject(this, type);

            return this;
        }

        #region PulseThis

        /// <summary>
        /// Send specified signal only to this node.
        /// </summary>
        /// <param name="type">Type of signal</param>
        /// <returns>this</returns>
        public VisualObject PulseThis(PulseType type)
        {
            try
            {
                // Overridable pulse handling method
                PulseThisNative(type);

                // Custom pulse handler
                Configuration.Custom.Pulse?.Invoke(this, type);
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
            return this;
        }

        #endregion
        #region PulseThisNative

        /// <summary>
        /// Overridable function to handle pulse signal for this node.
        /// </summary>
        /// <param name="type"></param>
        protected virtual void PulseThisNative(PulseType type)
        {
            switch (type)
            {
                case PulseType.Reset:
                    if (LayoutConfiguration != null)
                        LayoutOffset(0);
                    break;
                case PulseType.SetXYWH:
                    // Update position relative to Provider
                    ProviderX = X + (Parent?.ProviderX ?? 0);
                    ProviderY = Y + (Parent?.ProviderY ?? 0);
                    break;
            }
        }

        #endregion
        #region PulseChild

        /// <summary>
        /// Send specified signal to sub-tree without this node.
        /// </summary>
        /// <param name="type">Type of signal</param>
        /// <returns>this</returns>
        public VisualObject PulseChild(PulseType type)
        {
            foreach (VisualObject child in ChildrenFromTop)
                child.Pulse(type);
            return this;
        }

        #endregion
        #region PostPulseThis

        public VisualObject PostPulseThis(PulseType type)
        {
            try
            {
                // Overridable pulse handling method
                PostPulseThisNative(type);

                // Custom pulse handler
                Configuration.Custom.PostPulse?.Invoke(this, type);
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
            return this;
        }

        #endregion
        #region PostPulseThisNative

        /// <summary>
        /// Overridable function to handle post pulse signal for this node.
        /// </summary>
        /// <param name="type"></param>
        protected virtual void PostPulseThisNative(PulseType type) { }

        #endregion

        #endregion
        #region Update

        /// <summary> Updates the node and the child sub-tree. </summary>
        /// <returns> this </returns>
        public VisualObject Update()
        {
            Root?.PreUpdateObject(this);

            // Updates related to this node
            UpdateThis();

            // Recursive Update() call
            UpdateChild();

            // Updates related to this node and dependant on child updates
            PostUpdateThis();

            Root?.PostUpdateObject(this);

            return this;
        }

        #region UpdateThis

        /// <summary>
        /// Updates related to this node only.
        /// </summary>
        /// <returns>this</returns>
        private void UpdateThis()
        {
            try
            {
                // Overridable update method
                UpdateThisNative();

                // Custom update callback
                Configuration.Custom.Update?.Invoke(this);
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
        }

        #endregion
        #region UpdateThisNative

        /// <summary>
        /// Overridable method for updates related to this node. Do not change position/size in in this method.
        /// </summary>
        protected virtual void UpdateThisNative() { }

        #endregion

        #region UpdateChild

        /// <summary>
        /// Updates all Enabled child objects (sub-tree without this node).
        /// </summary>
        /// <returns>this</returns>
        private void UpdateChild()
        {
            foreach (VisualObject child in ChildrenFromTop)
                if (child.Enabled)
                    child.Update();
        }

        #endregion

        #region PostUpdateThis

        /// <summary>
        /// Updates related to this node and dependant on child updates. Executes after calling Update() on each child.
        /// </summary>
        /// <returns></returns>
        private void PostUpdateThis()
        {
            try
            {
                PostUpdateThisNative();
                Configuration.Custom.PostUpdate?.Invoke(this);
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
        }

        #endregion
        #region PostUpdateThisNative

        /// <summary>
        /// Overridable method for updates related to this node and dependant on child updates.
        /// </summary>
        protected virtual void PostUpdateThisNative() { }

        #endregion

        #endregion
        #region Apply

        /// <summary>
        /// Draws everything related to this VisualObject incluing all child sub-tree (directly changes tiles on tile provider).
        /// </summary>
        /// <returns>this</returns>
        public VisualObject Apply()
        {
            lock (ApplyLocker)
            {
                if (!IsActive)
                    throw new InvalidOperationException($"Applying inactive object: {FullName}");

                Root?.PreApplyObject(this);

                // Applying related to this node
                ApplyThis();

                // Recursive Apply call
                ApplyChild();

                // Post applying related to this node
                PostApplyThis();

                Root?.PostApplyObject(this);

                // Mark changes to be drawn
                RequestDrawChanges();
            }

            return this;
        }

        #region ApplyThis

        /// <summary>
        /// Draws everything related to this particular VisualObject. Doesn't include drawing child objects.
        /// </summary>
        /// <returns>this</returns>
        private void ApplyThis()
        {
            try
            {
                // Overridable apply function
                ApplyThisNative();

                // Custom apply callback
                Configuration.Custom.Apply?.Invoke(this);
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
        }

        #endregion
        #region ApplyThisNative

        /// <summary>
        /// Overridable method for apply related to this node. By default draws tiles and/or walls.
        /// </summary>
        protected virtual void ApplyThisNative() => ApplyTiles();

        #endregion
        #region ApplyTiles

        private void ApplyTiles()
        {
            // Default style tile changes
            if (!Style.CustomApplyTile && Style.Active == null && Style.InActive == null
                    && Style.Tile == null && Style.TileColor == null && Style.Wall == null
                    && Style.WallColor == null)
                return;

            foreach ((int x, int y) in Points)
            {
                dynamic tile = Tile(x, y);
                if (tile != null)
                    ApplyTile(x, y, tile);
            }
        }

        #endregion
        #region ApplyTile

        /// <summary>
        /// Overridable method for applying particular tile in <see cref="ApplyTiles"/>.
        /// </summary>
        /// <param name="x">X coordinate related to this node</param>
        /// <param name="y">Y coordinate related to this node</param>
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
        #region ApplyChild

        /// <summary>
        /// Apply sub-tree without applying this node.
        /// </summary>
        /// <returns>this</returns>
        private void ApplyChild()
        {
            bool forceSection = DrawWithSection;
            foreach (VisualObject child in ChildrenFromBottom)
                if (child.IsActiveThis)
                {
                    child.Apply();
                    forceSection = forceSection || child.DrawWithSection;
                }
            DrawWithSection = forceSection;
        }

        #endregion
        #region PostApplyThis

        /// <summary>
        /// Post apply handling
        /// </summary>
        /// <returns>this</returns>
        private void PostApplyThis()
        {
            try
            {
                // Overridable apply function
                PostApplyThisNative();

                // Custom apply callback
                Configuration.Custom.Apply?.Invoke(this);
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
        }

        #endregion
        #region PostApplyThisNative

        /// <summary>
        /// Overridable method for post apply related to this node.
        /// </summary>
        protected virtual void PostApplyThisNative() { }

        #endregion

        #endregion

        #region OutdatedPlayers

        /// <summary>
        /// Returns the players that have not yet recieved the latest version of this interface.
        /// </summary>
        /// <remarks>
        /// Returns <see langword="null"/> if called before the first call to <see cref="Update"/> on this
        /// widget.
        /// </remarks>
        /// <param name="playerIndex">
        /// If not -1 and <paramref name="toEveryone"/> is <see langword="false"/>, excludes all players except this one.
        /// </param>
        /// <param name="exceptPlayerIndex">
        /// If not -1 and <paramref name="toEveryone"/> is <see langword="false"/>, excludes this player.
        /// </param>
        /// <param name="toEveryone">
        /// If <see langword="true"/>, includes all players who have ever seen this interface.
        /// </param>
        public HashSet<int> OutdatedPlayers(int playerIndex = -1, int exceptPlayerIndex = -1, bool toEveryone = false)
        {
            if (Root == null)
                return null;

            ulong currentApplyCounter = Root.DrawState;
            HashSet<int> players = null;
            if (toEveryone)
            {
                // Sending to everyone who has ever seen this interface
                players = Root.PlayerApplyCounter.Keys.ToHashSet();
                //TODO: Add Root.Players?
            }
            else
            {
                players = playerIndex == -1
                    ? new HashSet<int>(Root.Players)
                    : new HashSet<int>() { playerIndex };
                players.Remove(exceptPlayerIndex);

                // Remove players that already received latest version of interface
                players.RemoveWhere(p =>
                    Root.PlayerApplyCounter.TryGetValue(p, out ulong applyCounter)
                    && currentApplyCounter == applyCounter);
            }

            return players;
        }

        #endregion
        #region Draw

        /// <summary> Sends SendTileSquare/SendSection packet to clients. </summary>
        /// <param name="dx"> X coordinate delta </param>
        /// <param name="dy"> Y coordinate delta </param>
        /// <param name="width"> Drawing rectangle width, -1 for object.Width </param>
        /// <param name="height"> Drawing rectangle height, -1 for object.Height </param>
        /// <param name="targetPlayers">
        /// Players to send to. If <see langword="null"/>, defaults to the result of <see cref="OutdatedPlayers(int, int, bool)"/>.
        /// </param>
        /// <param name="drawWithSection"> Whether to send with SendTileSquare or with SendSection, SendTileSquare (false) by default </param>
        /// <param name="frameSection"> Whether to send SectionFrame if sending with SendSection </param>
        /// <returns> this </returns>
        public virtual VisualObject Draw(int dx = 0, int dy = 0, int width = -1, int height = -1, HashSet<int> targetPlayers = null,
            bool? drawWithSection = null, bool? frameSection = null)
        {
            //if (!CanSend)
                //return this;
            if (targetPlayers == null)
                targetPlayers = OutdatedPlayers();
            if (Root.Observers is HashSet<int> observers)
                targetPlayers = targetPlayers.Where(player => observers.Contains(player)).ToHashSet();

            bool realDrawWithSection = drawWithSection ?? DrawWithSection;
            bool realFrame = frameSection ?? FrameSection;
            (int ax, int ay) = AbsoluteXY();
            TUI.DrawObject(this, targetPlayers, ax + dx, ay + dy, width >= 0 ? width : Width, height >= 0 ? height : Height,
                realDrawWithSection, realFrame);

            return this;
        }

        #endregion
        #region DrawPoints

        /// <summary> Draw list of points related to this node. </summary>
        /// <param name="points"> List of points </param>
        /// <param name="targetPlayers">
        /// Players to send to. If <see langword="null"/>, defaults to the result of <see cref="OutdatedPlayers(int, int, bool)"/>.
        /// </param>
        /// <param name="drawWithSection"> Whether to send with SendTileSquare or with SendSection, SendTileSquare (false) by default </param>
        /// <returns> this </returns>
        public virtual VisualObject DrawPoints(IEnumerable<(int, int)> points, HashSet<int> targetPlayers = null, bool? drawWithSection = null)
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

            return Draw(minX, minY, maxX - minX + 1, maxY - minY + 1, targetPlayers, drawWithSection);
        }

        #endregion
        #region Clear

        /// <summary>
        /// Clear all tiles with ITile.ClearEverything()
        /// </summary>
        /// <returns>this</returns>
        public VisualObject Clear()
        {
            lock (ApplyLocker)
            {
                foreach ((int x, int y) in Points)
                    Tile(x, y)?.ClearEverything();

                // Mark changes to be drawn
                RequestDrawChanges();
            }
            return this;
        }

        #endregion
        #region ShowGrid

        /// <summary>
        /// DEBUG function for showing grid bounds.
        /// </summary>
        public void ShowGrid()
        {
            lock (ApplyLocker)
            {
                for (int i = 0; i < GridConfiguration.Columns.Length; i++)
                    for (int j = 0; j < GridConfiguration.Lines.Length; j++)
                    {
                        (int columnX, int columnSize) = GridConfiguration.ResultingColumns[i];
                        (int lineY, int lineSize) = GridConfiguration.ResultingLines[j];
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

            // Mark changes to be drawn
            RequestDrawChanges();

            Draw();
        }

        #endregion

        #region Database

        #region DBRead

        /// <summary>
        /// Read data from database using overridable DBReadNative method
        /// </summary>
        /// <returns>true if read is successful</returns>
        public bool DBRead()
        {
            byte[] data = TUI.DBGet(FullName);
            if (data != null)
            {
                using (MemoryStream ms = new MemoryStream(data))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    try
                    {
                        DBReadNative(br);
                        Configuration.Custom.DBRead?.Invoke(this, br);
                    }
                    catch (Exception e)
                    {
                        TUI.HandleException(e);
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        #endregion
        #region DBReadNative

        /// <summary>
        /// Overridable method for reading from BinaryReader based on data from database
        /// </summary>
        /// <param name="br"></param>
        protected virtual void DBReadNative(BinaryReader br) { }

        #endregion
        #region DBWrite

        /// <summary>
        /// Write data to database using overridable DBWriteNative method
        /// </summary>
        public void DBWrite()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                try
                {
                    DBWriteNative(bw);
                    Configuration.Custom.DBWrite?.Invoke(this, bw);
                }
                catch (Exception e)
                {
                    TUI.HandleException(e);
                    return;
                }
                byte[] data = ms.ToArray();
                TUI.DBSet(FullName, data);
            }
        }

        #endregion
        #region DBWriteNative

        /// <summary>
        /// Overridable method for writing to BinaryWriter for data to be stored in database
        /// </summary>
        /// <param name="bw"></param>
        protected virtual void DBWriteNative(BinaryWriter bw) { }

        #endregion
        #region DBRemove

        /// <summary>
        /// Delete data
        /// </summary>
        public void DBRemove()
        {
            TUI.DBRemove(FullName);
        }

        #endregion
        #region UDBRead

        /// <summary>
        /// Read user data from database using overridable DBReadNative method
        /// </summary>
        /// <returns>true if read is successful</returns>
        public bool UDBRead(int user)
        {
            byte[] data = TUI.UDBGet(user, FullName);
            if (data != null)
            {
                using (MemoryStream ms = new MemoryStream(data))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    try
                    {
                        UDBReadNative(br, user);
                        Configuration.Custom.UDBRead?.Invoke(this, br, user);
                    }
                    catch (Exception e)
                    {
                        TUI.HandleException(e);
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        #endregion
        #region UDBReadNative

        /// <summary>
        /// Overridable method for reading from BinaryReader based on user data from database
        /// </summary>
        /// <param name="br"></param>
        protected virtual void UDBReadNative(BinaryReader br, int user) { }

        #endregion
        #region UDBWrite

        /// <summary>
        /// Write user data to database using overridable DBWriteNative method
        /// </summary>
        public void UDBWrite(int user)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                try
                {
                    UDBWriteNative(bw, user);
                    Configuration.Custom.UDBWrite?.Invoke(this, bw, user);
                }
                catch (Exception e)
                {
                    TUI.HandleException(e);
                    return;
                }
                byte[] data = ms.ToArray();
                TUI.UDBSet(user, FullName, data);
            }
        }

        #endregion
        #region UDBWriteNative

        /// <summary>
        /// Overridable method for writing to BinaryWriter for user data to be stored in database
        /// </summary>
        /// <param name="bw"></param>
        /// <param name="user"></param>
        protected virtual void UDBWriteNative(BinaryWriter bw, int user) { }

        #endregion
        #region UDBRemove

        /// <summary>
        /// Delete data
        /// </summary>
        /// <param name="user"></param>
        public void UDBRemove(int user)
        {
            TUI.UDBRemove(user, FullName);
        }

        #endregion
        #region NDBRead

        public int? NDBRead(int user) =>
            TUI.NDBGet(user, FullName);

        #endregion
        #region NDBWrite

        public void NDBWrite(int user, int number) =>
            TUI.NDBSet(user, FullName, number);

        #endregion
        #region NDBRemove

        public void NDBRemove(int user) =>
            TUI.NDBRemove(user, FullName);

        #endregion
        #region NDBSelect

        public List<(int User, int Number, string Username)> NDBSelect(bool ascending, int count, int offset = 0, bool requestNames = false) =>
            TUI.NDBSelect(FullName, ascending, count, offset, requestNames);

        #endregion

        #endregion
    }
}
