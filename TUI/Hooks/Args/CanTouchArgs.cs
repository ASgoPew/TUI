using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public class CanTouchArgs : HandledEventArgs
    {
        public VisualObject Node { get; private set; }
        public Touch Touch { get; private set; }

        public CanTouchArgs(VisualObject node, Touch touch)
        {
            Node = node;
            Touch = touch;
        }
    }
}
