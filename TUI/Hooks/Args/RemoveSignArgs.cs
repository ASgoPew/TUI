using System;
using TUI.Base;

namespace TUI.Hooks.Args
{
    public class RemoveSignArgs : EventArgs
    {
        public VisualObject Node { get; set; }
        public dynamic Sign { get; set; }

        public RemoveSignArgs(VisualObject node, dynamic sign)
        {
            Node = node;
            Sign = sign;
        }
    }
}
