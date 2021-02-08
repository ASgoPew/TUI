using System;
using System.IO;
using TerrariaUI.Base;

namespace TerrariaUI
{
    public class ApplicationSaver : VisualObject
    {
        public ApplicationType App;

        public ApplicationSaver(ApplicationType app)
            : base(0, 0, 0, 0)
        {
            App = app;
            Name = $"ApplicationSaver_{App.Name}";
        }

        protected override void UDBReadNative(BinaryReader br, int id)
        {
            try
            {
                int count = br.ReadByte();
                for (int i = 0; i < count; i++)
                {
                    int appIndex = br.ReadInt32();
                    App.LoadInstance(appIndex);
                }
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
                DBWrite();
            }
        }

        protected override void UDBWriteNative(BinaryWriter bw, int id)
        {
            App.Write(bw);
        }
    }
}
