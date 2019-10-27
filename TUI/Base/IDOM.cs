using System.Collections.Generic;

namespace TerrariaUI.Base
{
    internal interface IDOM<T>
    {
        T Parent { get; }

        T Add(T child, int? layer);
        T Remove(T child);
        T Select(T child);
        T Selected();
        T Deselect();
        T GetRoot();
        bool IsAncestorFor(T o);
        bool SetTop(T child);
        IEnumerable<T> DescendantDFS { get; }
        IEnumerable<T> DescendantBFS { get; }
    }
}
