using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace TUI.Base
{
    public abstract class VisualDOM : IDOM<VisualObject>, IVisual<VisualObject>
    {
        #region Data

            #region IDOM

            /// <summary>
            /// List of child objects.
            /// </summary>
            protected List<VisualObject> Child { get; private set; } = new List<VisualObject>();
            /// <summary>
            /// Parent object. Null for <see cref="RootVisualObject"/>.
            /// </summary>
            public VisualObject Parent { get; private set; } = null;

            #endregion
            #region IVisual

            /// <summary>
            /// X coordinate relative to Parent object.
            /// </summary>
            public int X { get; set; }
            /// <summary>
            /// Y coordinate relative to Parent object.
            /// </summary>
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            #endregion

        /// <summary>
        /// Root of the interface tree. Null before first <see cref="VisualObject.Update"/> call. Use <see cref="GetRoot"/> to calculate manually.
        /// </summary>
        public RootVisualObject Root { get; set; }
        /// <summary>
        /// True once the object was loaded. See <see cref="LoadThisNative"/>.
        /// </summary>
        public bool Loaded { get; private set; } = false;
        /// <summary>
        /// True once the object was disposed. See <see cref="DisposeThisNative"/>.
        /// </summary>
        public bool Disposed { get; private set; } = false;
        /// <summary>
        /// Whether the object is enabled. Disabled objects are invisible, you can't touch them and they don't receive updates.
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// Whether the object is visible. Object becomes invisible when it is outside of bounds of layout.
        /// </summary>
        public bool Visible { get; protected internal set; } = true;
        /// <summary>
        /// Layer in Parent's Child array. Objects with higher layer are always higher in this array.
        /// </summary>
        public virtual int Layer { get; private set; } = 0;
        /// <summary>
        /// Object touching and drawing settings.
        /// </summary>
        public UIConfiguration Configuration { get; set; }
        /// <summary>
        /// Locker for locking node related operations.
        /// </summary>
        protected object Locker { get; set; } = new object();
        /// <summary>
        /// Runtime storage for node related data.
        /// </summary>
        protected ConcurrentDictionary<string, object> Shortcuts { get; set; }

        /// <summary>
        /// Tile provider of current interface tree.
        /// </summary>
        public virtual dynamic Provider => Root?.Provider;
        /// <summary>
        /// True if Provider links to Main.tile provider.
        /// </summary>
        public bool UsesDefaultMainProvider => Provider is MainTileProvider;
        /// <summary>
        /// Index in Parent's Child array.
        /// </summary>
        public int IndexInParent => Parent.Child.IndexOf(this as VisualObject);
        /// <summary>
        /// DEBUG property: Child array size.
        /// </summary>
        /// <returns>Size of Child array</returns>
        public int ChildCount => Child.Count;
        /// <summary>
        /// Object is Active when it is Enabled and Visible (see <see cref="Enabled"/>, <see cref="Visible"/>)
        /// </summary>
        public virtual bool Active => Enabled && Visible;
        /// <summary>
        /// Overridable property for disabling ability to be ordered in Parent's Child array.
        /// </summary>
        public virtual bool Orderable => true;

        #endregion
        #region DescendantDFS

        /// <summary>
        /// Deep Fast Search method of iterating objects in sub-tree including this node.
        /// </summary>
        public IEnumerable<VisualObject> DescendantDFS
        {
            get
            {
                List<VisualObject> list = new List<VisualObject>();
                DFS(list);

                foreach (VisualObject o in list)
                    yield return o;
            }
        }

        #endregion
        #region DescendantBFS

        /// <summary>
        /// Broad Fast Search method of iterating objects in sub-tree including this node.
        /// </summary>
        public IEnumerable<VisualObject> DescendantBFS
        {
            get
            {
                List<VisualObject> list = new List<VisualObject>();
                BFS(list);

                foreach (VisualObject o in list)
                    yield return o;
            }
        }

        #endregion
        #region ChildrenFromTop

        /// <summary>
        /// Iterates over Child array starting with objects on top.
        /// </summary>
        protected IEnumerable<VisualObject> ChildrenFromTop
        {
            get
            {
                int index = Child.Count - 1;
                lock (Child)
                    while (index >= 0)
                        yield return Child[index--];
            }
        }

        #endregion
        #region ChildrenFromBottom

        /// <summary>
        /// Iterates over Child array starting with objects at bottom.
        /// </summary>
        protected IEnumerable<VisualObject> ChildrenFromBottom
        {
            get
            {
                int count = Child.Count;
                int index = 0;
                lock (Child)
                    while (index < count)
                        yield return Child[index++];
            }
        }

        #endregion
        #region Points

        /// <summary>
        /// Iterates over object points relative to this node.
        /// </summary>
        public IEnumerable<(int, int)> Points
        {
            get
            {
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        yield return (x, y);
            }
        }

        #endregion
        #region AbsolutePoints

        /// <summary>
        /// Iterates over object points relative to world map.
        /// </summary>
        public IEnumerable<(int, int)> AbsolutePoints
        {
            get
            {
                (int x, int y) = AbsoluteXY();
                for (int _x = x; _x < x + Width; _x++)
                    for (int _y = y; _y < y + Height; _y++)
                        yield return (_x, _y);
            }
        }

        #endregion
        #region ProviderPoints

        /// <summary>
        /// Iterates over object points relative to tile provider.
        /// </summary>
        public IEnumerable<(int, int)> ProviderPoints
        {
            get
            {
                (int x, int y) = ProviderXY();
                for (int _x = x; _x < x + Width; _x++)
                    for (int _y = y; _y < y + Height; _y++)
                        yield return (_x, _y);
            }
        }

        #endregion

        #region IDOM

            #region InitializeDOM

            internal void InitializeDOM()
            {
            }

            #endregion
            #region Add

            /// <summary>
            /// Adds object as a child in specified layer (0 by default). Does nothing if object is already a child.
            /// </summary>
            /// <param name="child">Object to add as a child.</param>
            /// <param name="layer">Layout to add object to.</param>
            /// <returns>Added object</returns>
            public virtual VisualObject Add(VisualObject child, int layer = 0)
            {
                VisualObject @this = this as VisualObject;
                lock (Child)
                {
                    if (Child.Contains(child))
                        return child;
                        //throw new InvalidOperationException("You can't add an object that is already a child: " + child.FullName);

                    int index = 0;
                    while (index < Child.Count && Child[index].Layer <= layer)
                        index++;
                    Child.Insert(index, child);
                }
                child.Parent = @this;
                child.Layer = layer;

                TUI.TryToLoadChild(@this, child);

                return child;
            }

            #endregion
            #region Remove

            /// <summary>
            /// Removes child object. Calls Dispose() on removed object so you can't use
            /// this object anymore.
            /// </summary>
            /// <param name="child">Child object to remove.</param>
            /// <returns>this</returns>
            public virtual VisualObject Remove(VisualObject child)
            {
                lock (Child)
                    if (!Child.Remove(child))
                        throw new InvalidOperationException("You can't remove object that isn't a child.");

                // Should probably update these fields in all sub-tree.
                // Currently removed object = disposed object. You can't use it anymore.
                //child.Parent = null;
                //child.Root = null;
                if (Shortcuts != null)
                    foreach (var pair in Shortcuts.Where(o => o.Value == child))
                        Shortcuts.TryRemove(pair.Key, out object _);

                child.Dispose();

                return this as VisualObject;
            }

            #endregion
            #region Select

            /// <summary>
            /// Enable specified child and disable all other child objects.
            /// </summary>
            /// <param name="o">Child to select.</param>
            /// <returns>this</returns>
            public virtual VisualObject Select(VisualObject o)
            {
                if (!Child.Contains(o))
                    throw new InvalidOperationException("Trying to Select an object that isn't a child of current VisualDOM");

                foreach (VisualObject child in ChildrenFromTop)
                    child.Disable();
                o.Enable();

                return this as VisualObject;
            }

            #endregion
            #region Selected

            /// <summary>
            /// Searches for first Enabled child. See <see cref="Select(VisualObject)"/>.
            /// </summary>
            /// <returns>Selected child object</returns>
            public virtual VisualObject Selected()
            {
                lock (Child)
                    return Child.FirstOrDefault(c => c.Enabled);
            }

            #endregion
            #region Deselect

            /// <summary>
            /// Enables all child objects. See <see cref="Select(VisualObject)"/>
            /// </summary>
            /// <returns>this</returns>
            public virtual VisualObject Deselect()
            {
                foreach (VisualObject child in ChildrenFromTop)
                    child.Enable();

                return this as VisualObject;
            }

            #endregion
            #region GetRoot

            /// <summary>
            /// Searches for root node (VisualObject) in hierarchy. Must be a RootVisualObject in a valid TUI tree.
            /// </summary>
            /// <returns>Root object</returns>
            public VisualObject GetRoot()
            {
                VisualDOM node = this;
                while (node.Parent != null)
                    node = node.Parent;
                return node as VisualObject;
            }

            #endregion
            #region IsAncestorFor

            /// <summary>
            /// Checks if this node is an ancestor for an object.
            /// </summary>
            /// <param name="child">Object to check whether current node is an ancestor for it.</param>
            public bool IsAncestorFor(VisualObject child)
            {
                VisualObject node = child.Parent;

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

            /// <summary>
            /// Places child object on top of layer. This function will be called automatically on child touch
            /// if object is orderable. See <see cref="UIConfiguration.Ordered"/>.
            /// </summary>
            /// <param name="child">Child object to place on top of layer.</param>
            /// <returns>True if child object position changed</returns>
            public virtual bool SetTop(VisualObject child)
            {
                lock (Child)
                {
                    int index = Child.IndexOf(child);
                    int previousIndex = index;
                    if (index < 0)
                        throw new InvalidOperationException("Trying to SetTop an object that isn't a child of current VisualDOM");
                    int count = Child.Count;
                    index++;
                    while (index < count && Child[index].Layer <= child.Layer)
                        index++;

                    if (index == previousIndex + 1)
                        return false;

                    Child.Remove(child);
                    Child.Insert(index - 1, child);
                    return true;
                }
            }

            #endregion
            #region DFS, BFS

            private void DFS(List<VisualObject> list)
            {
                list.Add(this as VisualObject);
                foreach (VisualObject child in ChildrenFromTop)
                    child.DFS(list);
            }

            private void BFS(List<VisualObject> list)
            {
                list.Add(this as VisualObject);
                int index = 0;
                while (index < list.Count)
                {
                    foreach (VisualObject o in list[index].ChildrenFromTop)
                        list.Add(o);
                    index++;
                }
            }

            #endregion

        #endregion
        #region IVisual

            #region InitializeVisual

            internal void InitializeVisual(int x, int y, int width, int height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }

            #endregion
            #region XYWH, SetXYWH

            /// <summary>
            /// Get object position and size.
            /// </summary>
            /// <param name="dx">X coordinate delta</param>
            /// <param name="dy">Y coordinate delta</param>
            /// <returns>Tuple of values (x, y, width, height)</returns>
            public virtual (int X, int Y, int Width, int Height) XYWH(int dx = 0, int dy = 0)
            {
                return (X + dx, Y + dy, Width, Height);
            }

            /// <summary>
            /// Sets object position and size.
            /// </summary>
            /// <param name="x">New x coordinate</param>
            /// <param name="y">New y coordinate</param>
            /// <param name="width">New width</param>
            /// <param name="height">New height</param>
            /// <returns>this</returns>
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
            public VisualObject SetXY((int x, int y) pair) =>
                SetXYWH(pair.x, pair.y, Width, Height);
            public VisualObject SetWH(int width, int height) =>
                SetXYWH(X, Y, width, height);
            public VisualObject SetWH((int width, int height) pair) =>
                SetXYWH(X, Y, pair.width, pair.height);

            #endregion
            #region Move

            /// <summary>
            /// Move object by delta x and delta y.
            /// </summary>
            /// <param name="dx">X coordinate delta</param>
            /// <param name="dy">Y coordinate delta</param>
            /// <returns>this</returns>
            public virtual VisualObject Move(int dx, int dy) =>
                SetXYWH(X + dx, Y + dy, Width, Height);

            /// <summary>
            /// Inverse function for <see cref="Move(int, int)"/>.
            /// </summary>
            /// <param name="dx">X coordinate delta</param>
            /// <param name="dy">Y coordinate delta</param>
            /// <returns>this</returns>
            public virtual VisualObject MoveBack(int dx, int dy) =>
                SetXYWH(X - dx, Y - dy, Width, Height);

            #endregion
            #region Contains, Intersecting

            /// <summary>
            /// Checks if point (x, y) relative to Parent object is inside current object.
            /// </summary>
            /// <param name="x">X point coordinate</param>
            /// <param name="y">Y point coordinate</param>
            public virtual bool Contains(int x, int y) =>
                x >= X && y >= Y && x < X + Width && y < Y + Height;

            /// <summary>
            /// Checks if point (x, y) relative to this object is inside this object.
            /// </summary>
            /// <param name="x">X point coordinate</param>
            /// <param name="y">Y point coordinate</param>
            public virtual bool ContainsRelative(int x, int y) =>
                x >= 0 && y >= 0 && x < Width && y < Height;

            /// <summary>
            /// Checks if rectangle (x, y, width, height) relative to Parent object is intersecting current object.
            /// </summary>
            /// <param name="x">X rectangle coordinate</param>
            /// <param name="y">Y rectangle coordinate</param>
            /// <param name="width">Rectangle width</param>
            /// <param name="height">Rectangle height</param>
            public virtual bool Intersecting(int x, int y, int width, int height) =>
                x < X + Width && X < x + width && y < Y + Height && Y < y + height;

            public virtual bool Intersecting(VisualObject o) => Intersecting(o.X, o.Y, o.Width, o.Height);

            #endregion

        #endregion

        #region Constructor

        public VisualDOM(int x, int y, int width, int height, UIConfiguration configuration = null)
        {
            InitializeDOM();
            InitializeVisual(x, y, width, height);

            Configuration = configuration ?? new UIConfiguration();
        }

        #endregion
        #region Load

        /// <summary>
        /// Called on every object in TUI.Child on TUI.Load() (GamePostInitialize) and on Add() function.
        /// </summary>
        internal void Load()
        {
            lock (Locker)
            {
                if (Loaded)
                    return;
                Loaded = true;
            }

            foreach (VisualObject child in ChildrenFromTop)
                child.Load();

            try
            {
                LoadThisNative();
            }
            catch (Exception e)
            {
                TUI.Hooks.Log.Invoke(new Hooks.Args.LogArgs("TUI Load Exception: " + e.ToString(), Hooks.Args.LogType.Error));
            }
        }

        #endregion
        #region LoadThisNative

        /// <summary>
        /// Overridable function that runs only once for loading resources purposes.
        /// <para></para>
        /// Guaranteed that the call will happen only once and that it would be at
        /// a moment when game would be already initialized (on/after GamePostInitialize).
        /// <para></para>
        /// All child objects would be already loaded at the moment of calling this function.
        /// </summary>
        protected virtual void LoadThisNative() { }

        #endregion
        #region Dispose

        /// <summary>
        /// Called on every object in TUI.Child on TUI.Dispose() (plugin disposing) and on Remove() function.
        /// </summary>
        internal void Dispose()
        {
            lock (Locker)
            {
                if (Disposed || !Loaded)
                    return;
                Disposed = true;
            }

            foreach (VisualObject child in ChildrenFromTop)
                child.Dispose();

            try
            {
                DisposeThisNative();
            }
            catch (Exception e)
            {
                TUI.Hooks.Log.Invoke(new Hooks.Args.LogArgs("TUI Dispose Exception: " + e.ToString(), Hooks.Args.LogType.Error));
            }
        }

        #endregion
        #region DisposeThisNative

        /// <summary>
        /// Overridable function that runs only once for releasing resources purposes.
        /// <para></para>
        /// Guaranteed that the call would happen only once and that it would only happen
        /// in case object was already loaded with Load().
        /// <para></para>
        /// All child objects would be already disposed at the moment of calling this function.
        /// </summary>
        protected virtual void DisposeThisNative() { }

        #endregion

        #region GetChild

        /// <summary>
        /// DEBUG function. Get child object by index in Child array.
        /// </summary>
        /// <param name="index">Index in Child array</param>
        /// <returns>Child with specified index in Child array</returns>
        public virtual VisualObject GetChild(int index) => Child[index];

        #endregion
        #region operator[]

        /// <summary>
        /// Get/set node related data in runtime storage.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>this</returns>
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
                    Shortcuts = new ConcurrentDictionary<string, object>();
                Shortcuts[key] = value;
            }
        }

        #endregion
        #region Enable

        /// <summary>
        /// Enables object. See <see cref="Enabled"/>.
        /// </summary>
        /// <returns>this</returns>
        public virtual VisualObject Enable()
        {
            Enabled = true;
            return this as VisualObject;
        }

        #endregion
        #region Disable

        /// <summary>
        /// Disables object. See <see cref="Enabled"/>.
        /// </summary>
        /// <returns>this</returns>
        public virtual VisualObject Disable()
        {
            Enabled = false;
            return this as VisualObject;
        }

        #endregion
        #region CalculateActive

        /// <summary>
        /// Finds out if every object including this node and up to the Root is Active. Root must be
        /// a RootVisualObject. See <see cref="Active"/>.
        /// </summary>
        public bool CalculateActive()
        {
            VisualDOM node = this;

            while (!(node is RootVisualObject) && node != null)
            {
                if (!node.Active)
                    return false;
                node = node.Parent;
            }
            return node is RootVisualObject;
        }

        #endregion
        #region RelativeXY

        /// <summary>
        /// Calculates coordinates relative to specified parent object.
        /// </summary>
        /// <param name="x">X coordinate delta</param>
        /// <param name="y">Y coordinate delta</param>
        /// <param name="parent">Object which calculation is relative to</param>
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
        #region AbsoluteXY

        /// <summary>
        /// Calculates coordinates relative to world map.
        /// </summary>
        /// <param name="dx">X coordinate delta</param>
        /// <param name="dy">Y coordinate delta</param>
        /// <returns>Coordinates relative to world map</returns>
        public (int X, int Y) AbsoluteXY(int dx = 0, int dy = 0) =>
            RelativeXY(dx, dy, null);

        #endregion
        #region ProviderXY

        /// <summary>
        /// Calculates coordinates relative to tile provider.
        /// </summary>
        /// <param name="dx">X coordinate delta</param>
        /// <param name="dy">Y coordinate delta</param>
        /// <returns>Coordinates relative to tile provider</returns>
        public (int X, int Y) ProviderXY(int dx = 0, int dy = 0) =>
            RelativeXY(dx, dy, UsesDefaultMainProvider ? null : Root);

        #endregion
    }
}
