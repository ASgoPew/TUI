using System;
using TerrariaUI.Hooks.Args;

namespace TerrariaUI.Hooks
{
    public class HookManager
    {
        public Hook<CreateProviderArgs> CreateProvider = new Hook<CreateProviderArgs>();
        public Hook<RemoveProviderArgs> RemoveProvider = new Hook<RemoveProviderArgs>();
        public Hook<LoadArgs> Load = new Hook<LoadArgs>();
        public Hook<LoadRootArgs> LoadRoot = new Hook<LoadRootArgs>();
        public Hook<EventArgs> Dispose = new Hook<EventArgs>();
        public Hook<DrawObjectArgs> DrawObject = new Hook<DrawObjectArgs>();
        public Hook<DrawRectangleArgs> DrawRectangle = new Hook<DrawRectangleArgs>();
        public Hook<SetXYWHArgs> SetXYWH = new Hook<SetXYWHArgs>();
        public Hook<SetTopArgs> SetTop = new Hook<SetTopArgs>();
        public Hook<EnabledArgs> Enabled = new Hook<EnabledArgs>();
        public Hook<CanTouchArgs> CanTouch = new Hook<CanTouchArgs>();
        public Hook<TouchCancelArgs> TouchCancel = new Hook<TouchCancelArgs>();
        public Hook<GetTileArgs> GetTile = new Hook<GetTileArgs>();

        public Hook<UpdateSignArgs> UpdateSign = new Hook<UpdateSignArgs>();
        public Hook<RemoveSignArgs> RemoveSign = new Hook<RemoveSignArgs>();
        public Hook<UpdateChestArgs> UpdateChest = new Hook<UpdateChestArgs>();
        public Hook<RemoveChestArgs> RemoveChest = new Hook<RemoveChestArgs>();

        public Hook<LogArgs> Log = new Hook<LogArgs>();
        public Hook<DatabaseArgs> Database = new Hook<DatabaseArgs>();
    }
}
