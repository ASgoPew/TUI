using System.ComponentModel;

namespace TUI
{
    public class CanTouchArgs : HandledEventArgs
    {
        public VisualObject Node { get; private set; }
        public Touch Touch { get; private set; }

        public CanTouchArgs(VisualObject node, Touch touch)
        {
            Node = node;
            Touch = touch;
        }
    }
}
