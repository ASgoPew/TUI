using System.Collections.Generic;

namespace TerrariaUI.Base
{
    internal interface IDOM<T>
    {
        T Parent { get; }

        U Add<U>(U child, int? layer) where U : VisualObject;
        T Remove(T child);
        void RemoveAll();
        T GetRoot();
        bool IsAncestorFor(T o);
        U GetAncestor<U>() where U : VisualObject;
        T GetChild(int index);
        U GetChild<U>() where U : VisualObject;
        bool HasChild(T node);
        bool SetTop(T child);
        IEnumerable<T> DescendantDFS { get; }
        IEnumerable<T> DescendantBFS { get; }
    }
}
