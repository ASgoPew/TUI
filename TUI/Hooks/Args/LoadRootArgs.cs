using System;
using TerrariaUI.Base;

namespace TerrariaUI.Hooks.Args
{
    public class LoadRootArgs : EventArgs
    {
        public RootVisualObject Root { get; private set; }

        public LoadRootArgs(RootVisualObject root)
        {
            Root = root;
        }
    }
}
