using System;
using System.Collections.Generic;
using System.Linq;
using TUI.Base;
using TUI.Base.Style;

namespace TUITests
{
    struct kek
    {

    }
    class Tile
    {
        void ClearEverything() { }
        void active(bool b) { }
        void inActive(bool b) { }
        short tile { get; set; }
        void color(byte b) { }
        void wall(byte b) { }
        void wallColor(byte b) { }
    }

    public class UIPlayer
    {
        public int Index { get; set; } = 0;
        public string Name { get; set; } = "ASgo";
        public bool HasPermission(string permission) => true;
        public void Teleport(int x, int y) { }
    }

    class Program
    {
        static Tile[,] tile = new Tile[200, 130];

        static void test(string plrs)
        {
            List<int> players = new List<int>();
            foreach (char c in plrs)
                Console.WriteLine("INDEX: " + (int)c);
        }

        static void Main(string[] args)
        {
            HashSet<UIPlayer> me = new HashSet<UIPlayer>() {
                new UIPlayer() { Name = "kek", Index = 1 },
                new UIPlayer() { Name = "lol", Index = 254 }
            };
            test(String.Join(String.Empty, me.Select(p => (char)p.Index).ToArray()));
            return;

            /*TUI.TUI.Initialize();
            TUI.TUI.InitializePlayer(me.Index);
            RootVisualObject root = TUI.TUI.Create(new RootVisualObject("Game", 55, 115, 50, 40));
            root.SetupGrid(new ISize[] { new Relative(100), new Absolute(20) }, new ISize[] { new Absolute(20), new Relative(100) }, new Offset() { Right = 1 });
            TUI.TUI.Update();
            TUI.TUI.Touched(me.Index, new Touch(124, 110, TouchState.Begin, 0, 0));
            TUI.TUI.Touched(me.Index, new Touch(125, 110, TouchState.Moving, 0, 0));
            TUI.TUI.Touched(me.Index, new Touch(124, 110, TouchState.Moving, 0, 0));
            TUI.TUI.Touched(me.Index, new Touch(125, 110, TouchState.Moving, 0, 0));
            TUI.TUI.Touched(me.Index, new Touch(124, 110, TouchState.End, 0, 0));
            TUI.TUI.Touched(me.Index, new Touch(124, 110, TouchState.Begin, 0, 0));
            TUI.TUI.Touched(me.Index, new Touch(125, 110, TouchState.Moving, 0, 0));
            TUI.TUI.Touched(me.Index, new Touch(124, 110, TouchState.Moving, 0, 0));
            TUI.TUI.Touched(me.Index, new Touch(125, 110, TouchState.Moving, 0, 0));
            TUI.TUI.Touched(me.Index, new Touch(124, 110, TouchState.End, 0, 0));*/
            //game.Remove(game["lol"]);
            //UI.Touched(me, new Touch(24, 10, TouchState.Begin, session));
        }
    }
}
