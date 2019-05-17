using System.Collections.Generic;

namespace TUI.Base
{
    public interface IVisual<T>
    {
        int X { get; set; }
        int Y { get; set; }
        int Width { get; set; }
        int Height { get; set; }

        (int X, int Y, int Width, int Height) XYWH(int dx, int dy);
        T SetXYWH(int x, int y, int width, int height);
        T Move(int dx, int dy);
        T MoveBack(int dx, int dy);
        bool Contains(int x, int y);
        bool ContainsRelative(int x, int y);
        bool Intersecting(int x, int y, int width, int height);
        bool Intersecting(T o);
        IEnumerable<(int, int)> Points { get; }
    }
}
