using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaUI.Base;

namespace TerrariaUI.Widgets
{
    public class PagesList : VisualContainer
    {
        #region Data

        public int Page { get; protected set; }
        public int PagesCount => ChildCount;

        #endregion

        #region Constructor

        public PagesList(int x, int y)
            : base(x, y, 0, 0, new UIConfiguration() { UseBegin = false }) { }

        #endregion
        #region GetSizeNative

        protected override (int, int) GetSizeNative()
        {
            if (Selected == null && Child.ElementAtOrDefault(0) is VisualObject child)
                Select(child, false);
            return (Selected.Width, Selected.Height);
        }

        #endregion

        #region SelectPage

        public void SelectPage(int index, bool draw = true)
        {
            if (index == Page)
                return;

            VisualObject child = Child.ElementAtOrDefault(index);
            if (child == null)
                throw new InvalidOperationException("Trying to select page that doesn't exit");

            Select(Child[index], false);
            Page = index;
            Parent.Update().Apply();
            if (draw)
                Parent.Draw();
        }

        #endregion
        #region NextPage

        public bool NextPage(bool draw)
        {
            int page = Page;
            if (page == PagesCount - 1)
                return false;
            SelectPage(page + 1, draw);
            return true;
        }

        #endregion
        #region PreviousPage

        public bool PreviousPage(bool draw)
        {
            int page = Page;
            if (page == 0)
                return false;
            SelectPage(page - 1, draw);
            return true;
        }

        #endregion
    }
}
