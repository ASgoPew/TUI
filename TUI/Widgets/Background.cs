using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    public class Background : VisualObject
    {
        public Background(UIStyle style = null)
            : base(0, 0, 0, 0, new UIConfiguration() { UseBegin = false}, style)
        {
            Style.Positioning.FullSize = FullSize.Both;
        }

        public Background(Background background)
            : this(new UIStyle(background.Style))
        {
        }
    }
}
