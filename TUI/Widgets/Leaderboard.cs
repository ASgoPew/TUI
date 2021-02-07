using TerrariaUI.Base;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Widgets
{

    #region LeaderboardStyle

    /// <summary>
    /// Drawing styles for RatingList widget.
    /// </summary>
    public class LeaderboardStyle : ContainerStyle
    {
        public bool Ascending { get; set; } = true;
        public int Count { get; set; } = 5;
        public int Offset { get; set; } = 0;

        public LeaderboardStyle() : base() { }

        public LeaderboardStyle(LeaderboardStyle style)
            : base(style)
        {
        }
    }

    #endregion

    public class Leaderboard : VisualContainer
    {
        public LeaderboardStyle LeaderboardStyle => Style as LeaderboardStyle;

        public string Key { get; protected set; }

        public Leaderboard(int x, int y, int width, int height, string name, LeaderboardStyle style = null)
            : base(x, y, width, height, null, style ?? new LeaderboardStyle())
        {
            Key = name;
            Name = $"leaderboard_{name}";

            // HASHING NUMBERS????????
            // Optional TUI library hashing?
            // EnableNumberHashing(string key)

            SetupGrid(lines: new ISize[] { new Absolute(4), new Relative(100), new Dynamic() });
            this[0, 0] = new Label(0, 0, Width, 4, Key, new LabelStyle() { Wall = 155 });
            this[0, 1].Style.Wall = 155;
            this[0, 1].Style.WallColor = PaintID2.Black;
            this[0, 1].SetupLayout(Alignment.Up, Direction.Down, Side.Center, childIndent: 0);
            this[0, 1].Add(new ScrollBackground());
        }

        public static void SetLeaderboardValue(string name, int user, int number) =>
            TUI.NDBSet(user, $"leaderboard_{name}", number);

        public static int? GetLeaderboardValue(string name, int user) =>
            TUI.NDBGet(user, $"leaderboard_{name}");

        public void LoadDBData()
        {
            foreach (var child in this[0, 1].ChildrenFromTop)
                if (child is Label)
                    this[0, 1].Remove(child);

            var list = NDBSelect(LeaderboardStyle.Ascending, LeaderboardStyle.Count, LeaderboardStyle.Offset, true);
            foreach (var lineData in list)
            {
                VisualContainer line = new VisualContainer(0, 0, 0, 4, new UIConfiguration() { UseBegin = false });
                line.SetFullSize(true, false)
                    .SetupGrid(columns: new ISize[] { new Relative(100), new Dynamic() });
                line[0, 0] = new Label(0, 0, 0, 0, lineData.Username, new LabelStyle() { TextColor = PaintID2.White }).SetFullSize(true, true);
                string number = lineData.Number.ToString();
                line[1, 0] = new Label(0, 0, number.Length * 2 + 2, 4, number, new LabelStyle() { TextColor = PaintID2.White });
                this[0, 1].AddToLayout(line);
            }
        }

        public void AddFooter(VisualObject child) =>
            this[0, 2] = child;
    }
}
