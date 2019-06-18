using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI.Base
{
    public class Input<T>
    {
        /// <summary>
        /// Data storage for input widgets like Slider, Checkbox, ...
        /// </summary>
        public T Value { get; set; }
        /// <summary>
        /// Default value which should be set on PulseType.Reset in input widgets.
        /// </summary>
        public T DefaultValue { get; set; }
        /// <summary>
        /// Storage for temporarily value (when in process of selecting value).
        /// Usually corresponds to what is drown for user currently.
        /// </summary>
        public T Temp { get; set; }
        /// <summary>
        /// Callback that should be invoked when Value field changes in input widgets.
        /// Parameters: Input widget, value, player
        /// </summary>
        public Action<VisualObject, T, int> Callback { get; set; }

        public Input(T value, T defaultValue, Action<VisualObject, T, int> callback = null)
        {
            Value = value;
            DefaultValue = defaultValue;
            Callback = callback;
        }

        /// <summary>
        /// Invoke Callback and set Value to Temp.
        /// </summary>
        public void SubmitTemp(VisualObject node, int player = -1)
        {
            if (!Temp.Equals(Value))
            {
                Callback?.Invoke(node, Temp, player);
                // Changing Value after callback so that now callback can access old Value
                Value = Temp;
            }
        }
    }
}
