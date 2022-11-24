using System;
using TerrariaUI.Base;

namespace TerrariaUI.Hooks
{
    public class CreateProviderArgs : EventArgs
    {
        public RootVisualObject Root { get; set; }
        public dynamic Provider { get; set; }

        public CreateProviderArgs(RootVisualObject root)
        {
            Root = root;
        }
    }
}