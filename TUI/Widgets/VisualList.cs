using System.Collections.Generic;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    public class VisualList : VisualObject
    {
        public VisualList(int x, int y, int width, int height, UIStyle style = null, IEnumerable<VisualObject> elements = null)
            : base(x, y, width, height, new UIConfiguration(), style)
        {

            foreach (VisualObject element in elements)
                AddToLayout(element);
        }

        public VisualList(VisualList visualObject) : base(visualObject)
        {
        }

        public VisualObject AddElement(VisualObject element) =>
            AddToLayout(element);

        public void AddElements(IEnumerable<VisualObject> elements)
        {
            foreach (VisualObject element in elements)
                AddElement(element);
        }
    }
}
