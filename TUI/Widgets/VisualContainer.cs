using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUI.Base;

namespace TUI.Widgets
{
    public class VisualContainer : VisualObject
    {
        public VisualContainer()
            : base(0, 0, 0, 0, new UIConfiguration() { UseBegin = false, FullSize = true })
        {
        }

        public VisualContainer(VisualContainer visualObject)
            : this()
        {
        }
    }
}
