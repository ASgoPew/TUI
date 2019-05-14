using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    #region ItemFrameStyle

    public class ItemFrameStyle : UIStyle
    {
        public short Type { get; set; }
    }

    #endregion

    public class ItemFrame : VisualObject
    {
        public ItemFrame(int x, int y, UIConfiguration configuration = null, ItemFrameStyle style = null,
                Func<VisualObject, Touch, bool> callback = null)
            : base(x, y, 2, 2, configuration ?? new UIConfiguration(), style ?? new ItemFrameStyle(), callback)
        {
        }

        protected override void ApplyThisNative()
        {
            base.ApplyThisNative();

        }
    }
}
