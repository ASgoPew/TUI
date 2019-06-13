using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI.Hooks.Args
{
    public enum DatabaseActionType
    {
        Get = 0,
        Set,
        Remove
    }

    public class DatabaseArgs : EventArgs
    {
        public DatabaseActionType Type { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public DatabaseArgs(DatabaseActionType type, string key, string value = null)
        {
            Type = type;
            Key = key;
            Value = value;
        }
    }
}
