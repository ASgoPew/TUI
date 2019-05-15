namespace TUI.Base.Style
{
    public class PositioningStyle
    {
        /// <summary>
        /// If set to true and object has a parent then X and Y would be ignored, instead object
        /// would be positioned in parent's layout.
        /// </summary>
        public bool InLayout { get; set; } = false;
        /// <summary>
        /// Object size matches parent horizontal/vertical/both size automatically.
        /// <para>If <see cref="FullSize"/> != None and <see cref="InLayout"/> == true then matching parent size consideres layout offset.</para>
        /// Doesn't work if object is a cell of parent's grid.
        /// </summary>
        public FullSize FullSize { get; set; } = FullSize.None;
    }

    public class LayoutStyle
    {
        public ushort ObjectsOffset { get; internal set; } = 0;
        public ushort ObjectCount { get; internal set; } = 0;
        /// <summary>
        /// Layout offset.
        /// </summary>
        public ExternalOffset Offset { get; set; }
        /// <summary>
        /// Object placing alignment in child layout. Looks to parent's grid default value if not present.
        /// </summary>
        public Alignment? Alignment { get; set; }
        /// <summary>
        /// Object placing direction in child layout. Looks to parent's grid default value if not present.
        /// </summary>
        public Direction? Direction { get; set; }
        /// <summary>
        /// Object placing side relative to direction in child layout. Looks to parent's grid default value if not present.
        /// </summary>
        public Side? Side { get; set; }
        /// <summary>
        /// Distance between objects in child layout. Looks to parent's grid default value if not present.
        /// </summary>
        public int? ChildIndent { get; set; }
    }

    public class GridStyle
    {
        internal (int Position, int Size)[] ResultingColumns;
        internal (int Position, int Size)[] ResultingLines;
        internal int MinWidth = 1;
        internal int MinHeight = 1;

        public ISize[] Columns { get; internal set; }
        public ISize[] Lines { get; internal set; }
        public Offset Offset { get; set; }
        public ExternalOffset DefaultOffset { get; set; }
        public Alignment? DefaultAlignment { get; set; }
        public Direction? DefaultDirection { get; set; }
        public Side? DefaultSide { get; set; }
        public ushort? DefaultChildIndent { get; set; }

        public GridStyle(ISize[] columns = null, ISize[] lines = null)
        {
            Columns = columns ?? new ISize[] { new Relative(100) };
            Lines = lines ?? new ISize[] { new Relative(100) };
        }

        public GridStyle(GridStyle style)
            : this((ISize[]) style.Columns.Clone(), (ISize[]) style.Lines.Clone())
        {
        }
    }

    public class UIStyle
    {
        /// <summary>
        /// Parent related positioning styles.
        /// </summary>
        public PositioningStyle Positioning { get; set; } = new PositioningStyle();
        /// <summary>
        /// Child layout related styles.
        /// </summary>
        public LayoutStyle Layout { get; set; } = new LayoutStyle();
        /// <summary>
        /// Grid related styles. Null by default. Use <see cref="VisualObject.SetupGrid(GridStyle, bool)"/> for initializing grid.
        /// </summary>
        public GridStyle Grid { get; internal set; }
        
        public bool? Active { get; set; }
        public ushort? Tile { get; set; }
        public byte? TileColor { get; set; }
        public byte? Wall { get; set; }
        public byte? WallColor { get; set; }
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