using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public class VisualObject : Touchable<VisualObject>
    {
        #region Data

        public UIStyle Style { get; set; }

        #endregion

        #region Touchable

        #region PostSetTop

        public override void PostSetTop()
        {
            Apply(true).Draw();
        }

        #endregion

        #endregion

        #region Initialize

        VisualObject(int x, int y, int width, int height, string name, UIConfiguration configuration, UIStyle style, Func<VisualObject, Touch<VisualObject>, bool> callback = null)
            : base(x, y, width, height, configuration, callback)
        {
            Style = style;
        }

        #endregion
        #region Apply

        public virtual VisualObject Apply(bool forceClear = false)
        {
            return this;
        }

        #endregion
        #region Draw

        public virtual VisualObject Draw()
        {
            return this;
        }

        #endregion
    }
}
