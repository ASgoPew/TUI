﻿namespace TerrariaUI.Base
{
    public interface IVisual<T>
    {
        int X { get; set; }
        int Y { get; set; }
        int Width { get; set; }
        int Height { get; set; }

        (int X, int Y, int Width, int Height) XYWH(int dx, int dy);
        T SetXYWH(int x, int y, int width, int height, bool draw);
        T Move(int dx, int dy, bool draw);
        bool ContainsParent(int x, int y);
        bool ContainsRelative(int x, int y);
        bool Intersecting(int x, int y, int width, int height);
        bool Intersecting(T o);
        (int x, int y) CenterPosition();
    }
}
