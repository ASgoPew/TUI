using System;
using System.Diagnostics;
using TUI.Base.Style;
using TUI.Hooks.Args;

namespace TUI.Base
{
    public class RootVisualObject : VisualObject
    {
        #region Data

        public override string Name { get; }
        public override dynamic Provider { get; }
        public override int Layer
        {
            get => UsesDefaultMainProvider ? 0 : Provider.IsPersonal ? 2 : 1;
            set => throw new Exception("You can't set a layer for RootVisualObject");
        }

        #endregion

        #region Initialize

        internal RootVisualObject(string name, int x, int y, int width, int height,
                UIConfiguration configuration = null, UIStyle style = null, object provider = null)
            : base(x, y, width, height, configuration ?? new UIConfiguration() { UseBegin = false }, style)
        {
            Configuration.UseOutsideTouches = false;
            Name = name;
            if (provider == null)
                Provider = new MainTileProvider();
            else
                Provider = provider;
        }

        #endregion
        #region SetXYWH

        public override VisualObject SetXYWH(int x, int y, int width = -1, int height = -1)
        {
            base.SetXYWH(x, y, width, height);
            // MainTileProvider ignores this SetXYWH
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
        #region ApplyThisNative

        protected override void ApplyThisNative(bool clearTiles = true)
        {
            Stopwatch sw = Stopwatch.StartNew();
            base.ApplyThisNative(clearTiles);
#if DEBUG
            Console.WriteLine($"Apply ({Name}): {sw.ElapsedMilliseconds}");
#endif
            sw.Stop();
        }

        #endregion
        #region Tile

        public override dynamic Tile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                throw new ArgumentOutOfRangeException($"{FullName}: Invalid tile x or y.");
            return Provider[ProviderX + x, ProviderY + y];
        }

        #endregion
    }
}
