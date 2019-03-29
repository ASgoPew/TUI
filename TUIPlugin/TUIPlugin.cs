using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.IO;
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
    public enum DesignState
    {
        Waiting = 0,
        Begin,
        Moving
    }

    [ApiVersion(2, 1)]
    public class TUIPlugin : TerrariaPlugin
    {
        public override string Author => "ASgo";
        public override string Description => "Plugin conntion to TUI library";
        public override string Name => "TUIPlugin";
        public override Version Version => new Version(0, 1, 0, 0);

        public static DesignState[] playerDesignState = new DesignState[256];
        public static ITile tile;

        public TUIPlugin(Main game)
            : base(game)
        {
        }

        public override void Initialize()
        {
            ServerApi.Hooks.ServerConnect.Register(this, OnServerConnect);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            ServerApi.Hooks.NetGetData.Register(this, OnGetData, 100);
            TShockAPI.GetDataHandlers.NewProjectile += OnNewProjectile;
            UI.Initialize(255);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UI.Deinitialize();
                ServerApi.Hooks.ServerConnect.Deregister(this, OnServerConnect);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
                ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
                TShockAPI.GetDataHandlers.NewProjectile -= OnNewProjectile;
            }
            base.Dispose(disposing);
        }

        public static void OnServerConnect(ConnectEventArgs args)
        {
            UI.InitializeUser(args.Who);
        }

        public static void OnServerLeave(LeaveEventArgs args)
        {
            UI.RemoveUser(args.Who);
            playerDesignState[args.Who] = DesignState.Waiting;
        }

        public static void OnGetData(GetDataEventArgs args)
        {
            if (args.Handled)
                return;
            if (args.MsgID == PacketTypes.MassWireOperation)
            {
                using (MemoryStream ms = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    short x = br.ReadInt16();
                    short y = br.ReadInt16();
                    byte designStateByte = br.ReadByte();
                    TSPlayer player = TShock.Players[args.Msg.whoAmI];
                    byte prefix;
                    if (player?.TPlayer != null && player.TPlayer.inventory[player.TPlayer.selectedItem].netID == ItemID.WireKite)
                        prefix = player.TPlayer.inventory[player.TPlayer.selectedItem].prefix;
                    else
                        return;

                    args.Handled = UI.Touched(player.Index, new Touch(x, y, TouchState.End, prefix, designStateByte));
                    playerDesignState[player.Index] = DesignState.Waiting;
                }
            }
            else if (args.MsgID == PacketTypes.ProjectileDestroy)
            {
                using (MemoryStream ms = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    short projectileID = br.ReadInt16();
                    byte owner = br.ReadByte();
                    if (owner != args.Msg.whoAmI)
                        return;
                    Touch previousTouch = UI.Session[owner].PreviousTouch;
                    if (previousTouch != null && previousTouch.State != TouchState.End && UI.Session[owner].ProjectileID == projectileID)
                    {
                        Touch simulatedEndTouch = previousTouch.SimulatedEndTouch();
                        simulatedEndTouch.Undo = true;
                        UI.Touched(owner, simulatedEndTouch);
                        playerDesignState[owner] = DesignState.Waiting;
                    }
                }
            }
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
                return; // This means player is holding another item.Obtains by hacks.

            if (playerDesignState[args.Owner] == DesignState.Waiting)
                playerDesignState[args.Owner] = DesignState.Begin;
            else if (playerDesignState[args.Owner] == DesignState.Begin)
            {
                int tileX = (int)Math.Round((args.Position.X + 5) / 16);
                int tileY = (int)Math.Round((args.Position.Y + 5) / 16);

                tile = Main.tile[tileX, tileY];

                if (UI.Touched(args.Owner, new Touch(tileX, tileY, TouchState.Begin, prefix, 0)))
                    UI.Session[args.Owner].ProjectileID = args.Identity;
                playerDesignState[args.Owner] = DesignState.Moving;
            }
		    else
            {
                int tileX = (int)Math.Round((args.Position.X + 5) / 16);
                int tileY = (int)Math.Round((args.Position.Y + 5) / 16);

                UI.Touched(args.Owner, new Touch(tileX, tileY, TouchState.Moving, prefix, 0));
            }
        }
    }
}
