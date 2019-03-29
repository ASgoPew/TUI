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
        public object Touch { get; set; }

        public CanTouchArgs(object touch)
        {
            Touch = touch;
        }
    }
}
