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
        private Summoning Summoning { get; set; }

        /// <summary>
        /// 0 for Root with MainTileProvider, 1 for root with any fake provider.
        /// </summary>
        public override int Layer => UsesDefaultMainProvider ? 0 : 1;
        public VisualObject Summoned => Summoning?.Summoned;

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
            Configuration.UseOutsideTouches = false;
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

        #region UpdateThisNative

        protected override void UpdateThisNative()
        {
            Provider.Update();
            if (!Loaded)
                LoadThisNative();
            UpdateSizeNative();
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
            Console.WriteLine($"Apply ({Name}): {sw.ElapsedMilliseconds}");
            sw.Stop();
#endif
        }

        #endregion
        #region ShowPopUp

        /// <summary>
        /// Draws popup object on top of all other child objects.
        /// </summary>
        /// <param name="style">Style of popup background</param>
        /// <param name="cancelCallback">Action to call when player touches popup background but not popup itself</param>
        /// <returns>PopUpBackground</returns>
        public virtual VisualContainer ShowPopUp(VisualObject popup, ContainerStyle style = null, Action<VisualObject> cancelCallback = null)
        {
            style = style ?? new ContainerStyle();
            style.Transparent = true;
            lock (Locker)
            {
                if (PopUpBackground == null)
                {
                    PopUpBackground = new VisualContainer(0, 0, 0, 0, new UIConfiguration()
                        { SessionAcquire=true }, style, (self, touch) =>
                    {
                        VisualObject selected = ((VisualContainer)self).Selected;
                        if (selected != null && PopUpCancelCallbacks.TryGetValue(selected, out Action<VisualObject> cancel))
                            cancel.Invoke(this);
                        else
                            HidePopUp();
                    });
                    Add(PopUpBackground, Int32.MaxValue);
                }
            }
            if (style != null)
                PopUpBackground.Style = style;
            PopUpBackground.SetFullSize(FullSize.Both);
            PopUpBackground.Add(popup);
            if (cancelCallback != null)
                PopUpCancelCallbacks[popup] = cancelCallback;
            PopUpBackground.DrawWithSection = DrawWithSection;
            PopUpBackground.Select(popup, false).Enable(false);
            Update();
            PopUpBackground.Apply().Draw();
            return PopUpBackground;
        }

        #endregion
        #region HidePopUp

        public virtual RootVisualObject HidePopUp()
        {
            if (PopUpBackground != null)
            {
                PopUpBackground.Disable(false);
                Apply().Draw();
            }
            return this;
        }

        #endregion
        #region Alert

        /// <summary>
        /// Show alert window with information text and "ok" button.
        /// </summary>
        /// <returns>this</returns>
        public virtual RootVisualObject Alert(string text, ContainerStyle windowStyle = null, ButtonStyle okButtonStyle = null)
        {
            ShowPopUp(new AlertWindow(text, windowStyle, okButtonStyle));
            return this;
        }

        #endregion
        #region Confirm

        /// <summary>
        /// Show confirm window with information text and "yes", "no" buttons.
        /// </summary>
        /// <returns>this</returns>
        public virtual RootVisualObject Confirm(string text, Action<bool> callback, ContainerStyle windowStyle = null,
            ButtonStyle yesButtonStyle = null, ButtonStyle noButtonStyle = null)
        {
            ShowPopUp(new ConfirmWindow(text, callback, windowStyle, yesButtonStyle, noButtonStyle));
            return this;
        }

        #endregion
        #region Summon

        public RootVisualObject Summon(VisualObject node, Alignment alignment = Alignment.Center)
        {
            lock (Locker)
            {
                if (Summoning != null)
                {
                    if (Summoned == node)
                        return this;
                    if (Summoning.RemoveOnUnsummon)
                        Remove(Summoned);
                }

                bool removeOnUnsummon = false;
                if (!Child.Contains(node))
                {
                    Add(node);
                    removeOnUnsummon = true;
                }

                (int oldX, int oldY, int oldWidth, int oldHeight) = XYWH();
                (int originalX, int originalY, int originalWidth, int originalHeight) =
                    (Summoning?.OldX ?? oldX, Summoning?.OldY ?? oldY,
                    Summoning?.OldWidth ?? oldWidth, Summoning?.OldHeight ?? oldHeight);
                Summoning = new Summoning(node, removeOnUnsummon, originalX, originalY, originalWidth, originalHeight);

                Select(node, false);
                int w = node.Width;
                int h = node.Height;

                int x;
                if (alignment == Alignment.UpLeft || alignment == Alignment.Left || alignment == Alignment.DownLeft)
                    x = originalX;
                else if (alignment == Alignment.UpRight || alignment == Alignment.Right || alignment == Alignment.DownRight)
                    x = originalX + (originalWidth - w);
                else
                    x = originalX + (originalWidth - w) / 2;

                int y;
                if (alignment == Alignment.UpLeft || alignment == Alignment.Up || alignment == Alignment.UpRight)
                    y = originalY;
                else if (alignment == Alignment.DownLeft || alignment == Alignment.Down || alignment == Alignment.DownRight)
                    y = originalY + (originalHeight - h);
                else
                    y = originalY + (originalHeight - h) / 2;

                SetXYWH(x, y, w, h, false);
                node.SetXY(0, 0, false);
                Update().Apply().Draw();

                DrawReposition(oldX, oldY, oldWidth, oldHeight);

                return this;
            }
        }

        #endregion
        #region Unsummon

        public RootVisualObject Unsummon()
        {
            lock (Locker)
            {
                (int oldX, int oldY, int oldWidth, int oldHeight) = XYWH();
                if (Summoning.RemoveOnUnsummon)
                    Remove(Summoned);
                SetXYWH(Summoning.OldX, Summoning.OldY, Summoning.OldWidth, Summoning.OldHeight, false);
                Deselect(false);
                DrawReposition(oldX, oldY, oldWidth, oldHeight);
                Summoning = null;

                return this;
            }
        }

        #endregion
    }
}
