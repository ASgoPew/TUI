using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Timers;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TUI.Base;
using TUI.Hooks.Args;
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
        private static Timer RegionTimer = new Timer(1000) { AutoReset = true };

        #endregion

        #region Constructor

        public TUIPlugin(Main game)
            : base(game)
        {
        }

        #endregion
        #region Initialize

        public override void Initialize()
        {
            ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize, Int32.MinValue);
            ServerApi.Hooks.ServerConnect.Register(this, OnServerConnect);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            ServerApi.Hooks.NetGetData.Register(this, OnGetData, 100);
            GetDataHandlers.NewProjectile += OnNewProjectile;
            TUI.TUI.Hooks.CanTouch.Event += OnCanTouch;
            TUI.TUI.Hooks.Draw.Event += OnDraw;
            TUI.TUI.Hooks.TouchCancel.Event += OnTouchCancel;
            TUI.TUI.Hooks.CreateSign.Event += OnCreateSign;
            TUI.TUI.Hooks.RemoveSign.Event += OnRemoveSign;
            TUI.TUI.Hooks.Log.Event += OnLog;
            RegionTimer.Elapsed += OnRegionTimer;
            RegionTimer.Start();

            if (!ImageData.Readers.ContainsKey(".dat"))
                ImageData.Readers.Add(".dat", ReadWorldEdit);
            if (!ImageData.Readers.ContainsKey(".TEditSch"))
                ImageData.Readers.Add(".TEditSch", ReadTEdit);

            TUI.TUI.Initialize(255);
        }

        #endregion
        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                TUI.TUI.Deinitialize();

                ServerApi.Hooks.ServerConnect.Deregister(this, OnServerConnect);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
                ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
                TShockAPI.GetDataHandlers.NewProjectile -= OnNewProjectile;
                TUI.TUI.Hooks.CanTouch.Event -= OnCanTouch;
                TUI.TUI.Hooks.Draw.Event -= OnDraw;
                TUI.TUI.Hooks.TouchCancel.Event -= OnTouchCancel;
                TUI.TUI.Hooks.CreateSign.Event -= OnCreateSign;
                TUI.TUI.Hooks.RemoveSign.Event -= OnRemoveSign;
                TUI.TUI.Hooks.Log.Event -= OnLog;
                RegionTimer.Elapsed -= OnRegionTimer;
                RegionTimer.Stop();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region OnGamePostInitialize

        private void OnGamePostInitialize(EventArgs args)
        {
            TUI.TUI.Update();
            TUI.TUI.Apply();
            TUI.TUI.Draw();
        }

        #endregion
        #region OnServerConnect

        private static void OnServerConnect(ConnectEventArgs args)
        {
            playerDesignState[args.Who] = DesignState.Waiting;
            TUI.TUI.InitializeUser(args.Who);
        }

        #endregion
        #region OnServerLeave

        private static void OnServerLeave(LeaveEventArgs args)
        {
            TUI.TUI.RemoveUser(args.Who);
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
                    args.Node.TrySetLock(args.Touch);
                    player.SendErrorMessage("You do not have access to this interface.");
                }
            }
        }

        #endregion
        #region OnDraw

        private static void OnDraw(DrawArgs args)
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

        private static void OnCreateSign(CreateSignArgs args)
        {
            if (args.Node.GetRoot().Provider.GetType().Name != "FakeTileRectangle")
            {
                Tile tile = new Tile() { type = 55, frameX = 0, frameY = 0 };
                Main.tile[args.X, args.Y] = tile;
                int id = Sign.ReadSign(args.X, args.Y);
                if (id >= 0)
                    args.Sign = Main.sign[id];
            }
            else
                CreateFakeSign(args);
        }

        #endregion
        #region OnRemoveSign

        private static void OnRemoveSign(RemoveSignArgs args)
        {
            if (args.Node.GetRoot().Provider.GetType().Name != "FakeTileRectangle")
                Sign.KillSign(args.Sign.x, args.Sign.y);
            else
                RemoveFakeSign(args);
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

        #region CreateFakeSign

        private static void CreateFakeSign(CreateSignArgs args)
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

        #endregion
        #region RemoveFakeSign

        private static void RemoveFakeSign(RemoveSignArgs args)
        {
            args.Node.GetRoot().Provider.RemoveSign(args.Sign);
        }

        #endregion

        #region OnRegionTimer

        private void OnRegionTimer(object sender, ElapsedEventArgs e)
        {
            foreach (RootVisualObject root in TUI.TUI.Roots)
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
                            TUI.TUI.Hooks.Draw.Invoke(new DrawArgs(root, x, y, root.Width,
                                root.Height, root.ForceSection, plr.Index, -1, true));
                    }
                    else
                        root.Players.Remove(plr.Index);
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
            Tile tile = new Tile
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
                image.Tiles = LoadTileData(br, w, h);
                LoadChestData(br);
                image.Signs = LoadSignData(br);
            }
        }

        #region LoadTileData

        private static ITile[,] LoadTileData(BinaryReader br, int w, int h)
        {
            ITile[,] tiles = new ITile[w, h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int rle;
                    ITile tile = DeserializeTileData(br, out rle);
                    tiles[x, y] = tile;
                    while (rle > 0)
                    {
                        y++;
                        if (y > h)
                        {
                            throw new Exception(string.Format("TEdit: Invalid Tile Data: RLE Compression outside of bounds [{0},{1}]", x, y));
                        }
                        tiles[x, y] = (Tile)tile.Clone();
                        rle--;
                    }
                }
            }
            return tiles;
        }

        #endregion
        #region DeserializeTileData

        private static ITile DeserializeTileData(BinaryReader br, out int rle)
        {
            ITile tile = new Tile();
            rle = 0;
            byte header3 = 0;
            byte header4 = 0;
            byte header5 = br.ReadByte();
            if ((header5 & 1) == 1)
            {
                header4 = br.ReadByte();
                if ((header4 & 1) == 1)
                {
                    header3 = br.ReadByte();
                }
            }
            if ((header5 & 2) == 2)
            {
                tile.active(true);
                int tileType;
                if ((header5 & 32) != 32)
                {
                    tileType = (int)br.ReadByte();
                }
                else
                {
                    byte lowerByte = br.ReadByte();
                    tileType = (int)br.ReadByte();
                    tileType = (tileType << 8 | (int)lowerByte);
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
                    {
                        tile.frameY = 0;
                    }
                }
                if ((header3 & 8) == 8)
                {
                    tile.color(br.ReadByte());
                }
            }
            if ((header5 & 4) == 4)
            {
                tile.wall = br.ReadByte();
                if ((header3 & 16) == 16)
                {
                    tile.wallColor(br.ReadByte());
                }
            }
            byte liquidType = (byte)((header5 & 24) >> 3);
            if (liquidType != 0)
            {
                tile.liquid = br.ReadByte();
                tile.liquidType(liquidType);
            }
            if (header4 > 1)
            {
                if ((header4 & 2) == 2)
                {
                    tile.wire(true);
                }
                if ((header4 & 4) == 4)
                {
                    tile.wire2(true);
                }
                if ((header4 & 8) == 8)
                {
                    tile.wire3(true);
                }
                byte brickStyle = (byte)((header4 & 112) >> 4);
                if (brickStyle != 0 && Main.tileSolid.Length > tile.type && Main.tileSolid[tile.type])
                {
                    if (brickStyle == 1)
                        tile.halfBrick(true);
                    else if (brickStyle > 0)
                        tile.slope((byte)(brickStyle - 1));
                }
            }
            if (header3 > 0)
            {
                if ((header3 & 2) == 2)
                {
                    tile.actuator(true);
                }
                if ((header3 & 4) == 4)
                {
                    tile.inActive(true);
                }
                if ((header3 & 32) == 32)
                {
                    tile.wire4(true);
                }
            }
            byte rleStorageType = (byte)((header5 & 192) >> 6);
            if (rleStorageType == 0)
            {
                rle = 0;
            }
            else if (rleStorageType != 1)
            {
                rle = (int)br.ReadInt16();
            }
            else
            {
                rle = (int)br.ReadByte();
            }
            return tile;
        }

        #endregion
        #region LoadChestData

        private static List<Chest> LoadChestData(BinaryReader r)
        {
            int totalChests = (int)r.ReadInt16();
            int maxItems = (int)r.ReadInt16();
            int itemsPerChest;
            int overflowItems;
            if (maxItems > Chest.maxItems)
            {
                itemsPerChest = Chest.maxItems;
                overflowItems = maxItems - Chest.maxItems;
            }
            else
            {
                itemsPerChest = maxItems;
                overflowItems = 0;
            }
            int num;
            List<Chest> chests = new List<Chest>();
            for (int i = 0; i < totalChests; i = num + 1)
            {
                Chest chest = new Chest
                {
                    x = r.ReadInt32(),
                    y = r.ReadInt32(),
                    name = r.ReadString()
                };
                for (int slot = 0; slot < itemsPerChest; slot++)
                {
                    short stackSize = r.ReadInt16();
                    chest.item[slot].stack = (int)stackSize;
                    if (stackSize > 0)
                    {
                        int id = r.ReadInt32();
                        byte prefix = r.ReadByte();
                        chest.item[slot].netID = id;
                        chest.item[slot].stack = (int)stackSize;
                        chest.item[slot].prefix = prefix;
                    }
                }
                for (int overflow = 0; overflow < overflowItems; overflow++)
                {
                    if (r.ReadInt16() > 0)
                    {
                        r.ReadInt32();
                        r.ReadByte();
                    }
                }
                chests.Add(chest);
                num = i;
            }
            return chests;
        }

        #endregion
        #region LoadSignData

        private static List<SignData> LoadSignData(BinaryReader br)
        {
            short totalSigns = br.ReadInt16();
            int num;
            List<SignData> signs = new List<SignData>();
            for (int i = 0; i < (int)totalSigns; i = num + 1)
            {
                string text = br.ReadString();
                int x = br.ReadInt32();
                int y = br.ReadInt32();
                signs.Add(new SignData()
                {
                    X = x,
                    Y = y,
                    Text = text
                });
                num = i;
            }
            return signs;
        }

        #endregion

        #endregion
    }
}
