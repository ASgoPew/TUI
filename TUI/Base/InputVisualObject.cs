using System;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Base
{
    public class InputVisualObject<T> : VisualObject
    {
        protected T Value;
        protected T DefaultValue;
        protected Action<InputVisualObject<T>, T> InputCallback;

        public InputVisualObject(int x, int y, int width, int height, UIConfiguration configuration = null,
                UIStyle style = null, Action<VisualObject, Touch> callback = null, T defaultValue = default, Action<InputVisualObject<T>, T> inputCallback = null)
            : base(x, y, width, height, configuration, style, callback)
        {
            InputCallback = inputCallback;
            DefaultValue = defaultValue;
        }

        public virtual T GetValue() => Value;

        public virtual void SetValue(T value)
        {
            if (value?.Equals(Value) == false || Value?.Equals(value) == false)
            {
                Value = value;
                InputCallback.Invoke(this, value);
            }
        }
    }
}
