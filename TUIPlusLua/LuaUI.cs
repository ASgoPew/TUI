using NLua;
using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUI;

namespace TUIPlusLua
{
    public static class LuaUI
    {
        public static UIConfiguration ConfigurationFromTable(LuaTable t)
        {
            UIConfiguration result = new UIConfiguration()
            {
                RootAcquire = (bool)(t["RootAcquire"] ?? true),
                BeginRequire = (bool)(t["BeginRequire"] ?? true),
                UseOutsideTouches = (bool)(t["UseOutsideTouches"] ?? false),
                Ordered = (bool)(t["Ordered"] ?? false),
                UseBegin = (bool)(t["UseBegin"] ?? true),
                UseMoving = (bool)(t["UseMoving"] ?? false),
                UseEnd = (bool)(t["UseEnd"] ?? false)
            };
            LuaFunction f = t["CustomApply"] as LuaFunction;
            if (f != null)
                result.CustomApply = (self) => { f.Call(self); return self; };
            f = t["CustomCanTouch"] as LuaFunction;
            if (f != null)
                result.CustomCanTouch = (self, touch) => (bool)(f.Call(self, touch).FirstOrDefault() ?? false);
            f = t["CustomUpdate"] as LuaFunction;
            if (f != null)
                result.CustomUpdate = (self) => { f.Call(self); return self; };
            return result;
        }

        public static UIStyle StyleFromTable(LuaTable t)
        {
            UIStyle result = new UIStyle();
            object value;
            if ((value = t["InActive"]) != null)
                result.InActive = (bool)value;
            if ((value = t["Tile"]) != null)
                result.Tile = (ushort)(long)value;
            if ((value = t["TileColor"]) != null)
                result.TileColor = (byte)(long)value;
            if ((value = t["Wall"]) != null)
                result.Wall = (byte)(long)value;
            if ((value = t["WallColor"]) != null)
                result.WallColor = (byte)(long)value;
            return result;
        }

        public static VisualObject Create(int x, int y, int width, int height, LuaTable configuration = null, LuaTable style = null, LuaFunction f = null) =>
            new VisualObject(x, y, width, height, ConfigurationFromTable(configuration), StyleFromTable(style),
                (self, touch) =>
                {
                    Console.WriteLine($"self: {self}, touch: {touch}");
                    return (bool)(f.Call(self, touch)?.FirstOrDefault() ?? true);
                });
    }
}
