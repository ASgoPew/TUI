using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Widgets
{

    #region PanelStyle

    /// <summary>
    /// Drawing styles for RatingList widget.
    /// </summary>
    public class RatingListStyle : ContainerStyle
    {
        public bool Ascending { get; set; } = true;
        public int Count { get; set; } = 5;
        public int Offset { get; set; } = 0;

        public RatingListStyle() : base() { }

        public RatingListStyle(RatingListStyle style)
            : base(style)
        {
        }
    }

    #endregion

    public class RatingList : VisualContainer
    {
        public RatingListStyle RatingListStyle => Style as RatingListStyle;

        public string Key { get; protected set; }

        public RatingList(int x, int y, int width, int height, string name, RatingListStyle style = null)
            : base(x, y, width, height, null, style ?? new RatingListStyle())
        {
            Key = name;
            Name = $"rating_{name}";

            // HASHING NUMBERS????????
            // Optional TUI library hashing?
            // EnableNumberHashing(string key)

            SetupGrid(lines: new ISize[] { new Absolute(4), new Relative(100), new Dynamic() });
            this[0, 0] = new Label(0, 0, Width, 4, Key);
            this[0, 1].SetupLayout(Alignment.Up, Direction.Down, Side.Center, childIndent: 0);

            var list = NDBSelect(RatingListStyle.Ascending, RatingListStyle.Count, RatingListStyle.Offset, true);
            foreach (var lineData in list)
            {
                VisualContainer line = new VisualContainer();
                line.SetupGrid(columns: new ISize[] { new Relative(100), new Dynamic() });
                line[0, 0] = new Label(0, 0, 0, 0, lineData.Username, new LabelStyle() {  }).SetFullSize(true, true);
                this[0, 1].AddToLayout(line);
            }
        }
    }
}
