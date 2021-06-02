using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
