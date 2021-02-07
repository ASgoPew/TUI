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
            Name = $"__saver_{App.Name}";
        }

        protected override void DBReadNative(BinaryReader br)
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

        protected override void DBWriteNative(BinaryWriter bw)
        {
            App.Write(bw);
        }
    }
}
