using System;

namespace TerrariaUI.Hooks.Args
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
        public int? User { get; set; }

        public DatabaseArgs(DatabaseActionType type, string key, byte[] data = null, int? user = null)
        {
            Type = type;
            Key = key;
            Data = data;
            User = user;
        }
    }
}
