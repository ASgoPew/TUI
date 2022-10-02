using System;
using TUI.Base.Style;

namespace TUI.Base
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

        public ContainerStyle ContainerStyle => Style as ContainerStyle;

        #endregion

        #region Constructor

        public VisualContainer(int x, int y, int width, int height, UIConfiguration configuration = null, ContainerStyle style = null, Action<VisualObject, Touch> callback = null)
            : base(x, y, width, height, configuration, style ?? new ContainerStyle(), callback)
        {
        }

        public VisualContainer()
            : this(0, 0, 0, 0, new UIConfiguration() { UseBegin = false })
        {
        }

        public VisualContainer(UIConfiguration configuration)
            : this(0, 0, 0, 0, configuration)
        {
        }

        public VisualContainer(ContainerStyle style)
            : this(0, 0, 0, 0, new UIConfiguration() { UseBegin = false }, style)
        {
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

        protected override void ApplyTile(int x, int y)
        {
            dynamic tile = Tile(x, y);
            if (tile == null)
                return;

            if (!ContainerStyle.Transparent)
                tile.ClearEverything();

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
    }
}
