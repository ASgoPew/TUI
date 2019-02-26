using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    enum LockLevel
    {
        None,
        Self,
        Root,
        FirstRoot
    }

    class UILock
    {
        public UILock(object locker, DateTime time, int delay, Touch touch)
        {

        }
    }
}
