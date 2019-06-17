using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI.Base
{
    public class Input<T>
    {
        public T Value { get; set; }
        public T OldValue { get; set; }
        public T DefaultValue { get; set; }
        public Action<VisualObject, T> Callback { get; set; }

        public Input(T value, T defaultValue, Action<VisualObject, T> callback)
        {
            Value = value;
            DefaultValue = defaultValue;
            Callback = callback;
        }
    }
}
