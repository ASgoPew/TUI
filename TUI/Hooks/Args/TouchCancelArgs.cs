using System;
using TerrariaUI.Base;

namespace TerrariaUI.Hooks.Args
{
    public class TouchCancelArgs : EventArgs
    {
        public int UserIndex { get; set; } = -1;
        public PlayerSession Session { get; set; }
        public Touch Touch { get; set; }

        public TouchCancelArgs(int userIndex, PlayerSession session, Touch touch)
        {
            UserIndex = userIndex;
            Session = session;
            Touch = touch;
        }
    }
}
