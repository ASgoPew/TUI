﻿using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Timers;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TUI.Base;
using TUI.Hooks.Args;
using TUI.Widgets;
using TUI.Widgets.Data;
using TUI.Widgets.Media;

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
        #region Data

        public override string Author => "ASgo & Anzhelika";
        public override string Description => "Plugin conntion to TUI library";
        public override string Name => "TUIPlugin";
        public override Version Version => new Version(0, 1, 0, 0);

        public static int RegionAreaX = 80;
        public static int RegionAreaY = 50;
        public static DesignState[] playerDesignState = new DesignState[Main.maxPlayers];
        public static bool FakesEnabled = false;
        private static Timer RegionTimer = new Timer(1000) { AutoReset = true };
        public static Command[] CommandList = new Command[]
        {
            new Command("TUI.control", TUICommand, "tui")
        };

        #endregion

        #region Constructor

        public TUIPlugin(Main game)
            : base(game)
        {
            Order = -1000;
        }

        #endregion
        #region Initialize

        public override void Initialize()
        {
            FakesEnabled = ServerApi.Plugins.Count(p => p.Plugin.Name == "FakeManager") > 0;

            ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize, Int32.MinValue);
            ServerApi.Hooks.ServerConnect.Register(this, OnServerConnect);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            ServerApi.Hooks.NetGetData.Register(this, OnGetData, 100);
            GetDataHandlers.NewProjectile += OnNewProjectile;
            TUI.TUI.Hooks.CanTouch.Event += OnCanTouch;
            TUI.TUI.Hooks.Draw.Event += OnDraw;
            TUI.TUI.Hooks.TouchCancel.Event += OnTouchCancel;
            TUI.TUI.Hooks.UpdateSign.Event += OnUpdateSign;
            TUI.TUI.Hooks.RemoveSign.Event += OnRemoveSign;
            TUI.TUI.Hooks.UpdateChest.Event += OnUpdateChest;
            TUI.TUI.Hooks.RemoveChest.Event += OnRemoveChest;
            TUI.TUI.Hooks.Log.Event += OnLog;
            TUI.TUI.Hooks.Database.Event += OnDatabase;
            RegionTimer.Elapsed += OnRegionTimer;
            RegionTimer.Start();

            Commands.ChatCommands.AddRange(CommandList);

            if (!ImageData.Readers.ContainsKey(".dat"))
                ImageData.Readers.Add(".dat", ReadWorldEdit);
            if (!ImageData.Readers.ContainsKey(".TEditSch"))
                ImageData.Readers.Add(".TEditSch", ReadTEdit);

            Database.ConnectDB();

            TUI.TUI.Initialize(255, Main.maxTilesX, Main.maxTilesY);
        }

        #endregion
        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                TUI.TUI.Dispose();

                ServerApi.Hooks.ServerConnect.Deregister(this, OnServerConnect);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
                ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
                TShockAPI.GetDataHandlers.NewProjectile -= OnNewProjectile;
                TUI.TUI.Hooks.CanTouch.Event -= OnCanTouch;
                TUI.TUI.Hooks.Draw.Event -= OnDraw;
                TUI.TUI.Hooks.TouchCancel.Event -= OnTouchCancel;
                TUI.TUI.Hooks.UpdateSign.Event -= OnUpdateSign;
                TUI.TUI.Hooks.RemoveSign.Event -= OnRemoveSign;
                TUI.TUI.Hooks.UpdateChest.Event -= OnUpdateChest;
                TUI.TUI.Hooks.RemoveChest.Event -= OnRemoveChest;
                TUI.TUI.Hooks.Log.Event -= OnLog;
                TUI.TUI.Hooks.Database.Event -= OnDatabase;
                RegionTimer.Elapsed -= OnRegionTimer;
                RegionTimer.Stop();

                foreach (Command cmd in CommandList)
                    Commands.ChatCommands.Remove(cmd);

                ImageData.Readers.Remove(".dat");
                ImageData.Readers.Remove(".TEditSch");
            }
            base.Dispose(disposing);
        }

        #endregion

        #region OnGamePostInitialize

        private void OnGamePostInitialize(EventArgs args)
        {
            TUI.TUI.Load();
            TUI.TUI.Update();
            TUI.TUI.Apply();
            TUI.TUI.Draw();
        }

        #endregion
        #region OnServerConnect

        private static void OnServerConnect(ConnectEventArgs args)
        {
            playerDesignState[args.Who] = DesignState.Waiting;
            TUI.TUI.InitializePlayer(args.Who);
        }

        #endregion
        #region OnServerLeave

        private static void OnServerLeave(LeaveEventArgs args)
        {
            TUI.TUI.RemovePlayer(args.Who);
        }

        #endregion
        #region OnGetData

        private static void OnGetData(GetDataEventArgs args)
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

                    TUI.TUI.Touched(player.Index, new Touch(ex, ey, TouchState.End, prefix, designStateByte));
                    args.Handled = TUI.TUI.EndTouchHandled(player.Index);
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
                    Touch previousTouch = TUI.TUI.Session[owner].PreviousTouch;
                    if (TUI.TUI.Session[owner].ProjectileID == projectileID && previousTouch != null && previousTouch.State != TouchState.End)
                    {
                        Touch simulatedEndTouch = previousTouch.SimulatedEndTouch();
                        simulatedEndTouch.Undo = true;
                        TUI.TUI.Touched(owner, simulatedEndTouch);
                        playerDesignState[owner] = DesignState.Waiting;
                    }
                }
            }
        }

        #endregion
        #region OnNewProjectile

        private static void OnNewProjectile(object sender, GetDataHandlers.NewProjectileEventArgs args)
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

                    if (TUI.TUI.Touched(args.Owner, new Touch(tileX, tileY, TouchState.Begin, prefix, 0)))
                        TUI.TUI.Session[args.Owner].ProjectileID = args.Identity;
                    playerDesignState[args.Owner] = DesignState.Moving;
                    //args.Handled = true;
                }
		        else
                {
                    int tileX = (int)Math.Floor((args.Position.X + 5) / 16);
                    int tileY = (int)Math.Floor((args.Position.Y + 5) / 16);
                    TUI.TUI.Touched(args.Owner, new Touch(tileX, tileY, TouchState.Moving, prefix, 0));
                }
            }
            catch (Exception e)
            {
                TShock.Log.ConsoleError(e.ToString());
            }
        }

        #endregion

        #region OnCanTouch

        private static void OnCanTouch(CanTouchArgs args)
        {
            if (args.Node.Configuration.Permission is string permission)
            {
                TSPlayer player = args.Touch.Player();
                args.CanTouch = player?.HasPermission(permission) ?? false;
                if (args.Touch.State == TouchState.Begin && player != null && args.CanTouch == false)
                {
                    args.Touch.Session.Enabled = false;
                    TUI.TUI.TrySetLockForObject(args.Node, args.Touch);
                    player.SendErrorMessage("You do not have access to this interface.");
                }
            }
        }

        #endregion
        #region OnDraw

        private static void OnDraw(DrawArgs args)
        {
            if (args.Node.Root == null)
                return;

            ulong currentApplyCounter = args.Node.Root.ApplyCounter;
            HashSet<int> players = args.PlayerIndex == -1
                ? args.Node.Root.Players
                : new HashSet<int>() { args.PlayerIndex };
            players.Remove(args.ExceptPlayerIndex);
            // Remove players that already received lastest version of interface
            players.RemoveWhere(p =>
                args.Node.Root.PlayerApplyCounter.TryGetValue(p, out ulong applyCounter)
                && currentApplyCounter == applyCounter);
            if (players.Count == 0)
                return;

            // Yes, we are converting HashSet<int> to NetworkText to pass it to NetMessage.SendData for FakeManager...
            Terraria.Localization.NetworkText playerList = FakesEnabled
                ? Terraria.Localization.NetworkText.FromLiteral(String.Concat(players.Select(p => (char)p)))
                : null;

            int size = Math.Max(args.Width, args.Height);
            if (size >= 50 || args.ForcedSection)
            {
                if (FakesEnabled)
                    NetMessage.SendData(10, -1, -1, playerList, args.X, args.Y, args.Width, args.Height);
                else
                    foreach (int i in players)
                        NetMessage.SendData(10, i, -1, null, args.X, args.Y, args.Width, args.Height);
                if (args.Frame)
                {
                    int lowX = Netplay.GetSectionX(args.X);
                    int highX = Netplay.GetSectionX(args.X + args.Width - 1);
                    int lowY = Netplay.GetSectionY(args.Y);
                    int highY = Netplay.GetSectionY(args.Y + args.Height - 1);
                    if (FakesEnabled)
                        NetMessage.SendData(11, -1, -1, playerList, lowX, lowY, highX, highY);
                    else
                        foreach (int i in players)
                            NetMessage.SendData(11, i, -1, null, lowX, lowY, highX, highY);
                }
            }
            else
            {
                if (FakesEnabled)
                    NetMessage.SendData(20, -1, -1, playerList, size, args.X, args.Y);
                else
                    foreach (int i in players)
                        NetMessage.SendData(20, i, -1, null, size, args.X, args.Y);
            }

            // Mark that these players received lastest version of interface
            foreach (int player in players)
                args.Node.Root.PlayerApplyCounter[player] = currentApplyCounter;
        }

        #endregion
        #region OnTouchCancel

        private static void OnTouchCancel(TouchCancelArgs args)
        {
            TSPlayer player = args.Touch.Player();
            player.SendWarningMessage("You are holding mouse for too long.");
            TUI.TUI.Hooks.Log.Invoke(new LogArgs($"TUI: Touch too long ({player.Name}).", LogType.Info));
            player.SendData(PacketTypes.ProjectileDestroy, null, args.Session.ProjectileID, player.Index);
            Touch simulatedEndTouch = args.Touch.SimulatedEndTouch();
            simulatedEndTouch.Undo = true;
            TUI.TUI.Touched(args.UserIndex, simulatedEndTouch);
            playerDesignState[args.UserIndex] = DesignState.Waiting;
        }

        #endregion
        #region OnCreateSign

        private static void OnUpdateSign(UpdateSignArgs args)
        {
            dynamic provider = (args.Node.Root ?? args.Node.GetRoot()).Provider;
            if (provider.GetType().Name != "FakeTileRectangle")
            {
                Main.tile[args.X, args.Y] = new Tile() { type = 55, frameX = 0, frameY = 0 };
                int id = Sign.ReadSign(args.X, args.Y);
                if (id >= 0)
                {
                    // Can lead to creating the same sign since ReadSign returns existing sign if there is one.
                    // Console.WriteLine($"{args.Node.FullName} OnCreateSign: {id} ({args.X}, {args.Y})");
                    args.Sign = Main.sign[id];
                }
            }
            else
            {
                Sign sign = new Sign()
                {
                    x = args.X,
                    y = args.Y
                };
                try
                {
                    provider.AddSign(sign, false);
                    args.Sign = sign;
                }
                catch (Exception e)
                {
                    TShock.Log.ConsoleError("Can't create FAKE sign: " + e);
                }
            }
        }

        #endregion
        #region OnRemoveSign

        private static void OnRemoveSign(RemoveSignArgs args)
        {
            dynamic provider = (args.Node.Root ?? args.Node.GetRoot()).Provider;
            if (provider.GetType().Name != "FakeTileRectangle")
                Sign.KillSign(args.Sign.x, args.Sign.y);
            else
                provider.RemoveSign(args.Sign);
        }

        #endregion
        #region OnCreateChest

        private static void OnUpdateChest(UpdateChestArgs args)
        {
            dynamic provider = (args.Node.Root ?? args.Node.GetRoot()).Provider;
            if (provider.GetType().Name != "FakeTileRectangle")
            {
                Main.tile[args.X, args.Y] = new Tile() { type = 21, sTileHeader = 32, frameX = 0, frameY = 0 };
                Main.tile[args.X + 1, args.Y] = new Tile() { type = 21, sTileHeader = 32, frameX = 18, frameY = 0 };
                Main.tile[args.X, args.Y + 1] = new Tile() { type = 21, sTileHeader = 32, frameX = 0, frameY = 18 };
                Main.tile[args.X + 1, args.Y + 1] = new Tile() { type = 21, sTileHeader = 32, frameX = 18, frameY = 18 };
                int id = Chest.FindChest(args.X, args.Y);
                if (id < 0)
                {
                    Console.WriteLine($"CREATING NEW AT {args.X}, {args.Y}");
                    id = Chest.CreateChest(args.X, args.Y);
                }
                if (id >= 0)
                {
                    // Can lead to creating the same chest since FindChest returns existing chest if there is one.
                    // Console.WriteLine($"{args.Node.FullName} OnCreateChest: {id} ({args.X}, {args.Y})");
                    args.Chest = Main.chest[id];
                    Console.WriteLine(id);
                }
                else
                    TShock.Log.ConsoleError("Can't create Main.chest chest.");
            }
            else
            {
                Chest chest = new Chest()
                {
                    x = args.X,
                    y = args.Y
                };
                for (int i = 0; i < 40; i++)
                    chest.item[i] = new Item();
                try
                {
                    provider.AddChest(chest, false);
                    args.Chest = chest;
                }
                catch (Exception e)
                {
                    TShock.Log.ConsoleError("Can't create FAKE chest: " + e);
                }
            }
        }

        #endregion
        #region OnRemoveChest

        private static void OnRemoveChest(RemoveChestArgs args)
        {
            dynamic provider = (args.Node.Root ?? args.Node.GetRoot()).Provider;
            if (provider.GetType().Name != "FakeTileRectangle")
            {
                int chestX = args.Chest.x;
                int chestY = args.Chest.y;
                for (int i = 0; i < 1000; i++)
                    if (Main.chest[i] is Chest chest && (chest.x == chestX && chest.y == chestY))
                        Main.chest[i] = null;
            }
            else
                provider.RemoveChest(args.Chest);
        }

        #endregion
        #region OnLog

        private static void OnLog(LogArgs args)
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

        #endregion
        #region OnDatabase

        private static void OnDatabase(DatabaseArgs args)
        {
            switch (args.Type)
            {
                case DatabaseActionType.Get:
                    //args.Value = JsonConvert.DeserializeObject(Database.GetData(args.Key), args.DataType);
                    if (args.User.HasValue)
                        args.Data = Database.GetData(args.User.Value, args.Key);
                    else
                        args.Data = Database.GetData(args.Key);
                    break;
                case DatabaseActionType.Set:
                    //Database.SetData(args.Key, JsonConvert.SerializeObject(args.Value));
                    if (args.User.HasValue)
                        Database.SetData(args.User.Value, args.Key, args.Data);
                    else
                        Database.SetData(args.Key, args.Data);
                    break;
                case DatabaseActionType.Remove:
                    if (args.User.HasValue)
                        Database.RemoveKey(args.User.Value, args.Key);
                    else
                        Database.RemoveKey(args.Key);
                    break;
            }
        }

        #endregion
        
        #region OnRegionTimer

        private void OnRegionTimer(object sender, ElapsedEventArgs e)
        {
            foreach (RootVisualObject root in TUI.TUI.GetRoots())
            {
                (int x, int y) = root.AbsoluteXY();
                int sx = x - RegionAreaX;
                int sy = y - RegionAreaY;
                int ex = x + RegionAreaX + root.Width - 1;
                int ey = y + RegionAreaY + root.Height - 1;
                foreach (TSPlayer plr in TShock.Players)
                {
                    // plr?.Active != true check won't work since broadcast is set to true
                    // after setting player.active.
                    // Sequence:
                    // 1. player.Spawn() <=> player.active = true
                    // 2. NetMessage.greetPlayer(...) - invokes a hook that can freeze the thread
                    // 3. buffer.broadcast = true
                    if (plr == null || !NetMessage.buffer[plr.Index].broadcast)
                        continue;
                    int tx = plr.TileX, ty = plr.TileY;
                    if ((tx >= sx) && (tx <= ex) && (ty >= sy) && (ty <= ey))
                        root.Players.Add(plr.Index);
                    else
                        root.Players.Remove(plr.Index);
                }

                root.Draw();
            }
        }

        #endregion

        #region FindRoot

        public static bool FindRoot(string name, TSPlayer player, out RootVisualObject found)
        {
            found = null;
            List<RootVisualObject> foundRoots = new List<RootVisualObject>();
            string lowerName = name.ToLower();
            foreach (RootVisualObject root in TUI.TUI.GetRoots())
            {
                if (root.Name == name)
                {
                    found = root;
                    return true;
                }
                else if (root.Name.ToLower().StartsWith(lowerName))
                    foundRoots.Add(root);
            }
            if (foundRoots.Count == 0)
            {
                player?.SendErrorMessage($"Invalid panel '{name}'.");
                return false;
            }
            else if (foundRoots.Count > 1)
            {
                if (player != null)
                    TShock.Utils.SendMultipleMatchError(player, foundRoots);
                return false;
            }
            else
            {
                found = foundRoots[0];
                return true;
            }
        }

        #endregion

        #region TUICommand

        public static void TUICommand(CommandArgs args)
        {
            string arg0 = args.Parameters.ElementAtOrDefault(0);
            switch (arg0?.ToLower())
            {
                case "list":
                {
                    if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out int page))
                        return;
                    List<string> lines = PaginationTools.BuildLinesFromTerms(TUI.TUI.GetRoots());
                    PaginationTools.SendPage(args.Player, page, lines, new PaginationTools.Settings()
                    {
                        HeaderFormat = "TUI interfaces ({0}/{1}):",
                        FooterFormat = "Type '/tui list {0}' for more.",
                        NothingToDisplayString = "There are no TUI interfaces yet."
                    });
                    break;
                }
                case "tp":
                case "teleport":
                {
                    if (args.Parameters.Count != 2)
                    {
                        args.Player.SendErrorMessage("/tui tp \"interface name\"");
                        return;
                    }
                    if (!FindRoot(args.Parameters[1], args.Player, out RootVisualObject root))
                        return;

                    args.Player.Teleport((root.X + root.Width / 2) * 16,
                        (root.Y + root.Height / 2) * 16);
                    args.Player.SendSuccessMessage($"Teleported to interface '{root.Name}'.");
                    break;
                }
                case "tphere":
                case "teleporthere":
                {
                    if (args.Parameters.Count < 2
                        || args.Parameters.Count > 3)
                    {
                        args.Player.SendErrorMessage("/tui tphere \"interface name\" [-confirm]");
                        return;
                    }
                    if (!FindRoot(args.Parameters[1], args.Player, out RootVisualObject root))
                        return;

                    if (root.UsesDefaultMainProvider
                        && args.Parameters.Last().ToLower() != "-confirm")
                    {
                        args.Player.SendErrorMessage($"Interface '{root.Name}' is drawn on main map.\n" +
                            $"Type '/tui tphere \"{root.Name}\" -confirm' " +
                            "to confirm the interface transfer.");
                        return;
                    }

                    int x = args.Player.TileX, y = args.Player.TileY;
                    TUI.TUI.SetXYWH(root, x, y, root.Width, root.Height);
                    args.Player.SendSuccessMessage($"Moved interface '{root.Name}' to ({x},{y}).");
                    break;
                }
                default:
                {
                    if (arg0 == null || arg0.ToLower() == "help")
                    {
                        args.Player.SendSuccessMessage("/tui subcommands:");
                        args.Player.SendInfoMessage("/tui \"interface name\" [<x> <y> [<width> <height>]]");
                        args.Player.SendInfoMessage("/tui tp \"interface name\"");
                        args.Player.SendInfoMessage("/tui tphere \"interface name\" [-confirm]");
                        args.Player.SendInfoMessage("/tui list [page]");
                        return;
                    }
                    if (!FindRoot(args.Parameters[0], args.Player, out RootVisualObject root))
                        return;

                    if (args.Parameters.Count == 1)
                    {
                        args.Player.SendInfoMessage("Position and size " +
                            $"of interface '{root.Name}': {root.XYWH()}");
                        return;
                    }

                    if (!int.TryParse(args.Parameters[1], out int x)
                        || !int.TryParse(args.Parameters[2], out int y)
                        || x < 0 || y < 0 || x >= Main.maxTilesX || y >= Main.maxTilesY)
                    {
                        args.Player.SendErrorMessage("Invalid coordinates " +
                            $"'{args.Parameters[1]},{args.Parameters[2]}'.");
                        return;
                    }

                    int width = root.Width;
                    int height = root.Height;
                    if (args.Parameters.Count == 5
                        && (!int.TryParse(args.Parameters[3], out width)
                        || !int.TryParse(args.Parameters[4], out height)
                        || width < 0 || x + width >= Main.maxTilesX
                        || height < 0 || y + height >= Main.maxTilesY))
                    {
                        args.Player.SendErrorMessage("Invalid size " +
                            $"'{args.Parameters[3]},{args.Parameters[4]}'.");
                        return;
                    }

                    TUI.TUI.SetXYWH(root, x, y, width, height);
                    args.Player.SendSuccessMessage("Set position and size " +
                        $"of interface '{root.Name}' to {root.XYWH()}.");
                    break;
                }
            }
        }

        #endregion

        #region ReadWorldEdit

        private void ReadWorldEdit(string path, ImageData image)
        {
            using (FileStream fs = File.Open(path, FileMode.Open))
            using (GZipStream zs = new GZipStream(fs, CompressionMode.Decompress))
            using (BufferedStream bs = new BufferedStream(zs, 1048576))
            using (BinaryReader br = new BinaryReader(bs))
            {
                br.ReadInt32();
                br.ReadInt32();
                int w = br.ReadInt32();
                int h = br.ReadInt32();
                image.Width = w;
                image.Height = h;
                ITile[,] tiles = new ITile[w, h];
                for (int i = 0; i < w; i++)
                    for (int j = 0; j < h; j++)
                        tiles[i, j] = ReadTile(br);
                image.Tiles = tiles;
                try
                {
                    int signCount = br.ReadInt32();
                    List<SignData> signs = new List<SignData>();
                    image.Signs = signs;
                    for (int i = 0; i < signCount; i++)
                        signs.Add(ReadSign(br));
                }
                catch (EndOfStreamException) { }
            }
        }

        #region ReadTile

        public static ITile ReadTile(BinaryReader br)
        {
            Tile tile = new Tile()
            {
                sTileHeader = br.ReadInt16(),
                bTileHeader = br.ReadByte(),
                bTileHeader2 = br.ReadByte()
            };

            if (tile.active())
            {
                tile.type = br.ReadUInt16();
                if (Main.tileFrameImportant[tile.type])
                {
                    tile.frameX = br.ReadInt16();
                    tile.frameY = br.ReadInt16();
                }
            }
            tile.wall = br.ReadByte();
            tile.liquid = br.ReadByte();
            return tile;
        }

        #endregion
        #region ReadSign

        public static SignData ReadSign(BinaryReader br) =>
            new SignData()
            {
                X = br.ReadInt32(),
                Y = br.ReadInt32(),
                Text = br.ReadString()
            };

        #endregion

        #endregion
        #region ReadTEdit

        private void ReadTEdit(string path, ImageData image)
        {
            using (FileStream fs = File.OpenRead(path))
            using (BinaryReader br = new BinaryReader(fs))
            {
                br.ReadString();
                br.ReadUInt32();
                int w = br.ReadInt32(), h = br.ReadInt32();
                image.Width = w;
                image.Height = h;
                image.Tiles = ReadTiles(br, w, h);
                ReadChests(br);
                image.Signs = ReadSigns(br);
            }
        }

        #region ReadTiles

        private static ITile[,] ReadTiles(BinaryReader br, int w, int h)
        {
            ITile[,] tiles = new ITile[w, h];
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    ITile tile = ReadTile(br, out int count);
                    tiles[x, y] = tile;
                    while (count > 0)
                    {
                        tiles[x, ++y] = (ITile)tile.Clone();
                        count--;
                    }
                }
            return tiles;
        }

        #endregion
        #region ReadTile

        private static ITile ReadTile(BinaryReader br, out int count)
        {
            ITile tile = new Tile();
            byte tileHeader1 = 0;
            byte tileHeader2 = 0;
            byte tileHeader3 = br.ReadByte();
            if ((tileHeader3 & 1) > 0)
            {
                tileHeader2 = br.ReadByte();
                if ((tileHeader2 & 1) > 0)
                    tileHeader1 = br.ReadByte();
            }
            if ((tileHeader3 & 2) > 0)
            {
                tile.active(true);
                int tileType;
                if ((tileHeader3 & 32) == 0)
                    tileType = br.ReadByte();
                else
                {
                    byte lowerByte = br.ReadByte();
                    tileType = br.ReadByte();
                    tileType = (tileType << 8 | lowerByte);
                }
                tile.type = (ushort)tileType;
                if (!Main.tileFrameImportant[tileType])
                {
                    tile.frameX = -1;
                    tile.frameY = -1;
                }
                else
                {
                    tile.frameX = br.ReadInt16();
                    tile.frameY = br.ReadInt16();
                    if (tile.type == 144)
                        tile.frameY = 0;
                }
                if ((tileHeader1 & 8) > 0)
                    tile.color(br.ReadByte());
            }
            if ((tileHeader3 & 4) > 0)
            {
                tile.wall = br.ReadByte();
                if ((tileHeader1 & 16) > 0)
                    tile.wallColor(br.ReadByte());
            }
            byte liquidType = (byte)((tileHeader3 & 24) >> 3);
            if (liquidType != 0)
            {
                tile.liquid = br.ReadByte();
                tile.liquidType(liquidType);
            }
            if (tileHeader2 > 1)
            {
                if ((tileHeader2 & 2) > 0)
                    tile.wire(true);
                if ((tileHeader2 & 4) > 0)
                    tile.wire2(true);
                if ((tileHeader2 & 8) > 0)
                    tile.wire3(true);
                byte brickStyle = (byte)((tileHeader2 & 112) >> 4);
                if (brickStyle != 0 && Main.tileSolid.Length > tile.type && Main.tileSolid[tile.type])
                {
                    if (brickStyle == 1)
                        tile.halfBrick(true);
                    else if (brickStyle > 0)
                        tile.slope((byte)(brickStyle - 1));
                }
            }
            if (tileHeader1 > 0)
            {
                if ((tileHeader1 & 2) > 0)
                    tile.actuator(true);
                if ((tileHeader1 & 4) > 0)
                    tile.inActive(true);
                if ((tileHeader1 & 32) > 0)
                    tile.wire4(true);
            }
            byte storageType = (byte)((tileHeader3 & 192) >> 6);
            count = storageType == 0
                ? 0
                : storageType != 1
                    ? br.ReadInt16()
                    : br.ReadByte();
            return tile;
        }

        #endregion
        #region LoadChestData

        private static List<Chest> ReadChests(BinaryReader br)
        {
            int chestCount = br.ReadInt16();
            int chestItems = br.ReadInt16();
            int itemsPerChest;
            int overflowItems;
            if (chestItems > Chest.maxItems)
            {
                itemsPerChest = Chest.maxItems;
                overflowItems = chestItems - Chest.maxItems;
            }
            else
            {
                itemsPerChest = chestItems;
                overflowItems = 0;
            }
            int num;
            List<Chest> chests = new List<Chest>();
            for (int i = 0; i < chestCount; i = num + 1)
            {
                Chest chest = new Chest()
                {
                    x = br.ReadInt32(),
                    y = br.ReadInt32(),
                    name = br.ReadString()
                };
                for (int slot = 0; slot < itemsPerChest; slot++)
                {
                    short stackSize = br.ReadInt16();
                    chest.item[slot].stack = stackSize;
                    if (stackSize > 0)
                    {
                        int id = br.ReadInt32();
                        byte prefix = br.ReadByte();
                        chest.item[slot].netID = id;
                        chest.item[slot].stack = stackSize;
                        chest.item[slot].prefix = prefix;
                    }
                }
                for (int overflow = 0; overflow < overflowItems; overflow++)
                {
                    if (br.ReadInt16() > 0)
                    {
                        br.ReadInt32();
                        br.ReadByte();
                    }
                }
                chests.Add(chest);
                num = i;
            }
            return chests;
        }

        #endregion
        #region LoadSignData

        private static List<SignData> ReadSigns(BinaryReader br)
        {
            short signCount = br.ReadInt16();
            List<SignData> signs = new List<SignData>();
            for (int i = 0; i < signCount; i++)
            {
                string text = br.ReadString();
                int x = br.ReadInt32();
                int y = br.ReadInt32();
                signs.Add(new SignData()
                {
                    Text = text,
                    X = x,
                    Y = y
                });
            }
            return signs;
        }

        #endregion

        #endregion
    }

    public class TUIChest : Chest
    {
        public VisualChest Node { get; internal set; }

        public override string ToString()
        {
            // Update Node.Items here?????????????????????????? LMAAAAAO
            return base.ToString();
        }
    }
}
