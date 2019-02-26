using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    internal interface IVisual<T>
    {
        int X { get; set; }
        int Y { get; set; }
        int Width { get; set; }
        int Height { get; set; }

        (int X, int Y, int Width, int Height) XYWH(int dx, int dy);
        T SetXYWH(int x, int y, int width, int height);
        T SetXYWH((int x, int y, int width, int height) data);
        T Move(int dx, int dy);
        T MoveBack(int dx, int dy);
        bool Contains(int x, int y);
        bool Intersecting(int x, int y, int width, int height);
        bool Intersecting((int x, int y, int width, int height) data);
        (int X, int Y, int Width, int Height) Padding(PaddingData paddingData);
        IEnumerable<Point> Points { get; }
    }
}
