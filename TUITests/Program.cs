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
        public int Index => 0;
        public string Name => "ASgo";
        public bool HasPermission(string permission) => true;
        public void Teleport(int x, int y) { }
    }

    class Program
    {
        static Tile[,] tile = new Tile[200, 130];

        static void Main(string[] args)
        {
            TUI.TUI.Initialize();
            UIPlayer me = new UIPlayer();
            TUI.TUI.InitializeUser(me.Index);
            RootVisualObject root = TUI.TUI.CreateRoot("Game", 55, 115, 50, 40);
            root.SetupGrid(new GridStyle(new ISize[] { new Relative(100), new Absolute(20) }, new ISize[] { new Absolute(20), new Relative(100) })
            {
                Offset = new Offset() { Right = 1 }
            });
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
            TUI.TUI.Touched(me.Index, new Touch(124, 110, TouchState.End, 0, 0));
            //game.Remove(game["lol"]);
            //UI.Touched(me, new Touch(24, 10, TouchState.Begin, session));
        }
    }
}
