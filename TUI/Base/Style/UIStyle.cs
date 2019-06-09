using System.Collections.Generic;
using System.Linq;

namespace TUI.Base.Style
{
    #region AlignmentStyle

    public class AlignmentStyle
    {
        /// <summary>
        /// Alignment inside parent.
        /// </summary>
        public Alignment Alignment { get; set; }
        /// <summary>
        /// Offset for <see cref="Alignment"/> inside parent.
        /// </summary>
        public ExternalOffset Offset { get; set; }
        /// <summary>
        /// Restrict drawing outside of offset or not.
        /// </summary>
        public bool BoundsIsOffset { get; set; }

        internal AlignmentStyle(Alignment alignment, ExternalOffset offset = null, bool boundsIsOffset = true)
        {
            Alignment = alignment;
            Offset = offset ?? new ExternalOffset(UIDefault.ExternalOffset);
            BoundsIsOffset = boundsIsOffset;
        }

        public AlignmentStyle(AlignmentStyle alignmentStyle)
            : this(alignmentStyle.Alignment, alignmentStyle.Offset) { }
    }

    #endregion
    #region LayoutStyle

    public class LayoutStyle
    {
        /// <summary>
        /// Begin index for <see cref="Objects"/> list in layout.
        /// </summary>
        public ushort Index { get; internal set; } = 0;
        /// <summary>
        /// Child elements inside layout.
        /// </summary>
        public List<VisualObject> Objects { get; internal set; } = null;
        /// <summary>
        /// Whole layout indentation.
        /// </summary>
        public int LayoutIndent { get; internal set; } = 0;
        public int IndentLimit { get; internal set; }

        /// <summary>
        /// Layout offset.
        /// </summary>
        public ExternalOffset Offset { get; set; }
        /// <summary>
        /// Object placing alignment in child layout.
        /// </summary>
        public Alignment Alignment { get; set; }
        /// <summary>
        /// Object placing direction in child layout.
        /// </summary>
        public Direction Direction { get; set; }
        /// <summary>
        /// Object placing side relative to direction in child layout.
        /// </summary>
        public Side Side { get; set; }
        /// <summary>
        /// Distance between objects in child layout.
        /// </summary>
        public int ChildIndent { get; set; }
        /// <summary>
        /// Restrict drawing outside of offset or not.
        /// </summary>
        public bool BoundsIsOffset { get; set; }

        internal LayoutStyle(Alignment alignment = Style.Alignment.Center, Direction direction = Style.Direction.Down, Side side = Style.Side.Center, ExternalOffset offset = null, int childIndent = 1, bool boundsIsOffset = true)
        {
            Alignment = alignment;
            Direction = direction;
            Side = side;
            Offset = offset ?? new ExternalOffset(UIDefault.ExternalOffset);
            ChildIndent = childIndent;
            BoundsIsOffset = boundsIsOffset;
        }

        internal LayoutStyle(LayoutStyle layoutStyle)
            : this(layoutStyle.Alignment, layoutStyle.Direction, layoutStyle.Side, layoutStyle.Offset, layoutStyle.ChildIndent) { }
    }

    #endregion
    #region GridStyle

    public class GridStyle
    {
        internal (int Position, int Size)[] ResultingColumns;
        internal (int Position, int Size)[] ResultingLines;
        internal int MinWidth = 1;
        internal int MinHeight = 1;

        /// <summary>
        /// Grid column sizes
        /// </summary>
        public ISize[] Columns { get; internal set; }
        /// <summary>
        /// Grid line sizes
        /// </summary>
        public ISize[] Lines { get; internal set; }
        /// <summary>
        /// Grid offset
        /// </summary>
        public Offset Offset { get; set; }

        internal GridStyle(IEnumerable<ISize> columns = null, IEnumerable<ISize> lines = null, Offset offset = null)
        {
            Columns = columns?.ToArray() ?? new ISize[] { new Relative(100) };
            Lines = lines?.ToArray() ?? new ISize[] { new Relative(100) };
            Offset = offset ?? new Offset(UIDefault.Offset);
        }

        internal GridStyle(GridStyle style)
            : this((ISize[]) style.Columns.Clone(), (ISize[]) style.Lines.Clone(), new Offset(style.Offset))
        {
        }
    }

    #endregion

    /// <summary>
    /// Object drawing style class.
    /// </summary>
    public class UIStyle
    {
        /// <summary>
        /// Node positioning with alignment inside parent. Not set by default (null).
        /// <para></para>
        /// Use <see cref="VisualObject.SetAlignmentInParent(AlignmentStyle)"/> to initialize alignment.
        /// </summary>
        public AlignmentStyle Alignment { get; internal set; } = null;

        /// <summary>
        /// Child objects positioning in Layout. Not set by default (null).
        /// <para></para>
        /// Use <see cref="VisualObject.SetupLayout(LayoutStyle)"/>
        /// </summary>
        public LayoutStyle Layout { get; internal set; } = null;
        /// <summary>
        /// Child objects positioning in grid. Not set by default (null).
        /// <para></para>
        /// Use <see cref="VisualObject.SetupGrid(GridStyle, bool)"/> to initialize grid.
        /// </summary>
        public GridStyle Grid { get; internal set; }

        /// <summary>
        /// If set to true and object has a parent then X and Y would be ignored, instead object
        /// would be positioned in parent's layout.
        /// </summary>
        public bool InLayout { get; set; } = false;
        /// <summary>
        /// Object size matches parent horizontal/vertical/both size automatically.
        /// <para></para>
        /// If node is in parent's layout/alignment then matching parent size consideres layout/alignment offset.
        /// <para></para>
        /// Can't be used with grid positioning.
        /// </summary>
        public FullSize FullSize { get; set; } = FullSize.None;

        /// <summary>
        /// Sets tile.active(Style.Active) for every tile.
        /// <para></para>
        /// If not specified sets to true in case Style.Tile is specified,
        /// otherwise to false in case Style.Wall is specified.
        /// </summary>
        public bool? Active { get; set; }
        /// <summary>
        /// Sets tile.type = Style.Tile for every tile.
        /// </summary>
        public ushort? Tile { get; set; }
        /// <summary>
        /// Sets tile.color(Style.TileColor) for every tile.
        /// </summary>
        public byte? TileColor { get; set; }
        /// <summary>
        /// Sets tile.wall = Style.Wall for every tile.
        /// </summary>
        public byte? Wall { get; set; }
        /// <summary>
        /// Sets tile.wallColor(Style.WallColor) for every tile.
        /// </summary>
        public byte? WallColor { get; set; }
        /// <summary>
        /// Sets tile.inActive(Style.InActive) for every tile.
        /// </summary>
        public bool? InActive { get; set; }

        public UIStyle() { }

        public UIStyle(UIStyle style)
        {
            if (style.Active.HasValue)
                this.Active = style.Active.Value;
            if (style.Tile.HasValue)
                this.Tile = style.Tile.Value;
            if (style.TileColor.HasValue)
                this.TileColor = style.TileColor.Value;
            if (style.Wall.HasValue)
                this.Wall = style.Wall.Value;
            if (style.WallColor.HasValue)
                this.WallColor = style.WallColor.Value;
            if (style.InActive.HasValue)
                this.InActive = style.InActive.Value;
        }
    }
}