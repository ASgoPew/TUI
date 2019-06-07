using System.Collections.Generic;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    public class RadioList : VisualObject
    {
        public RadioList(int x, int y, int width, int height, Direction direction, UIStyle button1, UIStyle button2, int selectedColor, IEnumerable<string> texts, UIStyle style = null)
            : base(x, y, width, height, null, style)
        {

            foreach (VisualObject element in elements)
                AddToLayout(element);
        }

        public RadioList(RadioList visualObject) : base(visualObject)
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
