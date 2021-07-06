using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TerrariaUI.Hooks;
using TerrariaUI.Hooks.Args;

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
        /// List of players who can see this interface if personal, null otherwise.
        /// </summary>
        public HashSet<int> Observers { get; } = new HashSet<int>();

        /// <summary>
        /// Unique interface root identifier.
        /// </summary>
        public override string Name { get; set; }
        public override dynamic Provider => _Provider;
        /// <summary>
        /// Counter of Apply() calls. Interface won't redraw to user if ApplyCounter hasn't changed.
        /// </summary>
        public ulong DrawState { get; protected internal set; }
        /// <summary>
        /// Tile provider object, default value - null (interface would
        /// be drawn on the Main.tile, tiles would be irrevocably modified).
        /// <para></para>
        /// TileProvider from [FakeProvider](https://github.com/AnzhelikaO/FakeProvider)
        /// can be passed as a value so that interface would be drawn above the Main.tile.
        /// </summary>
        protected dynamic _Provider { get; set; }
        protected VisualContainer PopUpBackground { get; set; }
        protected Dictionary<VisualObject, Action<VisualObject>> PopUpCancelCallbacks { get; set; } =
            new Dictionary<VisualObject, Action<VisualObject>>();
        public bool Freezed { get; private set; } = false;

        /// <summary>
        /// Personal interface can be seen by only specified players.
        /// </summary>
        public bool Personal => Observers != null;
        /// <summary>
        /// 0 for Root with MainTileProvider, 1 for root with any fake provider.
        /// </summary>
        public override int Layer => Personal ? 2 :
            !UsesDefaultMainProvider ? 1 : 0;
        public override bool Orderable => true;
        public override int MinWidth => Math.Max(base.MinWidth, 1);
        public override int MinHeight => Math.Max(base.MinHeight, 1);

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
                UIConfiguration configuration = null, ContainerStyle style = null, object provider = null, HashSet<int> observers = null)
            : base(x, y, width, height, configuration ?? new UIConfiguration() { UseBegin=true, UseMoving=true, UseEnd=true }, style)
        {
            Name = name;
            Observers = observers; // we have to initialize this field before trying to create provider
            _Provider = provider;
        }

        #endregion
        #region Tile

        public override dynamic Tile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                throw new ArgumentOutOfRangeException($"{FullName}: Invalid tile x or y.");
            return base.Tile(x, y);
        }

        #endregion
        #region SetXYWH

        public override VisualObject SetXYWH(int x, int y, int width, int height, bool draw)
        {
            int oldX = X, oldY = Y, oldWidth = Width, oldHeight = Height;
            base.SetXYWH(x, y, width, height, false);
            if (oldX != X || oldY != Y || oldWidth != Width || oldHeight != Height
                && IsActive)
            {
                // MainTileProvider ignores this SetXYWH
                Provider?.SetXYWH(x, y, width, height, false);
                TUI.Hooks.SetXYWH.Invoke(new SetXYWHArgs(this, x, y, width, height));

                if (draw)
                    DrawReposition(oldX, oldY, oldWidth, oldHeight);
            }
            return this;
        }

        #endregion
        #region Enable

        public override VisualObject Enable(bool draw)
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

        public override VisualObject Disable(bool draw)
        {
            if (Enabled)
            {
                if (UsesDefaultMainProvider)
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
            if (UsesDefaultMainProvider)
                ClearOld(oldX, oldY, oldWidth, oldHeight);
            else
                RequestDrawChanges();
            Draw(oldX - X, oldY - Y, oldWidth, oldHeight, OutdatedPlayers(toEveryone: true));

            Update();
            if (UsesDefaultMainProvider || oldWidth != Width || oldHeight != Height)
                Apply();
            else
                RequestDrawChanges();
            Draw(targetPlayers: OutdatedPlayers(toEveryone: true));
        }

        #endregion
        #region ClearOld

        private void ClearOld(int x, int y, int width, int height)
        {
            lock (ApplyLocker)
            {
                for (int _x = x; _x < x + width; _x++)
                    for (int _y = y; _y < y + height; _y++)
                        // getting tile directly to avoid bounds check
                        Provider[_x, _y]?.ClearEverything();

                // Mark changes to be drawn
                RequestDrawChanges();
            }
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
            RequestDrawChanges().Draw(targetPlayers: OutdatedPlayers(toEveryone: true));
        }

        #endregion
        #region CanSee

        public virtual bool CanSee(int playerIndex) =>
            Observers?.Contains(playerIndex) != false;

        #endregion
        #region Freeze

        public virtual RootVisualObject Freeze()
        {
            Freezed = true;
            return this;
        }

        #endregion
        #region Unfreeze

        public virtual RootVisualObject Unfreeze()
        {
            Freezed = false;
            return this;
        }

        #endregion

        #region LoadThisNative

        protected override void LoadThisNative()
        {
            base.LoadThisNative();

            if (Provider == null)
            {
                var args = new CreateProviderArgs(this);
                TUI.Hooks.CreateProvider.Invoke(args); // fake or personal fake is root.Observers is not null
                _Provider = args.Provider ?? new MainTileProvider();
            }
            if (Personal && Provider is MainTileProvider)
                throw new NotSupportedException("Personal UI is not supported with MainTileProvider: "+ FullName);
            SetXYWH(X, Y, Width, Height, false);

            TUI.Hooks.LoadRoot.Invoke(new LoadRootArgs(this));
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
        #region PulseThisNative

        protected override void PulseThisNative(PulseType type)
        {
            base.PulseThisNative(type);
            switch (type)
            {
                case PulseType.SetXYWH:
                    (ProviderX, ProviderY) = ProviderXY();
                    break;
            }
        }

        #endregion
        #region UpdateThisNative

        protected override void UpdateThisNative()
        {
            // MainTileProvider acquires Main.tile field
            Provider?.Update();
            base.UpdateThisNative();
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
            TUI.Log($"Apply ({Name}): {sw.ElapsedMilliseconds}");
            sw.Stop();
#endif
        }

        #endregion
    }
}
