using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI.Hooks.Args
{
    public class RemoveSignArgs : EventArgs
    {
        public dynamic Sign { get; set; }

        public RemoveSignArgs(dynamic sign)
        {
            Sign = sign;
        }
    }
}
