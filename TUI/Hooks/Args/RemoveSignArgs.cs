using System;
using TerrariaUI.Base;

namespace TerrariaUI.Hooks.Args
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
