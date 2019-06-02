using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    public class FormField : Label
    {
        public FormField(int x, int y, int width, int height, string text, LabelStyle style = null, Func<VisualObject, Touch, bool> callback = null)
            : base(x, y, width, height, text, new UIConfiguration() { UseBegin = false }, style, callback)
        {
        }
    }
}
