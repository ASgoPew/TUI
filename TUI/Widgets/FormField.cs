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
        public VisualObject Input { get; protected set; }

        public FormField(VisualObject input, int x, int y, int width, int height, string text, LabelStyle style = null)
            : base(x, y, width, height, text, new UIConfiguration() { UseBegin = false }, style)
        {
            Input = Add(input);
            Input.SetAlignmentInParent(new AlignmentStyle(Alignment.Right));
        }
    }
}
