using System;
using TerrariaUI.Base;

namespace TerrariaUI.Hooks.Args
{
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
