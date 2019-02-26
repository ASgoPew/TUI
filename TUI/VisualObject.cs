using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public class VisualObject : VisualDOM<VisualObject>, ITouchable
    {
        VisualObject(int x, int y, int width, int height, GridConfiguration gridConfig = null, bool rootable = false)
            : base(x, y, width, height, gridConfig, rootable)
        {

        }
    }
}
