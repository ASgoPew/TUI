using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI.Widgets
{
    enum LabelUnderline
    {
        Nothing = 0,
        Underline,
        UnderlineWithTiles
    }

    public class LabelStyle : UIStyle
    {
        public Indentation Indentation { get; set; }
        public Alignment? Alignment { get; set; }
        public Side? Side { get; set; }
        public byte? TextColor { get; set; }
    }

    public class Label : VisualObject
    {
        public new LabelStyle Style { get; set; }

        public Label(int x, int y, int width, int height, string text, LabelStyle style = null)
            : base(x, y, width, height, new UIConfiguration() { UseBegin = false }, style)
        {

        }
    }
}
