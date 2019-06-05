using System;
using System.Collections.Generic;
using System.Diagnostics;
using TUI.Base.Style;
using TUI.Hooks.Args;

namespace TUI.Base
{
    public class RootVisualObject : VisualObject
    {
        #region Data

        public HashSet<int> Players = new HashSet<int>();

        public override string Name { get; }
        public override dynamic Provider { get; }
        public override int Layer
        {
            get => UsesDefaultMainProvider ? 0 : Provider.IsPersonal ? 2 : 1;
            set => throw new Exception("You can't set a layer for RootVisualObject");
        }

        public VisualObject PopUpBackground { get; protected set; }
        protected Dictionary<VisualObject, Action<VisualObject>> PopUpCancelCallbacks =
            new Dictionary<VisualObject, Action<VisualObject>>();
        
        #endregion

        #region Constructor

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
        #region Tile

        public override dynamic Tile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                throw new ArgumentOutOfRangeException($"{FullName}: Invalid tile x or y.");
            return Provider[ProviderX + x, ProviderY + y];
        }

        #endregion
        #region SetXYWH

        public override VisualObject SetXYWH(int x, int y, int width = -1, int height = -1)
        {
            base.SetXYWH(x, y, width, height);
            // MainTileProvider ignores this SetXYWH
            Provider.SetXYWH(x, y, width, height);
            TUI.Hooks.SetXYWH.Invoke(new SetXYWHArgs(this, x, y, width, height));
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
                TUI.Hooks.Enabled.Invoke(new EnabledArgs(this, true));
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
                TUI.Hooks.Enabled.Invoke(new EnabledArgs(this, false));
            }
            return this;
        }

        #endregion
        #region ApplyThisNative

        protected override void ApplyThisNative()
        {
            Clear();

#if DEBUG
            Stopwatch sw = Stopwatch.StartNew();
#endif
            base.ApplyThisNative();
#if DEBUG
            Console.WriteLine($"Apply ({Name}): {sw.ElapsedMilliseconds}");
            sw.Stop();
#endif
        }

        #endregion
        #region UpdateThisNative

        protected override void UpdateThisNative()
        {
            base.UpdateThisNative();
            Provider.Update();
        }

        #endregion
        #region ShowPopUp

        /// <summary>
        /// Draws popup object.
        /// </summary>
        /// <returns>this</returns>
        public virtual VisualObject ShowPopUp(VisualObject popup, UIStyle background = null, Action<VisualObject> cancelCallback = null)
        {
            if (PopUpBackground == null)
            {
                PopUpBackground = new VisualObject(0, 0, 0, 0, new UIConfiguration() { SessionAcquire=true }, null, (self, touch) =>
                {
                    VisualObject selected = self.Selected();
                    if (selected != null && PopUpCancelCallbacks.TryGetValue(selected, out Action<VisualObject> cancel))
                        cancel.Invoke(this);
                    else
                        HidePopUp();
                    return true;
                });
                Add(PopUpBackground, Int32.MaxValue);
            }
            if (background != null)
                PopUpBackground.Style = background;
            PopUpBackground.SetFullSize();
            PopUpBackground.Add(popup);
            if (cancelCallback != null)
                PopUpCancelCallbacks[popup] = cancelCallback;
            PopUpBackground.ForceSection = ForceSection;
            Update();
            PopUpBackground.Select(popup).Enable().Apply().Draw();
            return this;
        }

        #endregion
        #region HidePopUp

        /// <summary>
        /// Hides popup.
        /// </summary>
        /// <returns>this</returns>
        public virtual VisualObject HidePopUp()
        {
            if (PopUpBackground != null)
            {
                PopUpBackground.Disable();
                return Apply().Draw();
            }
            return this;
        }

        #endregion
    }
}
