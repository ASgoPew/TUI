using System;
using System.Collections.Generic;
using System.Linq;
using TUI.Base.Style;
using TUI.Widgets;

namespace TUI.Base
{
    public class VisualObject : Touchable
    {
        #region Data

        public UIStyle Style { get; set; }
        protected VisualObject[,] Grid { get; set; }
        public GridCell Cell { get; private set; }
        protected internal bool ForceSection { get; protected set; } = false;

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
                    o.Apply(true).Draw();
            }

            private bool ChildIntersectingOthers(VisualObject o)
            {
                lock (Child)
                    foreach (VisualObject child in Child)
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

        #endregion
        #region operator[,]

        public VisualObject this[int column, int line]
        {
            get => Grid[column, line];
            set
            {
                if (Grid[column, line] != null)
                    Remove(Grid[column, line]);
                Grid[column, line] = value;
                value.Cell = new GridCell(column, line);
            }
        }

        #endregion
        #region SetupGrid

        public VisualObject SetupGrid(GridStyle gridConfig = null, bool fillWithEmptyContainers = true)
        {
            Style.Grid = gridConfig ?? new GridStyle();

            if (gridConfig.Columns == null)
                gridConfig.Columns = new ISize[] { new Relative(100) };
            if (gridConfig.Lines == null)
                gridConfig.Lines = new ISize[] { new Relative(100) };
            Grid = new VisualObject[gridConfig.Columns.Length, gridConfig.Lines.Length];
            if (fillWithEmptyContainers)
                for (int i = 0; i < gridConfig.Columns.Length; i++)
                    for (int j = 0; j < gridConfig.Lines.Length; j++)
                        Grid[i, j] = new VisualContainer();

            return this as VisualObject;
        }

        #endregion
        #region FullSize

        public VisualObject FullSize(bool horizontal = true, bool vertical = true)
        {
            if (horizontal && vertical)
                Style.Positioning.FullSize = Base.Style.FullSize.Both;
            else if (horizontal)
                Style.Positioning.FullSize = Base.Style.FullSize.Horizontal;
            else if (vertical)
                Style.Positioning.FullSize = Base.Style.FullSize.Vertical;
            else
                Style.Positioning.FullSize = Base.Style.FullSize.None;
            return this;
        }

        #endregion

        #region Update

        public virtual VisualObject Update()
        {
            // Updates related to this node
            UpdateThis();

            // Recursive Update call
            UpdateChild();

            return this as VisualObject;
        }

        #region UpdateThis

        public VisualObject UpdateThis()
        {
            UpdateThisNative();
            CustomUpdate();
            return this as VisualObject;
        }

        #endregion
        #region UpdateThisNative

        protected virtual void UpdateThisNative()
        {
            if (Root == null)
                Root = GetRoot() as RootVisualObject;
            UpdateLayout();
            if (Style.Grid != null)
                UpdateGrid();
            UpdateFullSize();
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

            (int layoutW, int layoutH, VisualObject firstChild) = CalculateLayoutSize(direction);
            if (firstChild == null)
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
                sy = Height - offset.Up - layoutH;
            else
                sy = (Height - layoutH + 1) / 2;

            // Updating cell objects padding
            int cx = direction == Direction.Left ? layoutW - firstChild.Width : 0;
            int cy = direction == Direction.Up ? layoutH - firstChild.Height : 0;
            lock (Child)
                foreach (VisualObject child in Child)
                {
                    if (!child.Enabled || child.Style.Positioning.FullSize != Base.Style.FullSize.None || !child.Style.Positioning.InLayout)
                        continue;


                }
            for (int k = 0; k < cell.Objects.Count; k++)
            {
                VisualObject obj = cell.Objects[k];
                if (!obj.Enabled || obj.Configuration.FullSize)
                    continue;

                // Calculating side alignment
                int sideDeltaX = 0, sideDeltaY = 0;
                if (direction == Direction.Left)
                {
                    if (side == Side.Left)
                        sideDeltaY = layoutH - obj.Height;
                    else if (side == Side.Center)
                        sideDeltaY = (layoutH - obj.Height) / 2;
                }
                else if (direction == Direction.Right)
                {
                    if (side == Side.Right)
                        sideDeltaY = layoutH - obj.Height;
                    else if (side == Side.Center)
                        sideDeltaY = (layoutH - obj.Height) / 2;
                }
                else if (direction == Direction.Up)
                {
                    if (side == Side.Right)
                        sideDeltaX = layoutW - obj.Width;
                    else if (side == Side.Center)
                        sideDeltaX = (layoutW - obj.Width) / 2;
                }
                else if (direction == Direction.Down)
                {
                    if (side == Side.Left)
                        sideDeltaX = layoutW - obj.Width;
                    else if (side == Side.Center)
                        sideDeltaX = (layoutW - obj.Width) / 2;
                }

                obj.SetXYWH(cell.X + sx + cx + sideDeltaX, cell.Y + sy + cy + sideDeltaY);

                if (k == cell.Objects.Count - 1)
                    break;

                if (direction == Direction.Right)
                    cx = cx + offset.Horizontal + obj.Width;
                else if (direction == Direction.Left)
                    cx = cx - offset.Horizontal - cell.Objects[k + 1].Width;
                else if (direction == Direction.Down)
                    cy = cy + offset.Vertical + obj.Height;
                else if (direction == Direction.Up)
                    cy = cy - offset.Vertical - cell.Objects[k + 1].Height;
            }
        }

        private (int, int, VisualObject) CalculateLayoutSize(Direction direction)
        {
            // Calculating total objects width and height
            int indent = Style.Layout.ChildIndent ?? Parent?.Style?.Grid?.DefaultChildIndent ?? UIDefault.Indent;
            int totalW = 0, totalH = 0;
            VisualObject first = null;
            lock (Child)
                foreach (VisualObject child in Child)
                {
                    if (!child.Enabled || child.Style.Positioning.FullSize || !child.Style.Positioning.InLayout)
                        continue;

                    if (first == null)
                        first = child;
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
            if (totalW > 0)
                totalW -= indent;
            if (totalH > 0)
                totalH -= indent;

            return (totalW, totalH, first);
        }

        #endregion
        #region UpdateGrid

        public VisualObject UpdateGridV2()
        {
            if (Configuration.Grid == null)
                return this as VisualObject;
            if (Grid == null)
                SetupGrid(new GridConfiguration());

            (int maxW, int maxH, int maxRelativeW, int maxRelativeH) = CalculateGridLimits();
        }

        public (int, int, int, int) CalculateGridLimits()
        {
            ISize[] columnSizes = Configuration.Grid.Columns;
            ISize[] lineSizes = Configuration.Grid.Lines;
            int maxW = 0, maxRelativeW = 0;
            for (int i = 0; i < columnSizes.Length; i++)
            {
                ISize size = columnSizes[i];
                if (size.IsAbsolute)
                    maxW += size.Value;
                else
                    maxRelativeW += size.Value;
            }
            if (maxW > Width)
                throw new ArgumentException($"{FullName} (UpdateGrid): maxW is too big");
            if (maxRelativeW > 100)
                throw new ArgumentException($"{FullName} (UpdateGrid): maxRelativeW is too big");

            int maxH = 0, maxRelativeH = 0;
            for (int i = 0; i < lineSizes.Length; i++)
            {
                ISize size = lineSizes[i];
                if (size.IsAbsolute)
                    maxH += size.Value;
                else
                    maxRelativeH += size.Value;
            }
            if (maxH > Height)
                throw new ArgumentException($"{FullName} (UpdateGrid): maxH is too big");
            if (maxRelativeH > 100)
                throw new ArgumentException($"{FullName} (UpdateGrid): maxRelativeH is too big");

            return (maxW, maxH, maxRelativeW, maxRelativeH);
        }

        public VisualObject UpdateGrid()
        {
            // Checking grid validity
            GridConfiguration gridConfig = Configuration.Grid;
            ISize[] columnSizes = gridConfig.Columns;
            ISize[] lineSizes = gridConfig.Lines;
            Offset gridIndentation = gridConfig.GridIndentation;
            int maxW = 0, maxRelativeW = 0;
            for (int i = 0; i < columnSizes.Length; i++)
            {
                ISize size = columnSizes[i];
                if (size.IsAbsolute)
                    maxW += size.Value;
                else
                    maxRelativeW += size.Value;
            }
            if (maxW > Width - gridIndentation.Horizontal * (columnSizes.Length - 1) - gridIndentation.Left - gridIndentation.Right)
                throw new ArgumentException("UpdateGrid: maxW is too big");
            if (maxRelativeW > 100)
                throw new ArgumentException("UpdateGrid: maxRelativeW is too big");

            int maxH = 0, maxRelativeH = 0;
            for (int i = 0; i < lineSizes.Length; i++)
            {
                ISize size = lineSizes[i];
                if (size.IsAbsolute)
                    maxH += size.Value;
                else
                    maxRelativeH += size.Value;
            }
            if (maxH > Height - gridIndentation.Vertical * (lineSizes.Length - 1) - gridIndentation.Up - gridIndentation.Down)
                throw new ArgumentException("UpdateGrid: maxH is too big");
            if (maxRelativeH > 100)
                throw new ArgumentException("UpdateGrid: maxRelativeH is too big");

            // Main cell loop
            int WCounter = gridIndentation.Left;
            int relativeW = Width - maxW - gridIndentation.Horizontal * (columnSizes.Length - 1) - gridIndentation.Left - gridIndentation.Right;
            int relativeH = Height - maxH - gridIndentation.Vertical * (lineSizes.Length - 1) - gridIndentation.Up - gridIndentation.Down;
            //Console.WriteLine($"maxW: {maxW}, maxH: {maxH}; relativeW: {relativeW}, relativeH: {relativeH}");
            for (int i = 0; i < columnSizes.Length; i++)
            {
                ISize columnISize = columnSizes[i];
                int columnSize = columnISize.Value;
                int movedWCounter;
                if (columnISize.IsAbsolute)
                    movedWCounter = WCounter + columnSize + gridIndentation.Horizontal;
                else
                    movedWCounter = WCounter + (int)(columnSize * relativeW / 100f) + gridIndentation.Horizontal;

                int HCounter = gridIndentation.Up;
                for (int j = 0; j < lineSizes.Length; j++)
                {
                    ISize lineISize = lineSizes[j];
                    int lineSize = lineISize.Value;
                    int movedHCounter;
                    if (lineISize.IsAbsolute)
                        movedHCounter = HCounter + lineSize + gridIndentation.Vertical;
                    else
                        movedHCounter = HCounter + (int)(lineSize * relativeH / 100f) + gridIndentation.Vertical;
                    GridCell cell = Grid[i, j];

                    Direction direction = cell.Direction ?? gridConfig.DefaultDirection;
                    Alignment alignment = cell.Alignment ?? gridConfig.DefaultAlignment;
                    Side side = cell.Side ?? gridConfig.DefaultSide;
                    Offset indentation = cell.Indentation ?? gridConfig.Indentation;

                    // Calculating cell position
                    cell.X = WCounter;
                    cell.Y = HCounter;
                    cell.Width = movedWCounter - cell.X - gridIndentation.Horizontal;
                    cell.Height = movedHCounter - cell.Y - gridIndentation.Vertical;

                    HCounter = movedHCounter;

                    if (cell.Objects.Count == 0)
                        continue;

                    // Calculating total objects width and height
                    int totalW = 0, totalH = 0;
                    for (int k = 0; k < cell.Objects.Count; k++)
                    {
                        VisualObject obj = cell.Objects[k];
                        obj.Cell = cell;

                        if (!obj.Enabled || obj.Configuration.FullSize)
                            continue;

                        if (direction == Direction.Left || direction == Direction.Right)
                        {
                            if (obj.Height > totalH)
                                totalH = obj.Height;
                            totalW += obj.Width;
                            if (k != cell.Objects.Count)
                                totalW += indentation.Horizontal;
                        }
                        else if (direction == Direction.Up || direction == Direction.Down)
                        {
                            if (obj.Width > totalW)
                                totalW = obj.Width;
                            totalH += obj.Height;
                            if (k != cell.Objects.Count)
                                totalH += indentation.Vertical;
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
                        if (!obj.Enabled || obj.Configuration.FullSize)
                            continue;

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
                WCounter = movedWCounter;
            }
            return this as VisualObject;
        }

        #endregion
        #region UpdateFullSize

        public virtual VisualObject UpdateFullSize()
        {
            lock (Child)
                foreach (VisualObject child in Child)
                    if (child.Configuration.FullSize)
                        if (child.Cell == null)
                            child.SetXYWH(0, 0, Width, Height);
                        else
                        {
                            GridCell cell = child.Cell;
                            Offset indentation = cell.Indentation;
                            child.SetXYWH(cell.X + indentation.Left, cell.Y + indentation.Up,
                                cell.Width - indentation.Left - indentation.Right,
                                cell.Height - indentation.Up - indentation.Down);
                        }
            return this as VisualObject;
        }

        #endregion
        #region CustomUpdate

        public VisualObject CustomUpdate()
        {
            Configuration.CustomUpdate?.Invoke(this as VisualObject);
            return this as VisualObject;
        }

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
        #region Apply

        public virtual VisualObject Apply(bool forceClear = false)
            {
                if (!Active())
                    throw new InvalidOperationException("Trying to call Apply() an not active object.");

                // Applying related to this node
                ApplyThis(forceClear);

                // Recursive Apply call
                ApplyChild();

                return this;
            }

            #region ApplyThis

            public VisualObject ApplyThis(bool forceClear = false)
            {
                ApplyThisNative(forceClear);
                CustomApply();
                return this;
            }

            #endregion
            #region ApplyThisNative

            protected virtual void ApplyThisNative(bool forceClear = false)
            {
                ForceSection = false;
                ApplyTiles(forceClear);
                if (UI.ShowGrid && Configuration.Grid != null)
                    ShowGrid();
            }

            #endregion
            #region ApplyTiles

            public virtual VisualObject ApplyTiles(bool forceClear)
            {
                if (!forceClear && Style.InActive == null && Style.Tile == null && Style.TileColor == null
                    && Style.Wall == null && Style.WallColor == null)
                    return this;

                foreach ((int x, int y) in ProviderPoints)
                {
                    dynamic tile = Provider[x, y];
                    if (tile == null)
                        throw new NullReferenceException($"tile is null: {x}, {y}");
                    if (forceClear)
                        tile.ClearEverything();
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
                for (int i = 0; i < Configuration.Grid.Columns.Length; i++)
                    for (int j = 0; j < Configuration.Grid.Lines.Length; j++)
                    {
                        GridCell cell = Grid[i, j];
                        (int cx, int cy) = ProviderXY(cell.X, cell.Y);
                        //Console.WriteLine($"GridCell: {cx}, {cy}");
                        for (int x = cx; x < cx + cell.Width; x++)
                            for (int y = cy; y < cy + cell.Height; y++)
                            {
                                dynamic tile = Provider[x, y];
                                tile.wall = (byte)155;
                                tile.wallColor((byte)(25 + (i + j) % 2));
                            }
                    }
            }

            #endregion
            #region CustomApply

            public VisualObject CustomApply()
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
                    foreach (VisualObject child in Child)
                        if (child.Enabled)
                        {
                            child.Apply(false);
                            forceSection = forceSection || child.ForceSection;
                        }
                ForceSection = forceSection;
                return this;
            }

            #endregion

        #endregion
        #region Clear

        public virtual VisualObject Clear()
        {
            UITileProvider provider = Provider;
            foreach ((int x, int y) in ProviderPoints)
                provider[x, y].ClearEverything();

            return this;
        }

        #endregion
        #region Draw

        public virtual VisualObject Draw(int dx = 0, int dy = 0, int width = -1, int height = -1)
        {
            (int ax, int ay) = AbsoluteXY();
            Console.WriteLine($"Draw ({Name}): {ax + dx}, {ay + dy}, {(width >= 0 ? width : Width)}, {(height >= 0 ? height : Height)}: {ForceSection}");
            UI.DrawRect(ax + dx, ay + dy, width >= 0 ? width : Width, height >= 0 ? height : Height, ForceSection);
            return this;
        }

        #endregion
        #region DrawPoints

        public virtual VisualObject DrawPoints(List<(int, int)> list)
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

            return Draw(minX, minY, maxX - minX + 1, maxY - minY + 1);
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
                popup = new VisualObject(0, 0, 0, 0, null, null, (self, touch) => Popdown() == this).FullSize();
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
