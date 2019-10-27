using System;
using TUI.Base;

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
        public VisualObject Node { get; set; }

        public LogArgs(string text, LogType type)
        {
            Text = text;
            Type = type;
        }

        public LogArgs(VisualObject node, string text, LogType type)
        {
            Node = node;
            Text = text;
            Type = type;
        }
    }
}
