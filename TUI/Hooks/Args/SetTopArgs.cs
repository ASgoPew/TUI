using TUI.Base;

namespace TUI.Hooks.Args
{
    public class SetTopArgs
    {
        public RootVisualObject Root { get; set; }

        public SetTopArgs(RootVisualObject root)
        {
            Root = root;
        }
    }
}
