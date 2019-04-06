using System.Collections.Generic;

namespace TUI.Base
{
    internal interface IDOM<T>
    {
        List<T> Child { get; }
        T Parent { get; }

        T Add(T child);
        T Remove(T child);
        T Select(T child);
        T Deselect();
        T GetRoot();
        bool IsAncestorFor(T o);
        bool SetTop(T child);
        IEnumerable<T> DescendantDFS { get; }
        IEnumerable<T> DescendantBFS { get; }
    }
}
