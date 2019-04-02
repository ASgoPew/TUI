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
            DateTime now = DateTime.Now;
            //UIPlayer p = new UIPlayer();
            dynamic p = new UIPlayer();
            int n = 0;
            while ((DateTime.Now - now).TotalMilliseconds < 1000)
                p.Teleport(0, n++);
            Console.WriteLine(n);
            return;

            UIPlayer me = new UIPlayer();
            RootVisualObject game = UI.Create("Game", 100, 100, 50, 20, new UITileProvider(null, 0, 0));
            game["lol"] = game.Add(new VisualObject(20, 10, 10, 10, null, null, (self, touch) =>
            {
                Console.WriteLine("Ok");
                return true;
            }));
            UIUserSession session = new UIUserSession(me.Index);
            //UI.InitializeUser(me.Index);
            UI.Touched(me.Index, new Touch(124, 110, TouchState.Begin, 0, 0));
            //game.Remove(game["lol"]);
            //UI.Touched(me, new Touch(24, 10, TouchState.Begin, session));
        }
    }
}
