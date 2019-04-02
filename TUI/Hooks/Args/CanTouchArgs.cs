using System.ComponentModel;

namespace TUI
{
    public class CanTouchArgs
    {
        public VisualObject Node { get; private set; }
        public Touch Touch { get; private set; }
        public bool CanTouch { get; set; }

        public CanTouchArgs(VisualObject node, Touch touch)
        {
            Node = node;
            Touch = touch;
            CanTouch = true;
        }
    }
}
