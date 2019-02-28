using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI.Widgets
{
    public class Background : VisualObject
    {
        public Background(UIStyle style = null)
            : base(0, 0, 0, 0, new UIConfiguration()
                { UseBegin = false, Padding = new PaddingConfig(0, 0, 0, 0) }, style)
            { }
    }
}
