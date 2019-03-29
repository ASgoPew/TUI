using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
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
