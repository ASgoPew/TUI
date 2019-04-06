namespace TUI.Hooks.Args
{
    public class DrawArgs
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool ForcedSection { get; set; }

        public DrawArgs(int x, int y, int width, int height, bool forcedSection)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            ForcedSection = forcedSection;
        }
    }
}