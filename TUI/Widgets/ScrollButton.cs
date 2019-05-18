using System;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    public class ScrollButton : VisualObject
    {
        public ScrollButton(int x, int y, int width, int height, UIConfiguration configuration = null, UIStyle style = null, Func<VisualObject, Touch, bool> callback = null) : base(x, y, width, height, configuration, style, callback)
        {
        }

        public ScrollButton(VisualObject visualObject) : base(visualObject)
        {
        }
    }
}
