using System;
using TerrariaUI.Base;

namespace TerrariaUI.Hooks.Args
{
    public class EnabledArgs : EventArgs
    {
        public RootVisualObject Root { get; set; }
        public bool Value { get; set; }

        public EnabledArgs(RootVisualObject root, bool value)
        {
            Root = root;
            Value = value;
        }
    }
}