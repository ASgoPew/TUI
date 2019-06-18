namespace TUI.Base
{
    public interface IInput
    {
        object Value { get; }
    }

    public interface IInput<T>
    {
        Input<T> Input { get; }
        T GetValue();
        /// <summary>
        /// Set temporarily value (Temp field) and draw it if draw parameter is true.
        /// </summary>
        void SetTempValue(T temp, bool draw);
        /// <summary>
        /// Submit temporarily value (Temp field), draw it if draw parameter is true.
        /// and save as actual value (Value field).
        /// </summary>
        void SetValue(T value, bool draw = false, int player = -1);
    }
}
