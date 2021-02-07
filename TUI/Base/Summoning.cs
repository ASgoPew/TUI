using System.Collections.Generic;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Base
{
    public class SummoningNode
    {
        public VisualObject Node { get; internal set; }
        public Alignment Alignment { get; internal set; }
        public bool WasChild { get; internal set; }
        public int OldX { get; internal set; }
        public int OldY { get; internal set; }
        public bool Drag { get; internal set; }
        public bool Resize { get; internal set; }
    }

    public class Summoning
    {
        public Stack<SummoningNode> Summoned = new Stack<SummoningNode>();
        public int OldX { get; }
        public int OldY { get; }
        public int OldWidth { get; }
        public int OldHeight { get; }

        public int Count => Summoned.Count;
        public SummoningNode Top =>
            Summoned.Count > 0
                ? Summoned.Peek()
                : null;

        public Summoning(int oldX, int oldY, int oldWidth, int oldHeight)
        {
            OldX = oldX;
            OldY = oldY;
            OldWidth = oldWidth;
            OldHeight = oldHeight;
        }

        public void Push(VisualObject node, bool wasChild, Alignment alignment, bool drag, bool resize) =>
            Summoned.Push(new SummoningNode()
            {
                Node = node,
                Alignment = alignment,
                WasChild = wasChild,
                OldX = node.X,
                OldY = node.Y,
                Drag = drag,
                Resize = resize
            });

        public SummoningNode Pop() =>
            Summoned.Count > 0
                ? Summoned.Pop()
                : null;
    }
}
