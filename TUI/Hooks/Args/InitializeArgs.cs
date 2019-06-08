using System;

namespace TUI.Hooks.Args
{
    public class LoadArgs : EventArgs
    {
        public int MaxUsers { get; private set; }

        public LoadArgs(int maxUsers)
        {
            MaxUsers = maxUsers;
        }
    }
}
