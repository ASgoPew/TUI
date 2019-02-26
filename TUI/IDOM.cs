using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    #region IDOM<T>

    internal interface IDOM<T>
    {
        List<T> Child { get; }
        bool Enabled { get; set; }
        T Parent { get; }
        bool Rootable { get; set; }

        T Add(T child);
        bool Remove(T child);
        T Select(T child);
        T Deselect();
        T GetRoot();
        bool IsAncestorFor(T o);
        bool SetTop(T child);
        bool Active();
        IEnumerable<T> DescendantDFS { get; }
        IEnumerable<T> DescendantBFS { get; }
    }

    #endregion

    #region VisualDOM<T> : IDOM<T>

    public partial class VisualDOM<T> : IDOM<T>, IVisual<T>
        where T : VisualDOM<T>
    {
        public List<T> Child { get; private set; }
        public bool Enabled { get; set; }
        public T Parent { get; private set; }
        public bool Rootable { get; set; }

        public IEnumerable<T> DescendantDFS => GetDescendantDFS();
        public IEnumerable<T> DescendantBFS => GetDescendantBFS();

        public void InitializeDOM(bool rootable)
        {
            Child = new List<T>();
            Enabled = true;
            Parent = null;
            Rootable = rootable;
        }

        public virtual T Add(T child)
        {
            Child.Add(child);
            child.Parent = (T)this;

            if (UI != null && child.UI == null)
                foreach (T o in child.GetDescendantBFS())
                    o.UI = UI;

            return child;
        }

        public virtual bool Remove(T child)
        {
            bool result = Child.Remove(child);

            if (result)
            {
                if (child.Cell != null)
                {
                    child.Cell.Objects.Remove(child);
                    child.Cell = null;
                }
                return true;
            }
            return false;
        }

        public virtual T Select(T o)
        {
            if (!Child.Contains(o))
                throw new InvalidOperationException("Trying to Select an object that isn't a child of current VisualDOM");

            foreach (T child in Child)
                child.Enabled = false;
            o.Enabled = true;

            return (T)this;
        }

        public virtual T Deselect()
        {
            foreach (T child in Child)
                child.Enabled = true;

            return (T)this;
        }

        public T GetRoot()
        {
            T node = (T)this;

            while (node.Parent != null)
            {
                node = (T)node.Parent;
                if (node.Rootable)
                    return node;
            }

            return node;
        }

        public bool IsAncestorFor(T o)
        {
            T node = (T)Parent;

            while (node != null)
            {
                if (this == node)
                    return true;
                node = node.Parent;
            }

            return false;
        }

        public virtual bool SetTop(T o)
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

        public bool Active()
        {
            T node = (T)this;

            HashSet<T> was = new HashSet<T>();
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

        private void DFS(List<T> list)
        {
            list.Add((T)this);
            foreach (T child in Child)
                child.DFS(list);
        }

        private void BFS(List<T> list)
        {
            list.Add((T)this);
            int index = 0;
            while (index < list.Count)
            {
                foreach (T o in list[index].Child)
                    list.Add(o);
                index++;
            }
        }

        private IEnumerable<T> GetDescendantDFS()
        {
            List<T> list = new List<T>();
            DFS(list);

            foreach (T o in list)
                yield return o;
        }

        private IEnumerable<T> GetDescendantBFS()
        {
            List<T> list = new List<T>();
            BFS(list);

            foreach (T o in list)
                yield return o;
        }
    }

    #endregion
}
