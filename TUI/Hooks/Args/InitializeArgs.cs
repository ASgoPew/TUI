namespace TUI.Hooks.Args
{
    public class InitializeArgs
    {
        public int MaxUsers { get; private set; }

        public InitializeArgs(int maxUsers)
        {
            MaxUsers = maxUsers;
        }
    }
}
