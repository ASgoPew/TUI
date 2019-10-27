using System;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Widgets
{
    /// <summary>
    /// Widget for adding a label to the left side of some other input widget (Checkbox/InputLabel/Slider/...).
    /// </summary>
    public class FormField : Label
    {
        #region Data

        /// <summary>
        /// Corresponding input widget.
        /// </summary>
        public IInput Input { get; protected set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Widget for adding a label to the left side of some other input widget (Checkbox/InputLabel/Slider/...).
        /// </summary>
        public FormField(IInput input, int x, int y, int width, int height, string text, LabelStyle style = null, ExternalIndent inputIndent = null)
            : base(x, y, width, height, text, new UIConfiguration() { UseBegin = false }, style)
        {
            if (!(input is VisualObject))
                throw new ArgumentException($"{nameof(input)} must be VisualObject, IInput.");
            Input = input;
            Add((VisualObject)input)
                .SetAlignmentInParent(Alignment.Right, inputIndent);
        }

        #endregion
    }
}
