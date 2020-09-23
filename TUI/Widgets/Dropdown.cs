using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Widgets
{
    #region DropdownStyle

    public class DropdownStyle : ContainerStyle
    {
        public DropdownStyle() : base() { }

        public DropdownStyle(DropdownStyle style) : base(style)
        {
            
        }
    }

    #endregion

    public class Dropdown : VisualContainer
    {
        #region Data



        #endregion

        #region Constructor

        public Dropdown(int X, int Y, DropdownStyle style = null, Input<int> input = null)
            : base(X, Y, 10, 10, new UIConfiguration() { UseBegin = false }, style ?? new DropdownStyle())
        {
            
        }

        #endregion
    }
}
