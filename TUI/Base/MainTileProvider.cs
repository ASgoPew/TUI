using System;
using System.Reflection;

namespace TerrariaUI.Base
{
    public class MainTileProvider
    {
        internal dynamic Tile;
        internal int X = 0;
        internal int Y = 0;
        internal int Width = 0;
        internal int Height = 0;
        internal bool Enabled = true;

        private FieldInfo TileField;

        public MainTileProvider(object tile = null)
        {
            Tile = tile;
            if (Tile == null)
            {
                Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly asm in asms)
                {
                    if (asm.FullName.Contains("OTAPI"))
                    {
                        Module[] modules = asm.GetModules();
                        if (modules.Length == 0)
                            continue;
                        TileField = modules[0]?.GetType("Terraria.Main")?.GetField("tile");
                        Tile = TileField?.GetValue(null);
                    }
                }
            }
            if (Tile == null)
                throw new Exception("Can't find OTAPI");
        }

        public object this[int x, int y]
        {
            get => Tile[x, y];
            set => Tile[x, y] = value;
        }

        public void SetXYWH(int x, int y, int width, int height)
        { }

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
        }

        public void Update()
        {
            Tile = TileField?.GetValue(null);
        }
    }
}
