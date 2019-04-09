using System;
using System.Reflection;

namespace TUI.Base
{
    public class MainTileProvider
    {
        internal dynamic Tile;
        internal int X = 0;
        internal int Y = 0;
        internal bool Enabled = true;

        public MainTileProvider(dynamic tile = null)
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
                        Tile = modules[0]?.GetType("Terraria.Main")?.GetField("tile")?.GetValue(null);
                    }
                }
            }
            if (Tile == null)
                throw new Exception("Can't find OTAPI");
        }

        public dynamic this[int x, int y]
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
    }
}
