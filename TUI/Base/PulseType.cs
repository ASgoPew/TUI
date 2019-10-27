namespace TerrariaUI.Base
{
    /// <summary>
    /// Type of pulse signal to send to VisualObject(s).
    /// </summary>
    public enum PulseType
    {
        /// <summary>
        /// Object reset signal. Input widgets (like InputLabel, Checkbox, Slider, ...)
        /// set value to default on this signal.
        /// </summary>
        Reset = 0,
        /// <summary>
        /// This signal is called automatically for whole sub-tree when root of this sub-tree
        /// changes position/size.
        /// </summary>
        PositionChanged,
        /// <summary>
        /// User defined signal 1
        /// </summary>
        User1,
        /// <summary>
        /// User defined signal 2
        /// </summary>
        User2,
        /// <summary>
        /// User defined signal 3
        /// </summary>
        User3
    }
}