using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public class SetTopArgs
    {
        public RootVisualObject Root { get; set; }

        public SetTopArgs(RootVisualObject root)
        {
            Root = root;
        }
    }
}
