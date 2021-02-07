using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using TerrariaUI.Base.Style;
using TerrariaUI.Hooks;
using TerrariaUI.Hooks.Args;
using TerrariaUI.Widgets;

namespace TerrariaUI.Base
{
    /// <summary>
    /// Basic root of user interface tree widget.
    /// </summary>
    public class RootVisualObject : VisualContainer
    {
        #region Data

        /// <summary>
        /// Last Apply counter drawn to a player.
        /// </summary>
        public ConcurrentDictionary<int, ulong> PlayerApplyCounter { get; } =
            new ConcurrentDictionary<int, ulong>();
        /// <summary>
        /// List of players that are currently close enough to this interface.
        /// </summary>
        public HashSet<int> Players { get; } = new HashSet<int>();
        /// <summary>
        /// Unique interface root identifier.
        /// </summary>
        public override string Name { get; set; }
        /// <summary>
        /// Tile provider object, default value - null (interface would
        /// be drawn on the Main.tile, tiles would be irrevocably modified).
        /// <para></para>
        /// FakeTileRectangle from [FakeManager](https://github.com/AnzhelikaO/FakeManager)
        /// can be passed as a value so that interface would be drawn above the Main.tile.
        /// </summary>
        public override dynamic Provider { get; }
        /// <summary>
        /// Counter of Apply() calls. Interface won't redraw to user if ApplyCounter hasn't changed.
        /// </summary>
        public ulong DrawState { get; protected internal set; }
        protected VisualContainer PopUpBackground { get; set; }
        protected Dictionary<VisualObject, Action<VisualObject>> PopUpCancelCallbacks { get; set; } =
            new Dictionary<VisualObject, Action<VisualObject>>();

        /// <summary>
        /// 0 for Root with MainTileProvider, 1 for root with any fake provider.
        /// </summary>
        public override int Layer => UsesDefaultMainProvider ? 0 : 1;

        #endregion

        #region Constructor

        /// <summary>
        /// Basic root of user interface tree widget.
        /// </summary>
        /// <param name="name">Unique interface identifier</param>
        /// be drawn on the Main.tile, tiles would be irrevocably modified).
        /// <para></para>
        /// FakeTileRectangle from [FakeManager](https://github.com/AnzhelikaO/FakeManager)
        /// can be passed as a value so that interface would be drawn above the Main.tile.</param>
        public RootVisualObject(string name, int x, int y, int width, int height,
                UIConfiguration configuration = null, ContainerStyle style = null, object provider = null)
            : base(x, y, width, height, configuration ?? new UIConfiguration() { UseBegin=true, UseMoving=true, UseEnd=true }, style)
        {
            Name = name;
            if (provider != null)
                Provider = provider;
            else
            {
                var args = new CreateProviderArgs(this);
                TUI.Hooks.CreateProvider.Invoke(args);
                Provider = args.Provider ?? new MainTileProvider();
            }
        }

        #endregion
        #region DisposeThisNative

        protected override void DisposeThisNative()
        {
            base.DisposeThisNative();
            if (!(Provider is MainTileProvider))
                TUI.Hooks.RemoveProvider.Invoke(new RemoveProviderArgs(Provider));
        }

        #endregion
        #region Tile

        public override dynamic Tile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                throw new ArgumentOutOfRangeException($"{FullName}: Invalid tile x or y.");
            return Provider?[ProviderX + x, ProviderY + y];
        }

        #endregion
        #region SetXYWH

        public override VisualObject SetXYWH(int x, int y, int width, int height, bool draw = true)
        {
            width = Math.Max(width, Configuration.Grid?.MinWidth ?? 1);
            height = Math.Max(height, Configuration.Grid?.MinHeight ?? 1);

            if (x != X || y != Y || width != Width || height != Height)
            {
                if (UsesDefaultMainProvider && draw)
                    Clear();

                // MainTileProvider ignores this SetXYWH
                Provider.SetXYWH(x, y, width, height, false);
                base.SetXYWH(x, y, width, height, draw);
                TUI.Hooks.SetXYWH.Invoke(new SetXYWHArgs(this, x, y, width, height, draw));
            }
            return this;
        }

        #endregion
        #region Enable

        public override VisualObject Enable(bool draw = true)
        {
            if (!Enabled)
            {
                Provider.Enable(false);
                base.Enable(draw);
                TUI.Hooks.Enabled.Invoke(new EnabledArgs(this, true));
            }
            return this;
        }

        #endregion
        #region Disable

        public override VisualObject Disable(bool draw = true)
        {
            if (Enabled)
            {
                if (UsesDefaultMainProvider && draw)
                    Clear();

                Provider.Disable(false);
                base.Disable(draw);
                TUI.Hooks.Enabled.Invoke(new EnabledArgs(this, false));
            }
            return this;
        }

        #endregion
        #region DrawReposition

        protected override void DrawReposition(int oldX, int oldY, int oldWidth, int oldHeight)
        {
            RequestDrawChanges();
            Draw(oldX - X, oldY - Y, oldWidth, oldHeight, toEveryone: true);

            if (UsesDefaultMainProvider || oldWidth != Width || oldHeight != Height)
                Update().Apply();
            else
                RequestDrawChanges();
            Draw(toEveryone: true);
        }

        #endregion
        #region DrawEnable

        protected override void DrawEnable()
        {
            Update().Apply().Draw();
        }

        #endregion
        #region DrawDisable

        protected override void DrawDisable()
        {
            RequestDrawChanges().Draw();
        }

        #endregion

        #region ApplyThisNative

        protected override void ApplyThisNative()
        {
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
    }
}
