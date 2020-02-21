using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariaUI.Base
{
    public class Summoning
    {
        public VisualObject Summoned { get; }
        public bool WasChild { get; }
        public int WasX { get; }
        public int WasY { get; }
        public int OldX { get; }
        public int OldY { get; }
        public int OldWidth { get; }
        public int OldHeight { get; }
        public Summoning(VisualObject node, bool wasChild, int oldX, int oldY, int oldWidth, int oldHeight)
        {
            Summoned = node;
            WasChild = wasChild;
            WasX = node.X;
            WasY = node.Y;
            OldX = oldX;
            OldY = oldY;
            OldWidth = oldWidth;
            OldHeight = oldHeight;
        }
    }
}
