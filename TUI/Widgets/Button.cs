using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUI.Base;

namespace TUI.Widgets
{
    public class ButtonStyle : UIStyle
    {

    }

    public class Button : VisualObject<ButtonStyle>
    {
        public Button(int x, int y, int width, int height, UIConfiguration configuration = null, ButtonStyle style = null,
            Func<VisualObjectBase, Touch, bool> callback = null) : base(x, y, width, height, configuration, style, callback)
        {

        }
    }
}
