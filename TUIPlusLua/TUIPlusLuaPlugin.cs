using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;

namespace TUIPlusLua
{
    [ApiVersion(2, 1)]
    class TUIPlusLuaPlugin : TerrariaPlugin
    {
        public override string Name => "TUIPlusLua";
        public override string Author => "ASgo";
        public override string Description => "Intermediate between Lua and TUI";
        public override Version Version => new Version(1, 0, 0, 0);

        public TUIPlusLuaPlugin(Main game)
            : base(game)
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
