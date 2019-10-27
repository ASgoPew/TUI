using System;
using TerrariaUI.Base;

namespace TerrariaUI.Hooks.Args
{
    public class RemoveChestArgs : EventArgs
    {
        public VisualObject Node { get; set; }
        public dynamic Chest { get; set; }

        public RemoveChestArgs(VisualObject node, dynamic chest)
        {
            Node = node;
            Chest = chest;
        }
    }
}
