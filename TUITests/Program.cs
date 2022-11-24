using TerrariaUI;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;

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
        static void Main(string[] args)
        {
            TUI.Initialize();
            UIPlayer me = new UIPlayer();
            TUI.InitializePlayer(me.Index);
            RootVisualObject root = TUI.Create(new RootVisualObject("Game", 55, 115, 50, 40));
            root.SetupGrid(new ISize[] { new Relative(100), new Absolute(20) }, new ISize[] { new Absolute(20), new Relative(100) }, new Indent() { Right = 1 });
            TUI.Update();
            TUI.Touched(me.Index, new Touch(124, 110, TouchState.Begin, false, 0, 0));
            TUI.Touched(me.Index, new Touch(125, 110, TouchState.Moving, false, 0, 0));
            TUI.Touched(me.Index, new Touch(124, 110, TouchState.Moving, false, 0, 0));
            TUI.Touched(me.Index, new Touch(125, 110, TouchState.Moving, false, 0, 0));
            TUI.Touched(me.Index, new Touch(124, 110, TouchState.End, false, 0, 0));
            TUI.Touched(me.Index, new Touch(124, 110, TouchState.Begin, false, 0, 0));
            TUI.Touched(me.Index, new Touch(125, 110, TouchState.Moving, false, 0, 0));
            TUI.Touched(me.Index, new Touch(124, 110, TouchState.Moving, false, 0, 0));
            TUI.Touched(me.Index, new Touch(125, 110, TouchState.Moving, false, 0, 0));
            TUI.Touched(me.Index, new Touch(124, 110, TouchState.End, false, 0, 0));
            //game.Remove(game["lol"]);
            //UI.Touched(me, new Touch(24, 10, TouchState.Begin, session));
        }
    }
}
