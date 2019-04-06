using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUI;
using Terraria;
using System.Threading;
using TUI.Base;

namespace TUITests
{
    public class UIPlayer
    {
        public int Index => 0;
        public string Name => "ASgo";
        public bool HasPermission(string permission) => true;
        public void Teleport(int x, int y) { }
    }

    class Program
    {
        static void Main(string[] args)
        {
            UI.Initialize();
            UIPlayer me = new UIPlayer();
            UI.InitializeUser(me.Index);
            RootVisualObject game = UI.Create("Game", 100, 100, 50, 20, new UITileProvider(null, 0, 0));
            game["lol"] = game.Add(new VisualObject(20, 10, 10, 10, new UIConfiguration() { UseMoving = true, UseEnd = true }, null, (self, touch) =>
            {
                Console.WriteLine("Ok");
                return true;
            }));
            UI.Update();
            UI.Touched(me.Index, new Touch(124, 110, TouchState.Begin, 0, 0));
            UI.Touched(me.Index, new Touch(125, 110, TouchState.Moving, 0, 0));
            UI.Touched(me.Index, new Touch(124, 110, TouchState.Moving, 0, 0));
            UI.Touched(me.Index, new Touch(125, 110, TouchState.Moving, 0, 0));
            UI.Touched(me.Index, new Touch(124, 110, TouchState.End, 0, 0));
            UI.Touched(me.Index, new Touch(124, 110, TouchState.Begin, 0, 0));
            UI.Touched(me.Index, new Touch(125, 110, TouchState.Moving, 0, 0));
            UI.Touched(me.Index, new Touch(124, 110, TouchState.Moving, 0, 0));
            UI.Touched(me.Index, new Touch(125, 110, TouchState.Moving, 0, 0));
            UI.Touched(me.Index, new Touch(124, 110, TouchState.End, 0, 0));
            //game.Remove(game["lol"]);
            //UI.Touched(me, new Touch(24, 10, TouchState.Begin, session));
        }
    }
}
