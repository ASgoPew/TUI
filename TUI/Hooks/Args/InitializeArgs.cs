using System;

namespace TUI.Hooks.Args
{
    public class InitializeArgs : EventArgs
    {
        public int MaxUsers { get; private set; }

        public InitializeArgs(int maxUsers)
        {
            MaxUsers = maxUsers;
        }
    }
}
