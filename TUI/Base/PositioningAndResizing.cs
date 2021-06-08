using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;

namespace TerrariaUI
{
    public interface IPositioning
    {
    }

    public interface IResizing
    {
    }

    public class InLayout : IPositioning { }
    /// <summary>
    /// Alignment configuration class for <see cref="VisualObject.SetParentAlignment"/>.
    /// </summary>
    public class InParentAlignment : IPositioning
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

        public InParentAlignment(Alignment alignment, ExternalIndent indent = null, bool boundsIsIndent = true)
        {
            Alignment = alignment;
            Indent = indent ?? new ExternalIndent();
            BoundsIsIndent = boundsIsIndent;
        }

        public InParentAlignment(InParentAlignment positioning)
            : this(positioning.Alignment, positioning.Indent, positioning.BoundsIsIndent) { }
    }

    public class InChildStretch : IResizing { }
    public class InParentStretch : IResizing { }

    public class InGrid : IPositioning, IResizing
    {
        public int Column;
        public int Line;
        public InGrid(int column, int line)
        {
            Column = column;
            Line = line;
        }
    }

    #region LayoutConfiguration

    /// <summary>
    /// Layout configuration class for <see cref="VisualObject.SetupLayout"/>.
    /// </summary>
    public class LayoutConfiguration
    {
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

        internal LayoutConfiguration(Alignment alignment = Alignment.Center, Direction direction = Direction.Down, Side side = Side.Center, ExternalIndent indent = null, int childOffset = 1)
        {
            Alignment = alignment;
            Direction = direction;
            Side = side;
            Indent = indent ?? new ExternalIndent();
            ChildOffset = childOffset;
        }

        internal LayoutConfiguration(LayoutConfiguration layoutConfiguration)
            : this(layoutConfiguration.Alignment, layoutConfiguration.Direction,
                  layoutConfiguration.Side, layoutConfiguration.Indent, layoutConfiguration.ChildOffset)
        { }
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
            Indent = indent ?? new Indent();
        }

        internal GridConfiguration(GridConfiguration gridConfiguration)
            : this((ISize[])gridConfiguration.Columns.Clone(), (ISize[])gridConfiguration.Lines.Clone(), // TODO: shallow copy doesnt help
                  new Indent(gridConfiguration.Indent))
        {
        }
    }

    #endregion
}
