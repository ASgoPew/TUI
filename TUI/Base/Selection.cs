using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaUI.Base;

namespace TerrariaUI.Base
{
    public class Selection
    {
        public VisualObject Selected { get; set; }
        public List<VisualObject> DisabledChildren { get; } = new List<VisualObject>();

        public Selection(VisualObject node)
        {
            Selected = node;
        }
    }
}
