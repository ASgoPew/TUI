using TShockAPI;
using TUI.Base;

namespace TUIPlugin
{
    public static class TouchExtensions
    {
        public static TSPlayer Player(this Touch touch) =>
            TShock.Players[touch.Session.UserIndex];
    }
}
