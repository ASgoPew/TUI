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
        bool SetTop(T child);
        IEnumerable<T> DescendantDFS { get; }
        IEnumerable<T> DescendantBFS { get; }
    }
}
