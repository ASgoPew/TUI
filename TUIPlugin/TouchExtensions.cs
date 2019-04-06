using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using TUI;
using TUI.Base;

namespace TUIPlugin
{
    public static class TouchExtensions
    {
        public static TSPlayer Player(this Touch touch) =>
            TShock.Players[touch.Session.UserIndex];
    }
}
