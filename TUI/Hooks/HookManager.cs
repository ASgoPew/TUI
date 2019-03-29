using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public class HookManager
    {
        public Hook<DrawArgs> Draw = new Hook<DrawArgs>();
        public Hook<SetXYWHArgs> SetXYWH = new Hook<SetXYWHArgs>();
        public Hook<SetTopArgs> SetTop = new Hook<SetTopArgs>();
        public Hook<EnabledArgs> Enabled = new Hook<EnabledArgs>();
        public Hook<CanTouchArgs> CanTouch = new Hook<CanTouchArgs>();
    }
}
