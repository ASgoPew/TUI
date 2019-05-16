using System;
using TUI.Hooks.Args;

namespace TUI.Hooks
{
    public class HookManager
    {
        public Hook<InitializeArgs> Initialize = new Hook<InitializeArgs>();
        public Hook<EventArgs> Deinitialize = new Hook<EventArgs>();
        public Hook<DrawArgs> Draw = new Hook<DrawArgs>();
        public Hook<SetXYWHArgs> SetXYWH = new Hook<SetXYWHArgs>();
        public Hook<SetTopArgs> SetTop = new Hook<SetTopArgs>();
        public Hook<EnabledArgs> Enabled = new Hook<EnabledArgs>();
        public Hook<CanTouchArgs> CanTouch = new Hook<CanTouchArgs>();
        public Hook<TouchCancelArgs> TouchCancel = new Hook<TouchCancelArgs>();
        public Hook<CreateSignArgs> CreateSign = new Hook<CreateSignArgs>();
        public Hook<RemoveSignArgs> RemoveSign = new Hook<RemoveSignArgs>();
    }
}
