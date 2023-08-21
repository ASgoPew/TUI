using System;
using System.Collections.Generic;
using System.Linq;
using TerrariaUI.Base;
using TerrariaUI.Base.Style;

namespace TerrariaUI.Widgets
{
    public class PaginationMenu : VisualContainer
    {
        #region Data

        protected PagesList Pages;

        #endregion

        #region Constructor

        public PaginationMenu(int x, int y, IEnumerable<string> values, int pageLimit, ButtonStyle style1 = null, ButtonStyle style2 = null,
                string title = null, LabelStyle titleStyle = null, Input<string> input = null, Action<Menu> titleCallback = null)
            : base(x, y, 0, 0)
        {
            SetupGrid(lines: new ISize[] { new Relative(100), new Absolute(4) });
            this[0, 0] = Pages = new PagesList(0, 0);
            while (values.Count() > 0)
            {
                Menu menu = new Menu(0, 0, 0, 0, values.Take(pageLimit), style1, style2, title, titleStyle, input, titleCallback);
                Pages.Add(menu);
                values = values.Skip(pageLimit);
            }
            Add(new Arrow(0, 0, new ArrowStyle() { Direction = Direction.Left })
                .SetParentAlignment(Alignment.DownLeft, new ExternalIndent() { Left = 1, Down = 1 }));
            Add(new Arrow(0, 0, new ArrowStyle() { Direction = Direction.Right })
                .SetParentAlignment(Alignment.DownRight, new ExternalIndent() { Right = 1, Down = 1 }));
            VisualObject footer = this[0, 1] = new VisualObject(0, pageLimit * 4 + (title != null ? 4 : 0), 0, 4);
            footer.SetupGrid(columns: new ISize[] { new Relative(50), new Relative(50) })
                .FillGrid();
            footer[0, 0].Callback = (self, touch) =>
            {
                if (Pages.PreviousPage(false))
                {
                    Update().Apply().Draw();
                }
            };
            footer[1, 0].Callback = (self, touch) =>
            {
                if (Pages.NextPage(false))
                {
                    Update().Apply().Draw();
                }
            };
        }

        #endregion
    }
}
