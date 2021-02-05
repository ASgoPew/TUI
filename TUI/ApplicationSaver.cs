using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaUI;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;

namespace TerrariaUI
{
    internal class ApplicationSaver : VisualObject
    {
        public Application App;

        public ApplicationSaver(Application app)
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
            bw.Write((byte)App.Instances.Count);
            foreach (var pair in App.Instances)
                bw.Write((int)pair.Key);
        }
    }
}
