namespace TUI
{
    public class RootVisualObject : VisualObject
    {
        #region Data

        public override string Name { get; protected set; }
        public override UITileProvider Provider { get; }

        #endregion

        #region Initialize

        internal RootVisualObject(string name, int x, int y, int width, int height, UITileProvider provider)
            : base(x, y, width, height)
        {
            Name = name;
            Provider = provider;
        }

        #endregion
        #region SetXYWH

        public override VisualObject SetXYWH(int x, int y, int width = -1, int height = -1)
        {
            base.SetXYWH(x, y, width, height);
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
                UI.Hooks.Enabled.Invoke(new EnabledArgs(this, false));
            }
            return this;
        }

        #endregion
    }
}
