using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    public class VisualContainer : VisualObject
    {
        public VisualContainer(UIStyle style = null)
            : base(0, 0, 0, 0, new UIConfiguration() { UseBegin = false }, style)
        {
            Style.Positioning.FullSize = FullSize.Both;
        }

        public VisualContainer(VisualContainer visualObject)
            : this(new UIStyle(visualObject.Style))
        {
        }
    }
}
