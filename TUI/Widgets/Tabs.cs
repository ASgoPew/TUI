using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    public class Tabs : VisualObject
    {
        #region Constructor

        public Tabs(int x, int y, int width, int height, UIConfiguration configuration = null,
                UIStyle style = null, Action<VisualObject, Touch> callback = null)
            : base(x, y, width, height, configuration, style, callback)
        {
        }

        #endregion
        #region Copy

        public Tabs(VisualObject visualObject)
            : base(visualObject)
        {
        }

        #endregion
    }
}
