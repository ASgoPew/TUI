using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace TUI
{
    public partial class VisualDOM<T> : IDOM<T>, IVisual<T>
        where T : VisualDOM<T>
    {
        IEnumerable<Point> AbsolutePoints => GetAbsolutePoints();
        
        public UI UI { get; private set; }
        public GridConfiguration GridConfig { get; private set; }
        public GridCell<T>[,] Grid { get; private set; }
        public GridCell<T> Cell { get; private set; }
        public PaddingData PaddingData { get; set; }

        public VisualDOM(int x, int y, int width, int height, GridConfiguration gridConfig = null, bool rootable = false)
        {
            InitializeDOM(rootable);
            InitializeVisual(x, y, width, height);

            if (gridConfig != null)
                SetupGrid(gridConfig);
        }

        public virtual T Add(T child, int column, int line)
        {
            Add(child);

            GridCell<T> cell = Grid[column, line];
            cell.Objects.Add(child);
            child.Cell = cell;

            return (T)this;
        }

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

        private IEnumerable<Point> GetAbsolutePoints()
        {
            (int x, int y, int width, int height) = AbsoluteXYWH();
            for (int _x = x; _x < x + Width; _x++)
                for (int _y = y; _y < y + Height; _y++)
                    yield return new Point(_x, _y);
        }

        #region Update

        public virtual T Update(bool updateChild = true)
        {
            UpdateThis();
            UpdateChildPadding();
            UpdateChild();
            return (T)this;
        }

        #region UpdateThis

        public virtual T UpdateThis()
        {
            if (GridConfig != null)
                UpdateGrid();

            return (T)this;
        }

        #endregion
        #region UpdateGrid

        public void UpdateGrid()
        {
            /*// Checking grid validity
            ISize[] columnSizes = GridConfig.Columns;
            ISize[] lineSizes = GridConfig.Lines;
            int maxW = 0, maxRelativeW = 0;
		    for (int i = 0; i < columnSizes.Length; i++)
            {
                if (columnSizes[i].Value == 0)
                    throw new ArgumentException();
			    assert(columnSizes[i] > 0)
			    if columnSizes[i] >= 1 then
				    maxW = maxW + columnSizes[i]
			    else
				    maxRelativeW = maxRelativeW + (columnSizes[i] * 10)
			    end
            }
		    assert(maxW <= self.w, self:full_name() .. ': maxW=' .. maxW .. ', self.w=' .. self.w)
		    assert(maxRelativeW <= 1, self:full_name() .. ': maxRelativeW=' .. maxRelativeW)

		    local maxH, maxRelativeH = 0, 0
		    for i = 1, #lineSizes do
			    assert(lineSizes[i] > 0)
			    if lineSizes[i] >= 1 then
				    maxH = maxH + lineSizes[i]
			    else
				    maxRelativeH = maxRelativeH + (lineSizes[i] * 10)
			    end
		    end
		    assert(maxH <= self.h, self:full_name() .. ': maxH=' .. maxH .. ', self.h=' .. self.h)
		    assert(maxRelativeH <= 1, self:full_name() .. ': maxRelativeH=' .. maxRelativeH)

		    -- Main cell loop
		    local WCounter = 0
		    local WCounterIndentation, HCounterIndentation = self.gridConfig.dx or 0, self.gridConfig.dy or 0
		    local relativeW, relativeH = self.w - maxW - WCounterIndentation * (#columnSizes - 1), self.h - maxH - HCounterIndentation * (#lineSizes - 1)
		    for i, columnSize in ipairs(columnSizes) do
			    local movedWCounter
			    if columnSize >= 1 then
				    movedWCounter = WCounter + columnSize + WCounterIndentation
			    else
				    movedWCounter = WCounter + (columnSize * 10) * relativeW + WCounterIndentation
			    end

			    local HCounter = 0
			    for j, lineSize in ipairs(lineSizes) do
				    local movedHCounter
				    if lineSize >= 1 then
					    movedHCounter = HCounter + lineSize + HCounterIndentation
				    else
					    movedHCounter = HCounter + (lineSize * 10) * relativeH + HCounterIndentation
				    end

				    local cell = self.grid[i][j]
				    local direction = cell.direction or self.gridConfig.direction or Direction.Down
				    local alignment = cell.alignment or self.gridConfig.alignment or Alignment.Center
				    local side = cell.side or self.gridConfig.side or Side.Center
				    local indentation = cell.indentation or self.gridConfig.indentation or self.gridDefualtIndentation
				    local full = cell.full or self.gridConfig.full

				    -- Calculating cell position
				    cell.x = math.floor(WCounter)
				    cell.y = math.floor(HCounter)
				    cell.w = math.floor(movedWCounter) - cell.x - WCounterIndentation
				    cell.h = math.floor(movedHCounter) - cell.y - HCounterIndentation
				    cell.i = i
				    cell.j = j

				    if full then
					    assert(#cell <= 1, 'More than one object in FULL cell')
					    if #cell == 1 then
						    cell[1]:set_xywh(cell.x + indentation.left, cell.y + indentation.up, cell.w - indentation.left -
							    indentation.right, cell.h - indentation.up - indentation.down)
						    cell[1].cell = cell
					    end
				    elseif #cell > 0 then
					    -- Calculating total objects width and height
					    local totalW, totalH = 0, 0
					    for k = 1, #cell do
						    cell[k].cell = cell
						    if direction == Direction.Left or direction == Direction.Right then
							    if cell[k].h > totalH then
								    totalH = cell[k].h
							    end
							    totalW = totalW + cell[k].w
							    if k ~= #cell then
								    totalW = totalW + indentation.horizontal
							    end
						    elseif direction == Direction.Up or direction == Direction.Down then
							    if cell[k].w > totalW then
								    totalW = cell[k].w
							    end
							    totalH = totalH + cell[k].h
							    if k ~= #cell then
								    totalH = totalH + indentation.vertical
							    end
						    end
					    end

					    -- Calculating cell objects position
					    local sx, sy, dx, dy

					    -- Initializing sx
					    if alignment == Alignment.UpLeft or alignment == Alignment.Left or alignment == Alignment.DownLeft then
						    sx = indentation.left
					    elseif alignment == Alignment.UpRight or alignment == Alignment.Right or alignment == Alignment.DownRight then
						    sx = cell.w - indentation.right - totalW
					    else
						    sx = math.floor((cell.w - totalW + 1)/2)
					    end

					    -- Initializing sy
					    if alignment == Alignment.UpLeft or alignment == Alignment.Up or alignment == Alignment.UpRight then
						    sy = indentation.up
					    elseif alignment == Alignment.DownLeft or alignment == Alignment.Down or alignment == Alignment.DownRight then
						    sy = cell.h - indentation.up - totalH
					    else
						    sy = math.floor((cell.h - totalH + 1)/2)
					    end

					    -- Updating cell objects padding
					    local cx = (direction == Direction.Left) and (totalW - cell[1].w) or 0
					    local cy = (direction == Direction.Up) and (totalH - cell[1].h) or 0
					    for k = 1, #cell do
						    if cell[k].enabled and not cell[k].paddingData then
							    -- Calculating side alignment
							    local sideDeltaX, sideDeltaY = 0, 0
							    if direction == Direction.Left then
								    if side == Side.Left then
									    sideDeltaY = totalH - cell[k].h
								    elseif side == Side.Right then
								    elseif side == Side.Center then
									    sideDeltaY = math.floor((totalH - cell[k].h) / 2)
								    end
							    elseif direction == Direction.Right then
								    if side == Side.Left then
								    elseif side == Side.Right then
									    sideDeltaY = totalH - cell[k].h
								    elseif side == Side.Center then
									    sideDeltaY = math.floor((totalH - cell[k].h) / 2)
								    end
							    elseif direction == Direction.Up then
								    if side == Side.Left then
								    elseif side == Side.Right then
									    sideDeltaX = totalW - cell[k].w
								    elseif side == Side.Center then
									    sideDeltaX = math.floor((totalW - cell[k].w) / 2)
								    end
							    elseif direction == Direction.Down then
								    if side == Side.Left then
									    sideDeltaX = totalW - cell[k].w
								    elseif side == Side.Right then
								    elseif side == Side.Center then
									    sideDeltaX = math.floor((totalW - cell[k].w) / 2)
								    end
							    end

							    cell[k]:set_xywh(cell.x + sx + cx + sideDeltaX, cell.y + sy + cy + sideDeltaY)

							    if not cell[k + 1] then
								    break
							    end

							    if direction == Direction.Right then
								    cx = cx + indentation.horizontal + cell[k].w
							    elseif direction == Direction.Left then
								    cx = cx - indentation.horizontal - cell[k + 1].w
							    elseif direction == Direction.Down then
								    cy = cy + indentation.vertical + cell[k].h
							    elseif direction == Direction.Up then
								    cy = cy - indentation.vertical - cell[k + 1].h
							    end
						    end
					    end
				    end
				    HCounter = movedHCounter
			    end
			    WCounter = movedWCounter
		    end*/
        }

        #endregion
        #region UpdateChildPadding

        public virtual void UpdateChildPadding()
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
