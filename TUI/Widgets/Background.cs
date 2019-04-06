using TUI.Base;

namespace TUI.Widgets
{
    public class Background : VisualObject
    {
        public Background(UIStyle style = null)
            : base(0, 0, 0, 0, new UIConfiguration() { UseBegin = false, FullSize = true }, style)
        {
        }

        public Background(Background background)
            : this(new UIStyle(background.Style))
        {
        }
    }
}
