using System.Collections.Generic;

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
