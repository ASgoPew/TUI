using System;
using TUI.Base;

namespace TUI.Hooks.Args
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
