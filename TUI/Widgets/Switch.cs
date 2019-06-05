using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    public class SwitchStyle
    {
        //public int 
    }

    public class Switch : VisualObject
    {
        public Switch(int x, int y, int width, int height, UIStyle style = null)
            : base(x, y, width, height, new UIConfiguration() {          }, style)
        {

        }
    }
}
