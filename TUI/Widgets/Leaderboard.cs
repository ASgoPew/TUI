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
            Name = GetDBKey(name);

            // TODO: HASHING NUMBERS????????
            // Optional TUI library hashing?
            // EnableNumberHashing(string key)

            SetupGrid(lines: new ISize[] { new Absolute(4), new Relative(100), new Dynamic() })
                .FillGrid();
            this[0, 0] = new Label(0, 0, Width, 4, Key, new LabelStyle() { Wall = 154, WallColor = 27 });
            this[0, 1].Style.Wall = 155;
            //this[0, 1].Style.WallColor = PaintID2.Black;
            this[0, 1].SetupLayout(Alignment.Up, Direction.Down, Side.Center, childIndent: 0);
            this[0, 1].Add(new ScrollBackground());
        }

        public static string GetDBKey(string name) => $"Leaderboard_{name}";

        public static void SetLeaderboardValue(string name, int user, int number) =>
            TUI.NDBSet(user, GetDBKey(name), number);

        public static int? GetLeaderboardValue(string name, int user) =>
            TUI.NDBGet(user, GetDBKey(name));

        public void LoadDBData()
        {
            foreach (var child in this[0, 1].ChildrenFromTop)
                if (child is Label)
                    this[0, 1].Remove(child);

            var list = NDBSelect(LeaderboardStyle.Ascending, LeaderboardStyle.Count, LeaderboardStyle.Offset, true);
            for (int i = 0; i < list.Count; i++)
            {
                var lineData = list[i];
                VisualContainer line = new VisualContainer(0, 0, 0, 4, new UIConfiguration() { UseBegin = false });
                line.SetParentStretch(true, false)
                    .SetupGrid(columns: new ISize[] { new Relative(100), new Dynamic() });
                byte color = i == 0
                    ? PaintID2.DeepYellow
                    : i == 1
                        ? PaintID2.Gray
                        : i == 2
                            ? PaintID2.Brown
                            : PaintID2.Black;
                line[0, 0] = new Label(0, 0, 0, 0, lineData.Username, new LabelStyle() { TextColor = color }).SetParentStretch(true, true);
                string number = lineData.Number.ToString();
                line[1, 0] = new Label(0, 0, number.Length * 2 + 2, 4, number, new LabelStyle() { TextColor = PaintID2.Black });
                this[0, 1].AddToLayout(line);
            }
        }

        public void AddFooter(VisualObject child) =>
            this[0, 2] = child;
    }
}
