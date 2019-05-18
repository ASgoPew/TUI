using System;
using System.Collections.Generic;
using System.Linq;

namespace TUI.Base
{
    public abstract class VisualDOM : IDOM<VisualObject>, IVisual<VisualObject>
    {
        #region Data

            #region IDOM

            protected List<VisualObject> Child { get; private set; } = new List<VisualObject>();
            public VisualObject Parent { get; private set; } = null;

            public IEnumerable<VisualObject> DescendantDFS => GetDescendantDFS();
            public IEnumerable<VisualObject> DescendantBFS => GetDescendantBFS();

            #endregion
            #region IVisual

            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public IEnumerable<(int, int)> Points => GetPoints();

            #endregion

        public int IndexInParent => Parent.Child.IndexOf(this as VisualObject);
        public RootVisualObject Root { get; set; }
        public virtual dynamic Provider => Root.Provider;
        public bool UsesDefaultMainProvider => Provider is MainTileProvider;
        public bool Enabled { get; set; } = true;
        public bool Visible { get; protected internal set; } = true;
        public virtual int Layer { get; set; } = 0;
        public UIConfiguration Configuration { get; set; }
        private Dictionary<string, object> Shortcuts { get; set; }
        //protected object UpdateLocker { get; set; } = new object();

        public virtual bool Active => Enabled && Visible;
        public virtual bool Orderable => true;
        public IEnumerable<VisualObject> ChildrenFromTop => GetChildrenFromTop();
        public IEnumerable<VisualObject> ChildrenFromBottom => GetChildrenFromBottom();
        public IEnumerable<(int X, int Y)> AbsolutePoints => GetAbsolutePoints();
        public IEnumerable<(int X, int Y)> ProviderPoints => GetProviderPoints();
        public (int X, int Y) AbsoluteXY(int dx = 0, int dy = 0) =>
            RelativeXY(dx, dy, null);
        public (int X, int Y) ProviderXY(int dx = 0, int dy = 0) =>
            RelativeXY(dx, dy, UsesDefaultMainProvider ? null : Root);

        #endregion

        #region IDOM

            #region Initialize

            public void InitializeDOM()
            {
            }

            #endregion
            #region Add

            public virtual VisualObject Add(VisualObject child, int layer = 0)
            {
                lock (Child)
                {
                    int index = 0;
                    while (index < Child.Count && Child[index].Layer <= layer)
                        index++;
                    Child.Insert(index, child);
                }
                child.Parent = this as VisualObject;
                child.Layer = layer;
                return child;
            }

            public virtual VisualObject AddToLayout(VisualObject child, int layer = 0)
            {
                Add(child, layer);
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
                    foreach (VisualObject child in ChildrenFromTop)
                        child.Enabled = false;
                o.Enabled = true;

                return this as VisualObject;
            }

            #endregion
            #region Deselect

            public virtual VisualObject Deselect()
            {
                lock (Child)
                    foreach (VisualObject child in ChildrenFromTop)
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
                    int previousIndex = index;
                    if (index < 0)
                        throw new InvalidOperationException("Trying to SetTop an object that isn't a child of current VisualDOM");
                    int count = Child.Count;
                    index++;
                    while (index < count && Child[index].Layer <= o.Layer)
                        index++;

                    if (index == previousIndex + 1)
                        return false;

                    Child.Remove(o);
                    Child.Insert(index - 1, o);
                    return true;
                }
            }

            /*public virtual bool SetTop(VisualObject o)
            {
                lock (Child)
                {
                    int index = Child.IndexOf(o);
                    int last = Child.Count - 1;
                    if (index < last)
                    {
                        Child.Remove(o);
                        Child.Add(o);
                        return true;
                    }
                    else if (index == last)
                        return false;
                    else
                        throw new InvalidOperationException("Trying to SetTop an object that isn't a child of current VisualDOM");
                }
            }*/

            #endregion
            #region DFS, BFS

            private void DFS(List<VisualObject> list)
            {
                list.Add(this as VisualObject);
                lock (Child)
                    foreach (VisualObject child in ChildrenFromTop)
                        child.DFS(list);
            }

            private void BFS(List<VisualObject> list)
            {
                list.Add(this as VisualObject);
                int index = 0;
                while (index < list.Count)
                {
                    lock (list[index].Child)
                        foreach (VisualObject o in list[index].ChildrenFromTop)
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

            public virtual bool ContainsRelative(int x, int y) =>
                x >= 0 && y >= 0 && x < Width && y < Height;

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
        #region CalculateActive

        public bool CalculateActive()
        {
            VisualDOM node = this;

            HashSet<VisualDOM> was = new HashSet<VisualDOM>();
            was.Add(node);
            while (node != null)
            {
                if (!node.Active)
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
        #region GetChildrenFromTop

        private IEnumerable<VisualObject> GetChildrenFromTop()
        {
            int index = Child.Count - 1;
            while (index >= 0)
                yield return Child[index--];
        }

        #endregion
        #region GetChildrenFromBottom

        private IEnumerable<VisualObject> GetChildrenFromBottom()
        {
            int count = Child.Count;
            int index = 0;
            while (index < count)
                yield return Child[index++];
        }

        #endregion
        #region GetAbsolutePoints

        private IEnumerable<(int, int)> GetAbsolutePoints()
        {
            (int x, int y) = AbsoluteXY();
            for (int _x = x; _x < x + Width; _x++)
                for (int _y = y; _y < y + Height; _y++)
                    yield return (_x, _y);
        }

        #endregion
        #region GetProviderPoints

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
