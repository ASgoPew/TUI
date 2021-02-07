using System;
using System.Collections.Generic;

namespace TerrariaUI.Hooks.Args
{
    public enum DatabaseActionType
    {
        Get = 0,
        Set,
        Remove,
        Select
    }

    public class DatabaseArgs : EventArgs
    {
        public DatabaseActionType Type { get; set; }
        public string Key { get; set; }
        public byte[] Data { get; set; }
        public int? User { get; set; }
        public int? Number { get; set; }
        public bool Ascending { get; set; }
        public int Count { get; set; }
        public int Offset { get; set; }
        public bool RequestNames { get; set; }
        public List<(int User, int Number, string Username)> Numbers { get; set; }

        public DatabaseArgs(DatabaseActionType type, string key, byte[] data = null, int? user = null, int? number = null, bool ascending = true, int count = -1, int offset = -1, bool requestNames = false)
        {
            Type = type;
            Key = key;
            Data = data;
            User = user;
            Number = number;
            Ascending = ascending;
            Count = count;
            Offset = offset;
            RequestNames = requestNames;
        }
    }
}
