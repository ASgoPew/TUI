using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUI;

namespace TUITests
{
    class Program
    {
        static void Main(string[] args)
        {
            RootVisualObject game = UI.Create("Game", 100, 100, 50, 20);
            game["lol"] = game.Add(new VisualObject(20, 10, 10, 10, null, null, (self, touch) =>
            {
                Console.WriteLine("Ok");
                return true;
            }));
            Console.WriteLine(game["lol"].Parent);
            UIUserSession session = new UIUserSession();
            game.Touched(new Touch(24, 10, TouchState.Begin, session));
            game.Remove(game["lol"]);
            game.Touched(new Touch(24, 10, TouchState.Begin, session));
        }
    }
}
