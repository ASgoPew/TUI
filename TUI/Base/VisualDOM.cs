using System;
using System.Collections.Generic;
using System.Linq;

namespace TUI.Base
{
    public abstract class VisualDOM : IDOM<VisualObject>, IVisual<VisualObject>
    {
        #region Data

        public int IndexInParent { get; private set; } = -1;
        public RootVisualObject Root { get; set; }
        public virtual dynamic Provider => Root.Provider;
        public bool UsesDefaultMainProvider => Provider is MainTileProvider;
        public bool Enabled { get; set; } = true;
        public UIConfiguration Configuration { get; set; }
        private Dictionary<string, object> Shortcuts { get; set; }
        //protected object UpdateLocker { get; set; } = new object();

        public IEnumerable<(int X, int Y)> AbsolutePoints => GetAbsolutePoints();
        public IEnumerable<(int X, int Y)> ProviderPoints => GetProviderPoints();
        public (int X, int Y) AbsoluteXY(int dx = 0, int dy = 0) =>
            RelativeXY(dx, dy, null);
        public (int X, int Y) ProviderXY(int dx = 0, int dy = 0) =>
            RelativeXY(dx, dy, UsesDefaultMainProvider ? null : Root);

        #endregion

        #region IDOM

            #region Data

            public List<VisualObject> Child { get; private set; } = new List<VisualObject>();
            public VisualObject Parent { get; private set; } = null;

            public IEnumerable<VisualObject> DescendantDFS => GetDescendantDFS();
            public IEnumerable<VisualObject> DescendantBFS => GetDescendantBFS();

            #endregion

            #region Initialize

            public void InitializeDOM()
            {
            }

            #endregion
            #region Add

            public virtual VisualObject Add(VisualObject child)
            {
                lock (Child)
                    Child.Add(child);
                child.Parent = this as VisualObject;
                child.IndexInParent = Child.Count - 1;
                return child;
            }

            public virtual VisualObject AddToLayout(VisualObject child)
            {
                Add(child);
                child.Style.Positioning.InLayout = true;
                return child;
            }

            #endregion
            #region Remove

            public virtual VisualObject Remove(VisualObject child)
            {
                bool removed;
                lock (Child)
                    removed = Child.Remove(child);
                if (removed)
                {
                    child.Parent = null;
                    child.IndexInParent = -1;
                    if (Shortcuts != null)
                        foreach (var pair in Shortcuts.Where(o => o.Value == child))
                            Shortcuts.Remove(pair.Key);
                    return child;
                }
                return null;
            }

            #endregion
            #region Select

            public virtual VisualObject Select(VisualObject o)
            {
                if (!Child.Contains(o))
                    throw new InvalidOperationException("Trying to Select an object that isn't a child of current VisualDOM");

                lock (Child)
                    foreach (VisualObject child in Child)
                        child.Enabled = false;
                o.Enabled = true;

                return this as VisualObject;
            }

            public virtual VisualObject Deselect()
            {
                lock (Child)
                    foreach (VisualObject child in Child)
                        child.Enabled = true;

                return this as VisualObject;
            }

            #endregion
            #region GetRoot

            public VisualObject GetRoot()
            {
                VisualDOM node = this;
                while (node.Parent != null)
                    node = node.Parent;
                return node as VisualObject;
            }

            #endregion
            #region IsAncestorFor

            public bool IsAncestorFor(VisualObject o)
            {
                VisualObject node = Parent;

                while (node != null)
                {
                    if (this == node)
                        return true;
                    node = node.Parent;
                }

                return false;
            }

            #endregion
            #region SetTop

            public virtual bool SetTop(VisualObject o)
            {
                lock (Child)
                {
                    int index = Child.IndexOf(o);
                    if (index > 0)
                    {
                        Child.Remove(o);
                        Child.Insert(0, o);
                        return true;
                    }
                    else if (index == 0)
                        return false;
                    else
                        throw new InvalidOperationException("Trying to SetTop an object that isn't a child of current VisualDOM");
                }
            }

            #endregion
            #region DFS, BFS

            private void DFS(List<VisualObject> list)
            {
                list.Add(this as VisualObject);
                lock (Child)
                    foreach (VisualObject child in Child)
                        child.DFS(list);
            }

            private void BFS(List<VisualObject> list)
            {
                list.Add(this as VisualObject);
                int index = 0;
                while (index < list.Count)
                {
                    lock (list[index].Child)
                        foreach (VisualObject o in list[index].Child)
                            list.Add(o);
                    index++;
                }
            }

            private IEnumerable<VisualObject> GetDescendantDFS()
            {
                List<VisualObject> list = new List<VisualObject>();
                DFS(list);

                foreach (VisualObject o in list)
                    yield return o;
            }

            private IEnumerable<VisualObject> GetDescendantBFS()
            {
                List<VisualObject> list = new List<VisualObject>();
                BFS(list);

                foreach (VisualObject o in list)
                    yield return o;
            }

            #endregion

        #endregion
        #region IVisual

            #region Data

            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public IEnumerable<(int, int)> Points => GetPoints();

            #endregion

            #region Initialize

            public void InitializeVisual(int x, int y, int width, int height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }

            #endregion
            #region XYWH, SetXYWH

            public virtual (int X, int Y, int Width, int Height) XYWH(int dx = 0, int dy = 0)
            {
                return (X + dx, Y + dy, Width, Height);
            }

            public virtual VisualObject SetXYWH(int x, int y, int width, int height)
            {
                X = x;
                Y = y;
                Width = width >= 0 ? width : Width;
                Height = height >= 0 ? height : Height;
                return this as VisualObject;
            }

            public VisualObject SetXYWH((int x, int y, int width, int height) data) =>
                SetXYWH(data.x, data.y, data.width, data.height);
            public VisualObject SetXY(int x, int y) =>
                SetXYWH(x, y, Width, Height);
            public VisualObject SetWH(int width, int height) =>
                SetXYWH(X, Y, width, height);

            #endregion
            #region Move

            public virtual VisualObject Move(int dx, int dy) =>
                SetXYWH(X + dx, Y + dy, Width, Height);

            public virtual VisualObject MoveBack(int dx, int dy) =>
                SetXYWH(X - dx, Y - dy, Width, Height);

            #endregion
            #region Contains, Intersecting

            public virtual bool Contains(int x, int y) =>
                x >= X && y >= Y && x < X + Width && y < Y + Height;

            public virtual bool Intersecting(int x, int y, int width, int height) =>
                x < X + Width && X < x + width && y < Y + Height && Y < y + height;

            public virtual bool Intersecting(VisualObject o) => Intersecting(o.X, o.Y, o.Width, o.Height);

            #endregion
            #region Points

            private IEnumerable<(int, int)> GetPoints()
            {
                for (int x = X; x < X + Width; x++)
                    for (int y = Y; y < Y + Height; y++)
                        yield return (x, y);
            }

            #endregion

        #endregion

        #region Initialize

        public VisualDOM(int x, int y, int width, int height, UIConfiguration configuration = null)
        {
            InitializeDOM();
            InitializeVisual(x, y, width, height);

            Configuration = configuration ?? new UIConfiguration();
        }

        #endregion
        #region operator[]

        public object this[string key]
        {
            get
            {
                object value = null;
                Shortcuts?.TryGetValue(key, out value);
                return value;
            }
            set
            {
                if (Shortcuts == null)
                    Shortcuts = new Dictionary<string, object>();
                Shortcuts[key] = value;
            }
        }

        #endregion
        #region Enable

        public virtual VisualObject Enable()
        {
            Enabled = true;
            return this as VisualObject;
        }

        #endregion
        #region Disable

        public virtual VisualObject Disable()
        {
            Enabled = false;
            return this as VisualObject;
        }

        #endregion
        #region Active

        public bool Active()
        {
            VisualDOM node = this;

            HashSet<VisualDOM> was = new HashSet<VisualDOM>();
            was.Add(node);
            while (node != null)
            {
                if (!node.Enabled)
                    return false;
                was.Add(node);
                node = node.Parent;
            }
            return true;
        }

        #endregion
        #region RelativeXY

        public (int X, int Y) RelativeXY(int x = 0, int y = 0, VisualDOM parent = null)
        {
            VisualDOM node = this;
            while (node != parent)
            {
                x += node.X;
                y += node.Y;
                node = node.Parent;
            }
            return (x, y);
        }

        #endregion
        #region AbsolutePoints

        private IEnumerable<(int, int)> GetAbsolutePoints()
        {
            (int x, int y) = AbsoluteXY();
            for (int _x = x; _x < x + Width; _x++)
                for (int _y = y; _y < y + Height; _y++)
                    yield return (_x, _y);
        }

        #endregion
        #region ProviderPoints

        private IEnumerable<(int, int)> GetProviderPoints()
        {
            (int x, int y) = ProviderXY();
            for (int _x = x; _x < x + Width; _x++)
                for (int _y = y; _y < y + Height; _y++)
                    yield return (_x, _y);
        }

        #endregion
    }
}
