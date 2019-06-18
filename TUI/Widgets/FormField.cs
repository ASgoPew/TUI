using System;
using TUI.Base;
using TUI.Base.Style;

namespace TUI.Widgets
{
    /// <summary>
    /// Widget for adding a label to the left side of some other input widget (Checkbox/InputLabel/Slider/...).
    /// </summary>
    public class FormField : Label
    {
        /// <summary>
        /// Corresponding input widget.
        /// </summary>
        public IInput Input { get; protected set; }

        /// <summary>
        /// Widget for adding a label to the left side of some other input widget (Checkbox/InputLabel/Slider/...).
        /// </summary>
        public FormField(IInput input, int x, int y, int width, int height, string text, LabelStyle style = null, ExternalOffset inputOffset = null)
            : base(x, y, width, height, text, new UIConfiguration() { UseBegin = false }, style)
        {
            if (!(input is VisualObject))
                throw new ArgumentException($"{nameof(input)} must be VisualObject, IInput.");
            Input = input;
            Add((VisualObject)input)
                .SetAlignmentInParent(Alignment.Right, inputOffset);
        }
    }
}
