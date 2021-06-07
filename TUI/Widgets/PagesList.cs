using System;
using System.Linq;
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

        public PagesList(int x, int y, int width = 0, int height = 0)
            : base(x, y, width, height, new UIConfiguration() { UseBegin = false }) { }

        #endregion

        #region SelectPage

        public void SelectPage(int index, bool draw = true)
        {
            if (index == Page)
                return;

            VisualObject child = _Child.ElementAtOrDefault(index);
            if (child == null)
                throw new InvalidOperationException("Trying to select page that doesn't exit");

            Select(_Child[index], false);
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
