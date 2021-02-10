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
        /// Child grid. Use operator[,] to get or set grid elements.
        /// </summary>
        protected VisualObject[,] Grid { get; set; }
        /// <summary>
        /// Cell of Parent's grid in which this object is. Null if not in Parent's grid.
        /// </summary>
        public GridCell Cell { get; private set; }
        /// <summary>
        /// Objects draw with SentTileSquare by default. Set this field to force drawing this object with SendSection.
        /// </summary>
        public bool DrawWithSection { get; set; } = false;
        /// <summary>
        /// Frame the section if <see cref="DrawWithSection"/> is set. True by default.
        /// </summary>
        public bool FrameSection { get; set; } = true;
        /// <summary>
        /// X coordinate relative to tile provider. Sets in Update() and PulseType.PositionChanged. Used in Tile() function.
        /// </summary>
        public int ProviderX { get; protected set; }
        /// <summary>
        /// Y coordinate relative to tile provider. Sets in Update() and PulseType.PositionChanged. Used in Tile() function.
        /// </summary>
        public int ProviderY { get; protected set; }
        /// <summary>
        /// Bounds (relative to this object) in which this object is allowed to draw.
        /// </summary>
        public ExternalIndent Bounds { get; protected set; } = new ExternalIndent();

        /// <summary>
        /// Overridable field for disabling ability to be ordered in Parent's Child array.
        /// </summary>
        public override bool Orderable => !Configuration.InLayout;

        private string _Name;
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
                    : Cell != null
                        ? $"{Parent.FullName}[{Cell.Column},{Cell.Line}].{Name}"
                        : $"{Parent.FullName}[{IndexInParent}].{Name}";

        #endregion

        #region IDOM

        #region Remove

        /// <summary>
        /// Removes child object. Calls Dispose() on removed object so you can't use
        /// this object anymore.
        /// </summary>
        /// <param name="child">Child object to remove.</param>
        /// <returns>this</returns>
        public override VisualObject Remove(VisualObject child)
        {
            base.Remove(child);
            GridCell cell = child.Cell;
            if (cell != null)
            {
                Grid[cell.Column, cell.Line] = null;
                child.Cell = null;
            }
            if (child.Configuration.InLayout)
                Configuration.Layout.Objects.Remove(child);
            return this;
        }

        #endregion

        #endregion
        #region Touchable

        #region PostSetTop

        /// <summary>
        /// Overridable function that is called when child comes on top of the layer.
        /// <para></para>
        /// Does Apply() and Draw() if object is intersecting at least one other child object by default.
        /// </summary>
        /// <param name="child">Child object that came on top of the layer</param>
        protected override void PostSetTopNative(VisualObject child)
        {
            if (ChildIntersectingOthers(child))
                child.Apply().Draw();
        }

        #endregion
        #region ChildIntersectingOthers

        private bool ChildIntersectingOthers(VisualObject o)
        {
            foreach (VisualObject child in ChildrenFromTop)
                if (child != o && child.Active && o.Intersecting(child))
                    return true;
            return false;
        }

        #endregion

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
        public VisualObject this[int column, int line]
        {
            get => Grid[column, line];
            set
            {
                if (value != null)
                {
                    value.Configuration.InLayout = false;
                    value.Configuration.Alignment = null;

                    if (Grid[column, line] != null)
                        Remove(Grid[column, line]);
                    value.Cell = new GridCell(column, line);
                    Grid[column, line] = Add(value);
                }
                else
                {
                    VisualObject child = Grid[column, line];
                    Grid[column, line] = null;
                    Remove(child);
                }
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
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                throw new ArgumentOutOfRangeException($"{FullName}: Invalid tile x or y.");
            ExternalIndent bounds = Bounds;
            if (x < bounds.Left || x >= Width - bounds.Right || y < bounds.Up || y >= Height - bounds.Down)
                return null;
            return Provider?[ProviderX + x, ProviderY + y];
        }

        #endregion
        #region Enable

        public override VisualObject Enable(bool draw = true)
        {
            if (!Enabled)
            {
                base.Enable(draw);
                if (draw)
                    DrawEnable();
            }
            return this;
        }

        #endregion
        #region Disable

        public override VisualObject Disable(bool draw = true)
        {
            if (Enabled)
            {
                base.Disable(draw);
                if (draw)
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
        /// <returns>this</returns>
        public override VisualObject SetXYWH(int x, int y, int width, int height, bool draw)
        {
            width = Math.Max(width, Configuration.Grid?.MinWidth ?? 0);
            height = Math.Max(height, Configuration.Grid?.MinHeight ?? 0);

            int oldX = X, oldY = Y, oldWidth = Width, oldHeight = Height;
            if (oldX != x || oldY != y || oldWidth != width || oldHeight != height)
            {
                base.SetXYWH(x, y, width, height, draw);
                Pulse(PulseType.PositionChanged);
                if (draw)
                    DrawReposition(oldX, oldY, oldWidth, oldHeight);
            }

            return this;
        }

        #endregion
        #region DrawReposition

        protected virtual void DrawReposition(int oldX, int oldY, int oldWidth, int oldHeight)
        {
            if (oldWidth != Width || oldHeight != Height)
                Update();
            Parent.Apply().Draw();
        }

        #endregion
        #region DrawEnable

        protected virtual void DrawEnable()
        {
            Parent.Update();
            if (Configuration.InLayout)
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
            child.Configuration.Alignment = null;
            if (child.Cell != null)
            {
                Grid[child.Cell.Column, child.Cell.Line] = null;
                child.Cell = null;
            }

            Add(child, layer);
            child.Configuration.InLayout = true;
            return child;
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
            Side side = Side.Center, ExternalIndent indent = null, int childIndent = 1, bool boundsIsIndent = true)
        {
            Configuration.Layout = new LayoutConfiguration(alignment, direction, side, indent, childIndent, boundsIsIndent);
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
            Indent indent = null, bool fillWithEmptyObjects = true)
        {
            GridConfiguration gridStyle = Configuration.Grid = new GridConfiguration(columns, lines, indent);

            if (gridStyle.Columns == null)
                gridStyle.Columns = new ISize[] { new Relative(100) };
            if (gridStyle.Lines == null)
                gridStyle.Lines = new ISize[] { new Relative(100) };
            Grid = new VisualObject[gridStyle.Columns.Length, gridStyle.Lines.Length];
            if (fillWithEmptyObjects)
                for (int i = 0; i < gridStyle.Columns.Length; i++)
                    for (int j = 0; j < gridStyle.Lines.Length; j++)
                        this[i, j] = new VisualContainer();

            return this as VisualObject;
        }

        #endregion
        #region SetAlignmentInParent

        /// <summary>
        /// Setup alignment positioning inside parent. Removes layout and grid positioning.
        /// </summary>
        /// <param name="alignment">Where to place this object in parent</param>
        /// <param name="indent">Alignment indent from Parent's borders</param>
        /// <param name="boundsIsIndent">Whether to draw tiles of this object that are outside of bounds of indent or not</param>
        /// <returns>this</returns>
        public VisualObject SetAlignmentInParent(Alignment alignment, ExternalIndent indent = null, bool boundsIsIndent = true)
        {
            if (Cell != null)
            {
                Parent.Grid[Cell.Column, Cell.Line] = null;
                Cell = null;
            }
            Configuration.InLayout = false;

            Configuration.Alignment = new AlignmentConfiguration(alignment, indent, boundsIsIndent);
            return this;
        }

        #endregion
        #region SetFullSize

        /// <summary>
        /// Set automatic stretching to parent size. Removes grid positioning.
        /// </summary>
        /// <param name="horizontal">Horizontal stretching</param>
        /// <param name="vertical">Vertical stretching</param>
        /// <returns>this</returns>
        public VisualObject SetFullSize(bool horizontal = true, bool vertical = true) =>
            SetFullSize(horizontal && vertical
                ? FullSize.Both
                : horizontal
                    ? FullSize.Horizontal
                    : vertical
                        ? FullSize.Vertical
                        : FullSize.None);

        /// <summary>
        /// Set automatic stretching to parent size. Removes grid positioning.
        /// </summary>
        /// <param name="fullSize">Horizontal and/or vertical (or None)</param>
        /// <returns>this</returns>
        public VisualObject SetFullSize(FullSize fullSize)
        {
            if (Cell != null && fullSize != FullSize.None)
            {
                Parent.Grid[Cell.Column, Cell.Line] = null;
                Cell = null;
            }

            Configuration.FullSize = fullSize;
            return this;
        }

        #endregion
        #region LayoutSkip

        public VisualObject LayoutSkip(ushort value)
        {
            if (Configuration.Layout == null)
                throw new Exception("Layout is not set for this object: " + FullName);

            Configuration.Layout.Index = value;
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
            if (Configuration.Layout == null)
                throw new Exception("Layout is not set for this object: " + FullName);

            Configuration.Layout.LayoutOffset = value;
            return this;
        }

        #endregion
        #region RequestDrawChanges

        public VisualObject RequestDrawChanges()
        {
            Root.DrawState++;
            return this;
        }

        #endregion
        #region ToString

        public override string ToString() => FullName;

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

        #region Pulse

        /// <summary>
        /// Send specified signal to all sub-tree including this node.
        /// </summary>
        /// <param name="type">Type of signal</param>
        /// <returns>this</returns>
        public virtual VisualObject Pulse(PulseType type)
        {
            // Pulse event handling related to this node
            PulseThis(type);

            // Recursive Pulse call
            PulseChild(type);

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
                    if (Configuration.Layout != null)
                        LayoutOffset(0);
                    break;
                case PulseType.PositionChanged:
                    // Update position relative to Provider
                    if (Root != null)
                        (ProviderX, ProviderY) = ProviderXY();
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

        #endregion
        #region Update

        /// <summary>
        /// Updates the node and the child sub-tree.
        /// </summary>
        /// <returns>this</returns>
        public VisualObject Update()
        {
            // Updates related to this node
            UpdateThis();

            // Updates related to child positioning
            UpdateChildPositioning();

            // Recursive Update() call
            UpdateChild();

            // Updates related to this node and dependant on child updates
            PostUpdateThis();

            return this;
        }

        #region UpdateThis

        /// <summary>
        /// Updates related to this node only.
        /// </summary>
        /// <returns>this</returns>
        public VisualObject UpdateThis()
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

            return this;
        }

        #endregion
        #region UpdateThisNative

        /// <summary>
        /// Overridable method for updates related to this node. Do not change position/size in in this method.
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
        }

        #endregion
        #region UpdateBounds

        /// <summary>
        /// Calculate Bounds for this node (intersection of Parent's layout indent/alignment indent and Parent's Bounds)
        /// </summary>
        protected void UpdateBounds()
        {
            bool layoutBounds = Configuration.InLayout && Parent.Configuration.Layout.BoundsIsIndent;
            bool alignmentBounds = Configuration.Alignment != null && Configuration.Alignment.BoundsIsIndent;
            if (layoutBounds || alignmentBounds)
            {
                ExternalIndent parentIndent = layoutBounds ? Parent.Configuration.Layout.Indent : Configuration.Alignment.Indent;
                Bounds = new ExternalIndent()
                {
                    Left = Math.Max(0, parentIndent.Left - X),
                    Up = Math.Max(0, parentIndent.Up - Y),
                    Right = Math.Max(0, (X + Width) - (Parent.Width - parentIndent.Right)),
                    Down = Math.Max(0, (Y + Height) - (Parent.Height - parentIndent.Down)),
                };
            }
            else
                Bounds = new ExternalIndent(UIDefault.ExternalIndent);

            // Intersecting bounds with parent's Bounds
            if (Parent != null)
            {
                ExternalIndent parentBounds = Parent.Bounds;
                if (parentBounds == null)
                    return;
                Bounds = new ExternalIndent()
                {
                    Left = Math.Max(Bounds.Left, parentBounds.Left - X),
                    Up = Math.Max(Bounds.Up, parentBounds.Up - Y),
                    Right = Math.Max(Bounds.Right, parentBounds.Right - (Parent.Width - (X + Width))),
                    Down = Math.Max(Bounds.Down, parentBounds.Down - (Parent.Height - (Y + Height)))
                };
            }
        }

        #endregion

        #region UpdateChildPositioning

        /// <summary>
        /// First updates child sizes, then calculates child positions based on sizes (layout, grid, alignment).
        /// </summary>
        /// <returns>this</returns>
        public VisualObject UpdateChildPositioning()
        {
            // Update child sizes first
            UpdateChildSize();

            // Update child objects with alignment
            UpdateAlignment();
            // Update child objects in layout
            if (Configuration.Layout != null)
                UpdateLayout();
            // Update child objects in grid
            if (Configuration.Grid != null)
                UpdateGrid();

            return this;
        }

        #endregion
        #region UpdateChildSize

        /// <summary>
        /// Updates child sizes with call of overridable child.UpdateSizeNative()
        /// </summary>
        public void UpdateChildSize()
        {
            foreach (VisualObject child in ChildrenFromTop)
            {
                child.SetWH(child.GetSizeNative(), false);
                if (child.Configuration.FullSize != FullSize.None)
                    child.UpdateFullSize();
            }
        }

        #endregion
        #region GetSizeNative

        /// <summary>
        /// Overridable method for determining object size depending on own data (image/text/etc size)
        /// </summary>
        /// <returns></returns>
        protected virtual (int, int) GetSizeNative() => (Width, Height);

        #endregion
        #region UpdateFullSize

        /// <summary>
        /// Updates this object size relative to Parent size if Configuration.FullSize is not None.
        /// </summary>
        protected void UpdateFullSize()
        {
            FullSize fullSize = Configuration.FullSize;
            // If child is in layout then FullSize should match parent size minus layout indent.
            // If Alignment is set then FullSize should match parent size minus alignment indent.
            ExternalIndent indent = Configuration.InLayout
                ? Parent.Configuration.Layout.Indent
                : Configuration.Alignment != null
                    ? Configuration.Alignment.Indent
                    : UIDefault.ExternalIndent;

            int newX = indent.Left;
            int newY = indent.Up;
            int newWidth = Parent.Width - newX - indent.Right;
            int newHeight = Parent.Height - newY - indent.Down;

            if (fullSize == FullSize.Both)
                SetXYWH(newX, newY, newWidth, newHeight, false);
            else if (fullSize == FullSize.Horizontal)
                SetXYWH(newX, Y, newWidth, Height, false);
            else if (fullSize == FullSize.Vertical)
                SetXYWH(X, newY, Width, newHeight, false);
        }

        #endregion
        #region UpdateAlignment

        /// <summary>
        /// Sets position of child objects with set Configuration.Alignment
        /// </summary>
        protected void UpdateAlignment()
        {
            foreach (VisualObject child in ChildrenFromTop)
            {
                AlignmentConfiguration positioning = child.Configuration.Alignment;
                if (positioning  == null)
                    continue;

                ExternalIndent indent = positioning.Indent ?? UIDefault.ExternalIndent;
                Alignment alignment = positioning.Alignment;
                int x, y;
                if (alignment == Alignment.UpLeft || alignment == Alignment.Left || alignment == Alignment.DownLeft)
                    x = indent.Left;
                else if (alignment == Alignment.UpRight || alignment == Alignment.Right || alignment == Alignment.DownRight)
                    x = Width - indent.Right - child.Width;
                else
                    x = (int)Math.Floor((Width - indent.Left - indent.Right - child.Width) / 2f) + indent.Left;

                if (alignment == Alignment.UpLeft || alignment == Alignment.Up || alignment == Alignment.UpRight)
                    y = indent.Up;
                else if (alignment == Alignment.DownLeft || alignment == Alignment.Down || alignment == Alignment.DownRight)
                    y = Height - indent.Down - child.Height;
                else
                    y = (int)Math.Floor((Height - indent.Up - indent.Down - child.Height) / 2f) + indent.Up;

                child.SetXY(x, y, false);
            }
        }

        #endregion
        #region UpdateLayout

        /// <summary>
        /// Set position for children in layout
        /// </summary>
        protected void UpdateLayout()
        {
            ExternalIndent indent = Configuration.Layout.Indent;
            Alignment alignment = Configuration.Layout.Alignment;
            Direction direction = Configuration.Layout.Direction;
            Side side = Configuration.Layout.Side;
            int offset = Configuration.Layout.ChildOffset;
            int layoutIndent = Configuration.Layout.LayoutOffset;

            (int abstractLayoutW, int abstractLayoutH, List<VisualObject> layoutChild) = CalculateLayoutSize(direction, offset);
            Configuration.Layout.Objects = layoutChild;
            for (int i = 0; i < Configuration.Layout.Index; i++)
                layoutChild[i].Visible = false;
            if (layoutChild.Count - Configuration.Layout.Index <= 0)
                return;
            layoutChild = layoutChild.Skip(Configuration.Layout.Index).ToList();

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
                if (Configuration.Layout.BoundsIsIndent)
                    child.Visible = Intersecting(resultX, resultY, child.Width, child.Height, indent.Left, indent.Up,
                        Width - indent.Right - indent.Left, Height - indent.Down - indent.Up);
                else
                    child.Visible = Intersecting(resultX, resultY, child.Width, child.Height, 0, 0, Width, Height);

                child.SetXY(resultX, resultY, false);

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
                Configuration.Layout.OffsetLimit = abstractLayoutW - layoutW;
            else if (direction == Direction.Up || direction == Direction.Down)
                Configuration.Layout.OffsetLimit = abstractLayoutH - layoutH;
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
                FullSize fullSize = child.Configuration.FullSize;
                if (!child.Enabled || !child.Configuration.InLayout || fullSize == FullSize.Both)
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
        #region Intersecting

        public bool Intersecting(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2) =>
            x1 < x2 + w2 && y1 < y2 + h2 && x2 < x1 + w1 && y2 < y1 + h1;

        #endregion
        #region UpdateGrid

        /// <summary>
        /// Sets position for children in grid
        /// </summary>
        protected void UpdateGrid()
        {
            CalculateGridSizes();

            // Main cell loop
            ISize[] columnSizes = Configuration.Grid.Columns;
            ISize[] lineSizes = Configuration.Grid.Lines;
                
            for (int i = 0; i < columnSizes.Length; i++)
            {
                (int columnX, int columnSize) = Configuration.Grid.ResultingColumns[i];
                if (columnSizes[i].IsDynamic)
                    columnSize = -1;
                for (int j = 0; j < lineSizes.Length; j++)
                {
                    (int lineX, int lineSize) = Configuration.Grid.ResultingLines[j];
                    if (lineSizes[j].IsDynamic)
                        lineSize = -1;
                    VisualObject cell = Grid[i, j];
                    if (cell == null)
                        continue;
                    cell.SetXYWH(columnX, lineX, columnSize == -1 ? cell.Width : columnSize,
                        lineSize == -1 ? cell.Height : lineSize, false);
                }
            }
        }

        #endregion
        #region CalculateGridSizes

        public void CalculateGridSizes()
        {
            Indent indent = Configuration.Grid.Indent ?? UIDefault.Indent;
            CalculateSizes(Configuration.Grid.Columns, Width, indent.Left, indent.Horizontal, indent.Right,
                ref Configuration.Grid.ResultingColumns, ref Configuration.Grid.MinWidth, true);
            CalculateSizes(Configuration.Grid.Lines, Height, indent.Up, indent.Vertical, indent.Down,
                ref Configuration.Grid.ResultingLines, ref Configuration.Grid.MinHeight, false);
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
                        for (int line = 0; line < Configuration.Grid.Lines.Count(); line++)
                            max = Math.Max(max, this[i, line]?.Width ?? 0);
                    else
                        for (int column = 0; column < Configuration.Grid.Columns.Count(); column++)
                            max = Math.Max(max, this[column, i]?.Height ?? 0);
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

        #region UpdateChild

        /// <summary>
        /// Updates all Enabled child objects (sub-tree without this node).
        /// </summary>
        /// <returns>this</returns>
        public VisualObject UpdateChild()
        {
            foreach (VisualObject child in ChildrenFromTop)
                if (child.Enabled)
                    child.Update();
            return this;
        }

        #endregion

        #region PostUpdateThis

        /// <summary>
        /// Updates related to this node and dependant on child updates. Executes after calling Update() on each child.
        /// </summary>
        /// <returns></returns>
        public VisualObject PostUpdateThis()
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
            return this;
        }

        #endregion
        #region PostUpdateThisNative

        /// <summary>
        /// Overridable method for updates related to this node and dependant on child updates.
        /// </summary>
        protected virtual void PostUpdateThisNative()
        {
            if (Configuration.Grid != null)
                lock (Child)
                {
                    Configuration.Grid.MinWidth = Math.Max(Configuration.Grid.MinWidth, Child.Max(o => o.Configuration.Grid?.MinWidth) ?? 1);
                    Configuration.Grid.MinHeight = Math.Max(Configuration.Grid.MinHeight, Child.Max(o => o.Configuration.Grid?.MinHeight) ?? 1);
                }
        }

        #endregion

        #endregion
        #region Apply

        /// <summary>
        /// Draws everything related to this VisualObject incluing all child sub-tree (directly changes tiles on tile provider).
        /// </summary>
        /// <returns>this</returns>
        public VisualObject Apply()
        {
#if DEBUG
            if (Root == null)
                TUI.Throw(this, "Applying before Update().");
            if (!CalculateActive())
                TUI.Throw(this, "Applying inactive object.");
#endif

            lock (Locker)
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
        /// <returns>this</returns>
        public VisualObject ApplyThis()
        {
            lock (Locker)
            {
                // Mark changes to be drawn
                RequestDrawChanges();

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
            return this;
        }

        #endregion
        #region ApplyThisNative

        /// <summary>
        /// Overridable method for apply related to this node. By default draws tiles and/or walls.
        /// </summary>
        protected virtual void ApplyThisNative() => ApplyTiles();

        #endregion
        #region ApplyTiles

        public VisualObject ApplyTiles()
        {
            // Default style tile changes
            lock (Locker)
            {
                if (!Style.CustomApplyTile && Style.Active == null && Style.InActive == null
                        && Style.Tile == null && Style.TileColor == null && Style.Wall == null
                        && Style.WallColor == null)
                    return this;

                foreach ((int x, int y) in Points)
                    ApplyTile(x, y);
            }
            return this;
        }

        #endregion
        #region ApplyTile

        /// <summary>
        /// Overridable method for applying particular tile in <see cref="ApplyTiles"/>.
        /// </summary>
        /// <param name="x">X coordinate related to this node</param>
        /// <param name="y">Y coordinate related to this node</param>
        protected virtual void ApplyTile(int x, int y)
        {
            dynamic tile = Tile(x, y);
            if (tile == null)
                return;
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
        public VisualObject ApplyChild()
        {
            lock (Locker)
            {
                bool forceSection = DrawWithSection;
                foreach (VisualObject child in ChildrenFromBottom)
                    if (child.Active)
                    {
                        child.Apply();
                        forceSection = forceSection || child.DrawWithSection;
                    }
                DrawWithSection = forceSection;
            }
            return this;
        }

        #endregion

        #endregion
        #region Draw

        /// <summary>
        /// Sends SendTileSquare/SendSection packet to clients.
        /// </summary>
        /// <param name="dx">X coordinate delta</param>
        /// <param name="dy">Y coordinate delta</param>
        /// <param name="width">Drawing rectangle width, -1 for object.Width</param>
        /// <param name="height">Drawing rectangle height, -1 for object.Height</param>
        /// <param name="playerIndex">Index of user to send to, -1 for all players</param>
        /// <param name="exceptPlayerIndex">Index of user to ignore on sending</param>
        /// <param name="drawWithSection">Whether to send with SendTileSquare or with SendSection, SendTileSquare (false) by default</param>
        /// <param name="frameSection">Whether to send SectionFrame if sending with SendSection</param>
        /// <param name="toEveryone">Whether to draw it to everyone who has ever seen this interface</param>
        /// <returns>this</returns>
        public virtual VisualObject Draw(int dx = 0, int dy = 0, int width = -1, int height = -1,
            int playerIndex = -1, int exceptPlayerIndex = -1, bool? drawWithSection = null,
            bool? frameSection = null, bool toEveryone = false)
        {
#if DEBUG
            if (Root == null)
                TUI.Throw(this, "Drawing before Update()");
#endif

            bool realDrawWithSection = drawWithSection ?? DrawWithSection;
            bool realFrame = frameSection ?? FrameSection;
            (int ax, int ay) = AbsoluteXY();
            TUI.DrawObject(this, ax + dx, ay + dy, width >= 0 ? width : Width, height >= 0 ? height : Height,
                realDrawWithSection, playerIndex, exceptPlayerIndex, realFrame, toEveryone);
            return this;
        }

        #endregion
        #region DrawPoints

        /// <summary>
        /// Draw list of points related to this node.
        /// </summary>
        /// <param name="points">List of points</param>
        /// <param name="userIndex">Index of user to send to, -1 for all users</param>
        /// <param name="exceptUserIndex">Index of user to ignore on sending</param>
        /// <param name="forceSection">Whether to send with SendTileSquare or with SendSection, SendTileSquare (false) by default</param>
        /// <returns>this</returns>
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

        /// <summary>
        /// Clear all tiles with ITile.ClearEverything()
        /// </summary>
        /// <returns>this</returns>
        public VisualObject Clear()
        {
            lock (Locker)
            {
                // Mark changes to be drawn
                RequestDrawChanges();

                foreach ((int x, int y) in Points)
                    Tile(x, y)?.ClearEverything();
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
            if (Configuration.Grid == null)
                throw new Exception("Grid not setup for this object.");

            lock (Locker)
            {
                for (int i = 0; i < Configuration.Grid.Columns.Length; i++)
                    for (int j = 0; j < Configuration.Grid.Lines.Length; j++)
                    {
                        (int columnX, int columnSize) = Configuration.Grid.ResultingColumns[i];
                        (int lineY, int lineSize) = Configuration.Grid.ResultingLines[j];
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
