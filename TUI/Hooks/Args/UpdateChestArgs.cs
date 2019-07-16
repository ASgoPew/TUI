using System;
using TUI.Base;
using TUI.Widgets.Data;

namespace TUI.Hooks.Args
{
    public class UpdateChestArgs : EventArgs
    {
        public int X { get; }
        public int Y { get; }
        public ItemData[] Items { get; }
        public object Chest { get; set; }
        public VisualObject Node { get; set; }
        
        public UpdateChestArgs(int x, int y, ItemData[] items, VisualObject node)
        {
            X = x;
            Y = y;
            Items = items;
            Node = node;
        }
    }
}
