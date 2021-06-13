using System;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Base
{
    #region ContainerStyle

    /// <summary>
    /// Drawing styles for VisualContainer.
    /// </summary>
    public class ContainerStyle : UIStyle
    {
        /// <summary>
        /// If set to false then container would inherit parent's wall and wall paint,
        /// also every Apply() would clear every tile before drawing.
        /// </summary>
        public bool Transparent { get; set; } = false;

        public ContainerStyle()
            : base()
        {
        }

        public ContainerStyle(ContainerStyle style)
            : base(style)
        {
            Transparent = style.Transparent;
        }
    }

    #endregion

    /// <summary>
    /// Widget-container for other widgets. It is strongly recommended to use this
    /// instead of just VisualObject, because it correctly supports Scrolling.
    /// </summary>
    public class VisualContainer : VisualObject
    {
        #region Data

        protected Selection Selecting { get; set; }

        public VisualObject Selected => Selecting?.Selected;
        public ContainerStyle ContainerStyle => Style as ContainerStyle;

        #endregion

        #region Constructor

        public VisualContainer(int x, int y, int width, int height, UIConfiguration configuration = null, ContainerStyle style = null, Action<VisualObject, Touch> callback = null)
            : base(x, y, width, height, configuration ?? new UIConfiguration() { UseBegin = false }, style ?? new ContainerStyle(), callback)
        {
        }

        public VisualContainer()
            : this(0, 0, 0, 0)
        {
        }

        public VisualContainer(UIConfiguration configuration)
            : this(0, 0, 0, 0, configuration)
        {
        }

        public VisualContainer(ContainerStyle style)
            : this(0, 0, 0, 0, null, style)
        {
        }

        #endregion

        #region Select

        /// <summary>
        /// Enable specified child and disable all other child objects.
        /// </summary>
        /// <param name="node">Child to select.</param>
        /// <returns>this</returns>
        public VisualContainer Select(VisualObject node)
        {
            if (!_Child.Contains(node))
                throw new InvalidOperationException("Trying to Select an object that isn't a child of current VisualDOM");

            if (Selecting != null)
            {
                if (Selected == node)
                    return this;
                Selected.Disable();
                Selecting.Selected = node;
                Selected.Enable();
                return this;
            }
            Selecting = new Selection(node);

            foreach (VisualObject child in ChildrenFromTop)
                if (child.Enabled)
                {
                    child.Disable();
                    Selecting.DisabledChildren.Add(child);
                }
            node.Enable();

            Update().Apply().Draw();

            return this;
        }

        #endregion
        #region Deselect

        /// <summary>
        /// Enables all child objects. See <see cref="Select(VisualObject)"/>
        /// </summary>
        /// <returns>this</returns>
        public VisualContainer Deselect()
        {
            if (Selecting == null)
                throw new InvalidOperationException("Trying to deselect without selecting.");

            if (!Selecting.DisabledChildren.Contains(Selected))
                Selected.Disable();
            foreach (VisualObject child in Selecting.DisabledChildren)
                if (_Child.Contains(child) && !child.Enabled)
                    child.Enable();
            Selecting = null;

            Update().Apply().Draw();

            return this;
        }

        #endregion

        #region UpdateThisNative

        protected override void UpdateThisNative()
        {
            base.UpdateThisNative();
            if (!ContainerStyle.Transparent)
                InheritParentStyle();
        }

        #endregion
        #region InheritParentStyle

        protected void InheritParentStyle()
        {
            VisualObject node = this;
            ushort? wall = null;
            byte? wallColor = null;
            while (node != null)
            {
                if (wall == null && node.Style.Wall != null)
                    wall = node.Style.Wall;
                if (wallColor == null && node.Style.WallColor != null)
                    wallColor = node.Style.WallColor;
                node = node.Parent;
            }
            Style.Wall = wall;
            Style.WallColor = wallColor ?? 0;
        }

        #endregion
        #region ApplyTile

        protected override void ApplyTile(int x, int y, dynamic tile)
        {
            if (!ContainerStyle.Transparent)
                tile.ClearEverything();

            if (Style.Active.HasValue)
                tile.active(Style.Active.Value);
            else if (Style.Tile.HasValue)
                tile.active(true);
            else if (Style.Wall.HasValue)
                tile.active(false);
            if (Style.InActive.HasValue)
                tile.inActive(Style.InActive.Value);
            if (Style.Tile.HasValue)
                tile.type = Style.Tile.Value;
            if (Style.TileColor.HasValue)
                tile.color(Style.TileColor.Value);
            if (Style.Wall.HasValue)
                tile.wall = Style.Wall.Value;
            if (Style.WallColor.HasValue)
                tile.wallColor(Style.WallColor.Value);
        }

        #endregion
    }
}
