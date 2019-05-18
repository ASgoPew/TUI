using System;

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
