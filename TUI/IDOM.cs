using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
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
        IEnumerable<T> DescendantDFS { get; }
        IEnumerable<T> DescendantBFS { get; }
    }
}
