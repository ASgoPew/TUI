using TUI.Base.Style;
using TUI.Hooks.Args;

namespace TUI.Base
{
    public class RootVisualObject : VisualObject
    {
        #region Data

        public override string Name { get; }
        public override dynamic Provider { get; }

        #endregion

        #region Initialize

        internal RootVisualObject(string name, int x, int y, int width, int height, dynamic provider,
                UIConfiguration configuration = null, UIStyle style = null)
            : base(x, y, width, height, configuration ?? new UIConfiguration() { UseBegin = false }, style)
        {
            Name = name;
            Provider = provider;
        }

        #endregion
        #region SetXYWH

        public override VisualObject SetXYWH(int x, int y, int width = -1, int height = -1)
        {
            base.SetXYWH(x, y, width, height);
            Provider.SetXYWH(x, y, width, height);
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
                Provider.SetEnabled(true);
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
                Provider.SetEnabled(false);
                UI.Hooks.Enabled.Invoke(new EnabledArgs(this, false));
            }
            return this;
        }

        #endregion
        #region Apply

        protected override void ApplyThisNative()
        {
            Clear();
            base.ApplyThisNative();
        }

        #endregion
    }
}
