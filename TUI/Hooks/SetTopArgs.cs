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
        public VisualObject Node { get; set; }

        public SetTopArgs(VisualObject node)
        {
            Node = node;
        }
    }
}
