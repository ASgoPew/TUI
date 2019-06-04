using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
