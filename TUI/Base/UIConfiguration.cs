using System.Collections.Generic;
using System.Linq;
using TUI.Base.Style;

namespace TUI.Base
{
    #region AlignmentConfiguration

    /// <summary>
    /// Alignment configuration class for <see cref="VisualObject.SetAlignmentInParent"/>.
    /// </summary>
    public class AlignmentConfiguration
    {
        /// <summary>
        /// Alignment inside parent.
        /// </summary>
        public Alignment Alignment { get; set; }
        /// <summary>
        /// Indent for <see cref="Alignment"/> inside parent.
        /// </summary>
        public ExternalIndent Indent { get; set; }
        /// <summary>
        /// Restrict drawing outside of indent or not.
        /// </summary>
        public bool BoundsIsIndent { get; set; }

        internal AlignmentConfiguration(Alignment alignment, ExternalIndent indent = null, bool boundsIsIndent = true)
        {
            Alignment = alignment;
            Indent = indent ?? new ExternalIndent(UIDefault.ExternalIndent);
            BoundsIsIndent = boundsIsIndent;
        }

        internal AlignmentConfiguration(AlignmentConfiguration alignmentConfiguration)
            : this(alignmentConfiguration.Alignment, alignmentConfiguration.Indent) { }
    }

    #endregion
    #region LayoutConfiguration

    /// <summary>
    /// Layout configuration class for <see cref="VisualObject.SetupLayout"/>.
    /// </summary>
    public class LayoutConfiguration
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
        /// Whole layout offset.
        /// </summary>
        public int LayoutOffset { get; internal set; } = 0;
        /// <summary>
        /// Maximum value of <see cref="LayoutOffset"/> (in other words layout size).
        /// </summary>
        public int OffsetLimit { get; internal set; }

        /// <summary>
        /// Layout indent.
        /// </summary>
        public ExternalIndent Indent { get; set; }
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
        public int ChildOffset { get; set; }
        /// <summary>
        /// Restrict drawing outside of indent or not.
        /// </summary>
        public bool BoundsIsIndent { get; set; }

        internal LayoutConfiguration(Alignment alignment = Alignment.Center, Direction direction = Style.Direction.Down, Side side = Style.Side.Center, ExternalIndent indent = null, int childIndent = 1, bool boundsIsIndent = true)
        {
            Alignment = alignment;
            Direction = direction;
            Side = side;
            Indent = indent ?? new ExternalIndent(UIDefault.ExternalIndent);
            ChildOffset = childIndent;
            BoundsIsIndent = boundsIsIndent;
        }

        internal LayoutConfiguration(LayoutConfiguration layoutConfiguration)
            : this(layoutConfiguration.Alignment, layoutConfiguration.Direction,
                  layoutConfiguration.Side, layoutConfiguration.Indent, layoutConfiguration.ChildOffset) { }
    }

    #endregion
    #region GridConfiguration

    /// <summary>
    /// Grid configuration class for <see cref="VisualObject.SetupGrid"/>.
    /// </summary>
    public class GridConfiguration
    {
        internal (int Position, int Size)[] ResultingColumns;
        internal (int Position, int Size)[] ResultingLines;
        public int MinWidth = 1;
        public int MinHeight = 1;

        /// <summary>
        /// Grid column sizes
        /// </summary>
        public ISize[] Columns { get; internal set; }
        /// <summary>
        /// Grid line sizes
        /// </summary>
        public ISize[] Lines { get; internal set; }
        /// <summary>
        /// Grid indent
        /// </summary>
        public Indent Indent { get; set; }

        internal GridConfiguration(IEnumerable<ISize> columns = null, IEnumerable<ISize> lines = null, Indent indent = null)
        {
            Columns = columns?.ToArray() ?? new ISize[] { new Relative(100) };
            Lines = lines?.ToArray() ?? new ISize[] { new Relative(100) };
            Indent = indent ?? new Indent(UIDefault.Indent);
        }

        internal GridConfiguration(GridConfiguration gridConfiguration)
            : this((ISize[])gridConfiguration.Columns.Clone(), (ISize[])gridConfiguration.Lines.Clone(),
                  new Indent(gridConfiguration.Indent))
        {
        }
    }

    #endregion

    /// <summary>
    /// Touching and drawing settings for VisualObject.
    /// </summary>
    public class UIConfiguration
    {
        #region Visual

        /// <summary>
        /// Node positioning with alignment inside parent. Not set by default (null).
        /// <para></para>
        /// Use <see cref="VisualObject.SetAlignmentInParent"/> to initialize alignment.
        /// </summary>
        public AlignmentConfiguration Alignment { get; internal set; } = null;

        /// <summary>
        /// Child objects positioning in Layout. Not set by default (null).
        /// <para></para>
        /// Use <see cref="VisualObject.SetupLayout"/>
        /// </summary>
        public LayoutConfiguration Layout { get; internal set; } = null;
        /// <summary>
        /// Child objects positioning in grid. Not set by default (null).
        /// <para></para>
        /// Use <see cref="VisualObject.SetupGrid"/> to initialize grid.
        /// </summary>
        public GridConfiguration Grid { get; internal set; } = null;

        /// <summary>
        /// If set to true and object has a parent then X and Y would be ignored, instead object
        /// would be positioned in parent's layout.
        /// </summary>
        public bool InLayout { get; set; } = false;
        /// <summary>
        /// Object size matches parent horizontal/vertical/both size automatically.
        /// <para></para>
        /// If node is in parent's layout/alignment then matching parent size consideres layout/alignment indent.
        /// <para></para>
        /// Can't be used with grid positioning.
        /// </summary>
        public FullSize FullSize { get; set; } = FullSize.None;

        #endregion

        /// <summary>
        /// Touching this node would prevent touches on it or on the whole root for some time.
        /// </summary>
        public Lock Lock { get; set; }
        /// <summary>
        /// Object that should be used for checking if user can touch this node (permission string for TShock).
        /// </summary>
        public object Permission { get; set; }
        /// <summary>
        /// Collection of custom callbacks.
        /// </summary>
        public CustomCallbacks Custom { get; set; } = new CustomCallbacks();
        /// <summary>
        /// Once node is touched all future touches within the same session will pass to this node.
        /// </summary>
        public bool SessionAcquire { get; set; } = true;
        /// <summary>
        /// Allows to touch this node only if current session began with touching it.
        /// </summary>
        public bool BeginRequire { get; set; } = true;
        /// <summary>
        /// Only for nodes with SessionAcquire. Passes touches even if they are not inside of this object.
        /// </summary>
        public bool UseOutsideTouches { get; set; } = false;
        /// <summary>
        /// Touching child node would place it on top of Child array layer so that it would be drawn
        /// above other objects with the same layer and check for touching first.
        /// </summary>
        public bool Ordered { get; set; } = false;
        /// <summary>
        /// Allows to touch this node if touch.State == TouchState.Begin. True by default.
        /// </summary>
        public bool UseBegin { get; set; } = true;
        /// <summary>
        /// Allows to touch this node if touch.State == TouchState.Moving. False by default.
        /// </summary>
        public bool UseMoving { get; set; } = false;
        /// <summary>
        /// Allows to touch this node if touch.State == TouchState.End. False by default.
        /// </summary>
        public bool UseEnd { get; set; } = false;

        /// <summary>
        /// Touching and drawing settings for VisualObject.
        /// </summary>
        public UIConfiguration() { }

        /// <summary>
        /// Touching and drawing settings for VisualObject.
        /// </summary>
        public UIConfiguration(UIConfiguration configuration)
        {
            this.Lock = new Lock(configuration.Lock);
            this.Permission = configuration.Permission;
            this.Custom = new CustomCallbacks(configuration.Custom);
            this.SessionAcquire = configuration.SessionAcquire;
            this.BeginRequire = configuration.BeginRequire;
            this.UseOutsideTouches = configuration.UseOutsideTouches;
            this.Ordered = configuration.Ordered;
            this.UseBegin = configuration.UseBegin;
            this.UseMoving = configuration.UseMoving;
            this.UseEnd = configuration.UseEnd;
        }
    }
}
