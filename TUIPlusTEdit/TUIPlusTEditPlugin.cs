using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TerrariaUI.Widgets.Data;
using TerrariaUI.Widgets.Media;

namespace TUIPlusTEdit
{
    [ApiVersion(2, 1)]
    public class TUIPlusTEditPlugin : TerrariaPlugin
    {
        public override string Name => "TUI + TEdit";

        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public override string Author => "ASgo & Anzhelika";

        public override string Description => "Adds '.TEditSch' scheme format parser for TUI images";

        public TUIPlusTEditPlugin(Main game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            if (!ImageData.Readers.ContainsKey(".TEditSch"))
                ImageData.Readers.Add(".TEditSch", ReadTEdit);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ImageData.Readers.TryGetValue(".TEditSch", out var handler) && handler == ReadTEdit)
                    ImageData.Readers.Remove(".TEditSch");
            }
            base.Dispose(disposing);
        }

        #region ReadTEdit

        private List<ImageData> ReadTEdit(string name, bool video)
        {
            if (!video && Path.HasExtension(name) && File.Exists(name))
            {
                ImageData image = new ImageData();
                using (FileStream fs = File.OpenRead(name))
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
                return new List<ImageData>() { image };
            }
            return null;
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
        #region ReadChests

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
        #region ReadSigns

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
}
