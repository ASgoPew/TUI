using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUI;
using Terraria;
using System.Threading;

namespace TUITests
{
    public class UIPlayer : IUIUser
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
            UIPlayer me = new UIPlayer();
            RootVisualObject game = UI.Create("Game", 100, 100, 50, 20);
            game["lol"] = game.Add(new VisualObject(20, 10, 10, 10, null, null, (self, touch) =>
            {
                Console.WriteLine("Ok");
                return true;
            }));
            UIUserSession session = new UIUserSession();
            UI.Touched(me, new Touch(124, 110, TouchState.Begin, session));
            //game.Remove(game["lol"]);
            //UI.Touched(me, new Touch(24, 10, TouchState.Begin, session));
        }
    }
}
