using System;
using System.Collections.Generic;
using System.Linq;

namespace TUI
{
    public class VisualObject : Touchable
    {
        #region Data

        public UIStyle Style { get; set; }
        public bool ForceSection { get; private set; } = false;
        private Dictionary<string, object> _Shortcuts { get; set; }

        #endregion

        #region IDOM

            #region Remove

            public override VisualObject Remove(VisualObject child)
            {
                foreach (var pair in _Shortcuts.Where(o => o.Value == child))
                    _Shortcuts.Remove(pair.Key);
                return base.Remove(child);
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
        }

        #endregion
        #region Clone

        public override object Clone() =>
            new VisualObject(X, Y, Width, Height, Configuration, Style, Callback);

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

        public virtual VisualObject Enable()
        {
            Enabled = true;
            return this;
        }

        #endregion
        #region Disable

        public virtual VisualObject Disable()
        {
            Enabled = false;
            return this;
        }

        #endregion
        #region Apply

        public virtual VisualObject Apply(bool forceClear = false)
        {
            if (!Active())
                throw new InvalidOperationException("Trying to call Apply() an not active object.");
            UI.SaveTime(this, "Apply");
            ApplyThis(forceClear);
            UI.SaveTime(this, "Apply", "This");
            ApplyChild();
            UI.ShowTime(this, "Apply", "Child");
            return this;
        }

        #endregion
        #region ApplyThis

        public virtual VisualObject ApplyThis(bool forceClear = false)
        {
            ApplyTiles(forceClear);
            if (UI.ShowGrid)
                ShowGrid();
            Configuration.CustomApply?.Invoke(this);
            return this;
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
                    foreach ((int x, int y) in Points)
                        Provider[x, y].wallColor((byte)(25 + (i + j) % 2));
        }

        #endregion
        #region ApplyChild

        public virtual VisualObject ApplyChild()
        {
            bool forceSection = false;
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
        #region FullSize

        public VisualObject FullSize(bool value = true)
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
    }
}
