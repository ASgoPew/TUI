using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace TUI
{
    public class VisualObject : Touchable<VisualObject>
    {
        #region Data

        public UIStyle Style { get; set; }
        public ITileCollection Provider { get; set; }
        public bool ForceSection { get; set; } = false;

        #endregion

        #region IDOM

            #region SetTop

            public override bool SetTop(VisualObject o)
            {
                bool result = base.SetTop(o);
                if (result && o.Provider != null)
                    UI.SetTopHook.Invoke(new SetTopArgs(o));
                return result;
            }

            #endregion
            #region PostSetTop

            public override void PostSetTop()
            {
                Apply(true).Draw();
            }

        #endregion

        #endregion
        #region IVisual

            #region SetXYWH

            public override VisualObject SetXYWH(int x, int y, int width = -1, int height = -1)
            {
                base.SetXYWH(x, y, width, height);
                if (Provider != null)
                    UI.SetXYWHHook.Invoke(new SetXYWHArgs(this, x, y, width, height));
                return this;
            }

            #endregion

        #endregion

        #region Initialize

        VisualObject(int x, int y, int width, int height, string name, UIConfiguration configuration, UIStyle style, Func<VisualObject, Touch<VisualObject>, bool> callback = null, ITileCollection provider = null)
            : base(x, y, width, height, configuration, callback)
        {
            Style = style;
            Provider = provider;
        }

        #endregion
        #region GetProvider

        protected TileProvider GetProvider()
        {
            (VisualObject node, int x, int y) = GetProviderNode();
            return new TileProvider(node?.Provider ?? Main.tile, x, y);
        }

        #endregion
        #region GetProviderNode

        public virtual (VisualObject, int, int) GetProviderNode()
        {
            VisualObject node = this;
            int x = 0, y = 0;
            while (node != null)
            {
                if (node.Provider != null)
                    return (node, x, y);
                else
                {
                    x += node.X;
                    y += node.Y;
                    node = node.Parent;
                }
            }
            return (null, x, y);
        }

        #endregion
        /*#region Resize

        public virtual bool Resize(int width, int height)
        {
            return false;
        }

        #endregion*/
        #region Enable

        public virtual VisualObject Enable()
        {
            if (!Enabled)
            {
                Enabled = true;
                if (Provider != null)
                    UI.EnabledHook.Invoke(new EnabledArgs(this, true));
            }
            return this;
        }

        #endregion
        #region Disable

        public virtual VisualObject Disable()
        {
            if (Enabled)
            {
                Enabled = false;
                if (Provider != null)
                    UI.EnabledHook.Invoke(new EnabledArgs(this, false));
            }
            return this;
        }

        #endregion
        #region Apply

        public virtual VisualObject Apply(bool forceClear = false)
        {
            if (!Active())
                throw new InvalidOperationException("Trying to call Apply() an not active object.");
            ApplyThis(forceClear);
            ApplyChild();
            return this;
        }

        #endregion
        #region ApplyThis

        public virtual VisualObject ApplyThis(bool forceClear = false)
        {
            ApplyTiles(forceClear);
            if (UI.ShowGrid)
                ShowGrid();
            Configuration.CustomApply?.Invoke(this);
            return this;
        }

        #endregion
        #region ApplyTiles

        public virtual VisualObject ApplyTiles(bool forceClear)
        {
            return this;
        }

        #endregion
        #region ShowGrid

        public void ShowGrid()
        {

        }

        #endregion
        #region ApplyChild

        public virtual VisualObject ApplyChild()
        {
            foreach (VisualObject child in Child)
                if (child.Enabled)
                {
                    child.Apply(false);
                    ForceSection = ForceSection || child.ForceSection;
                }
            return this;
        }

        #endregion
        #region Draw

        public virtual VisualObject Draw(int dx = 0, int dy = 0, int width = -1, int height = -1)
        {
            return this;
        }

        #endregion
        #region DrawPoints

        public virtual VisualObject DrawPoints(List<(int, int)> list)
        {
            return this;
        }

        #endregion
        #region Clear

        public virtual VisualObject Clear()
        {
            return this;
        }

        #endregion
        #region Popup

        public virtual VisualObject Popup()
        {
            return null;
        }

        public virtual VisualObject Popdown()
        {
            return null;
        }

        #endregion
        #region Database

        public virtual void Database()
        {

        }

        public virtual void UserDatabase()
        {

        }

        #endregion
        #region Clone

        public override object Clone()
        {
            return base.Clone();
        }

        #endregion
    }
}
