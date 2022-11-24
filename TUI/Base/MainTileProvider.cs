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

        private static FieldInfo TileField;

        public MainTileProvider(object tile = null)
        {
            Tile = tile;
            if (Tile == null && TileField == null)
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
                        break;
                    }
                }
                if (TileField == null)
                    throw new Exception("Can't find OTAPI");
            }
        }

        public object this[int x, int y]
        {
            get
            {
                if (Tile == null)
                    Tile = TileField?.GetValue(null);
                return Tile[x, y];
            }
            set => Tile[x, y] = value;
        }

        public void SetXYWH(int x, int y, int width, int height, bool Draw = true)
        { }

        public void Enable(bool draw = true)
        {
            if (!Enabled)
            {
                Enabled = true;
                TUI.DrawRectangle(X, Y, Width, Height, true, -1, -1, true);
            }
        }

        public void Disable(bool draw = true)
        {
            if (Enabled)
            {
                Enabled = false;
                TUI.DrawRectangle(X, Y, Width, Height, true, -1, -1, true);
            }
        }

        public void Update()
        {
            if (Tile == null)
                Tile = TileField?.GetValue(null);
        }
    }
}
