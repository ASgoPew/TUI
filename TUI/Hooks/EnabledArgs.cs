namespace TUI
{
    public class EnabledArgs
    {
        public VisualObject Node { get; set; }
        public bool Value { get; set; }

        public EnabledArgs(VisualObject node, bool value)
        {
            Node = node;
            Value = value;
        }
    }
}