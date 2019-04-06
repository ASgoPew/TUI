using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI.Base
{
    public class VisualObject<T> : VisualObjectBase
        where T : UIStyle
    {
        #region Data

        public T Style { get; set; }
        protected override UIStyle BaseStyle => Style;

        #endregion

        #region Initialize

        public VisualObject(int x, int y, int width, int height, UIConfiguration configuration = null, T style = null, Func<VisualObjectBase, Touch, bool> callback = null)
            : base(x, y, width, height, configuration, callback)
        {
            Style = style;
        }

        #endregion
        #region Clone

        public override object Clone() =>
            new VisualObject<T>(X, Y, Width, Height, Configuration, Style, Callback);

        #endregion
    }

    public class VisualObject : VisualObjectBase
    {
        #region Data

        public UIStyle Style { get; set; }
        protected override UIStyle BaseStyle => Style;

        #endregion

        #region Initialize

        public VisualObject(int x, int y, int width, int height, UIConfiguration configuration = null, UIStyle style = null, Func<VisualObjectBase, Touch, bool> callback = null)
            : base(x, y, width, height, configuration, callback)
        {
            Style = style ?? new UIStyle();
        }

        #endregion
        #region Clone

        public override object Clone() =>
            new VisualObject(X, Y, Width, Height, Configuration, BaseStyle, Callback);

        #endregion
    }
}
