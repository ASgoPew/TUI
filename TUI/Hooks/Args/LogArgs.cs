using System;

namespace TUI.Hooks.Args
{
    public enum LogType
    {
        Success = 0,
        Info,
        Warning,
        Error
    }

    public class LogArgs : EventArgs
    {
        public string Text { get; set; }
        public LogType Type { get; set; }

        public LogArgs(string text, LogType type)
        {
            Text = text;
            Type = type;
        }
    }
}
