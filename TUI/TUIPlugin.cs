using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;

namespace TUI
{
    [ApiVersion(2, 1)]
    public class TUIPlugin : TerrariaPlugin
    {
        public override string Author => "ASgo";
        public override string Description => "Terraria UI plugin";
        public override string Name => "TUI";
        public override Version Version => new Version(1, 0, 0, 0);

        public TUIPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
    }
}
