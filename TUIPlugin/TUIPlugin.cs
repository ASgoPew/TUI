using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TUI;
using static TShockAPI.GetDataHandlers;

namespace TUIPlugin
{
    public class TUIPlugin : TerrariaPlugin
    {
        public override string Author => "ASgo";
        public override string Description => "Plugin conntion to TUI library";
        public override string Name => "TUIPlugin";
        public override Version Version => new Version(0, 1, 0, 0);

        public static byte[] playerDesignState = new byte[256];
        public static ITile tile;

        public TUIPlugin(Main game)
            : base(game)
        {
        }

        public override void Initialize()
        {
            TShockAPI.GetDataHandlers.NewProjectile += OnNewProjectile;
        }

        public static void OnNewProjectile(object sender, NewProjectileEventArgs args)
        {
            if (args.Type != 651 || TShock.Players[args.Owner] == null || TShock.Players[args.Owner].TPlayer == null)
                return;

            TSPlayer player = TShock.Players[args.Owner];

            byte prefix;

            if (player.TPlayer.inventory[player.TPlayer.selectedItem].netID == ItemID.WireKite)
                prefix = player.TPlayer.inventory[player.TPlayer.selectedItem].prefix;
            else
                // This means player is holding another item.Obtains by hacks.
                return;

            if (playerDesignState[args.Owner] == 0)
                playerDesignState[args.Owner] = 1;
            else if (playerDesignState[args.Owner] == 1)
            {
                int tileX = (int)Math.Round((args.Position.X + 5) / 16);
                int tileY = (int)Math.Round((args.Position.Y + 5) / 16);

                tile = Main.tile[tileX, tileY];

                if (UI.Handle(new Touch(tileX, tileY, TouchState.Begin, prefix, 0, player)))
                    UI.session[player].projectileID = args.Identity;
                playerDesignState[args.Owner] = 2;
            }
		    else
            {
                int tileX = (int)Math.Round((args.Position.X + 5) / 16);
                int tileY = (int)Math.Round((args.Position.Y + 5) / 16);

                UI.Handle(new Touch(tileX, tileY, TouchState.Move, prefix, 0, player));
            }
        }
    }
}
