using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUI
{
    public class RootVisualObject : VisualObject
    {
        #region Data

        internal ITileCollection TileCollection { get; set; }
        public override string Name { get; protected set; }

        #endregion

        #region Initialize

        internal RootVisualObject(string name, int x, int y, int width, int height, ITileCollection tileCollection)
            : base(x, y, width, height)
        {
            Name = name;
            TileCollection = tileCollection;
        }

        #endregion
        #region SetXYWH

        public override VisualObject SetXYWH(int x, int y, int width = -1, int height = -1)
        {
            base.SetXYWH(x, y, width, height);
            if (TileCollection != null)
                UI.Hooks.SetXYWH.Invoke(new SetXYWHArgs(this, x, y, width, height));
            return this;
        }

        #endregion
        #region Enable

        public override VisualObject Enable()
        {
            if (!Enabled)
            {
                Enabled = true;
                if (TileCollection != null)
                    UI.Hooks.Enabled.Invoke(new EnabledArgs(this, true));
            }
            return this;
        }

        #endregion
        #region Disable

        public override VisualObject Disable()
        {
            if (Enabled)
            {
                Enabled = false;
                if (TileCollection != null)
                    UI.Hooks.Enabled.Invoke(new EnabledArgs(this, false));
            }
            return this;
        }

        #endregion
    }
}
