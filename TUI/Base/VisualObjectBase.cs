using System;
using System.Collections.Generic;
using System.Linq;

namespace TUI.Base
{
    public abstract class VisualObjectBase : Touchable
    {
        #region Data

        protected abstract UIStyle BaseStyle { get; }
        public bool ForceSection { get; protected set; } = false;
        private Dictionary<string, object> _Shortcuts { get; set; }

        #endregion

        #region IDOM

            #region Remove

            public override VisualObjectBase Remove(VisualObjectBase child)
            {
                foreach (var pair in _Shortcuts.Where(o => o.Value == child))
                    _Shortcuts.Remove(pair.Key);
                return base.Remove(child);
            }

            #endregion

        #endregion
        #region Touchable

            #region PostSetTop

            public override void PostSetTop(VisualObjectBase o)
            {
                if (ChildIntersectingOthers(o))
                    o.Apply(true).Draw();
            }

            private bool ChildIntersectingOthers(VisualObjectBase o)
            {
                lock (Child)
                    foreach (VisualObjectBase child in Child)
                        if (child != o && child.Enabled && o.Intersecting(child))
                            return true;
                return false;
            }

        #endregion

        #endregion

        #region Initialize

        public VisualObjectBase(int x, int y, int width, int height, UIConfiguration configuration = null, Func<VisualObjectBase, Touch, bool> callback = null)
            : base(x, y, width, height, configuration, callback)
        {
        }

        #endregion
        #region operator[]

        public object this[string key]
        {
            get
            {
                object value = null;
                _Shortcuts?.TryGetValue(key, out value);
                return value;
            }
            set
            {
                if (_Shortcuts == null)
                    _Shortcuts = new Dictionary<string, object>();
                _Shortcuts[key] = value;
            }
        }

        #endregion
        #region Enable

        public virtual VisualObjectBase Enable()
        {
            Enabled = true;
            return this;
        }

        #endregion
        #region Disable

        public virtual VisualObjectBase Disable()
        {
            Enabled = false;
            return this;
        }

        #endregion
        #region Apply

            public virtual VisualObjectBase Apply(bool forceClear = false)
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

            public VisualObjectBase ApplyThis(bool forceClear = false)
            {
                ApplyThisNative(forceClear);
                CustomApply();
                return this;
            }

            #endregion
            #region ApplyThisNative

            protected virtual void ApplyThisNative(bool forceClear = false)
            {
                ApplyTiles(forceClear);
                if (UI.ShowGrid)
                    ShowGrid();
            }

            #endregion
            #region ApplyTiles

            public virtual VisualObjectBase ApplyTiles(bool forceClear)
            {
                if (!forceClear && BaseStyle.InActive == null && BaseStyle.Tile == null && BaseStyle.TileColor == null
                    && BaseStyle.Wall == null && BaseStyle.WallColor == null)
                    return this;

                foreach ((int x, int y) in ProviderPoints)
                {
                    dynamic tile = Provider[x, y];
                    if (tile == null)
                        throw new NullReferenceException($"tile is null: {x}, {y}");
                    if (forceClear)
                        tile.ClearEverything();
                    if (BaseStyle.Active != null)
                        tile.active(BaseStyle.Active.Value);
                    if (BaseStyle.InActive != null)
                        tile.inActive(BaseStyle.InActive.Value);
                    if (BaseStyle.Tile != null)
                        tile.type = BaseStyle.Tile.Value;
                    if (BaseStyle.TileColor != null)
                        tile.color(BaseStyle.TileColor.Value);
                    if (BaseStyle.Wall != null)
                        tile.wall = BaseStyle.Wall.Value;
                    if (BaseStyle.WallColor != null)
                        tile.wallColor(BaseStyle.WallColor.Value);
                }
                return this;
            }

            #endregion
            #region ShowGrid

            public void ShowGrid()
            {
                for (int i = 0; i < Configuration.Grid.Columns.Length; i++)
                    for (int j = 0; j < Configuration.Grid.Lines.Length; j++)
                        foreach ((int x, int y) in Points)
                            Provider[x, y].wallColor((byte)(25 + (i + j) % 2));
            }

            #endregion
            #region CustomApply

            public VisualObjectBase CustomApply()
            {
                Configuration.CustomApply?.Invoke(this);
                return this;
            }

            #endregion
            #region ApplyChild

            public virtual VisualObjectBase ApplyChild()
            {
                bool forceSection = false;
                lock (Child)
                    foreach (VisualObjectBase child in Child)
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

        public virtual VisualObjectBase Clear()
        {
            UITileProvider provider = Provider;
            foreach ((int x, int y) in ProviderPoints)
                provider[x, y].ClearEverything();

            return this;
        }

        #endregion
        #region Draw

        public virtual VisualObjectBase Draw(int dx = 0, int dy = 0, int width = -1, int height = -1)
        {
            (int ax, int ay) = AbsoluteXY();
            UI.DrawRect(ax + dx, ay + dy, width >= 0 ? width : Width, height >= 0 ? height : Height, ForceSection);
            return this;
        }

        #endregion
        #region DrawPoints

        public virtual VisualObjectBase DrawPoints(List<(int, int)> list)
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
        #region FullSize

        public VisualObjectBase FullSize(bool value = true)
        {
            Configuration.FullSize = value;
            return this;
        }

        #endregion
        #region FullName

        public string FullName()
        {
            if (Parent != null)
                return $"{Parent.FullName()}.{Name}";
            return Name;
        }

        #endregion
        #region Popup

        public virtual VisualObjectBase Popup()
        {
            VisualObjectBase popup = this["popup"] as VisualObjectBase;
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

        public virtual VisualObjectBase Popdown()
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
    }
}
