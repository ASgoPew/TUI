using System;
using TerrariaUI.Base;

namespace TerrariaUI.Hooks.Args
{
    public class SetTopArgs : EventArgs
    {
        public RootVisualObject Root { get; set; }

        public SetTopArgs(RootVisualObject root)
        {
            Root = root;
        }
    }
}
