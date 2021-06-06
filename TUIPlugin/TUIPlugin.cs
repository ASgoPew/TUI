using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TerrariaUI.Base;
using TerrariaUI.Hooks.Args;
using TerrariaUI;
using TerrariaUI.Widgets;
using Microsoft.Xna.Framework;
using TShockAPI.Hooks;
using OTAPI;

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
        public override string Description => "Plugin connection to TUI library";
        public override string Name => "TUIPlugin";
        public override Version Version => new Version(0, 1, 0, 0);

        public static int RegionAreaX = 80;
        public static int RegionAreaY = 50;
        public static DesignState[] playerDesignState = new DesignState[Main.maxPlayers];
        public static bool FakesEnabled = false;
        private static Timer RegionTimer = new Timer(1000) { AutoReset = true };
        private static int[] PlaceStyles = new int[Main.maxItemTypes];
        public static Command[] CommandList = new Command[]
        {
            new Command(TUI.ControlPermission, TUICommand, "tui")
        };

        #endregion

        #region Constructor

        public TUIPlugin(Main game)
            : base(game)
        {
            // ????????????????????????
            Order = -1000;
        }

        #endregion
        #region Initialize

        public override void Initialize()
        {
            try
            {
                TUI.Hooks.Log.Event += OnLog;

                FakesEnabled = ServerApi.Plugins.Count(p => p.Plugin.Name == "FakeProvider") > 0;

                var old = Main.player[Main.myPlayer];
                Main.player[Main.myPlayer] = new Player();
                for (int i = 0; i < Main.maxItemTypes; i++)
                {
                    Item item = new Item();
                    item.netDefaults(i);
                    PlaceStyles[i] = item.placeStyle;
                }
                Main.player[Main.myPlayer] = old;

                ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize, Int32.MinValue);
                ServerApi.Hooks.ServerConnect.Register(this, OnServerConnect);
                ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
                ServerApi.Hooks.NetGetData.Register(this, OnGetData, 100);
                PlayerHooks.PlayerLogout += OnPlayerLogout;
                GetDataHandlers.NewProjectile += OnNewProjectile;
                if (FakesEnabled)
                    Hooks.World.IO.PostSaveWorld += OnPostSaveWorld;
                TUI.Hooks.LoadRoot.Event += OnLoadRoot;
                TUI.Hooks.CanTouch.Event += OnCanTouch;
                TUI.Hooks.DrawObject.Event += OnDrawObject;
                TUI.Hooks.DrawRectangle.Event += OnDrawRectangle;
                TUI.Hooks.TouchCancel.Event += OnTouchCancel;
                TUI.Hooks.GetTile.Event += OnGetTile;
                TUI.Hooks.GetPlaceStyle.Event += OnGetPlaceStyle;
                TUI.Hooks.UpdateSign.Event += OnUpdateSign;
                TUI.Hooks.RemoveSign.Event += OnRemoveSign;
                TUI.Hooks.UpdateChest.Event += OnUpdateChest;
                TUI.Hooks.RemoveChest.Event += OnRemoveChest;
                TUI.Hooks.Database.Event += OnDatabase;
                RegionTimer.Elapsed += OnRegionTimer;

                Commands.ChatCommands.AddRange(CommandList);

                TUI.Initialize(255, Main.maxTilesX, Main.maxTilesY);
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
        }

        #endregion
        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    RegionTimer.Stop();

                    TUI.Dispose();

                    ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGamePostInitialize);
                    ServerApi.Hooks.ServerConnect.Deregister(this, OnServerConnect);
                    ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
                    ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
                    PlayerHooks.PlayerLogout -= OnPlayerLogout;
                    GetDataHandlers.NewProjectile -= OnNewProjectile;
                    if (FakesEnabled)
                        Hooks.World.IO.PostSaveWorld -= OnPostSaveWorld;
                    TUI.Hooks.LoadRoot.Event -= OnLoadRoot;
                    TUI.Hooks.CanTouch.Event -= OnCanTouch;
                    TUI.Hooks.DrawObject.Event -= OnDrawObject;
                    TUI.Hooks.DrawRectangle.Event -= OnDrawRectangle;
                    TUI.Hooks.TouchCancel.Event -= OnTouchCancel;
                    TUI.Hooks.GetTile.Event -= OnGetTile;
                    TUI.Hooks.GetPlaceStyle.Event -= OnGetPlaceStyle;
                    TUI.Hooks.UpdateSign.Event -= OnUpdateSign;
                    TUI.Hooks.RemoveSign.Event -= OnRemoveSign;
                    TUI.Hooks.UpdateChest.Event -= OnUpdateChest;
                    TUI.Hooks.RemoveChest.Event -= OnRemoveChest;
                    TUI.Hooks.Log.Event -= OnLog;
                    TUI.Hooks.Database.Event -= OnDatabase;
                    RegionTimer.Elapsed -= OnRegionTimer;

                    foreach (Command cmd in CommandList)
                        Commands.ChatCommands.Remove(cmd);
                }
                catch (Exception e)
                {
                    TUI.HandleException(e);
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region OnGamePostInitialize

        private void OnGamePostInitialize(EventArgs args)
        {
            try
            {
                Database.ConnectDB();

                TUI.Load(Main.worldID);
                TUI.Update();
                TUI.Apply();
                TUI.Draw();

                RegionTimer.Start();
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
        }

        #endregion
        #region OnServerConnect

        private static void OnServerConnect(ConnectEventArgs args)
        {
            try
            {
                playerDesignState[args.Who] = DesignState.Waiting;
                TUI.InitializePlayer(args.Who);
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
        }

        #endregion
        #region OnServerLeave

        private static void OnServerLeave(LeaveEventArgs args)
        {
            try
            {
                TUI.RemovePlayer(args.Who);
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
        }

        #endregion
        #region OnGetData

        private static void OnGetData(GetDataEventArgs args)
        {
            try
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

                        TUI.Touched(player.Index, new Touch(ex, ey, TouchState.End, prefix, designStateByte));
                        args.Handled = TUI.EndTouchHandled(player.Index);
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
                        Touch previousTouch = TUI.Session[owner].PreviousTouch;
                        if (TUI.Session[owner].ProjectileID == projectileID && previousTouch != null && previousTouch.State != TouchState.End)
                        {
                            Touch simulatedEndTouch = previousTouch.SimulatedEndTouch();
                            simulatedEndTouch.Undo = true;
                            TUI.Touched(owner, simulatedEndTouch);
                            playerDesignState[owner] = DesignState.Waiting;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
        }

        #endregion
        #region OnPlayerLogout

        private static void OnPlayerLogout(PlayerLogoutEventArgs args)
        {
            try
            {
                foreach (var pair in TUI.ApplicationPlayerSessions[args.Player.Index])
                    pair.Key.OnPlayerLogout(args.Player.Index);
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
        }

        #endregion
        #region OnNewProjectile

        private static void OnNewProjectile(object sender, GetDataHandlers.NewProjectileEventArgs args)
        {
            try
            {
                TSPlayer player = TShock.Players[args.Owner];
                if (args.Handled || args.Type != 651 || player?.TPlayer == null)
                    return;

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

                    if (TUI.Touched(args.Owner, new Touch(tileX, tileY, TouchState.Begin, prefix, 0)))
                        TUI.Session[args.Owner].ProjectileID = args.Identity;
                    playerDesignState[args.Owner] = DesignState.Moving;
                    //args.Handled = true;
                }
                else
                {
                    int tileX = (int)Math.Floor((args.Position.X + 5) / 16);
                    int tileY = (int)Math.Floor((args.Position.Y + 5) / 16);
                    TUI.Touched(args.Owner, new Touch(tileX, tileY, TouchState.Moving, prefix, 0));
                }
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
        }

        #endregion
        #region OnPostSaveWorld

        private static void OnPostSaveWorld(bool Cloud, bool ResetTime)
        {
            TUI.RequestDrawChanges();
        }

        #endregion

        #region FindNearPlayers

        private static void FindNearPlayers(RootVisualObject root)
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
        }

        #endregion

        #region OnLoadRoot

        private static void OnLoadRoot(LoadRootArgs args)
        {
            FindNearPlayers(args.Root);
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
                    TUI.TrySetLockForObject(args.Node, args.Touch);
                    player.SendErrorMessage("You do not have access to this interface.");
                }
            }
        }

        #endregion
        #region OnDrawObject

        private static void OnDrawObject(DrawObjectArgs args)
        {
            VisualObject node = args.Node;
            HashSet<int> players = args.TargetPlayers;
            if (players.Count == 0)
                return;

#if DEBUG
            TUI.Log($"Draw ({node.Name} -> " +
                string.Join(",", players.Select(i => TShock.Players[i]?.Name)) +
                $"): {args.X}, {args.Y}, {args.Width}, {args.Height}: {args.DrawWithSection}");
#endif

            // Yes, we are converting HashSet<int> to NetworkText to pass it to NetMessage.SendData for FakeManager...
            Terraria.Localization.NetworkText playerList = FakesEnabled
                ? Terraria.Localization.NetworkText.FromLiteral(string.Concat(players.Select(p => (char) p)))
                : null;

            if (args.Width * args.Height >= 2500 || args.DrawWithSection)
            {
                if (FakesEnabled)
                    NetMessage.SendData(10, -1, -1, playerList, args.X, args.Y, args.Width, args.Height);
                else
                    foreach (int i in players)
                        NetMessage.SendData(10, i, -1, null, args.X, args.Y, args.Width, args.Height);
                if (args.FrameSection)
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
                    NetMessage.SendData(20, -1, -1, playerList, args.X, args.Y, args.Width, args.Height);
                else
                    foreach (int i in players)
                        NetMessage.SendData(20, i, -1, null, args.X, args.Y, args.Width, args.Height);
            }

            // Mark that these players received latest version of interface
            foreach (int player in players)
                node.Root.PlayerApplyCounter[player] = node.Root.DrawState;
        }

        #endregion
        #region OnDrawRectangle

        private static void OnDrawRectangle(DrawRectangleArgs args)
        {
            if (args.Width * args.Height >= 2500 || args.DrawWithSection)
            {
                NetMessage.SendData(10, args.PlayerIndex, args.ExceptPlayerIndex, null,
                    args.X, args.Y, args.Width, args.Height);
                if (args.FrameSection)
                {
                    int lowX = Netplay.GetSectionX(args.X);
                    int highX = Netplay.GetSectionX(args.X + args.Width - 1);
                    int lowY = Netplay.GetSectionY(args.Y);
                    int highY = Netplay.GetSectionY(args.Y + args.Height - 1);
                    NetMessage.SendData(11, args.PlayerIndex, args.ExceptPlayerIndex, null,
                        lowX, lowY, highX, highY);
                }
            }
            else
                NetMessage.SendData(20, args.PlayerIndex, args.ExceptPlayerIndex, null, args.X, args.Y, args.Width, args.Height);
        }

        #endregion
        #region OnTouchCancel

        private static void OnTouchCancel(TouchCancelArgs args)
        {
            TSPlayer player = args.Touch.Player();
            player.SendWarningMessage("You are holding mouse for too long.");
            TUI.Hooks.Log.Invoke(new LogArgs($"TUI: Touch too long ({player.Name}).", LogType.Info));
            player.SendData(PacketTypes.ProjectileDestroy, null, args.Session.ProjectileID, player.Index);
            Touch simulatedEndTouch = args.Touch.SimulatedEndTouch();
            simulatedEndTouch.Undo = true;
            TUI.Touched(args.UserIndex, simulatedEndTouch);
            playerDesignState[args.UserIndex] = DesignState.Waiting;
        }

        #endregion
        #region OnGetTile

        private static void OnGetTile(GetTileArgs args)
        {
            if (args.X < 0 || args.Y < 0 || args.X >= Main.maxTilesX || args.Y >= Main.maxTilesY)
                return;

            args.Tile = Main.tile[args.X, args.Y];
        }

        #endregion
        #region OnGetPlaceStyle

        private void OnGetPlaceStyle(GetPlaceStyleArgs args)
        {
            args.PlaceStyle = PlaceStyles[args.Item];
        }

        #endregion
        #region OnUpdateSign

        private static void OnUpdateSign(UpdateSignArgs args)
        {
            dynamic provider = (args.Node.Root ?? args.Node.GetRoot()).Provider;
            if (provider is MainTileProvider)
            {
                Main.tile[args.X, args.Y] = new Tile() { type = 55, frameX = 0, frameY = 0 };
                int id = Sign.ReadSign(args.X, args.Y);
                if (id >= 0)
                {
                    // Can lead to creating the same sign since ReadSign returns existing sign if there is one.
                    // Console.WriteLine($"{args.Node.FullName} OnCreateSign: {id} ({args.X}, {args.Y})");
                    args.Sign = Main.sign[id];
                }
                else
                    TUI.HandleException(args.Node, new Exception("TUIPlugin: Can't create Main.sign sign."));
            }
            else
            {
                try
                {
                    args.Sign = provider.AddSign(args.X - provider.X, args.Y - provider.Y, "");
                }
                catch (Exception e)
                {
                    TUI.HandleException(args.Node, new Exception("TUIPlugin: Can't create FAKE sign", e));
                }
            }
        }

        #endregion
        #region OnRemoveSign

        private static void OnRemoveSign(RemoveSignArgs args)
        {
            dynamic provider = (args.Node.Root ?? args.Node.GetRoot()).Provider;
            if (provider is MainTileProvider)
                Sign.KillSign(args.Sign.x, args.Sign.y);
            else
                // TODO: No such entity in this tile provider (when doing /py reset with disabled panel but enabled provider)
                provider.RemoveEntity(args.Sign);
        }

        #endregion
        #region OnUpdateChest

        private static void OnUpdateChest(UpdateChestArgs args)
        {
            dynamic provider = (args.Node.Root ?? args.Node.GetRoot()).Provider;
            if (provider is MainTileProvider)
            {
                Main.tile[args.X, args.Y] = new Tile() { type = 21, sTileHeader = 32, frameX = 0, frameY = 0 };
                Main.tile[args.X + 1, args.Y] = new Tile() { type = 21, sTileHeader = 32, frameX = 18, frameY = 0 };
                Main.tile[args.X, args.Y + 1] = new Tile() { type = 21, sTileHeader = 32, frameX = 0, frameY = 18 };
                Main.tile[args.X + 1, args.Y + 1] = new Tile() { type = 21, sTileHeader = 32, frameX = 18, frameY = 18 };
                int id = Chest.FindChest(args.X, args.Y);
                if (id < 0)
                {
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
                    TUI.HandleException(args.Node, new Exception("TUIPlugin: Can't create Main.chest chest."));
            }
            else
            {
                try
                {
                    args.Chest = provider.AddChest(args.X - provider.X, args.Y - provider.Y);
                }
                catch (Exception e)
                {
                    TUI.HandleException(args.Node, new Exception("TUIPlugin: Can't create FAKE chest", e));
                }
            }
        }

        #endregion
        #region OnRemoveChest

        private static void OnRemoveChest(RemoveChestArgs args)
        {
            dynamic provider = (args.Node.Root ?? args.Node.GetRoot()).Provider;
            if (provider is MainTileProvider)
            {
                int chestX = args.Chest.x;
                int chestY = args.Chest.y;
                for (int i = 0; i < 1000; i++)
                    if (Main.chest[i] is Chest chest && (chest.x == chestX && chest.y == chestY))
                        Main.chest[i] = null;
            }
            else
                provider.RemoveEntity(args.Chest);
        }

        #endregion
        #region OnLog

        private static void OnLog(LogArgs args)
        {
            args.Text = $"TUI{(args.Node != null ? $" ({args.Node.FullName})" : "")}: {args.Text}";
            if (TShock.Log is ILog log)
            {
                switch (args.Type)
                {
                    case LogType.Success:
                        log.ConsoleInfo(args.Text);
                        break;
                    case LogType.Info:
                        log.ConsoleInfo(args.Text);
                        break;
                    case LogType.Warning:
                        log.ConsoleError(args.Text);
                        break;
                    case LogType.Error:
                        log.ConsoleError(args.Text);
                        break;
                }
                for (int i = 0; i < 255; i++)
                {
                    TSPlayer player = TShock.Players[i];
                    if (player?.Active == true && player.HasPermission(TUI.ControlPermission))
                        switch (args.Type)
                        {
                            case LogType.Success:
                                player.SendSuccessMessage(args.Text);
                                break;
                            case LogType.Info:
                                player.SendInfoMessage(args.Text);
                                break;
                            case LogType.Warning:
                                player.SendWarningMessage(args.Text);
                                break;
                            case LogType.Error:
                                player.SendErrorMessage(args.Text);
                                break;
                        }
                }
            }
            else
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                switch (args.Type)
                {
                    case LogType.Success:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogType.Info:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogType.Warning:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogType.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                }
                Console.WriteLine(args.Text);
                Console.ForegroundColor = oldColor;
            }
        }

        #endregion
        #region OnDatabase

        private static void OnDatabase(DatabaseArgs args)
        {
            switch (args.Type)
            {
                case DatabaseActionType.Get:
                    if (args.Number.HasValue)
                        args.Number = Database.GetNumber(args.User.Value, args.Key);
                    else if (args.User.HasValue)
                        args.Data = Database.GetData(args.User.Value, args.Key);
                    else
                        args.Data = Database.GetData(args.Key);
                    break;
                case DatabaseActionType.Set:
                    if (args.Number.HasValue)
                        Database.SetNumber(args.User.Value, args.Key, args.Number.Value);
                    else if (args.User.HasValue)
                        Database.SetData(args.User.Value, args.Key, args.Data);
                    else
                        Database.SetData(args.Key, args.Data);
                    break;
                case DatabaseActionType.Remove:
                    if (args.Number.HasValue)
                        Database.RemoveNumber(args.User.Value, args.Key);
                    else if (args.User.HasValue)
                        Database.RemoveKey(args.User.Value, args.Key);
                    else
                        Database.RemoveData(args.Key);
                    break;
                case DatabaseActionType.Select:
                    args.Numbers = Database.SelectNumbers(args.Key, args.Ascending, args.Count, args.Offset, args.RequestNames);
                    break;
            }
        }

        #endregion

        #region OnRegionTimer

        private void OnRegionTimer(object sender, ElapsedEventArgs args)
        {
            try
            {
                foreach (RootVisualObject root in TUI.Roots.Where(r => r.Enabled))
                {
                    FindNearPlayers(root);
                    root.Draw();
                }

                foreach (var pair in TUI.ApplicationTypes)
                    foreach (var pair2 in pair.Value.IterateInstances)
                    {
                        Application app = pair2.Value;
                        (int centerX, int centerY) = app.CenterPosition();
                        Vector2 center = new Vector2(centerX, centerY);
                        int[] players = app.SessionPlayers;
                        bool sessionActive = players != null;

                        // Player session
                        if (sessionActive)
                        {
                            // Players too far
                            foreach (int index in players)
                            {
                                TSPlayer player = TShock.Players[index];
                                if (player?.Active != true)
                                    app.OnPlayerLeave(index);
                                else if (index != app.ApplicationStyle.TrackingPlayer &&
                                        Vector2.Distance(new Vector2(player.TileX, player.TileY), center) >=
                                        app.ApplicationStyle.MaxDistance)
                                    app.OnPlayerTooFar(index);
                            }
                            // Timeout
                            if (app.PlayerSessionTimeout >= 0 &&
                                    (DateTime.UtcNow - app.PlayerSessionCreateTime).TotalSeconds >= app.PlayerSessionTimeout)
                                app.OnPlayerSessionTimeout();
                        }
                        // Lifetime
                        if (app.ApplicationStyle.Timeout >= 0 &&
                                (DateTime.UtcNow - app.CreateTime).TotalSeconds >= app.ApplicationStyle.Timeout)
                            app.OnTimeout();
                        // Tracking
                        int trackingPlayer = app.ApplicationStyle.TrackingPlayer;
                        if (trackingPlayer >= 0)
                        {
                            TSPlayer player = TShock.Players[trackingPlayer];
                            if (player?.Active != true)
                            {
                                app.OnPlayerLeave(trackingPlayer);
                                app.ApplicationStyle.TrackingPlayer = -1;
                            }
                            else if (Vector2.Distance(new Vector2(player.TileX, player.TileY), center) >=
                                    app.ApplicationStyle.TrackingDistance
                                    && (app.ApplicationStyle.TrackInMotion || player.TPlayer.velocity.Length() < 0.1f))
                                app.TrackingTeleport(player.TileX, player.TileY);
                        }
                    }
            }
            catch (Exception e)
            {
                TUI.HandleException(e);
            }
        }

        #endregion

        #region FindRoot

        public static bool FindRoot(string name, TSPlayer player, out RootVisualObject found)
        {
            found = null;
            List<RootVisualObject> foundRoots = new List<RootVisualObject>();
            string lowerName = name.ToLower();
            foreach (RootVisualObject root in TUI.Roots)
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
                player?.SendErrorMessage($"Interface '{name}' not found.");
                return false;
            }
            else if (foundRoots.Count > 1)
            {
                if (player != null)
                    player.SendMultipleMatchError(foundRoots);
                return false;
            }
            else
            {
                found = foundRoots[0];
                return true;
            }
        }

        #endregion
        #region FindAppType

        public static bool FindAppType(string name, TSPlayer player, out ApplicationType found)
        {
            found = null;
            List<ApplicationType> foundRoots = new List<ApplicationType>();
            string lowerName = name.ToLower();
            foreach (var pair in TUI.ApplicationTypes.Where(pair => pair.Value.AllowManualRun))
            {
                if (pair.Key == name)
                {
                    found = pair.Value;
                    return true;
                }
                else if (pair.Key.ToLower().StartsWith(lowerName))
                    foundRoots.Add(pair.Value);
            }
            if (foundRoots.Count == 0)
            {
                player?.SendErrorMessage($"Application '{name}' not found.");
                return false;
            }
            else if (foundRoots.Count > 1)
            {
                if (player != null)
                    player.SendMultipleMatchError(foundRoots);
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

                    int x = args.Player.TileX - root.Width / 2;
                    int y = args.Player.TileY - root.Height / 2;
                    root.SetXY(x, y, true);
                    args.Player.SendSuccessMessage($"Moved interface '{root.Name}' successfully.");
                    break;
                }
                case "e":
                case "en":
                case "enable":
                {
                    if (args.Parameters.Count < 2
                        || args.Parameters.Count > 3)
                    {
                        args.Player.SendErrorMessage("/tui enable \"interface name\" [-confirm]");
                        return;
                    }
                    if (!FindRoot(args.Parameters[1], args.Player, out RootVisualObject root))
                        return;

                    if (root.UsesDefaultMainProvider
                        && args.Parameters.Last().ToLower() != "-confirm")
                    {
                        args.Player.SendErrorMessage($"Interface '{root.Name}' will be drawn on main map.\n" +
                            $"Type '/tui enable \"{root.Name}\" -confirm' " +
                            "to confirm the interface activation.");
                        return;
                    }

                    root.Enable(true);
                    if (root is Panel panel)
                        panel.SavePanel();
                    args.Player.SendSuccessMessage($"Enabled interface '{root.Name}'.");
                    break;
                }
                case "d":
                case "dis":
                case "disable":
                {
                    if (args.Parameters.Count < 2
                        || args.Parameters.Count > 3)
                    {
                        args.Player.SendErrorMessage("/tui disable \"interface name\"");
                        return;
                    }
                    if (!FindRoot(args.Parameters[1], args.Player, out RootVisualObject root))
                        return;

                    root.Disable(true);
                    if (root is Panel panel)
                        panel.SavePanel();
                    args.Player.SendSuccessMessage($"Disabled interface '{root.Name}'.");
                    break;
                }
                case "i":
                case "info":
                {
                    if (args.Parameters.Count != 2)
                    {
                        args.Player.SendErrorMessage("/tui info \"interface name\"");
                        return;
                    }
                    if (!FindRoot(args.Parameters[1], args.Player, out RootVisualObject root))
                        return;

                    string provider_text = root.Provider is MainTileProvider
                            ? nameof(MainTileProvider)
                            : $"{root.Provider.Name} ({root.Provider.GetType().Name})";
                    args.Player.SendInfoMessage(
$@"Interface '{root.Name}'
Position and size: {root.XYWH()}
Enabled: {root.Enabled}
Tile provider: {provider_text}
Layer: {root.Layer}
Observers: {(root.Observers != null ? string.Join(",", root.Observers.Where(observer => TShock.Players[observer]?.Active == true)
    .Select(observer => TShock.Players[observer]?.Name)) : "all")}
Draw state: {root.DrawState}");
                    break;
                }
                case "xywh":
                {
                    if (args.Parameters.Count != 4 && args.Parameters.Count != 6)
                    {
                        args.Player.SendErrorMessage("/tui xywh \"interface name\" <x> <y> [<width> <height>]");
                        return;
                    }
                    if (!FindRoot(args.Parameters[1], args.Player, out RootVisualObject root))
                        return;

                    if (!int.TryParse(args.Parameters[2], out int x)
                        || !int.TryParse(args.Parameters[3], out int y)
                        || x < 0 || y < 0 || x >= Main.maxTilesX || y >= Main.maxTilesY)
                    {
                        args.Player.SendErrorMessage("Invalid coordinates " +
                            $"'{args.Parameters[2]},{args.Parameters[3]}'.");
                        return;
                    }

                    int width = root.Width;
                    int height = root.Height;
                    if (args.Parameters.Count == 6
                        && (!int.TryParse(args.Parameters[4], out width)
                        || !int.TryParse(args.Parameters[5], out height)
                        || width < 0 || x + width >= Main.maxTilesX
                        || height < 0 || y + height >= Main.maxTilesY))
                    {
                        args.Player.SendErrorMessage("Invalid size " +
                            $"'{args.Parameters[4]},{args.Parameters[5]}'.");
                        return;
                    }

                    root.SetXYWH(x, y, width, height, true);
                    args.Player.SendSuccessMessage("Set position and size " +
                        $"of interface '{root.Name}' to {root.XYWH()}.");
                    break;
                }
                case "reset":
                {
                    if (args.Parameters.Count != 2)
                    {
                        args.Player.SendErrorMessage("/tui reset \"interface name\"");
                        return;
                    }
                    if (!FindRoot(args.Parameters[1], args.Player, out RootVisualObject root))
                        return;

                    root.Pulse(PulseType.Reset);
                    if (root is Panel panel)
                    {
                        panel.HidePopUp();
                        panel.UnsummonAll();
                    }

                    args.Player.SendSuccessMessage($"Interface '{root.Name}' was reset.");
                    break;
                }
                case "del":
                case "destroy":
                case "delete":
                case "rm":
                case "remove":
                {
                    if (args.Parameters.Count != 3)
                    {
                        args.Player.SendErrorMessage("/tui remove \"interface name\" -confirm");
                        return;
                    }
                    if (!FindRoot(args.Parameters[1], args.Player, out RootVisualObject root))
                        return;

                    TUI.Destroy(root);

                    args.Player.SendSuccessMessage($"Interface '{root.Name}' was destroyed.");
                    break;
                }
                case "app":
                case "application":
                {
                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage("/tui app <add/remove> \"app name\"");
                        args.Player.SendErrorMessage("/tui app list");
                        return;
                    }
                    switch (args.Parameters[1])
                    {
                        case "add":
                        {
                            if (args.Parameters.Count != 3)
                            {
                                args.Player.SendErrorMessage("/tui app add \"app name\"");
                                return;
                            }
                            if (!FindAppType(args.Parameters[2], args.Player, out ApplicationType appType))
                                return;

                            appType.CreateInstance(args.Player.TileX, args.Player.TileY);
                            args.Player.SendSuccessMessage($"Created new app instance: {appType.Name}");
                            break;
                        }
                        case "private":
                        {
                            if (args.Parameters.Count != 3)
                            {
                                args.Player.SendErrorMessage("/tui app private \"app name\"");
                                return;
                            }
                            if (!FindAppType(args.Parameters[2], args.Player, out ApplicationType appType))
                                return;

                            appType.CreateInstance(args.Player.TileX, args.Player.TileY,
                                new HashSet<int>() { args.Player.Index });
                            args.Player.SendSuccessMessage($"Created new private app instance: {appType.Name}");
                            break;
                        }
                        case "remove":
                        {
                            if (args.Parameters.Count < 3 || args.Parameters.Count > 4)
                            {
                                args.Player.SendErrorMessage("/tui app remove \"app name\" [<index>/all]");
                                return;
                            }
                            if (!FindAppType(args.Parameters[2], args.Player, out ApplicationType appType))
                                return;

                            if (args.Parameters.Count == 3)
                            {
                                if (!appType.TryDestroy(args.Player.TileX, args.Player.TileY, out string name))
                                    args.Player.SendErrorMessage($"There are not app instances of type '{appType.Name}' at this point.");
                                else
                                    args.Player.SendSuccessMessage($"Removed app instance: {name}.");
                            }
                            else
                            {
                                if (args.Parameters[3].ToLower() == "all")
                                {
                                    appType.DestroyAll();
                                    args.Player.SendSuccessMessage($"Removed app instances of type '{appType.Name}'");
                                }
                                else if (!Int32.TryParse(args.Parameters[3], out int index))
                                    args.Player.SendErrorMessage($"Invalid index: {args.Parameters[3]}");
                                else
                                {
                                    if (appType[index] is Application app)
                                        TUI.Destroy(app);
                                    args.Player.SendSuccessMessage($"Removed app instance of type '{appType.Name}' with index {index}");
                                }
                            }
                            break;
                        }
                        case "list":
                        {
                            if (!PaginationTools.TryParsePageNumber(args.Parameters, 2, args.Player, out int page))
                                return;
                            List<string> lines = PaginationTools.BuildLinesFromTerms(TUI.ApplicationTypes.Values.Where(appType => appType.AllowManualRun));
                            PaginationTools.SendPage(args.Player, page, lines, new PaginationTools.Settings()
                            {
                                HeaderFormat = "TUI apps ({0}/{1}):",
                                FooterFormat = "Type '/tui app list {0}' for more.",
                                NothingToDisplayString = "There are no TUI apps yet."
                            });
                            break;
                        }
                        default:
                        {
                            args.Player.SendInfoMessage("/tui app <add/remove> \"app name\"");
                            args.Player.SendInfoMessage("/tui app list");
                            break;
                        }
                    }
                    break;
                }
                case "l":
                case "list":
                {
                    if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out int page))
                        return;
                    List<string> lines = PaginationTools.BuildLinesFromTerms(TUI.Roots);
                    PaginationTools.SendPage(args.Player, page, lines, new PaginationTools.Settings()
                    {
                        HeaderFormat = "TUI interfaces ({0}/{1}):",
                        FooterFormat = "Type '/tui list {0}' for more.",
                        NothingToDisplayString = "There are no TUI interfaces yet."
                    });
                    break;
                }
                default:
                {
                    args.Player.SendSuccessMessage("/tui subcommands:");
                    args.Player.SendInfoMessage("/tui info \"interface name\"");
                    args.Player.SendInfoMessage("/tui xywh \"interface name\" <x> <y> [<width> <height>]");
                    args.Player.SendInfoMessage("/tui tp \"interface name\"");
                    args.Player.SendInfoMessage("/tui tphere \"interface name\" [-confirm]");
                    args.Player.SendInfoMessage("/tui enable \"interface name\" [-confirm]");
                    args.Player.SendInfoMessage("/tui disable \"interface name\"");
                    args.Player.SendInfoMessage("/tui reset \"interface name\"");
                    args.Player.SendInfoMessage("/tui remove \"interface name\" -confirm");
                    args.Player.SendInfoMessage("/tui app <add/remove> \"app name\"");
                    args.Player.SendInfoMessage("/tui app list");
                    args.Player.SendInfoMessage("/tui list [page]");
                    break;
                }
            }
        }

        #endregion
    }
}
