using System;

namespace TerrariaUI.Hooks.Args
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
