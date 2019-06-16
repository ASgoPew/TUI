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
        public byte[] Data { get; set; }

        public DatabaseArgs(DatabaseActionType type, string key, byte[] data = null)
        {
            Type = type;
            Key = key;
            Data = data;
        }
    }
}
