using System;

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
        public object Value { get; set; }
        public Type DataType { get; set; }

        public DatabaseArgs(DatabaseActionType type, string key, object value = null, Type dataType = null)
        {
            Type = type;
            Key = key;
            Value = value;
            DataType = dataType;
        }
    }
}
