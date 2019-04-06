using System.ComponentModel;
using TUI.Base;

namespace TUI.Hooks.Args
{
    public class CanTouchArgs
    {
        public VisualObjectBase Node { get; private set; }
        public Touch Touch { get; private set; }
        public bool CanTouch { get; set; }

        public CanTouchArgs(VisualObjectBase node, Touch touch)
        {
            Node = node;
            Touch = touch;
            CanTouch = true;
        }
    }
}
