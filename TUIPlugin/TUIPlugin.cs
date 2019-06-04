﻿using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TUI;
using TUI.Base;
using TUI.Hooks.Args;

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

        public static int RegionAreaX = 80;
        public static int RegionAreaY = 50;
        public static DesignState[] playerDesignState = new DesignState[Main.maxPlayers];
        private static Timer RegionTimer = new Timer(1000) { AutoReset = true };

        public TUIPlugin(Main game)
            : base(game)
        {
        }

        public override void Initialize()
        {
            ServerApi.Hooks.ServerConnect.Register(this, OnServerConnect);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            ServerApi.Hooks.NetGetData.Register(this, OnGetData, 100);
            GetDataHandlers.NewProjectile += OnNewProjectile;
            UI.Hooks.CanTouch.Event += OnCanTouch;
            UI.Hooks.Draw.Event += OnDraw;
            UI.Hooks.TouchCancel.Event += OnTouchCancel;
            UI.Hooks.CreateSign.Event += OnCreateSign;
            UI.Hooks.RemoveSign.Event += OnRemoveSign;
            UI.Hooks.Log.Event += OnLog;
            RegionTimer.Elapsed += OnRegionTimer;
            RegionTimer.Start();

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
                UI.Hooks.CanTouch.Event -= OnCanTouch;
                UI.Hooks.Draw.Event -= OnDraw;
                UI.Hooks.TouchCancel.Event -= OnTouchCancel;
                UI.Hooks.CreateSign.Event -= OnCreateSign;
                UI.Hooks.RemoveSign.Event -= OnRemoveSign;
                UI.Hooks.Log.Event -= OnLog;
                RegionTimer.Elapsed -= OnRegionTimer;
                RegionTimer.Stop();
            }
            base.Dispose(disposing);
        }

        public static void OnServerConnect(ConnectEventArgs args)
        {
            playerDesignState[args.Who] = DesignState.Waiting;
            UI.InitializeUser(args.Who);
        }

        public static void OnServerLeave(LeaveEventArgs args)
        {
            UI.RemoveUser(args.Who);
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
                    short sx = br.ReadInt16();
                    short sy = br.ReadInt16();
                    short ex = br.ReadInt16();
                    short ey = br.ReadInt16();
                    byte designStateByte = br.ReadByte();
                    TSPlayer player = TShock.Players[args.Msg.whoAmI];
                    byte prefix;
                    if (player?.TPlayer != null && player.TPlayer.inventory[player.TPlayer.selectedItem].netID == ItemID.WireKite)
                        prefix = player.TPlayer.inventory[player.TPlayer.selectedItem].prefix;
                    else
                        return;

                    UI.Touched(player.Index, new Touch(ex, ey, TouchState.End, prefix, designStateByte));
                    args.Handled = UI.EndTouchHandled(player.Index);
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
                    if (UI.Session[owner].ProjectileID == projectileID && previousTouch != null && previousTouch.State != TouchState.End)
                    {
                        Touch simulatedEndTouch = previousTouch.SimulatedEndTouch();
                        simulatedEndTouch.Undo = true;
                        UI.Touched(owner, simulatedEndTouch);
                        playerDesignState[owner] = DesignState.Waiting;
                    }
                }
            }
        }

        public static void OnNewProjectile(object sender, GetDataHandlers.NewProjectileEventArgs args)
        {
            TSPlayer player = TShock.Players[args.Owner];
            if (args.Handled || args.Type != 651 || player?.TPlayer == null)
                return;

            try
            {
                byte prefix;

                if (player.TPlayer.inventory[player.TPlayer.selectedItem].netID == ItemID.WireKite)
                    prefix = player.TPlayer.inventory[player.TPlayer.selectedItem].prefix;
                else
                    return; // This means player is holding another item.Obtains by hacks.

                if (playerDesignState[args.Owner] == DesignState.Waiting)
                    playerDesignState[args.Owner] = DesignState.Begin;
                else if (playerDesignState[args.Owner] == DesignState.Begin)
                {
                    int tileX = (int)Math.Floor((args.Position.X + 5) / 16);
                    int tileY = (int)Math.Floor((args.Position.Y + 5) / 16);

                    if (UI.Touched(args.Owner, new Touch(tileX, tileY, TouchState.Begin, prefix, 0)))
                        UI.Session[args.Owner].ProjectileID = args.Identity;
                    playerDesignState[args.Owner] = DesignState.Moving;
                    //args.Handled = true;
                }
		        else
                {
                    int tileX = (int)Math.Floor((args.Position.X + 5) / 16);
                    int tileY = (int)Math.Floor((args.Position.Y + 5) / 16);
                    UI.Touched(args.Owner, new Touch(tileX, tileY, TouchState.Moving, prefix, 0));
                }
            }
            catch (Exception e)
            {
                TShock.Log.ConsoleError(e.ToString());
            }
        }

        public static void OnCanTouch(CanTouchArgs args)
        {
            if (args.Node.Configuration.Permission is string permission)
            {
                TSPlayer player = args.Touch.Player();
                args.CanTouch = player?.HasPermission(permission) ?? false;
                if (args.Touch.State == TouchState.Begin && player != null && args.CanTouch == false)
                {
                    args.Node.TrySetLock(args.Touch);
                    player.SendErrorMessage("You do not have access to this interface.");
                }
            }
        }

        public static void OnDraw(DrawArgs args)
        {
            HashSet<int> players;
            if (args.UserIndex == -1)
                players = (args.Node.GetRoot() as RootVisualObject).Players;
            else
                players = new HashSet<int>() { args.UserIndex };
            players.Remove(args.ExceptUserIndex);

            int size = Math.Max(args.Width, args.Height);
            if (size >= 50 || args.ForcedSection)
            {
                int lowX = Netplay.GetSectionX(args.X);
                int highX = Netplay.GetSectionX(args.X + args.Width - 1);
                int lowY = Netplay.GetSectionY(args.Y);
                int highY = Netplay.GetSectionY(args.Y + args.Height - 1);
                foreach (int i in players)
                    NetMessage.SendData(10, i, -1, null, args.X, args.Y, args.Width, args.Height);
                if (args.Frame)
                    foreach (int i in players)
                        NetMessage.SendData(11, i, -1, null, lowX, lowY, highX, highY);
            }
            else
                foreach (int i in players)
                    NetMessage.SendData(20, i, -1, null, size, args.X, args.Y);
        }

        public static void OnTouchCancel(TouchCancelArgs args)
        {
            TSPlayer player = args.Touch.Player();
            player.SendWarningMessage("You are holding mouse for too long.");
            Console.WriteLine("TUI: TOO LONG");
            player.SendData(PacketTypes.ProjectileDestroy, null, args.Session.ProjectileID, player.Index);
            Touch simulatedEndTouch = args.Touch.SimulatedEndTouch();
            simulatedEndTouch.Undo = true;
            UI.Touched(args.UserIndex, simulatedEndTouch);
            playerDesignState[args.UserIndex] = DesignState.Waiting;
        }

        public static void OnCreateSign(CreateSignArgs args)
        {
            if (args.Node.GetRoot().Provider.GetType().Name != "FakeTileRectangle")
            {
                Main.tile[args.X, args.Y] = new Tile() { type = 55, frameX = 0, frameY = 0 };
                int id = Sign.ReadSign(args.X, args.Y);
                if (id >= 0)
                    args.Sign = Main.sign[id];
            }
            else
                CreateFakeSign(args);
        }

        public static void CreateFakeSign(CreateSignArgs args)
        {
            Sign sign = new Sign()
            {
                x = args.X,
                y = args.Y
            };
            try
            {
                args.Node.GetRoot().Provider.AddSign(sign);
                args.Sign = sign;
            }
            catch (Exception e)
            {
                TShock.Log.ConsoleError("Can't create FAKE sign: " + e);
            }
        }

        public static void OnRemoveSign(RemoveSignArgs args)
        {
            if (args.Node.GetRoot().Provider.GetType().Name != "FakeTileRectangle")
                Sign.KillSign(args.Sign.x, args.Sign.y);
            else
                RemoveFakeSign(args);
        }

        public static void RemoveFakeSign(RemoveSignArgs args)
        {
            args.Node.GetRoot().Provider.RemoveSign(args.Sign);
        }

        public static void OnLog(LogArgs args)
        {
            args.Text = "TUI: " + args.Text;
            if (args.Type == LogType.Success)
                TShock.Log.ConsoleInfo(args.Text);
            else if (args.Type == LogType.Info)
                TShock.Log.ConsoleInfo(args.Text);
            else if (args.Type == LogType.Warning)
                TShock.Log.ConsoleError(args.Text);
            else if (args.Type == LogType.Error)
                TShock.Log.ConsoleError(args.Text);
        }

        private void OnRegionTimer(object sender, ElapsedEventArgs e)
        {
            foreach (RootVisualObject root in UI.Roots)
            {
                (int x, int y) = root.AbsoluteXY();
                int sx = x - RegionAreaX;
                int sy = y - RegionAreaY;
                int ex = x + RegionAreaX + root.Width - 1;
                int ey = y + RegionAreaY + root.Height - 1;
                foreach (TSPlayer plr in TShock.Players)
                {
                    if (plr?.Active != true)
                        continue;
                    int tx = plr.TileX, ty = plr.TileY;
                    if ((tx >= sx) && (tx <= ex) && (ty >= sy) && (ty <= ey))
                    {
                        if (root.Players.Add(plr.Index))
                            UI.Hooks.Draw.Invoke(new DrawArgs(root, x, y, root.Width,
                                root.Height, root.ForceSection, plr.Index, -1, true));
                    }
                    else
                        root.Players.Remove(plr.Index);
                }
            }
        }
    }
}
