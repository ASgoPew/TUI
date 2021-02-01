using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TerrariaUI.Widgets.Data;
using TerrariaUI.Widgets.Media;
using WorldEdit;

namespace TUIPlusWorldEdit
{
    [ApiVersion(2, 1)]
    public class TUIPlusWorldEditPlugin : TerrariaPlugin
    {
        public override string Name => "TUI + WorldEdit";

        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public override string Author => "ASgo & Anzhelika";

        public override string Description => "Adds '.dat' scheme format parser for TUI images";

        public TUIPlusWorldEditPlugin(Main game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            if (!ImageData.Readers.ContainsKey(".dat"))
                ImageData.Readers.Add(".dat", ReadWorldEdit);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ImageData.Readers.TryGetValue(".dat", out var handler) && handler == ReadWorldEdit)
                    ImageData.Readers.Remove(".dat");
            }
            base.Dispose(disposing);
        }

        private List<ImageData> ReadWorldEdit(string name, bool video)
        {
            if (!video && Path.HasExtension(name) && File.Exists(name))
            {
                ImageData image = new ImageData();
                WorldSectionData data = Tools.LoadWorldData(File.Open(name, FileMode.Open));
                image.Width = data.Width;
                image.Height = data.Height;
                image.Tiles = data.Tiles;
                foreach (var sign in data.Signs)
                    image.Signs.Add(new SignData() { X = sign.X, Y  = sign.Y, Text = sign.Text });
                return new List<ImageData>() { image };
            }
            return null;
        }
    }
}
